using System;
using System.ComponentModel;
using System.IO;
using System.Media;
using System.Threading.Tasks;
using System.Windows;
using MonoTorrent.Client;

namespace Client_torrent
{
    [Serializable]
    public class Torrent : INotifyPropertyChanged
    {
        private ClientEngine engine;
        private TorrentManager torrentManager;

        #region "Fonctions"
        public async Task<bool> Start(string torrentFilePath, string downloadPath)
        {
            this.path = torrentFilePath;
            return await Download(torrentFilePath, downloadPath);
        }

        public async void Pause()
        {
            await torrentManager.PauseAsync();
        }

        public async void Reprendre()
        {
            await torrentManager.StartAsync();
        }

        public async void Supprimer()
        {
            await torrentManager.StopAsync();
        }

        public async Task<bool> Download(string torrentFilePath, string downloadPath)
        {
            if (File.Exists(torrentFilePath))
            {
                try
                {
                    this.Nom = System.IO.Path.GetFileName(torrentFilePath).ToString().Replace(".torrent", "");
                    TorrentSettings torrentSettings = new TorrentSettings() { MaximumDownloadSpeed = 0, MaximumUploadSpeed = 1 };
                    var torrent = MonoTorrent.Torrent.Load(torrentFilePath);
                    engine = new ClientEngine(new EngineSettings());
                    torrentManager = new TorrentManager(torrent, downloadPath, torrentSettings);
                    await engine.Register(torrentManager);

                    torrentManager.TorrentStateChanged += (sender, e) =>
                    {
                        Application.Current.Dispatcher.Invoke(async () =>
                        {
                            await Event_TorrentStateChanged(sender, e);
                        });
                    };

                    torrentManager.PieceHashed += (sender, e) =>
                    {
                        Event_PieceHashed();
                    };

                    await torrentManager.StartAsync();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return false;
        }
        #endregion

        #region "Events"
        public async Task<int> Event_TorrentStateChanged(object sender, TorrentStateChangedEventArgs e)
        {
            this.Status = e.NewState.ToString().Replace("Downloading", "Téléchargement").Replace("Hashing", "Hachage").Replace("Stopped", "Terminé").Replace("HachagePaused", "Pause").Replace("Paused", "Pause");
            if (this.Status == "Seeding")
            {
                await torrentManager.StopAsync();
                this.DebitDownload = "0 Ko/s";
                this.DebitUpload = "0 Ko/s";
                SystemSounds.Asterisk.Play();
            }
            return 0;
        }

        public void Event_PieceHashed()
        {
            long downloadSpeedInBytesPerSecond = torrentManager.Monitor.DownloadSpeed;
            double downloadSpeedInKilobytesPerSecond = downloadSpeedInBytesPerSecond / 1000.0;
            double downloadSpeedInMegabytesPerSecond = downloadSpeedInKilobytesPerSecond / 1000.0;
            long uploadSpeedInBytesPerSecond = torrentManager.Monitor.UploadSpeed;
            double uploadSpeedInKilobytesPerSecond = uploadSpeedInBytesPerSecond / 1000.0;
            double uploadSpeedInMegabytesPerSecond = uploadSpeedInKilobytesPerSecond / 1000.0;
            double SizeInBytes = torrentManager.Torrent.Size;
            double SizeInKiloBytes = SizeInBytes / 1000;
            double SizeInMegaBytes = SizeInKiloBytes / 1000;
            double percentComplete = (double)torrentManager.Bitfield.PercentComplete;

            this.Progression = $"{Math.Round(percentComplete, 0)}";
            this.Seeds = torrentManager.Peers.Seeds.ToString();
            this.Peers = torrentManager.Peers.Leechs.ToString();
            this.DebitDownload = (downloadSpeedInKilobytesPerSecond > 1000) ? $"{Math.Round(downloadSpeedInMegabytesPerSecond, 2)} Mo/s" : $"{Math.Round(downloadSpeedInKilobytesPerSecond, 2)} Ko/s";
            this.DebitUpload = (uploadSpeedInKilobytesPerSecond > 1000) ? $"{Math.Round(uploadSpeedInMegabytesPerSecond, 2)} Mo/s" : $"{Math.Round(uploadSpeedInKilobytesPerSecond, 2)} Ko/s";
            this.Taille = $"{Math.Round(SizeInMegaBytes * percentComplete / 100, 0)}/{Math.Round(SizeInMegaBytes, 0)} Mo";

            double Time = (downloadSpeedInKilobytesPerSecond > 0) ? (SizeInKiloBytes - (SizeInKiloBytes * percentComplete / 100)) / downloadSpeedInKilobytesPerSecond : 0;
            this.Duree = TimeSpan.FromSeconds(Time).ToString(@"hh\:mm\:ss");
        }
        #endregion

        #region "Propertys"
        private string _path = "";
        public string path
        {
            get { return _path; }
            set { if (path != value) { _path = value; OnPropertyChanged(nameof(path)); } }
        }

        private string _Nom = "";
        public string Nom
        {
            get { return _Nom; }
            set { if (Nom != value) { _Nom = value; OnPropertyChanged(nameof(Nom)); } }
        }

        private string _Status = "";
        public string Status
        {
            get { return _Status; }
            set { if (Status != value) { _Status = value; OnPropertyChanged(nameof(Status)); } }
        }

        private string _Progression = "";
        public string Progression
        {
            get { return _Progression; }
            set { if (Progression != value) { _Progression = value; OnPropertyChanged(nameof(Progression)); } }
        }

        private string _Taille = "";
        public string Taille
        {
            get { return _Taille; }
            set { if (Taille != value) { _Taille = value; OnPropertyChanged(nameof(Taille)); } }
        }

        private string _Seeds = "";
        public string Seeds
        {
            get { return _Seeds; }
            set { if (Seeds != value) { _Seeds = value; OnPropertyChanged(nameof(Seeds)); } }
        }

        private string _Peers = "";
        public string Peers
        {
            get { return _Peers; }
            set { if (Peers != value) { _Peers = value; OnPropertyChanged(nameof(Peers)); } }
        }

        private string _DebitDownload = "";
        public string DebitDownload
        {
            get { return _DebitDownload; }
            set { if (DebitDownload != value) { _DebitDownload = value; OnPropertyChanged(nameof(DebitDownload)); } }
        }

        private string _DebitUpload = "";
        public string DebitUpload
        {
            get { return _DebitUpload; }
            set { if (DebitUpload != value) { _DebitUpload = value; OnPropertyChanged(nameof(DebitUpload)); } }
        }

        private string _Duree = "";
        public string Duree
        {
            get { return _Duree; }
            set { if (Duree != value) { _Duree = value; OnPropertyChanged(nameof(Duree)); } }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        #endregion
    }

}
