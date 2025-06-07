using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Threading.Tasks;

namespace UniversalCodePatcher.Avalonia;

public partial class InfoDialog : BaseDialog
{
    public InfoDialog(string message)
    {
        InitializeComponent();
        MessageText.Text = message;
    }

    public static async Task ShowAsync(Window parent, string message)
    {
        var dlg = new InfoDialog(message);
        await dlg.ShowDialog(parent);
    }

    private void OnOk(object? sender, RoutedEventArgs e) => SetOKResult();
}
