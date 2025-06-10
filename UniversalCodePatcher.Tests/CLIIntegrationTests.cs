using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;

namespace UniversalCodePatcher.Tests
{
    [TestClass]
    public class CLIIntegrationTests
    {
        private const string CalcOriginal = @"namespace Demo
{
    public class Calculator
    {
        public int Add(int a, int b)
        {
            return a + b;
        }

        public int Multiply(int a, int b)
        {
            return a * b;
        }

        public void PrintResult(int result)
        {
            Console.WriteLine(""Result: "" + result);
        }
    }
}
";

        private const string HelperOriginal = @"namespace Demo
{
    public static class Helper
    {
        public static void Log(string message)
        {
            Console.WriteLine(message);
        }

        public static int Square(int number)
        {
            return number * number;
        }
    }
}
";

        [TestMethod]
        public void PatchCommand_AppliesPatchAndCreatesBackup()
        {
            var root = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(root);
            File.WriteAllText(Path.Combine(root, "Calculator.cs"), CalcOriginal);
            File.WriteAllText(Path.Combine(root, "Helper.cs"), HelperOriginal);

            var diff = Path.Combine(root, "patch.diff");
            var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../"));
            File.Copy(Path.Combine(repoRoot, "test.patch"), diff);

            var backup = Path.Combine(root, "backup");
            var cliProject = Path.Combine(repoRoot, "UniversalCodePatcher.CLI");
            var psi = new ProcessStartInfo("dotnet", $"run --project {cliProject} -- patch {diff} {root} {backup}")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            var process = Process.Start(psi)!;
            process.WaitForExit();
            string output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
            Assert.AreEqual(0, process.ExitCode, output);
            Assert.IsTrue(File.ReadAllText(Path.Combine(root, "Calculator.cs")).Contains("Subtract"));
            Assert.IsTrue(Directory.Exists(backup));

            // compile the patched files to ensure they build
            var projectDir = Path.Combine(root, "buildproj");
            var newProj = Process.Start(new ProcessStartInfo("dotnet", $"new classlib --output {projectDir} --no-restore") { RedirectStandardOutput = true, RedirectStandardError = true });
            newProj!.WaitForExit();
            Assert.AreEqual(0, newProj.ExitCode, newProj.StandardOutput.ReadToEnd() + newProj.StandardError.ReadToEnd());
            File.Delete(Path.Combine(projectDir, "Class1.cs"));
            File.Copy(Path.Combine(root, "Calculator.cs"), Path.Combine(projectDir, "Calculator.cs"));
            File.Copy(Path.Combine(root, "Helper.cs"), Path.Combine(projectDir, "Helper.cs"));
            var build = Process.Start(new ProcessStartInfo("dotnet", $"build {projectDir}") { RedirectStandardOutput = true, RedirectStandardError = true });
            build!.WaitForExit();
            Assert.AreEqual(0, build.ExitCode, build.StandardOutput.ReadToEnd() + build.StandardError.ReadToEnd());

            // call CLI entry directly for coverage
            var backup2 = Path.Combine(root, "backup2");
            Directory.CreateDirectory(backup2);
            var exit = UniversalCodePatcher.CLI.Program.Main(new[] { "patch", diff, root, backup2 });
            Assert.AreEqual(0, exit);

            Directory.Delete(root, true);
        }
    }
}
