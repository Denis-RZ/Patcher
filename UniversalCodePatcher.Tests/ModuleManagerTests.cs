using Microsoft.VisualStudio.TestTools.UnitTesting;
using UniversalCodePatcher.Core;
using UniversalCodePatcher.Interfaces;
using System;

namespace UniversalCodePatcher.Tests
{
    [TestClass]
    public class ModuleManagerTests
    {
        private class DummyModule : BaseModule
        {
            public override string ModuleId => "dummy";
            public override string Name => "Dummy";
            public override Version Version => new(1,0,0);
            public override string Description => "";
            protected override bool OnInitialize() => true;
        }

        [TestMethod]
        public void LoadAndUnloadModule_RaisesEvents()
        {
            var container = new ServiceContainer();
            var manager = new ModuleManager(container);
            bool loaded = false, unloaded = false;
            manager.ModuleLoaded += (_, e) => { if(e.Module.ModuleId=="dummy") loaded = true; };
            manager.ModuleUnloaded += (_, e) => { if(e.Module.ModuleId=="dummy") unloaded = true; };
            Assert.IsTrue(manager.LoadModule(typeof(DummyModule)));
            Assert.IsTrue(loaded);
            var module = manager.GetModule("dummy");
            Assert.IsNotNull(module);
            Assert.IsTrue(manager.UnloadModule("dummy"));
            Assert.IsTrue(unloaded);
        }

        [TestMethod]
        public void LoadModule_Twice_ReturnsFalse()
        {
            var container = new ServiceContainer();
            var manager = new ModuleManager(container);
            Assert.IsTrue(manager.LoadModule(typeof(DummyModule)));
            Assert.IsFalse(manager.LoadModule(typeof(DummyModule)));
        }
    }
}
