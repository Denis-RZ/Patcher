using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia;
using System.Collections.Generic;
using System.IO;

namespace UniversalCodePatcher.Avalonia;

public partial class MainWindow : Window
{
    private string? _projectPath;

    public MainWindow()
    {
        InitializeComponent();
    }

    private async void OnNewProject(object? sender, RoutedEventArgs e)
    {
        var picker = TopLevel.GetTopLevel(this)?.StorageProvider;
        if (picker == null)
            return;
        var folder = await picker.OpenFolderPickerAsync(new FolderPickerOpenOptions());
        var path = folder.Count > 0 ? folder[0].Path.LocalPath : null;
        if (path != null)
        {
            _projectPath = path;
            LoadProject(path);
        }
    }

    private void OnOpenProject(object? sender, RoutedEventArgs e)
    {
        OnNewProject(sender, e);
    }

    private void OnSaveProject(object? sender, RoutedEventArgs e)
    {
        // Placeholder for saving project state
    }

    private void OnExit(object? sender, RoutedEventArgs e)
    {
        (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Shutdown();
    }

    private void OnUndo(object? sender, RoutedEventArgs e) { }
    private void OnRedo(object? sender, RoutedEventArgs e) { }

    private async void OnAbout(object? sender, RoutedEventArgs e)
    {
        var about = new Window
        {
            Width = 300,
            Height = 150,
            Title = "About",
            Content = new TextBlock { Text = "Universal Code Patcher", Margin = new Thickness(20) }
        };
        await about.ShowDialog(this);
    }

    private void LoadProject(string path)
    {
        ProjectTree.Items.Clear();
        foreach (var item in BuildTree(path))
            ProjectTree.Items.Add(item);
        StatusText.Text = $"Loaded: {path}";
    }

    private IEnumerable<TreeViewItem> BuildTree(string path)
    {
        var root = new TreeViewItem { Header = Path.GetFileName(path), Tag = path };
        foreach (var file in Directory.GetFiles(path))
        {
            root.Items.Add(new TreeViewItem { Header = Path.GetFileName(file), Tag = file });
        }
        foreach (var dir in Directory.GetDirectories(path))
        {
            root.Items.Add(BuildSubTree(dir));
        }
        return new[] { root };
    }

    private TreeViewItem BuildSubTree(string dir)
    {
        var node = new TreeViewItem { Header = Path.GetFileName(dir), Tag = dir };
        foreach (var file in Directory.GetFiles(dir))
        {
            node.Items.Add(new TreeViewItem { Header = Path.GetFileName(file), Tag = file });
        }
        foreach (var sub in Directory.GetDirectories(dir))
        {
            node.Items.Add(BuildSubTree(sub));
        }
        return node;
    }
}
