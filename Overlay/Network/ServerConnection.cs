using Overlay.Steam;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Overlay.Network
{
    internal sealed class ServerConnection
    {
        private const int RetryDelayMs = 5000;
        private static readonly Encoding Utf8 = new UTF8Encoding(false);

        private readonly ServerAddress _address;
        private TcpClient _client;
        private bool _isRunning;

        public event Action<ConnectionStatus> StatusChanged;
        public event Action<string> MessageReceived;

        public ServerAddress Address => _address;

        public ServerConnection(ServerAddress address)
        {
            _address = address;
        }

        public void Start()
        {
            _isRunning = true;
            _ = Run();
        }

        public void Stop()
        {
            _isRunning = false;
            _client?.Close();
        }

        private async Task Run()
        {
            while (_isRunning)
            {
                bool shouldRetry = await TryConnect();
                if (!shouldRetry)
                {
                    return;
                }

                await Task.Delay(RetryDelayMs);
            }
        }

        private async Task<bool> TryConnect()
        {
            string userId = SteamAccount.GetUserId();
            if (userId == null)
            {
                SetStatus(ConnectionStatus.SteamNotRunning);
                return true;
            }

            using (TcpClient client = new TcpClient())
            {
                _client = client;

                try
                {
                    await client.ConnectAsync(_address.Host, _address.Port);

                    NetworkStream stream = client.GetStream();
                    StreamReader reader = new StreamReader(stream, Utf8);
                    StreamWriter writer = new StreamWriter(stream, Utf8) { AutoFlush = true };

                    await writer.WriteLineAsync("AUTH " + userId);
                    string response = await reader.ReadLineAsync();

                    return await HandleResponse(response, reader);
                }
                catch (Exception e) when (e is SocketException || e is IOException || e is ObjectDisposedException)
                {
                    SetStatus(ConnectionStatus.Unreachable);
                    return true;
                }
                finally
                {
                    _client = null;
                }
            }
        }

        private async Task<bool> HandleResponse(string response, StreamReader reader)
        {
            switch (response)
            {
                case "welcome":
                    SetStatus(ConnectionStatus.Connected);
                    await ReadMessages(reader);
                    SetStatus(ConnectionStatus.Connecting);
                    return true;

                case "refused":
                    SetStatus(ConnectionStatus.WaitingForGame);
                    return true;

                case "banned":
                    SetStatus(ConnectionStatus.Banned);
                    return false;

                case "alreadyinuse":
                    SetStatus(ConnectionStatus.AlreadyInUse);
                    return false;

                default:
                    SetStatus(ConnectionStatus.Unreachable);
                    return true;
            }
        }

        private async Task ReadMessages(StreamReader reader)
        {
            string line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                MessageReceived?.Invoke(line);
            }
        }

        private void SetStatus(ConnectionStatus status)
        {
            if (_isRunning)
            {
                StatusChanged?.Invoke(status);
            }
        }
    }
}
