using Microsoft.VisualStudio.TestTools.UnitTesting;
using UniversalCodePatcher.Modules.DiffModule;
using UniversalCodePatcher.Core;
using System.Linq;
using System.Collections.Generic;

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

        private class DummyAlgorithm : IDiffAlgorithm
        {
            public IEnumerable<string> GetDiff(string oldText, string newText)
            {
                yield return "custom";
            }
        }

        [TestMethod]
        public void GetDiff_UsesAlgorithmFromContainer()
        {
            var container = new ServiceContainer();
            container.RegisterInstance<IDiffAlgorithm>(new DummyAlgorithm());
            var module = new DiffModule(container);
            var diff = module.GetDiff("x", "y").ToArray();
            Assert.AreEqual(1, diff.Length);
            Assert.AreEqual("custom", diff[0]);
        }
    }
}
