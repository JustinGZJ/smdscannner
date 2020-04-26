using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stylet;

namespace DAQ.Pages
{
    public class TCPSettingViewModel:Screen
    {
        public ObservableCollection<TcpIpEndPoint> TcpIpEndPoints { get;  } =
            new ObservableCollection<TcpIpEndPoint>();

        public TCPSettingViewModel()
        {
            DisplayName = "TCP End Points";
            TcpIpEndPoints.Add(new TcpIpEndPoint() {Name = "N1 Scanner", IpAddress = "192.168.0.1",Port=9004});
            TcpIpEndPoints.Add(new TcpIpEndPoint() { Name = "N2 Scanner", IpAddress = "192.168.0.2",Port=9004 });
            TcpIpEndPoints.Add(new TcpIpEndPoint() { Name = "N3 Scanner", IpAddress = "192.168.0.3",Port=9004 });
            TcpIpEndPoints.Add(new TcpIpEndPoint() { Name = "N4 Scanner", IpAddress = "192.168.0.4" ,Port=9004});
            TcpIpEndPoints.Add(new TcpIpEndPoint() { Name = "N5 Scanner", IpAddress = "192.168.0.5",Port=9004 });
            TcpIpEndPoints.Add(new TcpIpEndPoint() { Name = "N6 Scanner", IpAddress = "192.168.0.6",Port=9004 });
            TcpIpEndPoints.Add(new TcpIpEndPoint() { Name = "N7 Scanner", IpAddress = "192.168.0.7",Port=9004});
            TcpIpEndPoints.Add(new TcpIpEndPoint() { Name = "N8 Scanner", IpAddress = "192.168.0.8", Port = 9004 });
            TcpIpEndPoints.Add(new TcpIpEndPoint() { Name = "N9 Scanner", IpAddress = "192.168.0.9", Port = 9004 });
            TcpIpEndPoints.Add(new TcpIpEndPoint() { Name = "N10 Scanner", IpAddress = "192.168.0.10", Port = 9004 });
            TcpIpEndPoints.Add(new TcpIpEndPoint() { Name = "N11 Scanner", IpAddress = "192.168.0.11", Port = 9004 });
            TcpIpEndPoints.Add(new TcpIpEndPoint() { Name = "N12 Scanner", IpAddress = "192.168.0.12", Port = 9004 });
            TcpIpEndPoints.Add(new TcpIpEndPoint() { Name = "N1 Machine", IpAddress = "192.168.0.21", Port = 502 });
            TcpIpEndPoints.Add(new TcpIpEndPoint() { Name = "N2 Machine", IpAddress = "192.168.0.22", Port = 502 });
            TcpIpEndPoints.Add(new TcpIpEndPoint() { Name = "N3 Machine", IpAddress = "192.168.0.23", Port = 502 });
            TcpIpEndPoints.Add(new TcpIpEndPoint() { Name = "N4 Machine", IpAddress = "192.168.0.24", Port = 502 });
            TcpIpEndPoints.Add(new TcpIpEndPoint() { Name = "N5 Machine", IpAddress = "192.168.0.25", Port = 502 });
            TcpIpEndPoints.Add(new TcpIpEndPoint() { Name = "N6 Machine", IpAddress = "192.168.0.26", Port = 502 });
            TcpIpEndPoints.Add(new TcpIpEndPoint() { Name = "N7 Machine", IpAddress = "192.168.0.27", Port = 502 });
            TcpIpEndPoints.Add(new TcpIpEndPoint() { Name = "N8 Machine", IpAddress = "192.168.0.28", Port = 502 });
            TcpIpEndPoints.Add(new TcpIpEndPoint() { Name = "N9 Machine", IpAddress = "192.168.0.29", Port = 502 });
            TcpIpEndPoints.Add(new TcpIpEndPoint() { Name = "N10 Machine", IpAddress = "192.168.0.30", Port = 502 });
            TcpIpEndPoints.Add(new TcpIpEndPoint() { Name = "N11 Machine", IpAddress = "192.168.0.31", Port = 502 });
            TcpIpEndPoints.Add(new TcpIpEndPoint() { Name = "N12 Machine", IpAddress = "192.168.0.32", Port = 502 });
            TcpIpEndPoints.Add(new TcpIpEndPoint() { Name = "Local Modbus Tcp Server", IpAddress = "127.0.0.1",Port=502 });
            TcpIpEndPoints.Add(new TcpIpEndPoint() { Name = "Remote IO", IpAddress = "192.168.0.239", Port = 4396 });
            TcpIpEndPoints.Add(new TcpIpEndPoint() { Name = "Keyence Laser Machine", IpAddress = "192.168.0.240", Port = 9004 });
        }
    }

    public class TcpIpEndPoint
    {
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public int Port  { get; set; }
    }
}
