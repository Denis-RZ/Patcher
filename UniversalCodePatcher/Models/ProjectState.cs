using System.Collections.Generic;

namespace UniversalCodePatcher.Models
{
    public class ProjectState
    {
        public string? ProjectPath { get; set; }
        public List<SimplePatchRule> Rules { get; set; } = new();
        public List<string> SelectedFiles { get; set; } = new();
    }

    public class SimplePatchRule
    {
        public string? Name { get; set; }
        public string Pattern { get; set; } = string.Empty;
        public string Replace { get; set; } = string.Empty;
    }
}
