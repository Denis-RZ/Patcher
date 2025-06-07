using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Reflection;

namespace UniversalCodePatcher.Avalonia;

public partial class AboutWindow : BaseDialog
{
    public AboutWindow()
    {
        InitializeComponent();
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0";
        Label.Text = $"Universal Code Patcher\nVersion {version}";
    }

    private void OnOk(object? sender, RoutedEventArgs e) => SetOKResult();
}
