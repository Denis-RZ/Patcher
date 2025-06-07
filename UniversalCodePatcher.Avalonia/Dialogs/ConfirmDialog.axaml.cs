using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Threading.Tasks;
using System;

namespace UniversalCodePatcher.Avalonia;

public partial class ConfirmDialog : BaseDialog
{
    public ConfirmDialog(string message, string title)
    {
        InitializeComponent();
        Title = title;
        MessageText.Text = message;
    }

    public static async Task<bool?> ShowAsync(Window parent, string message, string title)
    {
        var dlg = new ConfirmDialog(message, title);
        await dlg.ShowDialog(parent);
        return dlg.DialogResult;
    }

    private void OnOk(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("Confirm dialog accepted");
        SetOKResult();
    }

    private void OnCancel(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("Confirm dialog cancelled");
        SetCancelResult();
    }
}
