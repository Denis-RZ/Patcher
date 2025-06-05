using Microsoft.VisualStudio.TestTools.UnitTesting;
using UniversalCodePatcher.DiffEngine;
using System.IO;
using System.Linq;

namespace UniversalCodePatcher.Tests
{
    [TestClass]
    public class PatchApplierTests
    {
        [TestMethod]
        public void Apply_SuccessfulHunk_ModifiesFile()
        {
            Directory.CreateDirectory("root");
            File.WriteAllText("root/Hello.cs", "namespace Demo {}\n");
            var diff = "--- a/Hello.cs\n+++ b/Hello.cs\n@@ -1,1 +1,2 @@\n namespace Demo {}\n+// Added comment\n";
            File.WriteAllText("patch.diff", diff);
            var applier = new PatchApplier(new SimpleLogger());
            var result = applier.ApplyDiff("patch.diff", "root", "backup", false);
            Assert.IsTrue(result.Success);
            var lines = File.ReadAllLines("root/Hello.cs");
            Assert.IsTrue(lines.Any(l => l.Contains("Added comment")));
        }

        [TestMethod]
        public void Apply_ContextMismatch_RollsBack()
        {
            Directory.CreateDirectory("root2");
            File.WriteAllText("root2/Program.cs", "Console.WriteLine(\"Old\");\n");
            var diff = "--- a/Program.cs\n+++ b/Program.cs\n@@ -1,1 +1,1 @@\n-Console.WriteLine(\"Different\");\n+Console.WriteLine(\"New\");\n";
            File.WriteAllText("patch2.diff", diff);
            var applier = new PatchApplier(new SimpleLogger());
            var result = applier.ApplyDiff("patch2.diff", "root2", "backup2", false);
            Assert.IsFalse(result.Success);
            var content = File.ReadAllText("root2/Program.cs");
            Assert.IsTrue(content.Contains("Old"));
        }
    }
}
