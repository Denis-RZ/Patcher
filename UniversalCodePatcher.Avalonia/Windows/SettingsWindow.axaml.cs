using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using UniversalCodePatcher.Avalonia.Models;

namespace UniversalCodePatcher.Avalonia;

public partial class SettingsWindow : BaseDialog
{
    private readonly AppSettings _settings;

    // Parameterless constructor required for XAML designer
    public SettingsWindow() : this(new AppSettings())
    {
    }

    public SettingsWindow(AppSettings settings)
    {
        _settings = settings;
        InitializeComponent();
        DataContext = _settings;
    }

    private void OnOk(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("Settings saved");
        SetOKResult();
    }

    private void OnCancel(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("Settings cancelled");
        SetCancelResult();
    }
}
