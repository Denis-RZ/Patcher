using Microsoft.VisualStudio.TestTools.UnitTesting;
using UniversalCodePatcher.DiffEngine;

namespace UniversalCodePatcher.Tests
{
    [TestClass]
    public class UnifiedDiffParserTests
    {
        [TestMethod]
        public void Parse_SimplePatch_ReturnsFileDiff()
        {
            var diff = "--- test.txt\n+++ test.txt\n@@ -1,1 +1,2 @@\n old\n+new";
            var parser = new UnifiedDiffParser();
            var patch = parser.Parse(diff);
            Assert.AreEqual(1, patch.Files.Count);
            var file = patch.Files[0];
            Assert.AreEqual("test.txt", file.OriginalPath);
            Assert.AreEqual("test.txt", file.ModifiedPath);
            Assert.AreEqual(1, file.Hunks.Count);
            Assert.AreEqual(2, file.Hunks[0].Lines.Count);
        }
    }
}
