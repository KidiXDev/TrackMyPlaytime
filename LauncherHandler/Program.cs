using LauncherHandler.Modules;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace LauncherHandler
{
    internal class Program
    {
        private string gameDir, launchParameter;
        private bool runas;
        private Process proc;

        static void Main(string[] args)
        {
            Console.Title = "Launcher Handler";
            Console.CursorVisible = false;
            if (args.Length > 0)
            {
                try
                {
                    Program _pr = new Program();
                    _pr.gameDir = args[0];
                    _pr.launchParameter = string.Join(" ", args.Skip(2));
                    _pr.runas = Convert.ToBoolean(args[1]);
                    _pr.startProcess();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception Thrown: {ex.Message}");
                    Console.ReadLine();
                }
            }
            else
            {
                Console.WriteLine("No argument received...");
                Console.Write("Closing in: ");
                for (int a = 3; a > 0; a--)
                {
                    Console.Write(a);
                    Thread.Sleep(1000);
                    Console.Write("\b");
                }
            }
        }

        private void startProcess()
        {
            try
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version;


                AsciiArt _art = new AsciiArt();
                Console.WriteLine(_art.devArt);
                Console.WriteLine($"Launcher Handler (.NET) v{version.Major}.{version.Minor}.{version.Build}");
                Console.WriteLine($"Date: {DateTime.Now.ToString("dd-MM-yyyy")}");
                Console.WriteLine($"Time: {DateTime.Now.ToString("HH:mm")}");
                Console.WriteLine($"Administrator Privileges: {runas}");
                Console.WriteLine("\nLaunching Program...");
                Console.WriteLine("=========================================================");

                proc = new Process();
                proc.StartInfo.FileName = gameDir;
                proc.StartInfo.WorkingDirectory = Path.GetDirectoryName(gameDir);

                if (runas)
                    proc.StartInfo.Verb = "runas";

                if (!string.IsNullOrEmpty(launchParameter))
                    proc.StartInfo.Arguments = launchParameter;

                proc.Start();

                Console.WriteLine("Done");

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n\n=========================================================================");
                Console.WriteLine("Warning: Don't close this console if your game still running.");
                Console.WriteLine("=========================================================================");
                Console.ResetColor();
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception Thrown: " + ex.Message);
                Console.ReadLine();
                Environment.Exit(1);
            }
        }
    }
}
