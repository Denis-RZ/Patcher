using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
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

        public int MaxBackups { get; set; } = 5;

        protected override bool OnInitialize() => true;

        public string CreateBackup(string filePath)
        {
            var backupPath = filePath + ".bak_" + DateTime.Now.ToString("yyyyMMddHHmmss");
            File.Copy(filePath, backupPath, true);
            CleanupOldBackups(filePath);
            return backupPath;
        }

        private void CleanupOldBackups(string filePath)
        {
            var dir = Path.GetDirectoryName(filePath);
            if (dir == null) return;
            var pattern = Path.GetFileName(filePath) + ".bak_*";
            var backups = Directory.GetFiles(dir, pattern)
                .OrderByDescending(f => File.GetCreationTime(f))
                .ToList();
            foreach (var old in backups.Skip(MaxBackups))
            {
                try { File.Delete(old); } catch { }
            }
        }
    }
}
