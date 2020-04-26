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
        private bool laserFinish=false;
        private MsgFileSaver<Laser> _saver;
        Properties.Settings settings = Properties.Settings.Default;
        public LaserService([Inject] IEventAggregator @event,[Inject] MsgFileSaver<Laser> saver)
        {
            Events = @event;
            _saver = saver;
        }
        public void CreateServer()
        {
            try
            {
                modbusTcp.IpAddress = "192.168.0.240";
                modbusTcp.Port = 502;
                modbusTcp.AddressStartWithZero = false;
                _laserClient?.Disconnect();
                _laserClient = new SimpleTcpClient();
                _laserClient.Connect("192.168.0.239", 9004);
                Events.Publish(new MsgItem { Level = "D", Time = DateTime.Now, Value = "Server initialize: " + IPAddress.Any.ToString() + ":9004" });
            }
            catch (Exception EX)
            {
                Events.PostError(EX);
                return;
            }

            Task.Run(() =>
            {

                while (true)
                {
                    var result = modbusTcp.ReadCoil("1");
                    if (result.IsSuccess)
                    {
                        if (laserFinish != result.Content)
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
                                    await task;
                                    modbusTcp.WriteCoil("17", false);
                                });
                            }
                            laserFinish = result.Content;
                        }

                    }
                    else
                    {
                        Events.PostError(new Exception(result.Message));
                    }
                }
            });
        }

        private void SaveLaserLog(Message m1)
        {
        
            var splits = m1.MessageString.Split(',');
            if (splits.Length >= 3)
            {
                if(int.TryParse(splits[1],out int result))
                {
                    if (result == 0)
                    {
                        _saver.Process(new Laser
                        {
                            BobbinCode = splits[2],
                            BobbinLotNo = settings.BobbinLotNo,
                            LineNo = settings.LineNo,
                            Shift = settings.Shift,
                            CodeQuality = "A",
                            Production = settings.Production,
                            EmployeeNo = settings.EmployeeNo,
                            MachineNo = settings.MachineNo,
                            BobbinCavityNo = settings.BobbinCavityNo,
                            BobbinToolNo = settings.BobbinToolNo,
                            Order = settings.Order,
                            ShiftName = settings.ShiftName
                        });
                    }
                    else
                    {
                        Events.PostError(new Exception("get laser info error.code "+ splits[2]));
                    }
                }
                else
                {
                    Events.PostError(new Exception("Format error " + splits[2]));
                }
       
            }
        }

        public void Dispose()
        {
            _laserClient?.Disconnect();
        }

        private string UY01 => "UY,009,000,0" + Environment.NewLine;
        private string UY02 => "UY,009,001,0" + Environment.NewLine;
        private string UY03 => "UY,009,002,0" + Environment.NewLine;



    }
}