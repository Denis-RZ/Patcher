using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using UniversalCodePatcher.Avalonia.Models;

namespace UniversalCodePatcher.Avalonia;

public partial class SettingsWindow : BaseDialog
{
    private readonly AppSettings _settings;
    private readonly AppSettings _draft;

     // Parameterless constructor required for XAML designer
    public SettingsWindow() : this(new AppSettings())
    {
    }

    public SettingsWindow(AppSettings settings)
    {
        _settings = settings;
        _draft = new AppSettings
        {
            ShowHiddenFiles = settings.ShowHiddenFiles,
            ThemeVariant = settings.ThemeVariant
        };
        InitializeComponent();
        DataContext = _draft;
    }

    private void OnOk(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("Settings saved");
        _settings.ShowHiddenFiles = _draft.ShowHiddenFiles;
        _settings.ThemeVariant = _draft.ThemeVariant;
        SetOKResult();
    }

    private void OnCancel(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("Settings cancelled");
        SetCancelResult();
    }
}
