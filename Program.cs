using System;
using System.Windows.Forms;

namespace ConfigMerger
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            ThemeManager.LoadThemeSettings();
            Application.Run(new MainForm());
        }
    }
}