using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace UniversalCodePatcher.Avalonia;

public partial class MainWindow : Window
{
    private string? _projectPath;
    private bool _showHiddenFiles;

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

    private void OnCut(object? sender, RoutedEventArgs e) => SourceBox.Cut();
    private void OnCopy(object? sender, RoutedEventArgs e) => SourceBox.Copy();
    private void OnPaste(object? sender, RoutedEventArgs e) => SourceBox.Paste();
    private void OnFind(object? sender, RoutedEventArgs e) { }

    private void OnRefresh(object? sender, RoutedEventArgs e)
    {
        if (_projectPath != null)
            LoadProject(_projectPath);
    }

    private void OnToggleHidden(object? sender, RoutedEventArgs e)
    {
        _showHiddenFiles = !_showHiddenFiles;
        if (_projectPath != null)
            LoadProject(_projectPath);
    }

    private void OnExpandTree(object? sender, RoutedEventArgs e) => SetExpand(ProjectTree, true);
    private void OnCollapseTree(object? sender, RoutedEventArgs e) => SetExpand(ProjectTree, false);

    private static void SetExpand(TreeView view, bool expand)
    {
        foreach (var item in view.Items)
            SetExpand(item as TreeViewItem, expand);
    }

    private static void SetExpand(TreeViewItem? item, bool expand)
    {
        if (item == null) return;
        item.IsExpanded = expand;
        foreach (var child in item.Items)
            SetExpand(child as TreeViewItem, expand);
    }

    private async void OnOptions(object? sender, RoutedEventArgs e)
    {
        var dlg = new Window
        {
            Title = "Settings",
            Width = 400,
            Height = 300,
            Content = new TextBlock { Text = "Settings", Margin = new Thickness(20) }
        };
        await dlg.ShowDialog(this);
    }

    private async void OnBackupManager(object? sender, RoutedEventArgs e)
    {
        if (_projectPath == null) return;
        var dlg = new Window
        {
            Title = "Backups",
            Width = 600,
            Height = 400,
            Content = new TextBlock { Text = "Backup Manager", Margin = new Thickness(20) }
        };
        await dlg.ShowDialog(this);
    }

    private async void OnModuleSettings(object? sender, RoutedEventArgs e)
    {
        var dlg = new Window
        {
            Title = "Module Settings",
            Width = 400,
            Height = 300,
            Content = new TextBlock { Text = "Modules", Margin = new Thickness(20) }
        };
        await dlg.ShowDialog(this);
    }

    private void OnDocumentation(object? sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://github.com/",
            UseShellExecute = true
        });
    }

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
            if (!_showHiddenFiles && (File.GetAttributes(file) & FileAttributes.Hidden) != 0)
                continue;
            root.Items.Add(new TreeViewItem { Header = Path.GetFileName(file), Tag = file });
        }
        foreach (var dir in Directory.GetDirectories(path))
        {
            if (!_showHiddenFiles && (File.GetAttributes(dir) & FileAttributes.Hidden) != 0)
                continue;
            root.Items.Add(BuildSubTree(dir));
        }
        return new[] { root };
    }

    private TreeViewItem BuildSubTree(string dir)
    {
        var node = new TreeViewItem { Header = Path.GetFileName(dir), Tag = dir };
        foreach (var file in Directory.GetFiles(dir))
        {
            if (!_showHiddenFiles && (File.GetAttributes(file) & FileAttributes.Hidden) != 0)
                continue;
            node.Items.Add(new TreeViewItem { Header = Path.GetFileName(file), Tag = file });
        }
        foreach (var sub in Directory.GetDirectories(dir))
        {
            if (!_showHiddenFiles && (File.GetAttributes(sub) & FileAttributes.Hidden) != 0)
                continue;
            node.Items.Add(BuildSubTree(sub));
        }
        return node;
    }
}
