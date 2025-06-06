using Microsoft.VisualStudio.TestTools.UnitTesting;
using UniversalCodePatcher.Models;
using System.Collections.Generic;

namespace UniversalCodePatcher.Tests
{
    [TestClass]
    public class PatchResultTests
    {
        [TestMethod]
        public void Metadata_AllowsAddingValues()
        {
            var result = new PatchResult();
            result.Metadata["foo"] = "bar";
            Assert.AreEqual("bar", result.Metadata["foo"]);
        }

        [TestMethod]
        public void Metadata_IsInitialized()
        {
            var result = new PatchResult();
            Assert.IsNotNull(result.Metadata);
            Assert.AreEqual(0, result.Metadata.Count);
        }
    }
}
