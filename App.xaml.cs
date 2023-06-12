using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using TMP.NET.Modules;

namespace TMP.NET
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// 
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var proc = Process.GetCurrentProcess();
            var processName = proc.ProcessName.Replace(".vshost", "");
            var runningProcess = Process.GetProcesses().FirstOrDefault(x => 
            (x.ProcessName == processName || x.ProcessName == proc.ProcessName || x.ProcessName == proc.ProcessName + ".vshost") && x.Id != proc.Id);

            if (runningProcess == null)
            {
                var app = new App();
                app.InitializeComponent();
                var window = new MainWindow();
                MainWindow.HandleParameter(args);
                app.Run(window);
                return;
            }

            if (args.Length > 0)
                UnsafeNative.SendMessage(runningProcess.MainWindowHandle, string.Join(" ", args));
        }
    }
    public partial class App : Application
    {
        /*private static string appGuid = ((GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), false).GetValue(0)).Value.ToString();
        private static string MutexName = string.Format("Global\\{{{0}}}", appGuid);
        private static Mutex mutex;*/

        /*protected override void OnStartup(StartupEventArgs e)
        {
            bool createdNew;
            mutex = new Mutex(true, MutexName, out createdNew);

            if (!createdNew)
            {
                MessageBox.Show("Program is already running.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
            }
            else
            {

                base.OnStartup(e);
            }
        }*/

        /*protected override void OnExit(ExitEventArgs e)
        {
            *//*mutex?.ReleaseMutex();
            mutex?.Close();

            base.OnExit(e);*//*
        }*/

        
    }
}
