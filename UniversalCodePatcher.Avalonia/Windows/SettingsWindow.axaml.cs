using Avalonia.Controls;
using Avalonia.Interactivity;

namespace UniversalCodePatcher.Avalonia;

public partial class SettingsWindow : BaseDialog
{
    public SettingsWindow()
    {
        InitializeComponent();
    }

    private void OnOk(object? sender, RoutedEventArgs e) => SetOKResult();
    private void OnCancel(object? sender, RoutedEventArgs e) => SetCancelResult();
}
