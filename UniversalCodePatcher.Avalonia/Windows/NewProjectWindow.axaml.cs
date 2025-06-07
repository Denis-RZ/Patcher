using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System;

namespace UniversalCodePatcher.Avalonia;

public partial class NewProjectWindow : BaseDialog
{
    public string ProjectName => NameBox.Text;
    public string ProjectPath => PathBox.Text;

    public NewProjectWindow()
    {
        InitializeComponent();
    }

    private async void OnBrowse(object? sender, RoutedEventArgs e)
    {
        var picker = TopLevel.GetTopLevel(this)?.StorageProvider;
        if (picker == null) return;
        var folder = await picker.OpenFolderPickerAsync(new FolderPickerOpenOptions());
        if (folder.Count > 0)
            PathBox.Text = folder[0].Path.LocalPath;
    }

    private void OnOk(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine($"New project created: {ProjectName} at {ProjectPath}");
        SetOKResult();
    }

    private void OnCancel(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("New project creation cancelled");
        SetCancelResult();
    }
}
