using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia;
using System.Collections.Generic;
 
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System;
using Avalonia.VisualTree;
using Avalonia.Layout;
using System.IO;
 

namespace UniversalCodePatcher.Avalonia;

public partial class MainWindow : Window
{
    private string? _projectPath;
 
    private bool _showHiddenFiles;
    private bool _isDirty;
    private readonly List<string> _recentProjects = new();
    private const int MaxRecent = 5;

    private string RecentFile => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "UniversalCodePatcher", "recent.txt");
 
    public MainWindow()
    {
        InitializeComponent();
 
        LoadRecentProjects();
        SourceBox.PropertyChanged += (_, e) =>
        {
            if (e.Property == TextBox.TextProperty)
                _isDirty = true;
        };
 
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
            AddRecentProject(path);
            _isDirty = false;
 
        }
    }

    private void OnOpenProject(object? sender, RoutedEventArgs e)
    {
        OnNewProject(sender, e);
    }

 
    private void AddRecentProject(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(RecentFile)!);
        _recentProjects.Remove(path);
        _recentProjects.Insert(0, path);
        if (_recentProjects.Count > MaxRecent)
            _recentProjects.RemoveAt(_recentProjects.Count - 1);
        File.WriteAllLines(RecentFile, _recentProjects);
        PopulateRecentMenu();
    }

    private void LoadRecentProjects()
    {
        if (File.Exists(RecentFile))
            _recentProjects.AddRange(File.ReadAllLines(RecentFile));
        PopulateRecentMenu();
    }

    private void PopulateRecentMenu()
    {
        if (this.FindControl<MenuItem>("RecentMenu") is { } recent)
        {
            recent.Items.Clear();
            foreach (var p in _recentProjects)
            {
                var item = new MenuItem { Header = p };
                item.Click += (_, _) => { _projectPath = p; LoadProject(p); };
                recent.Items.Add(item);
            }
        }
    }

    private async void OnSaveProject(object? sender, RoutedEventArgs e)
    {
        if (_projectPath == null)
            return;
        var picker = TopLevel.GetTopLevel(this)?.StorageProvider;
        if (picker == null)
            return;
        var file = await picker.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            SuggestedFileName = "project.json"
        });
        if (file != null)
        {
            var state = new { ProjectPath = _projectPath };
            await using var stream = await file.OpenWriteAsync();
            await JsonSerializer.SerializeAsync(stream, state);
            _isDirty = false;
        }
    }

    private async void OnExit(object? sender, RoutedEventArgs e)
    {
        if (_isDirty)
        {
            var confirm = new Window
            {
                Width = 300,
                Height = 120,
                Title = "Exit",
                Content = new StackPanel
                {
                    Margin = new Thickness(20),
                    Children =
                    {
                        new TextBlock { Text = "Unsaved changes. Exit anyway?" },
                        new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            HorizontalAlignment = HorizontalAlignment.Right,
                            Children =
                            {
                                new Button { Content = "Yes", Margin = new Thickness(5), IsDefault = true },
                                new Button { Content = "No", Margin = new Thickness(5), IsCancel = true }
                            }
                        }
                    }
                }
            };
            var yesButton = ((confirm.Content as StackPanel)!.Children[1] as StackPanel)!.Children[0] as Button;
            var noButton = ((confirm.Content as StackPanel)!.Children[1] as StackPanel)!.Children[1] as Button;
            bool result = false;
            yesButton!.Click += (_, _) => { result = true; confirm.Close(); };
            noButton!.Click += (_, _) => { confirm.Close(); };
            await confirm.ShowDialog(this);
            if (!result)
                return;
        }
        (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Shutdown();
    }

    private void OnUndo(object? sender, RoutedEventArgs e)
    {
        SourceBox.Undo();
    }

    private void OnRedo(object? sender, RoutedEventArgs e)
    {
        SourceBox.Redo();
    }
