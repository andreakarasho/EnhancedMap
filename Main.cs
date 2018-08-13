using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;
using EnhancedMap.Core;
using EnhancedMap.Core.Network;
using EnhancedMap.GUI;
using Microsoft.Win32;

namespace EnhancedMap
{
    public static class MainCore
    {
        private static Assembly _assembly;
        private static DateTime _expireDate = new DateTime(2018, 6, 1);


        public static bool IsRunning { get; private set; }
        public static Version MapVersion => _assembly.GetName().Version;


        [STAThread]
        private static void Main(string[] args)
        {
            if (!IsUserAdministrator())
            {
                MessageBox.Show("EnhancedMap requires administrator privileges to run.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!CheckNet45Installed())
            {
                if (MessageBox.Show("EnhancedMap requires .Net Framework 4.6.2 to run.\r\nPress OK to download.", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                    Process.Start("https://www.microsoft.com/en-us/download/details.aspx?id=53344");
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
#if !DEBUG
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
#endif

                CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");

                IsRunning = true;
                _assembly = Assembly.GetEntryAssembly();

#if BETA
                string text = "";
                string a = "";
                int processId = -1;
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "--source" && i < args.Length - 1)
                    {
                        text = args[i + 1];
                    }
                    if (args[i] == "--action" && i < args.Length - 1)
                    {
                        a = args[i + 1];
                    }
                    if (args[i] == "--pid" && i < args.Length - 1)
                    {
                        processId = int.Parse(args[i + 1]);
                    }
                }
                if (a == "update")
                {
                    try
                    {
                        Process.GetProcessById(processId);
                        Thread.Sleep(1000);
                        Process.GetProcessById(processId).Kill();
                    }
                    catch
                    {
                    }

                    DirectoryInfo dir = new DirectoryInfo(Path.GetDirectoryName(text));
                    FileInfo[] files = dir.GetFiles();
                    foreach (FileInfo file in files)
                    {
                        try
                        {
                            //File.Delete(file.FullName);
                            if (file.Extension == ".exe" || file.Extension == ".dll" || file.Extension == ".lua")
                                file.Delete();
                        }
                        catch (Exception e)
                        {
                        }

                        //File.Copy(Application.ExecutablePath, file.FullName);
                    }

                    dir = new DirectoryInfo(Application.StartupPath);
                    files = dir.GetFiles();
                    foreach (FileInfo file in files)
                    {
                        if (file.Extension == ".exe" || file.Extension == ".dll" || file.Extension == ".lua")
                            File.Copy(file.FullName, Path.Combine(Path.GetDirectoryName(text),  file.Name));
                    }

                    /*foreach (FileInfo file in fromTempPath.GetFiles())
                    {

                    }
                    */
                    /*try
                    {
                        File.Delete(text);
                    }
                    catch
                    {

                    }

                    File.Copy(Application.ExecutablePath, text);
                    */

                    new Process
                    {
                        StartInfo =
                    {
                        FileName = text,
                        UseShellExecute = false,
                        Arguments =
                            $"--source \"{Application.ExecutablePath}\" --pid {Process.GetCurrentProcess().Id} --action cleanup"
                    }
                    }.Start();
                    return;
                }
                if (a == "cleanup")
                {
                    try
                    {
                        Process.GetProcessById(processId);
                        Thread.Sleep(1000);
                        Process.GetProcessById(processId).Kill();
                    }
                    catch (Exception e)
                    {
                    }
                    try
                    {
                        DirectoryInfo directory = new DirectoryInfo(text);
                        directory.Delete(true);
                        //File.Delete(text);
                    }
                    catch (Exception e)
                    {
                    }
                }

                /*if (_expireDate <= DateTime.Now)
                {
                    MessageBox.Show("BETA version is expired...");
                    return;
                }*/
#endif

                try
                {
                    Update.CheckUpdates();
                }
                catch
                {
                }

                FilesManager.Load();
                Application.Run(new MainWindow());
            }
        }

        private static bool IsUserAdministrator()
        {
            try
            {
                var user = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(user);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (UnauthorizedAccessException e)
            {
            }
            catch (Exception e)
            {
            }

            return false;
        }

        private static bool CheckNet45Installed()
        {
            const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";

            using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkey))
            {
                if (ndpKey != null && ndpKey.GetValue("Release") != null)
                {
                    int version = (int) ndpKey.GetValue("Release");
                    if (version >= 394802)
                    {
                        Console.WriteLine(".NET Framework found: {0}", version);
                        return true;
                    }
                }
                else
                    Console.WriteLine(".NET Framework Version 4.6.2 is not detected.");
            }

            return false;
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            IsRunning = false;

            try
            {
                if (e.IsTerminating)
                {
                    new MessageDialog("Unhandled Exception", !e.IsTerminating, e.ExceptionObject.ToString()).ShowDialog(Global.MainWindow);

                    if (!(e.ExceptionObject is Exception exception) || exception is ThreadAbortException)
                        return;

                    using (var txt = new StreamWriter("Crash.log", true))
                    {
                        txt.AutoFlush = true;
                        txt.WriteLine("Exception @ {0}", DateTime.Now.ToString("MM-dd-yy HH:mm:ss.ffff"));
                        txt.WriteLine(exception.ToString());
                        txt.WriteLine("");
                        txt.WriteLine("");
                    }
                }
            }
            catch (Exception ex)
            {
                Application.Exit();
            }
        }
    }
}