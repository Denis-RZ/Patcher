using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace UniversalCodePatcher.Avalonia;

public partial class NewProjectWindow : Window
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

    private void OnOk(object? sender, RoutedEventArgs e) => Close(true);
}
