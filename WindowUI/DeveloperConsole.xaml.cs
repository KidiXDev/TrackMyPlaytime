using System;
using System.Windows;

namespace TMP.NET.WindowUI
{
    /// <summary>
    /// Interaction logic for DeveloperConsole.xaml
    /// </summary>
    public partial class DeveloperConsole : Window
    {
        private readonly string[] args = Environment.GetCommandLineArgs();

        public DeveloperConsole()
        {
            InitializeComponent();

            foreach (string arg in args)
            {
                tbConsole.Text += arg;
                tbConsole.Text += "\n";
            }
        }

        public void debug(string msg)
        {
            tbConsole.Text += msg + "\n";
        }
    }
}
