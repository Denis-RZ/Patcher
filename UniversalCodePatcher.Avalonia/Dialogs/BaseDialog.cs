using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace UniversalCodePatcher.Avalonia;

public abstract class BaseDialog : Window
{
    protected BaseDialog()
    {
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ShowInTaskbar = false;
        CanResize = false;
        SystemDecorations = SystemDecorations.BorderOnly;
        Background = Brush.Parse("#F0F0F0");
        FontFamily = new FontFamily("Segoe UI");
        FontSize = 12;
        KeyDown += OnKeyDown;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            DialogResult = false;
            Close();
        }
    }

    public bool? DialogResult { get; protected set; }

    protected void SetOKResult()
    {
        DialogResult = true;
        Close();
    }

    protected void SetCancelResult()
    {
        DialogResult = false;
        Close();
    }
}
