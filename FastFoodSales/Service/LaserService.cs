using System;
using System.Net;
using System.Threading.Tasks;
using DAQ.Pages;
using DAQ.Properties;
using HslCommunication.ModBus;
using SimpleTCP;
using Stylet;
using StyletIoC;

namespace DAQ.Service
{
    public class LaserService : IDisposable
    {
        IEventAggregator Events;
        SimpleTcpClient _laserClient = null;
        private ModbusTcpNet modbusTcp = new ModbusTcpNet();
        private bool laserFinish;
        private MsgFileSaver<TLaser> saver = new MsgFileSaver<TLaser>();
        Properties.Settings settings = Properties.Settings.Default;
        public LaserService([Inject] IEventAggregator @event)
        {
            CreateServer();
        }
        public void CreateServer()
        {
            modbusTcp.IpAddress = "192.168.0.240";
            modbusTcp.Port = 4196;
            modbusTcp.AddressStartWithZero = false;
            _laserClient?.Disconnect();
            _laserClient = new SimpleTcpClient();
            _laserClient.Connect("192.168.0.239", 9004);
          

            Events.Publish(new MsgItem { Level = "D", Time = DateTime.Now, Value = "Server initialize: " + IPAddress.Any.ToString() + ":9004" });

            Task.Run(() =>
            {

                while (true)
                {
                    var result = modbusTcp.ReadCoil("1");
                    if (result.IsSuccess)
                    {
                        var finish = laserFinish;
                        if (finish != result.Content)
                        {
                            if (result.Content)
                            {
                                Task.Run(async () =>
                                {
                                    modbusTcp.WriteCoil("17", true);
                                    Task task = Task.Delay(500);

                                    Events.PostMessage($"LASER SEND:{UY01}");
                                    var m1 = _laserClient.WriteLineAndGetReply(UY01, TimeSpan.FromMilliseconds(200));
                                    if (m1 != null)
                                    {
                                        Events.PostMessage($"LASER RECV: {m1.MessageString}");
                                        SaveLaserLog(m1);
                                    }
                                    Events.PostMessage($"LASER SEND:{UY02}");
                                    var m2 = _laserClient.WriteLineAndGetReply(UY02, TimeSpan.FromMilliseconds(200));
                                    if (m2 != null)
                                    {
                                        Events.PostMessage($"LASER RECV: {m2.MessageString}");
                                        SaveLaserLog(m2);
                                    }
                                    Events.PostMessage($"LASER SEND:{UY03}");
                                    var m3 = _laserClient.WriteLineAndGetReply(UY03, TimeSpan.FromMilliseconds(200));
                                    if (m3 != null)
                                    {
                                        Events.PostMessage($"LASER RECV: {m3.MessageString}");
                                        SaveLaserLog(m3);
                                    }
                                    modbusTcp.WriteCoil("17", false);
                                    await task;
                                });
                            }
                            laserFinish = result.Content;
                        }
                    }
                }
            });
        }

        private void SaveLaserLog(Message m1)
        {
        
            var splits = m1.MessageString.Split(',');
            if (splits.Length >= 3)
            {
                saver.Process(new TLaser
                {
                    BobbinCode = splits[2],
                    BobbinLotNo = settings.BobbinLotNo,
                    LineNo = settings.LineNo,
                    Shift = settings.Shift,
                    Source = "LaserCode",
                    CodeQuality = "A",
                    ProductionOrder = settings.LotNo,
                    EmployeeNo = settings.EmployeeNo,
                    MachineNo = settings.MachineNo,
                    FlyWireLotNo=settings.FlyWireLotNo,
                     TubeLotNo=settings.TubeLotNo
                    
                });
            }
        }

        public void Dispose()
        {
            _laserClient?.Disconnect();

        }

        private string UY01 => "UY,000,000,0" + Environment.NewLine;
        private string UY02 => "UY,000,001,0" + Environment.NewLine;
        private string UY03 => "UY,000,002,0" + Environment.NewLine;



    }
}