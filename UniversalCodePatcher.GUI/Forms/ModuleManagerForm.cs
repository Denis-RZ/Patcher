using System.Linq;
using System.Windows.Forms;
using UniversalCodePatcher.Core;
using UniversalCodePatcher.Interfaces;

namespace UniversalCodePatcher.Forms
{
    public class ModuleManagerForm : Form
    {
        private readonly CheckedListBox moduleList = new() { Dock = DockStyle.Fill };
        private readonly ModuleManager manager;
        private readonly Button unloadButton = new() { Text = "Unload", Width = 80 };
        private readonly Button closeButton = new() { Text = "Close", Width = 80 };

        public ModuleManagerForm(ModuleManager manager)
        {
            this.manager = manager;
            Text = "Module Manager";
            Width = 400;
            Height = 300;
            StartPosition = FormStartPosition.CenterParent;

            var buttons = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Bottom,
                Padding = new Padding(8),
                Height = 40
            };
            buttons.Controls.Add(closeButton);
            buttons.Controls.Add(unloadButton);
            Controls.Add(buttons);

            Controls.Add(moduleList);
            LoadModules();

            AcceptButton = unloadButton;
            CancelButton = closeButton;
            moduleList.DoubleClick += OnUnload;

            closeButton.Click += (_, __) => Close();
            unloadButton.Click += OnUnload;
        }

        private void LoadModules()
        {
            moduleList.Items.Clear();
            moduleList.DisplayMember = nameof(IModule.Name);
            foreach (var mod in manager.LoadedModules)
            {
                moduleList.Items.Add(mod, true);
            }
        }

        private void OnUnload(object? sender, System.EventArgs e)
        {
            var toRemove = moduleList.CheckedItems.Cast<object>().ToList();
            foreach (var obj in toRemove)
            {
                if (obj is IModule mod)
                {
                    manager.UnloadModule(mod.ModuleId);
                    moduleList.Items.Remove(obj);
                }
            }
        }
    }
}
