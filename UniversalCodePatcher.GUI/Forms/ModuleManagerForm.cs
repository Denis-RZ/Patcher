using System.Windows.Forms;
using UniversalCodePatcher.Core;

namespace UniversalCodePatcher.Forms
{
    public class ModuleManagerForm : Form
    {
        private readonly CheckedListBox moduleList = new() { Dock = DockStyle.Fill };
        private readonly ModuleManager manager;

        public ModuleManagerForm(ModuleManager manager)
        {
            this.manager = manager;
            Text = "Module Manager";
            Width = 400;
            Height = 300;
            StartPosition = FormStartPosition.CenterParent;
            Controls.Add(moduleList);
            LoadModules();
        }

        private void LoadModules()
        {
            moduleList.Items.Clear();
            foreach (var mod in manager.LoadedModules)
            {
                moduleList.Items.Add(mod.Name, true);
            }
        }
    }
}
