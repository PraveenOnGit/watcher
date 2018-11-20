
namespace Mfs.Watcher.Dashboard.Entities
{
    public enum ConnectionState
    {
        Connected,
        NotConnected
    }

    public class ConnectionStatus
    {
        public string ProcessName { get; set; }
        public ConnectionState Status { get; set; }
        public string Error { get; set; }
    }
}
