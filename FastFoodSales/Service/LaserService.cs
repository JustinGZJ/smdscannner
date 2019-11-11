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
        private readonly FileSaverFactory _factory;
        SimpleTcpClient _laserClient = null;
        private ModbusTcpNet modbusTcp = new ModbusTcpNet();
        private bool laserFinish=false;
        Properties.Settings settings = Properties.Settings.Default;
        public event EventHandler<Laser> LaserHandler;

        public LaserService([Inject] IEventAggregator @event,[Inject] FileSaverFactory factory)
        {
            Events = @event;
            _factory = factory;
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
                                        SaveLaserLog(m1,1);
                                    }
                                    Events.PostMessage($"LASER SEND:{UY02}");
                                    var m2 = _laserClient.WriteLineAndGetReply(UY02, TimeSpan.FromMilliseconds(200));
                                    if (m2 != null)
                                    {
                                        Events.PostMessage($"LASER RECV: {m2.MessageString}");
                                        SaveLaserLog(m2,2);
                                    }
                                    Events.PostMessage($"LASER SEND:{UY03}");
                                    var m3 = _laserClient.WriteLineAndGetReply(UY03, TimeSpan.FromMilliseconds(200));
                                    if (m3 != null)
                                    {
                                        Events.PostMessage($"LASER RECV: {m3.MessageString}");
                                        SaveLaserLog(m3,3);
                                    }
                                    await task.ConfigureAwait(false);
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

        private void SaveLaserLog(Message m1,int nunit)
        {
        
            var splits = m1.MessageString.Split(',');
            if (splits.Length >= 3)
            {
                if(int.TryParse(splits[1],out int result))
                {
                    if (result == 0)
                    {
                        var laser = new Laser
                        {
                            BobbinCode = splits[2].Trim('\r','\n'),
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
                        };
                        _factory.GetFileSaver<Laser>((nunit).ToString()).Save(laser);
                        OnLaserHandler(laser);
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


        protected virtual void OnLaserHandler(Laser e)
        {
            LaserHandler?.Invoke(this, e);
        }
    }
}