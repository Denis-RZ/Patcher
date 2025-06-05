using System.Collections.Generic;

namespace UniversalCodePatcher.DiffEngine
{
    public class PatchResult
    {
        public List<string> PatchedFiles { get; } = new();
        public Dictionary<string, string> RolledBackFiles { get; } = new();
        public Dictionary<string, string> SkippedFiles { get; } = new();
    }
}