=======
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
 
            if (!_showHiddenFiles && Path.GetFileName(file).StartsWith('.'))
                continue;
 
            root.Items.Add(new TreeViewItem { Header = Path.GetFileName(file), Tag = file });
        }
        foreach (var dir in Directory.GetDirectories(path))
        {
 
            if (!_showHiddenFiles && Path.GetFileName(dir).StartsWith('.'))
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
 
            if (!_showHiddenFiles && Path.GetFileName(file).StartsWith('.'))
                continue;
 
            node.Items.Add(new TreeViewItem { Header = Path.GetFileName(file), Tag = file });
        }
        foreach (var sub in Directory.GetDirectories(dir))
        {
 
            if (!_showHiddenFiles && Path.GetFileName(sub).StartsWith('.'))
                continue;
 
            node.Items.Add(BuildSubTree(sub));
        }
        return node;
    }
 
    private void OnRefresh(object? sender, RoutedEventArgs e)
    {
        if (_projectPath != null)
            LoadProject(_projectPath);
    }

    private void OnToggleHidden(object? sender, RoutedEventArgs e)
    {
        _showHiddenFiles = HiddenToggle.IsChecked == true;
        OnRefresh(sender, e);
    }

    private void ExpandCollapseAll(bool expand)
    {
        foreach (var item in ProjectTree.Items)
            ExpandNode(item as TreeViewItem, expand);
    }

    private void ExpandNode(TreeViewItem? node, bool expand)
    {
        if (node == null) return;
        node.IsExpanded = expand;
        foreach (var child in node.Items)
            ExpandNode(child as TreeViewItem, expand);
    }

    private void OnExpandAll(object? sender, RoutedEventArgs e) => ExpandCollapseAll(true);
    private void OnCollapseAll(object? sender, RoutedEventArgs e) => ExpandCollapseAll(false);

    private void OnSelectAllFiles(object? sender, RoutedEventArgs e)
    {
        foreach (var item in ProjectTree.Items)
            SelectNode(item as TreeViewItem);
    }

    private void SelectNode(TreeViewItem? node)
    {
        if (node == null) return;
        node.IsSelected = true;
        foreach (var child in node.Items)
            SelectNode(child as TreeViewItem);
    }

    private async void OnFindInFiles(object? sender, RoutedEventArgs e)
    {
        if (_projectPath == null) return;
        var input = new Window
        {
            Width = 300,
            Height = 120,
            Title = "Find",
            Content = new StackPanel
            {
                Margin = new Thickness(10),
                Children =
                {
                    new TextBox { Name = "QueryBox" },
                    new Button { Content = "Search", Margin = new Thickness(0,5,0,0), HorizontalAlignment = HorizontalAlignment.Right }
                }
            }
        };
        var queryBox = input.FindControl<TextBox>("QueryBox");
        var searchButton = input.GetVisualDescendants().OfType<Button>().First();
        string? query = null;
        searchButton.Click += (_, _) => { query = queryBox.Text; input.Close(); };
        await input.ShowDialog(this);
        if (string.IsNullOrWhiteSpace(query)) return;
        var results = new List<string>();
        foreach (var file in Directory.EnumerateFiles(_projectPath, "*", SearchOption.AllDirectories))
        {
            if (!_showHiddenFiles && Path.GetFileName(file).StartsWith('.'))
                continue;
            var text = await File.ReadAllTextAsync(file);
            if (text.Contains(query, StringComparison.OrdinalIgnoreCase))
                results.Add(file);
        }
        var dlg = new Window
        {
            Width = 400,
            Height = 300,
            Title = "Search Results",
            Content = new ListBox { ItemsSource = results }
        };
        await dlg.ShowDialog(this);
    }

    private async void OnSettings(object? sender, RoutedEventArgs e)
    {
        var dlg = new Window { Width = 400, Height = 300, Title = "Settings" };
        await dlg.ShowDialog(this);
    }

    private async void OnBackupManager(object? sender, RoutedEventArgs e)
    {
        var dlg = new Window { Width = 400, Height = 300, Title = "Backup Manager" };
        await dlg.ShowDialog(this);
    }

    private async void OnModuleManager(object? sender, RoutedEventArgs e)
    {
        var dlg = new Window { Width = 400, Height = 300, Title = "Module Manager" };
        await dlg.ShowDialog(this);
    }

    private void OnDocumentation(object? sender, RoutedEventArgs e)
    {
        var url = "https://example.com/docs";
        try
        {
            Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
        }
        catch { }
    }
 
}
