using Avalonia.Controls;
using Avalonia.Interactivity;
using System;

namespace UniversalCodePatcher.Avalonia;

public partial class SettingsWindow : BaseDialog
{
    public SettingsWindow()
    {
        InitializeComponent();
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
