using System.Collections.Generic;
namespace UniversalCodePatcher.Modules.DiffModule
{
    /// <summary>
    /// Interface for diff generation algorithms.
    /// </summary>
    public interface IDiffAlgorithm
    {
        /// <summary>
        /// Generate a diff between two text versions.
        /// </summary>
        IEnumerable<string> GetDiff(string oldText, string newText);
    }
}
