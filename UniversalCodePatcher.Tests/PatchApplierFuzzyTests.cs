using Microsoft.VisualStudio.TestTools.UnitTesting;
using UniversalCodePatcher.DiffEngine;
using System.IO;
using System.Linq;

namespace UniversalCodePatcher.Tests
{
    [TestClass]
    public class PatchApplierFuzzyTests
    {
        [TestMethod]
        public void Apply_IgnoresWhitespaceDifferences()
        {
            Directory.CreateDirectory("rootw");
            File.WriteAllText("rootw/Test.cs", "namespace   Demo {}\n");
            var diff = "--- a/Test.cs\n+++ b/Test.cs\n@@ -1,1 +1,2 @@\n namespace Demo {}\n+//added\n";
            File.WriteAllText("patchw.diff", diff);
            var applier = new PatchApplier(new SimpleLogger());
            var result = applier.ApplyDiff("patchw.diff", "rootw", "backupw", false);
            Assert.IsTrue(result.Success);
            var lines = File.ReadAllLines("rootw/Test.cs");
            Assert.IsTrue(lines.Any(l => l.Contains("added")));
        }
    }
}
