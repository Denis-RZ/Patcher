using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Threading.Tasks;
using System;

namespace UniversalCodePatcher.Avalonia;

public partial class ErrorDialog : BaseDialog
{
    public ErrorDialog(string message)
    {
        InitializeComponent();
        MessageText.Text = message;
    }

    public static async Task ShowAsync(Window parent, string message)
    {
        var dlg = new ErrorDialog(message);
        await dlg.ShowDialog(parent);
    }

    private void OnOk(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("Error dialog closed");
        SetOKResult();
    }
}
