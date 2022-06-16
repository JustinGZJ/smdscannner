using SimpleTCP;
using System;
using System.IO;
using System.Net;
using Stylet;
using StyletIoC;
using System.Net.Sockets;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;
using DAQ.Properties;
using Newtonsoft.Json;


namespace DAQ.Service
{
    public class MaterialManager
    {

        public string[] FlyWires { get; set; } = new string[8];
        public string[] Tubes { get; set; } = new string[8];
        /// <summary>
        /// 6个二维码
        /// </summary>
        public string[] LaserLocations { get; set; } = new string[8];
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="OutOfMemoryException"></exception>
        public (string, string) GetMaterial(int index)
        {
            if (index > 7 || index < 0)
                throw new OutOfMemoryException("index mush be between 0 and 7");
            return (FlyWires[index], Tubes[index]);
        }
    }

    public class ScannerService : IDisposable
    {
        IEventAggregator Events;
        SimpleTcpServer _server = null;

        public ScannerService([Inject] IEventAggregator @event)
        {
            Events = @event;
            CreateServer();
        }



        /// <summary>
        /// 
        /// </summary>
        public void CreateServer()
        {

            _server?.Stop();
            _server = new SimpleTcpServer().Start(9004, AddressFamily.InterNetwork);
            var ips = _server.GetListeningIPs();
            ips.ForEach(x => Events.Publish(new MsgItem
            { Level = "D", Time = DateTime.Now, Value = "Listening IP: " + x.ToString() + ":9004" }));
            Events.Publish(new MsgItem { Level = "D", Time = DateTime.Now, Value = "Server initialize: " + IPAddress.Any.ToString() + ":9004" });
            _server.Delimiter = 0x0d;
            _server.DelimiterDataReceived -= Client_DelimiterDataReceived;
            _server.DelimiterDataReceived += Client_DelimiterDataReceived;
        }

        public void Dispose()
        {
            _server?.Stop();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Client_DelimiterDataReceived(object sender, Message e)
        {
            try
            {
                var addr = ((IPEndPoint)e.TcpClient.Client.RemoteEndPoint).Address.GetAddressBytes()[3];
                var str = e.MessageString.Trim('\r', '\n');
                Events.Publish(str, addr.ToString());
                Events.Publish(new MsgItem { Level = "D", Time = DateTime.Now, Value = ((IPEndPoint)e.TcpClient.Client.RemoteEndPoint).Address.ToString() + ":" + str });
            }
            catch (Exception ex)
            {
                Events.Publish(new MsgItem { Level = "E", Time = DateTime.Now, Value = ex.Message });
            }
        }

    }
}