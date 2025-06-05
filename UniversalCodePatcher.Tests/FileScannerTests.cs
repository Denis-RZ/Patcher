using Microsoft.VisualStudio.TestTools.UnitTesting;
using UniversalCodePatcher.Helpers;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace UniversalCodePatcher.Tests
{
    [TestClass]
    public class FileScannerTests
    {
        [TestMethod]
        public void FindPatchableFiles_RespectsExtensionsAndExcludes()
        {
            var root = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(root);
            Directory.CreateDirectory(Path.Combine(root, "bin"));
            File.WriteAllText(Path.Combine(root, "a.cs"), "");
            File.WriteAllText(Path.Combine(root, "b.txt"), "");
            File.WriteAllText(Path.Combine(root, "bin", "c.cs"), "");

            var files = FileScanner.FindPatchableFiles(root, new List<string>{".cs"}, new List<string>{"bin"}).ToList();
            Assert.AreEqual(1, files.Count);
            Assert.IsTrue(files[0].EndsWith("a.cs"));
            Directory.Delete(root, true);
        }
    }
}
