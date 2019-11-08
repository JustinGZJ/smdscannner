using SimpleTCP;
using System;
using System.IO;
using System.Net;
using Stylet;
using StyletIoC;
using System.Net.Sockets;
using System.Diagnostics;
using System.Linq;


namespace DAQ.Service
{
    public class ScannerService : IDisposable
    {

        IEventAggregator Events;
        SimpleTcpServer _server = null;

        public ScannerService([Inject] IEventAggregator @event)
        {
            Events = @event;
            CreateServer();
        }


        public void CreateServer()
        {

            _server?.Stop();
            _server = new SimpleTcpServer();
            _server.Start(9004, AddressFamily.InterNetwork);

            Events.Publish(new MsgItem { Level = "D", Time = DateTime.Now, Value = "Server initialize: " + IPAddress.Any.ToString() + ":9004" });
            _server.Delimiter = 0x0d;
            _server.DelimiterDataReceived -= Client_DelimiterDataReceived;
            _server.DelimiterDataReceived += Client_DelimiterDataReceived;
        }

        public void Dispose()
        {
            _server?.Stop();
        }

        private void Client_DelimiterDataReceived(object sender, Message e)
        {
            try
            {
                var addr = ((IPEndPoint)e.TcpClient.Client.RemoteEndPoint).Address.GetAddressBytes()[3];
                var str = e.MessageString.Trim('\r', '\n');
                Events.Publish(str, addr.ToString(), addr.ToString());
                Events.Publish(new MsgItem { Level = "D", Time = DateTime.Now, Value = ((IPEndPoint)e.TcpClient.Client.RemoteEndPoint).Address.ToString() + ":" + str });
            }
            catch (Exception ex)
            {
                Events.Publish(new MsgItem { Level = "E", Time = DateTime.Now, Value = ex.Message });
            }
        }

    }

    public static class PostMsg
    {
        public static void PostError(this IEventAggregator @event,Exception exception)
        {
            @event?.Publish(new MsgItem { Level = "E", Time = DateTime.Now, Value = exception.Message });
        }

        public static void PostMessage(this IEventAggregator @event, string message)
        {
            @event?.Publish(new MsgItem { Level = "D", Time = DateTime.Now, Value = message });
        }

    }
}