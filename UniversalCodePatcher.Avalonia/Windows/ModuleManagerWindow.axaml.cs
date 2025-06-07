using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Linq;
using System;
using UniversalCodePatcher.Core;
using UniversalCodePatcher.Interfaces;

namespace UniversalCodePatcher.Avalonia;

public partial class ModuleManagerWindow : BaseDialog
{
    private readonly ModuleManager _manager;

    public ModuleManagerWindow(ModuleManager manager)
    {
        _manager = manager;
        InitializeComponent();
        ModuleList.DisplayMemberPath = "Name";
        foreach (var m in _manager.LoadedModules)
            ModuleList.Items.Add(m);
    }

    private void OnClose(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("Module manager closed");
        SetCancelResult();
    }

    private void OnUnload(object? sender, RoutedEventArgs e)
    {
        var modules = ModuleList.SelectedItems.Cast<IModule>().ToList();
        foreach (var m in modules)
        {
            _manager.UnloadModule(m.ModuleId);
            ModuleList.Items.Remove(m);
        }
    }
}
