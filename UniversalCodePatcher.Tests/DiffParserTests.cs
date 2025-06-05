using Microsoft.VisualStudio.TestTools.UnitTesting;
using UniversalCodePatcher.DiffEngine;
using System.IO;
using System.Linq;

namespace UniversalCodePatcher.Tests
{
    [TestClass]
    public class DiffParserTests
    {
        [TestMethod]
        public void Parse_SimpleAddHunk_ReturnsSingleDiffFile()
        {
            var diffText = "--- a/FileA.cs\n+++ b/FileA.cs\n@@ -0,0 +1,3 @@\n+using System;\n+public class A { }\n";
            File.WriteAllText("test.diff", diffText);
            var parser = new DiffParser();
            var result = parser.Parse("test.diff");
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("FileA.cs", result[0].NewFilePath);
            Assert.IsTrue(result[0].Hunks.Count == 1);
            File.Delete("test.diff");
        }

        [TestMethod]
        [ExpectedException(typeof(DiffParseException))]
        public void Parse_MalformedDiff_Throws()
        {
            File.WriteAllText("bad.diff", "This is not a diff");
            var parser = new DiffParser();
            parser.Parse("bad.diff");
        }
    }
}
