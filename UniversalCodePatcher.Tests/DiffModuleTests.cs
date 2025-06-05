using Microsoft.VisualStudio.TestTools.UnitTesting;
using UniversalCodePatcher.Modules.DiffModule;
using System.Linq;

namespace UniversalCodePatcher.Tests
{
    [TestClass]
    public class DiffModuleTests
    {
        [TestMethod]
        public void GetDiff_ReturnsChangedLines()
        {
            var module = new DiffModule();
            var diff = module.GetDiff("a\nb", "a\nc").ToArray();
            Assert.AreEqual(2, diff.Length);
            Assert.AreEqual("- b", diff[0]);
            Assert.AreEqual("+ c", diff[1]);
        }
    }
}
