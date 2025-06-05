using Microsoft.VisualStudio.TestTools.UnitTesting;
using UniversalCodePatcher.Core;
using System;

namespace UniversalCodePatcher.Tests
{
    [TestClass]
    public class ServiceContainerTests
    {
        private interface ITestService { Guid Id { get; } }
        private class TestService : ITestService { public Guid Id { get; } = Guid.NewGuid(); }

        [TestMethod]
        public void RegisterSingleton_CreatesAndReturnsSameInstance()
        {
            var container = new ServiceContainer();
            container.RegisterSingleton<ITestService, TestService>();
            var s1 = container.GetService<ITestService>();
            var s2 = container.GetService<ITestService>();
            Assert.AreSame(s1, s2);
        }

        [TestMethod]
        public void RegisterInstance_ReturnsProvidedInstance()
        {
            var container = new ServiceContainer();
            var instance = new TestService();
            container.RegisterInstance<ITestService>(instance);
            var resolved = container.GetService<ITestService>();
            Assert.AreSame(instance, resolved);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetService_Unregistered_Throws()
        {
            var container = new ServiceContainer();
            container.GetService<ITestService>();
        }

        [TestMethod]
        public void IsRegistered_ChecksCorrectly()
        {
            var container = new ServiceContainer();
            Assert.IsFalse(container.IsRegistered<ITestService>());
            container.RegisterSingleton<ITestService, TestService>();
            Assert.IsTrue(container.IsRegistered(typeof(ITestService)));
        }
    }
}
