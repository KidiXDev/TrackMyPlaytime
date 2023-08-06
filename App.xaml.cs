using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using TMP.NET.Modules;
using TMP.NET.Modules.Ext;
using TMP.NET.WindowUI.SplashScreenWindow;

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
                //var window = new MainWindow();
                //MainWindow.HandleParameter(args);
                //app.Run(window);
                var splashScreen = new SplashScreenUI(args);
                app.Run(splashScreen);
                return;
            }

            if (args.Length > 0)
                UnsafeNative.SendMessage(runningProcess.MainWindowHandle, string.Join(" ", args));
        }
    }
    public partial class App : Application
    {
        public App() : base()
        {
            this.Dispatcher.UnhandledException += OnDispatcherUnhandledException;
        }

        void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            TMP.NET.MainWindow.log.Fatal("PROGRAM CRASH", e.Exception);
            MessageBox.Show("Unhandled exception occurred: \n" + e.Exception.Message, "Crash Report", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
#if DEBUG
            ConsoleManager.Show();
#endif
            ToastNotificationManagerCompat.OnActivated += toastArgs =>
            {
                ToastArguments args = ToastArguments.Parse(toastArgs.Argument);
                Application.Current.Dispatcher.Invoke(delegate
                {
                    if (args.Contains("openss"))
                    {
                        Process.Start(args.Get("openss"));
                    }
                    else if (args.Contains("openweb"))
                    {
                        string repoUrl = "https://trackmyplaytime.netlify.app/";
                        Process.Start(new ProcessStartInfo { FileName = repoUrl, UseShellExecute = true });
                    }
                });
            };
        }
    }
}
