using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace UniversalCodePatcher.Avalonia;

public partial class FindWindow : BaseDialog
{
    private readonly string _root;
    public event Action<string>? FileSelected;

    public FindWindow(string root)
    {
        _root = root;
        InitializeComponent();
    }

    private void OnSearch(object? sender, RoutedEventArgs e)
    {
        ResultsList.Items.Clear();
        if (!string.IsNullOrWhiteSpace(SearchBox.Text) && Directory.Exists(_root))
        {
            var files = Directory.GetFiles(_root, "*.*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                try
                {
                    var text = File.ReadAllText(file);
                    if (text.Contains(SearchBox.Text, StringComparison.OrdinalIgnoreCase))
                        ResultsList.Items.Add(file);
                }
                catch { }
            }
        }
    }

    private void OnDouble(object? sender, RoutedEventArgs e)
    {
        if (ResultsList.SelectedItem is string file)
        {
            FileSelected?.Invoke(file);
            Close();
        }
    }
}
