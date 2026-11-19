using Microsoft.VisualBasic;
using Overlay.Network;
using Overlay.Properties;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Overlay
{
    internal class OverlayApp : ApplicationContext
    {
        private const int MaxIconTextLength = 63;

        private readonly NotifyIcon _icon = new NotifyIcon { Icon = SystemIcons.Application, Text = "Overlay", Visible = true };
        private ServerConnection _connection;

        public OverlayApp()
        {
            _icon.ContextMenuStrip = new ContextMenuStrip();
            _icon.ContextMenuStrip.Items.Add("Change server...", null, (sender, e) => AskServerAddress());
            _icon.ContextMenuStrip.Items.Add("Exit", null, (sender, e) => ExitThread());

            Connect();
        }

        private void Connect()
        {
            _connection?.Stop();
            _connection = null;

            if (!ServerAddress.TryParse(Settings.Default.ServerAddress, out ServerAddress address))
            {
                SetIconText("invalid server adress");
                return;
            }

            _connection = new ServerConnection(address);
            _connection.StatusChanged += OnStatusChanged;
            _connection.MessageReceived += OnMessageReceived;
            _connection.Start();
        }

        private void AskServerAddress()
        {
            string input = Interaction.InputBox("Address of the server (example.com)", "Overlay", Settings.Default.ServerAddress);
            if (string.IsNullOrWhiteSpace(input))
            {
                return;
            }

            if (!ServerAddress.TryParse(input, out ServerAddress address))
            {
                MessageBox.Show($"Invalid address : {input}", "Overlay", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Settings.Default.ServerAddress = address.ToString();
            Settings.Default.Save();
            Connect();
        }

        private void OnStatusChanged(ConnectionStatus status)
        {
            string text = GetStatusText(status);
            SetIconText(text);
            Debug.WriteLine($"[Statut] {text}");

            if (status == ConnectionStatus.Banned || status == ConnectionStatus.AlreadyInUse)
            {
                _icon.ShowBalloonTip(5000, "Overlay", text, ToolTipIcon.Warning);
            }
        }

        private void OnMessageReceived(string line)
        {
            Debug.WriteLine($"[Serveur] {line}");
        }

        private string GetStatusText(ConnectionStatus status)
        {
            switch (status)
            {
                case ConnectionStatus.Connecting: return "connecting…";
                case ConnectionStatus.Connected: return "connected";
                case ConnectionStatus.WaitingForGame: return "waiting for the game...";
                case ConnectionStatus.Unreachable: return "server not reachable";
                case ConnectionStatus.SteamNotRunning: return "Staem isn't launched";
                case ConnectionStatus.Banned: return "you are banned from this server";
                case ConnectionStatus.AlreadyInUse: return "another overlay is already running";
                default: return status.ToString();
            }
        }

        private void SetIconText(string status)
        {
            string text = _connection == null ? $"Overlay {status}" : $"Overlay {status} ({_connection.Address})";
            _icon.Text = text.Length <= MaxIconTextLength ? text : text.Substring(0, MaxIconTextLength);
        }

        protected override void ExitThreadCore()
        {
            _connection?.Stop();
            _icon.Dispose();
            base.ExitThreadCore();
        }
    }
}
