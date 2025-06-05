using System;
using System.IO;
using System.Collections.Generic;
using UniversalCodePatcher.Interfaces;
using UniversalCodePatcher.Core;

namespace UniversalCodePatcher.Modules.BackupModule
{
    public class BackupModule : BaseModule
    {
        public override string ModuleId => "backup-module";
        public override string Name => "Backup Module";
        public override Version Version => new(1, 0, 0);
        public override string Description => "Provides backup management";
        public override IEnumerable<string> Dependencies => Array.Empty<string>();

        protected override bool OnInitialize()
        {
            return true;
        }

        public string CreateBackup(string filePath)
        {
            var backupPath = filePath + ".bak_" + DateTime.Now.ToString("yyyyMMddHHmmss");
            File.Copy(filePath, backupPath, true);
            return backupPath;
        }
    }
}
