using System;
using System.Windows;
using System.Threading;
using System.IO.Pipes;
using System.Text;

namespace Client_torrent
{
    public partial class App : Application
    {
        private const string PipeName = "Pipe_ClientTorrent";
        private NamedPipeClientStream pipeClient;
        private Mutex mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            string mutexName = $"Local\\MutexClientTorrent_{Environment.UserName}";
            bool createdNew = false;
            mutex = new Mutex(true, mutexName, out createdNew);

            if (!createdNew)
            {
                try
                {
                    pipeClient = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
                    pipeClient.Connect();

                    string messageToSend = "";

                    foreach (string path in e.Args)
                    {
                        messageToSend += path + "\n";
                    }

                    byte[] messageBytes = Encoding.UTF8.GetBytes(messageToSend);
                    pipeClient.Write(messageBytes, 0, messageBytes.Length);
                    pipeClient.Close();
                }
                catch (Exception)
                {
                }

                Shutdown();
                return;
            }

            base.OnStartup(e);

            foreach (string path in e.Args)
            {
                Datas.Paths.Add(path);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            mutex?.Dispose();
            base.OnExit(e);
        }
    }
}
