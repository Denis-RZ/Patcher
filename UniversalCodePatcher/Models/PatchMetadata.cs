using System;
using System.Collections.Generic;

namespace UniversalCodePatcher.Models
{
    /// <summary>
    /// Container for additional data produced during patching.
    /// Inherits from Dictionary for flexible key/value storage.
    /// </summary>
    public class PatchMetadata : Dictionary<string, object>
    {
        public PatchMetadata() : base(StringComparer.OrdinalIgnoreCase) { }
    }
}
