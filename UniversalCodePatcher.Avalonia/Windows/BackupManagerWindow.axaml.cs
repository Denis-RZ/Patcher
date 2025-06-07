using Avalonia.Controls;
using Avalonia.Interactivity;
using System.IO;
using System.Linq;

namespace UniversalCodePatcher.Avalonia;

public partial class BackupManagerWindow : BaseDialog
{
    public BackupManagerWindow(string directory)
    {
        InitializeComponent();
        if (Directory.Exists(directory))
        {
            foreach (var file in Directory.GetFiles(directory, "*.bak_*", SearchOption.AllDirectories))
            {
                List.Items.Add($"{file} - {File.GetLastWriteTime(file)}");
            }
        }
    }

    private void OnClose(object? sender, RoutedEventArgs e) => SetCancelResult();
}
