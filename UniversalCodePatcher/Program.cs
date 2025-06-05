using System;
using System.Windows.Forms;
using UniversalCodePatcher.Forms;

namespace UniversalCodePatcher
{
    /// <summary>
    /// Точка входа в приложение
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            try
            {
                Application.Run(new PatchForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Fatal error: {ex.Message}\n\nApplication will close.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}
