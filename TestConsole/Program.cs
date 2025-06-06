using System;
using System.IO;
using UniversalCodePatcher.DiffEngine;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Universal Code Patcher Test ===");
            
            string testDir = "TestProject";
            string patchFile = "test.patch";
            
            try 
            {
                Console.WriteLine("1. Testing patch application...");
                var result = DiffApplier.ApplyDiff(patchFile, testDir, $"{testDir}/backups", false);
                
                var patched = (System.Collections.Generic.List<string>)result.Metadata["PatchedFiles"];
                Console.WriteLine($"✓ Patched files: {patched.Count}");
                var failures = (System.Collections.Generic.Dictionary<string,string>)result.Metadata["RolledBackFiles"];
                Console.WriteLine($"✗ Failed files: {failures.Count}");

                foreach (var file in patched)
                {
                    Console.WriteLine($"  Patched: {file}");
                }

                if (failures.Count > 0)
                {
                    foreach (var failure in failures)
                        Console.WriteLine($"  Failed: {failure.Key} -> {failure.Value}");
                }
                
                Console.WriteLine("Test completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
            }
        }
    }
}
