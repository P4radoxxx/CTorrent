using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Shapes;

namespace Client_torrent
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private const string PipeName = "Pipe_ClientTorrent";
        string Backup = "Torrents.xml";

        private ObservableCollection<Torrent> _Torrents = new ObservableCollection<Torrent>();

        // New lock here
        private readonly object _testLock = new object();
        public ObservableCollection<Torrent> Torrents
        {
            get { return _Torrents; }
            set { if (Torrents != value) { _Torrents = value; OnPropertyChanged(nameof(Torrents)); } }
        }

        public MainWindow()
        {
            InitializeComponent();
            StartPipeServer();
            this.DataContext = this;
            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(MainWindow_DragEnter);
            this.Drop += new DragEventHandler(MainWindow_Drop);
            RelancerTorrents();
        }

        #region "PipeServer"

        private async void StartPipeServer()
        {
            try
            {
                while (true)
                {
                    var pipeServer = new NamedPipeServerStream(PipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
                    await pipeServer.WaitForConnectionAsync();

                    Task.Run(async () =>
                    {
                        try
                        {
                            // Using StringBuilder as a cache. The app can now start and then it will read the object
                            var SB = new StringBuilder();
                            using (var reader = new StreamReader(pipeServer))
                            {
                                char[] buffer = new char[4096];
                                int bytesRead;

                                while (true)
                                {
                                    bytesRead = await reader.ReadAsync(buffer, 0, buffer.Length);
                                    if (bytesRead == 0)
                                    {
                                        // Get out of the loop if not data received
                                        break;
                                    }

                                    SB.Append(buffer, 0, bytesRead);

                                    if (reader.EndOfStream)
                                    {
                                        string message = SB.ToString();
                                        Application.Current.Dispatcher.Invoke(() =>
                                        {
                                            HandleReceivedData(message);
                                        });

                                        SB.Clear();

                                    }

                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Erreur : " + ex.Message);
                        }
                        finally
                        {
                            pipeServer.Disconnect();
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message);
            }
        }


        private async void HandleReceivedData(string data)
        {
            foreach (string path in data.Split('\n'))
            {
                await AjouterTorrent(path);
            }
        }
        #endregion

        #region "Fonctions"
        public async Task AjouterTorrent(string path)
        {

            if (ObjectSearch.FindIndexByPropertyValue<Torrent>(Torrents, "path", path) == -1)
            {
                Torrent NewTorrent = new Torrent();
                bool Auth = await NewTorrent.Start(path, System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"));

                if (Auth)

                    // Syncho l'acces à la liste avec le verrou 
                    lock (_testLock)
                    {
                        Torrents.Add(NewTorrent);
                        Serialization.Serialiser(Torrents, Backup);
                    }
            }
        }

        public async void RelancerTorrents()
        {
            if (File.Exists(Backup))
            {
                ObservableCollection<Torrent> torrents = Serialization.Deserialiser<ObservableCollection<Torrent>>(Backup);
                foreach (Torrent torrent in torrents)
                {
                    if (File.Exists(torrent.path))
                    {
                        await AjouterTorrent(torrent.path);
                    }
                }
            }

            foreach (string path in Datas.Paths)
            {
                if (File.Exists(path))
                {
                    await AjouterTorrent(path);
                }
            }
        }

        public void AutoSuppression()
        {
            List<int> i = new List<int>();
            int j = 0;
            foreach (Torrent torrent in Torrents)
            {
                if (torrent.Status == "Terminé")
                {
                    i.Add(j);
                }
                j++;
            }
            i.Sort((a, b) => b.CompareTo(a));
            foreach (int r in i)
            {
                try
                {
                    File.Delete(Torrents[r].path);
                }
                catch (Exception)
                {

                }
                Torrents.RemoveAt(r);
            }
        }
        #endregion

        #region "Events"
        private void MainWindow_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length >= 1)
                {
                    e.Effects = DragDropEffects.Copy;
                }
            }
        }

        private async void MainWindow_Drop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string path in files)
            {
                if (File.Exists(path))
                {
                    await AjouterTorrent(path);
                }
            }
        }

        private void BT_Reprendre_Click(object sender, RoutedEventArgs e)
        {
            if (Torrents.Count > 0 && LV_TORRENTS.SelectedIndex > -1)
            {
                Torrents[LV_TORRENTS.SelectedIndex].Reprendre();
            }
        }

        private void BT_PAUSE_Click(object sender, RoutedEventArgs e)
        {
            if (Torrents.Count > 0 && LV_TORRENTS.SelectedIndex > -1)
            {
                Torrents[LV_TORRENTS.SelectedIndex].Pause();
            }

        }

        private void BT_SUPPRIMER_Click(object sender, RoutedEventArgs e)
        {
            if (Torrents.Count > 0 && LV_TORRENTS.SelectedIndex > -1)
            {
                Torrents[LV_TORRENTS.SelectedIndex].Supprimer();
                Torrents.RemoveAt(LV_TORRENTS.SelectedIndex);
                Serialization.Serialiser(Torrents, Backup);
            }
        }

        private void MainClosing(object sender, CancelEventArgs e)
        {
            AutoSuppression();
            Serialization.Serialiser(Torrents, Backup);
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
