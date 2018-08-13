using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using EnhancedMap.Core;

namespace EnhancedMap.GUI.MapMakerSteps
{
    public partial class CreationMapsStep : UserControl
    {
        private readonly IProgress<string> _progress;

        public CreationMapsStep()
        {
            InitializeComponent();

            Progress<string> progress = new Progress<string>();
            progress.ProgressChanged += (sender, e) => { textBox1.Do(s => s.AppendText(e + "\r\n")); };

            _progress = progress;
        }


        public Task ExecuteAction(int cmd, List<MapEntry> maps)
        {
            return Task.Run(() =>
            {
                switch (cmd)
                {
                    case 0: // download
                    {
                        try
                        {
                            const string MAPS_DOWNLOAD_URL = "http://razorenhanced.org/download/EnhancedMapMaps/";

                            using (WebClient wc = new WebClient {Proxy = null})
                            {
                                wc.DownloadProgressChanged += (sender, e) =>
                                {
                                    //_progress.Report(e.ProgressPercentage.ToString());
                                    _progress.Report("Downloading: " + Utility.GetSizeAdaptive(e.BytesReceived) + "/" + Utility.GetSizeAdaptive(e.TotalBytesToReceive));
                                };

                                if (!Directory.Exists("Maps"))
                                    Directory.CreateDirectory("Maps");


                                for (int i = 0; i < 6; i++)
                                {
                                    string map = "2Dmap" + i + ".png";
                                    wc.DownloadFileTaskAsync(new Uri(MAPS_DOWNLOAD_URL + map), Path.Combine("Maps", map)).Wait();
                                }

                                for (int i = 0; i < 6; i++)
                                {
                                    string map = "map" + i + ".png";
                                    wc.DownloadFileTaskAsync(new Uri(MAPS_DOWNLOAD_URL + map), Path.Combine("Maps", map)).Wait();
                                }
                            }
                        }
                        catch (WebException webEx)
                        {
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                        break;
                    case 1: // generate original
                    {
                        MapsManager.CreateImages(maps, true, _progress);
                        MapsManager.CreateImages(maps, false, _progress);
                    }
                        break;
                    case 2: // generate custom
                    {
                        if (maps != null)
                        {
                            MapsManager.CreateImages(maps, true, _progress);
                            MapsManager.CreateImages(maps, false, _progress);
                        }
                    }
                        break;
                }
            });
        }
    }
}