using EnhancedMap.GUI;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EnhancedMap.Core.Network
{
    public class Update
    {
        private static WebRequest _WebRequest;

        public static void CheckUpdates()
        {
            if (_WebRequest == null)
                _WebRequest = new WebRequest(new WebClient());
            if (!_WebRequest.Checking)
                _WebRequest.Start();
        }

        private class WebRequest
        {
            private WebClient _client;

            public WebRequest(WebClient client)
            {
                _client = client;
                Checking = false;
            }

            public bool Checking { get; private set; }

            public void Start()
            {
                Console.WriteLine("Checking for update.");

                if (_client == null)
                    _client = new WebClient();

                _client.Proxy = null;
                Checking = true;

                try
                {
#if BETA
                    CustomForm form = new CustomForm()
                    {
                        Text = "Check for updates",
                        StartPosition = FormStartPosition.CenterScreen,
                        ControlBox = false,
                        Sizable = false,
                        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath),
                        Size = new Size(350, 150),
                        TopMost = true
                    };

                    Label label = new Label()
                    {
                        Font = new Font("Segoe UI", 20, FontStyle.Regular),
                        ForeColor = Color.White,
                        Text = "Checking...",
                        AutoSize = true,
                    };
                    label.Location = new Point(form.Width / 2 - label.Width / 2 - 25, form.Height / 2 - label.Height / 2);
                    label.Parent = form;
                    form.Show();

                    Version checkedVersion = new Version(_client.DownloadString(new Uri("http://razorenhanced.org/download/Version-EM-Beta.txt")));

                    if (checkedVersion > MainCore.MapVersion)
                    {
                        form.Close();

                        form = new CustomForm()
                        {
                            Text = "New updates available!",
                            StartPosition = FormStartPosition.CenterScreen,
                            ControlBox = false,
                            Sizable = false,
                            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath),
                            Size = new Size(350, 150),
                            TopMost = true
                        };

                        label = new Label()
                        {
                            Font = new Font("Segoe UI", 20, FontStyle.Regular),
                            ForeColor = Color.White,
                            Text = "Updating...",
                            AutoSize = true,
                        };
                        label.Location = new Point(form.Width / 2 - label.Width / 2 - 25, form.Height / 2 - label.Height / 2);
                        label.Parent = form;

                        Task.Run(() =>
                       {
                           string path = Path.Combine(Path.GetTempPath(), "Enhanced-Map-Beta.zip");

                           if (File.Exists(path))
                               File.Delete(path);

                           _client.DownloadFile("http://razorenhanced.org/download/Enhanced-Map-Beta.zip", path);

                           string pathtoextract = Path.Combine(Path.GetTempPath(), "map-beta");
                           if (Directory.Exists(pathtoextract))
                               Directory.Delete(pathtoextract, true);

                           ZipFile.ExtractToDirectory(path, pathtoextract);

                           Process p = new Process
                           {
                               StartInfo =
                                {
                                    FileName = Path.Combine(pathtoextract, "EnhancedMap.exe"),
                                    UseShellExecute = false,
                                    Arguments =
                                        $"--source \"{Application.ExecutablePath}\" --pid {Process.GetCurrentProcess().Id} --action update"
                                }
                           };
                           p.Start();
                           Process.GetCurrentProcess().Kill();
                       });

                        form.ShowDialog();
                    }
                    if (form != null && !form.Disposing)
                        form.Close();
#else
                    _client.DownloadStringCompleted += m_Client_DownloadStringCompleted;
                    _client.DownloadStringAsync(new Uri("http://razorenhanced.org/download/Version-EM.txt"));
#endif
                }
                catch (WebException webEx)
                {
                    Checking = false;
                    MessageBox.Show("Failed to comunicate with server", "Error");
                }
                catch (Exception ex)
                {
                    Checking = false;
                    MessageBox.Show("Failed to download new version.", "Error");
                }
            }

            private void m_Client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
            {
                try
                {
                    Version checkedVersion = new Version(e.Result);

                    if (checkedVersion > MainCore.MapVersion)
                    {
                        Console.WriteLine("New version available: {0}", checkedVersion);
                       
                        var dialogResult =
                            MessageBox.Show($"New version {checkedVersion} is available.\r\n Download now?",
                                "New Update", MessageBoxButtons.YesNo);

                        if (dialogResult == DialogResult.Yes)
                        {
                            if (File.Exists("EnhancedUpdater.exe"))
                            {
                                Process.Start("EnhancedUpdater.exe");
                                Process.GetCurrentProcess().Kill();
                            }
                            else
                                MessageBox.Show("EnhancedUpdater not found.", "Error");
                        }
                    }
                    else Console.WriteLine("EnhancedMap is already running latest version.");
                }
                catch
                {
                }

                _client.DownloadStringCompleted -= m_Client_DownloadStringCompleted;
                _client?.Dispose();
                _client = null;
                Checking = false;
            } 
        }
    }
}