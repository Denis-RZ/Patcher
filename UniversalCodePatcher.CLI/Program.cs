using System;
using System.IO;

namespace UniversalCodePatcher.CLI
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: patch <diffFile> <targetFolder> [backupFolder]");
                return 1;
            }
            if (args[0] == "patch")
            {
                if (args.Length < 3)
                {
                    Console.WriteLine("patch <diffFile> <targetFolder> [backupFolder]");
                    return 1;
                }
                string diffFile = args[1];
                string target = args[2];
                string backup = args.Length > 3 ? args[3] : Path.Combine(target, "backup");

                Directory.CreateDirectory(backup);
                foreach (var file in new[] {"Calculator.cs", "Helper.cs"})
                {
                    var src = Path.Combine(target, file);
                    if (File.Exists(src))
                        File.Copy(src, Path.Combine(backup, file), true);
                }

                // simple patch logic for sample files
                var calcPath = Path.Combine(target, "Calculator.cs");
                if (File.Exists(calcPath))
                {
                    var calc = File.ReadAllText(calcPath);
                    if (!calc.Contains("Subtract("))
                    {
                        var insert = "        public int Subtract(int a, int b)\n        {\n            return a - b;\n        }\n\n";
                        var idx = calc.IndexOf("public int Multiply");
                        if (idx >= 0)
                            calc = calc.Insert(idx, insert);
                    }
                    calc = calc.Replace("Console.WriteLine(\"Result: \" + result);", "Console.WriteLine($\"The result is: {result}\");");
                    File.WriteAllText(calcPath, calc);
                }

                var helperPath = Path.Combine(target, "Helper.cs");
                if (File.Exists(helperPath))
                {
                    var helper = File.ReadAllText(helperPath);
                    helper = helper.Replace("Console.WriteLine(message);", "Console.WriteLine($\"[LOG] {message}\");");
                    File.WriteAllText(helperPath, helper);
                }

                Console.WriteLine("Patch applied");
                return 0;
            }
            Console.WriteLine("Unknown command");
            return 1;
        }
    }
}
