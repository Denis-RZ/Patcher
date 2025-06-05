using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UniversalCodePatcher.Helpers
{
    public static class FileScanner
    {
        public static IEnumerable<string> FindPatchableFiles(string rootFolder, IReadOnlyCollection<string> includeExtensions, IReadOnlyCollection<string> excludeFolderNames)
        {
            var comparer = System.StringComparer.OrdinalIgnoreCase;
            foreach (var file in Directory.EnumerateFiles(rootFolder, "*.*", SearchOption.AllDirectories))
            {
                string relative = file.Substring(rootFolder.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                var segments = relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                if (segments.Any(s => excludeFolderNames.Contains(s, comparer)))
                    continue;
                if (includeExtensions.Contains(Path.GetExtension(file), comparer))
                    yield return file;
            }
        }
    }
}
