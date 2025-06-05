using System;
// Simplified entry point for non-Windows environments
// GUI forms are excluded from the build to allow cross platform compilation.

namespace UniversalCodePatcher
{
    /// <summary>
    /// Точка входа в приложение
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            Console.WriteLine("UniversalCodePatcher GUI is unavailable in this build.");
        }
    }
}
