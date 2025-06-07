using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia;
using Avalonia.Threading;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System;
using UniversalCodePatcher.Core;
using UniversalCodePatcher.Interfaces;
using UniversalCodePatcher.Modules.JavaScriptModule;
using UniversalCodePatcher.Modules.CSharpModule;
using UniversalCodePatcher.Modules.BackupModule;
using UniversalCodePatcher.Modules.DiffModule;
using UniversalCodePatcher.Models;

namespace UniversalCodePatcher.Avalonia;

public partial class MainWindow : Window
{
    private string? _projectPath;
    private bool _showHiddenFiles;
    private readonly Stack<string> _undoStack = new();
    private readonly Stack<string> _redoStack = new();
    private readonly List<string> _recent = new();
    private readonly ServiceContainer _services = new();
    private readonly ModuleManager _moduleManager;
    private readonly Dictionary<string, IPatcher> _patchersByLang = new();
    private BackupModule? _backupModule;
    private IDiffEngine _diffEngine;
    private string? _currentFile;

    public MainWindow()
    {
        InitializeComponent();

 
        // set window icon from embedded Base64 PNG to avoid binary resources
        const string iconBase64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAHUlEQVR4nGP8z8Dwn4ECwESJ5lEDRg0YNWAwGQAAWG0CHvXMz6IAAAAASUVORK5CYII=";
        if (IsValidBase64(iconBase64))
        {
            try
            {
                var iconBytes = Convert.FromBase64String(iconBase64);
                using var iconStream = new MemoryStream(iconBytes);
                Icon = new WindowIcon(iconStream);
            }
            catch (FormatException ex)
            {
                Debug.WriteLine($"Invalid window icon data: {ex.Message}");
            }
        }
 
 

        _moduleManager = new ModuleManager(_services);
        _moduleManager.ModuleError += (_, e) =>
        {
            Dispatcher.UIThread.Post(async () => await ErrorDialog.ShowAsync(this, e.Error));
        };

        _moduleManager.LoadModule(typeof(JavaScriptModule));
        _moduleManager.LoadModule(typeof(CSharpModule));
        _moduleManager.LoadModule(typeof(BackupModule));

        foreach (var mod in _moduleManager.LoadedModules)
        {
            if (mod is BackupModule bm)
                _backupModule = bm;

            if (mod is IPatcher p && mod is ICodeAnalyzer analyzer)
            {
                foreach (var lang in analyzer.SupportedLanguages)
                    _patchersByLang[lang] = p;
            }
        }

        _diffEngine = new DiffModule(_services);

        var recentFile = Path.Combine(AppContext.BaseDirectory, "recent.txt");
        if (File.Exists(recentFile))
            _recent.AddRange(File.ReadAllLines(recentFile));
        UpdateRecentMenu();
    }

    private async void OnNewProject(object? sender, RoutedEventArgs e)
    {
        var dlg = new NewProjectWindow();
        var result = await dlg.ShowDialog<bool?>(this);
        if (result == true && !string.IsNullOrWhiteSpace(dlg.ProjectPath))
        {
            if (!Directory.Exists(dlg.ProjectPath))
                Directory.CreateDirectory(dlg.ProjectPath);
            _projectPath = dlg.ProjectPath;
            LoadProject(_projectPath);
            AddRecent(_projectPath);
        }
    }

    private void OnOpenProject(object? sender, RoutedEventArgs e)
    {
        _ = OpenExistingProjectAsync();
    }

    private async System.Threading.Tasks.Task OpenExistingProjectAsync()
    {
        var picker = TopLevel.GetTopLevel(this)?.StorageProvider;
        if (picker == null)
            return;
        var folder = await picker.OpenFolderPickerAsync(new FolderPickerOpenOptions());
        var path = folder.Count > 0 ? folder[0].Path.LocalPath : null;
        if (path != null)
        {
            _projectPath = path;
            LoadProject(_projectPath);
            AddRecent(_projectPath);
        }
    }

