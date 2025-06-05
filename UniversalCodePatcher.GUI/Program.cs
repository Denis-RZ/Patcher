using System;
using System.Windows.Forms;
using UniversalCodePatcher.Forms;

namespace UniversalCodePatcher.GUI
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
