namespace Overlay.Network
{
    internal class ServerAddress
    {
        public const int DefaultPort = 7790;

        public string Host { get; }
        public int Port { get; }

        private ServerAddress(string host, int port)
        {
            Host = host;
            Port = port;
        }

        public static bool TryParse(string input, out ServerAddress address)
        {
            address = null;

            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            input = input.Trim();
            int separator = input.LastIndexOf(':');

            if (separator < 0)
            {
                address = new ServerAddress(input, DefaultPort);
                return true;
            }

            string host = input.Substring(0, separator);
            string portText = input.Substring(separator + 1);

            if (host.Length == 0 || !int.TryParse(portText, out int port) || port < 1 || port > 65535)
            {
                return false;
            }

            address = new ServerAddress(host, port);
            return true;
        }

        public override string ToString()
        {
            return $"{Host}:{Port}";
        }
    }
}