    private async void OnSaveProject(object? sender, RoutedEventArgs e)
    {
        if (_projectPath == null) return;
        var picker = TopLevel.GetTopLevel(this)?.StorageProvider;
        if (picker == null) return;
        var file = await picker.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            SuggestedFileName = "project.json",
            DefaultExtension = "json"
        });
        if (file == null) return;
        var state = new ProjectState { ProjectPath = _projectPath };
        File.WriteAllText(file.Path.LocalPath, System.Text.Json.JsonSerializer.Serialize(state));
    }

    private void OnExit(object? sender, RoutedEventArgs e)
    {
        (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Shutdown();
    }

    private void OnUndo(object? sender, RoutedEventArgs e)
    {
        if (_undoStack.Count > 0)
        {
            _redoStack.Push(SourceBox.Text);
            SourceBox.Text = _undoStack.Pop();
        }
    }

    private void OnRedo(object? sender, RoutedEventArgs e)
    {
        if (_redoStack.Count > 0)
        {
            _undoStack.Push(SourceBox.Text);
            SourceBox.Text = _redoStack.Pop();
        }
    }

    private void OnCut(object? sender, RoutedEventArgs e) => SourceBox.Cut();
    private void OnCopy(object? sender, RoutedEventArgs e) => SourceBox.Copy();
    private void OnPaste(object? sender, RoutedEventArgs e) => SourceBox.Paste();

    private async void OnFind(object? sender, RoutedEventArgs e)
    {
        if (_projectPath == null) return;
        var dlg = new FindWindow(_projectPath);
        dlg.FileSelected += file =>
        {
            foreach (var item in ProjectTree.Items.OfType<TreeViewItem>())
                if (SelectNode(item, file)) break;
        };
        await dlg.ShowDialog(this);
    }

    private bool SelectNode(TreeViewItem item, string file)
    {
        if (item.Tag?.ToString() == file)
        {
            item.IsSelected = true;
            return true;
        }
        foreach (var child in item.Items.OfType<TreeViewItem>())
        {
            if (SelectNode(child, file))
            {
                item.IsExpanded = true;
                return true;
            }
        }
        return false;
    }

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

    private void OnTreeSelect(object? sender, SelectionChangedEventArgs e)
    {
        if (ProjectTree.SelectedItem is TreeViewItem item && item.Tag is string file && File.Exists(file))
        {
            _currentFile = file;
            SourceBox.Text = File.ReadAllText(file);
            PreviewBox.Clear();
        }
    }

    private void OnCancel(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OnPreview(object? sender, RoutedEventArgs e)
    {
        if (_currentFile == null) return;
        PreviewBox.Text = SourceBox.Text;
        MainTabs.SelectedIndex = 1;
    }

    private void OnApply(object? sender, RoutedEventArgs e)
    {
        if (_projectPath == null) return;
        foreach (var item in GetAllNodes(ProjectTree))
        {
            if (item.Tag is not string file || !File.Exists(file)) continue;
            _backupModule?.CreateBackup(file);
        }
    }

    private IEnumerable<TreeViewItem> GetAllNodes(TreeView tree)
    {
        foreach (var item in tree.Items.OfType<TreeViewItem>())
        {
            foreach (var n in GetAllNodes(item))
                yield return n;
        }
    }

    private IEnumerable<TreeViewItem> GetAllNodes(TreeViewItem item)
    {
        yield return item;
        foreach (var child in item.Items.OfType<TreeViewItem>())
        {
            foreach (var n in GetAllNodes(child))
                yield return n;
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        var recentFile = Path.Combine(AppContext.BaseDirectory, "recent.txt");
        File.WriteAllLines(recentFile, _recent.Take(5));
    }

    private string GetLanguageFromFile(string file)
    {
        var ext = Path.GetExtension(file).ToLowerInvariant();
        return ext switch
        {
            ".cs" => "CSharp",
            ".js" => "JavaScript",
            ".ts" => "TypeScript",
            _ => "Unknown"
        };
    }

    private void AddRecent(string path)
    {
        _recent.Remove(path);
        _recent.Insert(0, path);
        if (_recent.Count > 5) _recent.RemoveAt(5);
        UpdateRecentMenu();
    }

    private void UpdateRecentMenu()
    {
        if (RecentMenu == null) return;
        RecentMenu.Items.Clear();
        foreach (var p in _recent)
        {
            var item = new MenuItem { Header = p };
            item.Click += (_, __) => { _projectPath = p; LoadProject(p); };
            RecentMenu.Items.Add(item);
        }
    }

    private async void OnOptions(object? sender, RoutedEventArgs e)
    {
        var dlg = new SettingsWindow();
        await dlg.ShowDialog(this);
    }

    private async void OnBackupManager(object? sender, RoutedEventArgs e)
    {
        if (_projectPath == null) return;
        var dlg = new BackupManagerWindow(_projectPath);
        await dlg.ShowDialog(this);
    }

    private async void OnModuleSettings(object? sender, RoutedEventArgs e)
    {
        var dlg = new ModuleManagerWindow(_moduleManager);
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
        var about = new AboutWindow();
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

    private static bool IsValidBase64(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;
        Span<byte> buffer = new Span<byte>(new byte[input.Length]);
        return Convert.TryFromBase64String(input, buffer, out _);
    }
}
