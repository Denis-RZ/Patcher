using Avalonia.Controls;
using Avalonia.Interactivity;
using System.IO;
using System.Linq;
using System;

namespace UniversalCodePatcher.Avalonia;

public partial class BackupManagerWindow : BaseDialog
{
    private readonly string _directory;

    public BackupManagerWindow(string directory)
    {
        _directory = directory;
        InitializeComponent();
        LoadBackups();
    }

    private void LoadBackups()
    {
        List.Items.Clear();
        if (Directory.Exists(_directory))
        {
            foreach (var file in Directory.GetFiles(_directory, "*.bak_*", SearchOption.AllDirectories))
            {
                List.Items.Add(file);
            }
        }
    }

    private void OnClose(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("Backup manager closed");
        SetCancelResult();
    }

    private void OnRestore(object? sender, RoutedEventArgs e)
    {
        if (List.SelectedItem is not string backupPath) return;
        var originalPath = GetOriginalPath(backupPath);
        try
        {
            File.Copy(backupPath, originalPath, true);
            Console.WriteLine($"Restored {originalPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Restore failed: {ex.Message}");
        }
    }

    private void OnDelete(object? sender, RoutedEventArgs e)
    {
        if (List.SelectedItem is not string backupPath) return;
        try
        {
            File.Delete(backupPath);
            List.Items.Remove(backupPath);
            Console.WriteLine($"Deleted {backupPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Delete failed: {ex.Message}");
        }
    }

    private static string GetOriginalPath(string backupPath)
    {
        var idx = backupPath.LastIndexOf(".bak_");
        return idx > 0 ? backupPath.Substring(0, idx) : backupPath;
    }
}
