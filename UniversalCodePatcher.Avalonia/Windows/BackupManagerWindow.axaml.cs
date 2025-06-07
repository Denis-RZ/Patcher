using Avalonia.Controls;
using System.IO;
using System.Linq;

namespace UniversalCodePatcher.Avalonia;

public partial class BackupManagerWindow : Window
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
}
