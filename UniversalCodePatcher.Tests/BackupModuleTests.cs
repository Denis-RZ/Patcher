using Microsoft.VisualStudio.TestTools.UnitTesting;
using UniversalCodePatcher.Modules.BackupModule;
using System.IO;

namespace UniversalCodePatcher.Tests
{
    [TestClass]
    public class BackupModuleTests
    {
        [TestMethod]
        public void CreateBackup_CopiesFile()
        {
            var module = new BackupModule();
            string temp = Path.GetTempFileName();
            File.WriteAllText(temp, "data");
            var backup = module.CreateBackup(temp);
            Assert.IsTrue(File.Exists(backup));
            Assert.AreEqual(File.ReadAllText(temp), File.ReadAllText(backup));
            File.Delete(temp);
            File.Delete(backup);
        }
    }
}
