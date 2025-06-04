using System;
using System.IO;
using UniversalCodePatcher.Interfaces;

namespace UniversalCodePatcher.Modules.BackupModule
{
    public class BackupModule : IModule
    {
        public string Name => "BackupModule";
        public string Version => "1.0";
        public void Initialize(IServiceContainer services) { }
        public void Unload() { }

        public string CreateBackup(string filePath)
        {
            var backupPath = filePath + ".bak_" + DateTime.Now.ToString("yyyyMMddHHmmss");
            File.Copy(filePath, backupPath, true);
            return backupPath;
        }
    }
}
