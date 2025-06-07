using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Linq;
using UniversalCodePatcher.Core;

namespace UniversalCodePatcher.Avalonia;

public partial class ModuleManagerWindow : BaseDialog
{
    public ModuleManagerWindow(ModuleManager manager)
    {
        InitializeComponent();
        foreach (var m in manager.LoadedModules)
            ModuleList.Items.Add(m.Name);
    }

    private void OnClose(object? sender, RoutedEventArgs e) => SetCancelResult();
}
