using Microsoft.VisualStudio.TestTools.UnitTesting;
using UniversalCodePatcher;

namespace UniversalCodePatcher.Tests
{
    [TestClass]
    public class LoggerTests
    {
        [TestMethod]
        public void SimpleLogger_RaisesEvent()
        {
            var logger = new SimpleLogger();
            LogEntry? captured = null;
            logger.OnLogged += e => captured = e;
            logger.LogInfo("hello");
            Assert.IsNotNull(captured);
            Assert.AreEqual(LogLevel.Info, captured!.Level);
            Assert.AreEqual("hello", captured.Message);
        }
    }
}
