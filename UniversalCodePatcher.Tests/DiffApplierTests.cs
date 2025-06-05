using Microsoft.VisualStudio.TestTools.UnitTesting;
using UniversalCodePatcher.DiffEngine;
using System.IO;
using System.Linq;

namespace UniversalCodePatcher.Tests
{
    [TestClass]
    public class DiffApplierTests
    {
        [TestMethod]
        public void ApplyDiff_PatchesFileAndCreatesBackup()
        {
            var root = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(root);
            var file = Path.Combine(root, "test.txt");
            File.WriteAllText(file, "old");
            string diffText = "--- test.txt\n+++ test.txt\n@@ -1,1 +1,2 @@\n old\n+new";
            var diffPath = Path.Combine(root, "patch.diff");
            File.WriteAllText(diffPath, diffText);
            var backupDir = Path.Combine(root, "backup");
            var result = DiffApplier.ApplyDiff(diffPath, root, backupDir, false);
            var rolledBack = (System.Collections.Generic.Dictionary<string, string>)result.Metadata["RolledBackFiles"];
            Assert.AreEqual(0, rolledBack.Count);
            Assert.AreEqual("old\nnew\n", File.ReadAllText(file).Replace("\r", ""));
            Assert.IsTrue(Directory.GetFiles(backupDir, "*", System.IO.SearchOption.AllDirectories).Length > 0);
            Directory.Delete(root, true);
        }

        [TestMethod]
        public void ApplyDiff_ContextMismatch_RollsBack()
        {
            var root = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(root);
            var file = Path.Combine(root, "test.txt");
            File.WriteAllText(file, "old");
            string diffText = "--- test.txt\n+++ test.txt\n@@ -1,1 +1,1 @@\n-different\n+new";
            var diffPath = Path.Combine(root, "patch.diff");
            File.WriteAllText(diffPath, diffText);
            var backupDir = Path.Combine(root, "backup");
            var result = DiffApplier.ApplyDiff(diffPath, root, backupDir, false);
            var rolledBack = (System.Collections.Generic.Dictionary<string, string>)result.Metadata["RolledBackFiles"];
            Assert.IsTrue(rolledBack.ContainsKey(file));
            Assert.AreEqual("old", File.ReadAllText(file));
            Directory.Delete(root, true);
        }

        [TestMethod]
        public void ApplyDiff_IgnoresWhitespaceDifferences()
        {
            var root = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(root);
            var file = Path.Combine(root, "Example.cs");
            string fileContent = string.Join('\n', new[]
            {
                "        public int Add(int a, int b)",
                "        {",
                "            return a + b;",
                "        }",
                "",
                "        public int Multiply(int a, int b)",
                "        {",
                "            return a * b;",
                "        }",
                ""
            });
            File.WriteAllText(file, fileContent);
            string diffText = string.Join('\n', new[]
            {
                "--- Example.cs",
                "+++ Example.cs",
                "@@ -1,9 +1,14 @@",
                "        public int Add(int a, int b)",
                "        {",
                "             return a + b;",
                "        }",
                "",
                "        public int Subtract(int a, int b)",
                "        {",
                "             return a - b;",
                "        }",
                "",
                "        public int Multiply(int a, int b)",
                "        {",
                "            return a * b;",
                "        }",
                ""
            });
            var diffPath = Path.Combine(root, "patch.diff");
            File.WriteAllText(diffPath, diffText);
            var backupDir = Path.Combine(root, "backup");
            var result = DiffApplier.ApplyDiff(diffPath, root, backupDir, false);
            var rolledBack = (System.Collections.Generic.Dictionary<string, string>)result.Metadata["RolledBackFiles"];
            Assert.AreEqual(0, rolledBack.Count);
            var lines = File.ReadAllLines(file);
            Assert.IsTrue(lines.Any(l => l.Contains("Subtract")));
            Directory.Delete(root, true);
        }
    }
}
