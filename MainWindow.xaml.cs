Licensed to the Apache Software Foundation (ASF) under one
or more contributor license agreements.  See the NOTICE file
distributed with this work for additional information
regarding copyright ownership.  The ASF licenses this file
to you under the Apache License, Version 2.0 (the
"License"); you may not use this file except in compliance
with the License.  You may obtain a copy of the License at

  http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing,
software distributed under the License is distributed on an
"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
KIND, either express or implied.  See the License for the
specific language governing permissions and limitations
under the License.    

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;

namespace JuegoLauncher
{
    enum LauncherStatus
    {
        ready,
        failed,
        downloadingGame,
        downloadingUpdate
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string rootPath;
        private string versionFile;
        private string gameZip;
        private string gameExe;

        private LauncherStatus _status;
        internal LauncherStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                switch (_status)
                {
                    case LauncherStatus.ready:
                        JugarBotón.Content = "Jugar";
                        break;
                    case LauncherStatus.failed:
                        JugarBotón.Content = "Actualización fallida, reintente";
                        break;
                    case LauncherStatus.downloadingUpdate:
                        JugarBotón.Content = "Descargando actualización";
                        break;
                    case LauncherStatus.downloadingGame:
                        JugarBotón.Content = "Descargando";
                        break;
                    default:
                        break;
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            rootPath = @"C:\ProgramData\KingsIsle Entertainment\Wizard101\Data\GameData";
            versionFile = Path.Combine(rootPath, "Version.txt");
            gameZip = Path.Combine(rootPath, "Files.zip");
            gameExe = Path.Combine(rootPath, "Runtime 2.bat");

        }

        private void ImageButton_Click(object sender, RoutedEventArgs e)
        {
            // Abre el navegador con la URL deseada
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://discord.gg/6jdpFT4",
                UseShellExecute = true
            });
        }

        private void ImageButton_Click2(object sender, RoutedEventArgs e)
        {
            // Abre el navegador con la URL deseada
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://www.wizard101.com/game/updatenotes/ghastlyconspiracyraid",
                UseShellExecute = true
            });
        }

        private void ImageButton_Click3(object sender, RoutedEventArgs e)
        {
            // Abre el navegador con la URL deseada
            Process.Start(new ProcessStartInfo
            {
                FileName = "Configurator.exe",
                WorkingDirectory = @"C:\ProgramData\KingsIsle Entertainment\Wizard101\PatchClient\BankA",
                UseShellExecute = true
            });
        }

        private void CheckForUpdates()
        {
            if (File.Exists(versionFile))
            {
                Version localVersion = new Version(File.ReadAllText(versionFile));
                VersionText.Text = localVersion.ToString();

                try
                {
                    WebClient webClient = new WebClient();
                    Version onlineVersion = new Version(webClient.DownloadString("https://drive.google.com/uc?export=download&id=1YwVAXqgBolw-08Hd30U-X7fBnkawjQbf"));

                    if (onlineVersion.IsDifferentThan(localVersion))
                    {
                        InstallGameFiles(true, onlineVersion);
                    }
                    else
                    {
                        Status = LauncherStatus.ready;
                    }
                }
                catch (Exception ex)
                {
                    Status = LauncherStatus.failed;
                    MessageBox.Show($"Error al comprobar actualizaciones: {ex}");
                }
            }
            else
            {
                InstallGameFiles(false, Version.zero);
            }
        }

        private void InstallGameFiles(bool _isUpdate, Version _onlineVersion)
        {
            try
            {
                WebClient webClient = new WebClient();
                if (_isUpdate)
                {
                    Status = LauncherStatus.downloadingUpdate;
                }
                else
                {
                    Status = LauncherStatus.downloadingGame;
                    _onlineVersion = new Version(webClient.DownloadString("https://drive.google.com/uc?export=download&id=1YwVAXqgBolw-08Hd30U-X7fBnkawjQbf"));
                }

                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadGameCompletedCallback);
                webClient.DownloadFileAsync(new Uri("https://drive.google.com/uc?export=download&id=1Yy-w0hHzASZghMqckNy9UJmyjDnp2ZWN"), gameZip, _onlineVersion);
            }
            catch (Exception ex)
            {
                Status = LauncherStatus.failed;
                MessageBox.Show($"Error al instalar: {ex}");
            }
        }


        private void DownloadGameCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                string onlineVersion = ((Version)e.UserState).ToString();
                ZipFile.ExtractToDirectory(gameZip, rootPath, true);
                File.Delete(gameZip);

                File.WriteAllText(versionFile, onlineVersion);

                VersionText.Text = onlineVersion;
                Status = LauncherStatus.ready;
            }
            catch (Exception ex)
            {
                Status = LauncherStatus.failed;
                MessageBox.Show($"Error al descargar: {ex}");
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            CheckForUpdates();
        }

        private void JugarBotón_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(gameExe) && Status == LauncherStatus.ready)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(gameExe);
                startInfo.WorkingDirectory = Path.Combine(rootPath);
                Process.Start(startInfo);

                Close();
            }
            else if (Status == LauncherStatus.failed)
            {
                CheckForUpdates();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UpdateText_Click(object sender, RoutedEventArgs e)
        {

        }
    }

    struct Version
    {
        internal static Version zero = new Version(0, 0, 0);

        private short major;
        private short minor;
        private short subMinor;

        internal Version(short _major, short _minor, short _subMinor)
        {
            major = _major;
            minor = _minor;
            subMinor = _subMinor;
        }
        internal Version(string _Version)
        {
            string[] _versionStrings = _Version.Split('.');
            if (_versionStrings.Length != 3)
            {
                major = 0;
                minor = 0;
                subMinor = 0;
                return;
            }

            major = short.Parse(_versionStrings[0]);
            minor = short.Parse(_versionStrings[1]);
            subMinor = short.Parse(_versionStrings[2]);
        }


        internal bool IsDifferentThan(Version _otherversion)
        {
            if (major != _otherversion.major)
            {
                return true;
            }
            else
            {
                if (minor != _otherversion.minor)
                {
                    return true;
                }
                else
                {
                    if (subMinor != _otherversion.subMinor)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override string ToString()
        {
            return $"{major}.{minor}.{subMinor}";
        }
    }
}