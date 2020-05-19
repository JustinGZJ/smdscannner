using System;
using System.Diagnostics.Eventing.Reader;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DAQ.Pages;
using DAQ.Properties;
using SimpleTCP;
using Stylet;
using StyletIoC;

namespace DAQ.Service
{
    public interface IIoService
    {
        bool GetInput(uint index);
        void SetOutput(uint index, bool value);
        bool IsConnected { get; }
    }

    public class LaserService : IDisposable
    {
        IEventAggregator Events;
        private readonly FileSaverFactory _factory;
        SimpleTcpClient _laserClient = null;

        private bool laserFinish = false;
        Properties.Settings settings = Settings.Default;
        private IIoService _ioService;
        public event EventHandler<Laser> LaserHandler;

        public LaserService([Inject] IEventAggregator @event, [Inject] IIoService ioService, [Inject] FileSaverFactory factory)
        {
            Events = @event;
            _factory = factory;
            _ioService = ioService;
        }

        public int GetMarkingNo()
        {
            Events.PostMessage($"LASER SEND:FE");
            var m = _laserClient.WriteLineAndGetReply("FE" + Environment.NewLine, TimeSpan.FromMilliseconds(1000));
            Events.PostMessage($"LASER RECV:{m?.MessageString}");
            if (m != null && m.MessageString.Contains("FE,0"))
            {

                if (int.TryParse(m.MessageString.Trim('\r', '\n').Substring(5), out int num))
                {

                    return num;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                return -1;
            }
        }

        public void CreateServer()
        {
            try
            {
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


            Task.Run(async () =>
            {
                _ioService.SetOutput(0, false);
                while (true)
                {
                    if (!_ioService.IsConnected)
                    {
                        Events.PostError("远程IO未连接");
                        await Task.Delay(500);
                    }
                    var input = _ioService.GetInput(0);
                    if (input)
                    {
                        GetLaserData();
                        SpinWait.SpinUntil(() => _ioService.GetInput(0) == false);
                    }
                    await Task.Delay(10);
                }
            });
        }

        public void GetLaserData()
        {
            _ioService.SetOutput(0, true);

            if (true)  //读取二维码等级
            {
                string cmd = $"WX,Check2DCode=1,{settings.LaserLoc1}{Environment.NewLine}";
                Events.PostMessage($"LASER SEND:{cmd}");
                var m1 = _laserClient.WriteLineAndGetReply(cmd, TimeSpan.FromMilliseconds(3000));
                if (m1 != null)
                {
                    Events.PostMessage($"LASER RECV: {m1.MessageString}");
                    SaveLaserLog(m1, 1);
                }
                cmd = $"WX,Check2DCode=1,{settings.LaserLoc2}{Environment.NewLine}";
                Events.PostMessage($"LASER SEND:{cmd}");
                var m2 = _laserClient.WriteLineAndGetReply(cmd, TimeSpan.FromMilliseconds(3000));
                if (m2 != null)
                {
                    Events.PostMessage($"LASER RECV: {m2.MessageString}");
                    SaveLaserLog(m2, 2);
                }
                cmd = $"WX,Check2DCode=1,{settings.LaserLoc3}{Environment.NewLine}";
                Events.PostMessage($"LASER SEND:{cmd}");
                var m3 = _laserClient.WriteLineAndGetReply(cmd, TimeSpan.FromMilliseconds(3000));
                if (m3 != null)
                {
                    Events.PostMessage($"LASER RECV: {m3.MessageString}");
                    SaveLaserLog(m3, 3);
                }
            }
            _ioService.SetOutput(0, false);

        }

        private void SaveLaserLog(Message m1, int nunit)
        {

            var splits = m1.MessageString.Split(',');
            if (splits.Length >= 3)
            {
                if (m1.MessageString.ToUpper().Contains("WX,OK"))
                {
                    var laser = new Laser
                    {
                        BobbinCode = splits[3].Trim('\r', '\n'),
                        BobbinLotNo = settings.BobbinLotNo,
                        LineNo = settings.LineNo,
                        Shift = settings.Shift,
                        CodeQuality = splits[2],
                        ProductionOrder = settings.ProductionOrder,
                        EmployeeNo = settings.EmployeeNo,
                        MachineNo = settings.MachineNo,
                        BobbinCavityNo = settings.BobbinCavityNo,
                        BobbinToolNo = settings.BobbinToolNo,
                        ShiftName = settings.ShiftName
                    };
                    OnLaserHandler(laser);
                    _factory.GetFileSaver<Laser>((nunit).ToString()).Save(laser);
                    _factory.GetFileSaver<Laser>((nunit).ToString(), @"D:\\SumidaFile\Monitor").Save(laser);
                }
                else
                {
                    string cmd = $"UY,{settings.MarkingNo.ToString().PadLeft(3, '0')},{(nunit - 1).ToString().PadLeft(3, '0')},0{Environment.NewLine}";
                    Events.PostMessage($"LASER SEND: {cmd}");
                    var reply = _laserClient.WriteLineAndGetReply(cmd, TimeSpan.FromMilliseconds(2000));
                    if (reply == null) return;
                    Events.PostMessage($"LASER RECV: {reply.MessageString}");
                    SaveLaserLog2(reply, nunit);
                }
            }
            else
            {
                Events.PostError(new Exception("Format error " + splits[2]));
            }
        }


        private void SaveLaserLog2(Message m1, int nunit)
        {

            var splits = m1.MessageString.Split(',');
            if (splits.Length >= 3)
            {
                if (int.TryParse(splits[1], out int result))
                {
                    if (result != 0)
                    {
                        Events.PostError(new Exception("get laser info error.code " + splits[2]));
                    }
                    else
                    {
                        if (splits[2].Length > 5)
                        {
                            if (!int.TryParse(splits[2].Substring(splits[2].Length - 5), out int value)) return;
                            var code = splits[2].Substring(0, splits[2].Length - 6) + (value - 2).ToString().PadLeft(5, '0');
                            var laser = new Laser
                            {
                                BobbinCode = code.Trim('\r', '\n'),
                                BobbinLotNo = settings.BobbinLotNo,
                                LineNo = settings.LineNo,
                                Shift = settings.Shift,
                                CodeQuality = "E",
                                ProductionOrder = settings.ProductionOrder,
                                EmployeeNo = settings.EmployeeNo,
                                MachineNo = settings.MachineNo,
                                BobbinPartName = settings.BobbinPartName,
                                BobbinCavityNo = settings.BobbinCavityNo,
                                BobbinToolNo = settings.BobbinToolNo,
                                ShiftName = settings.ShiftName
                            };
                            OnLaserHandler(laser);
                            _factory.GetFileSaver<Laser>((nunit).ToString()).Save(laser);
                            _factory.GetFileSaver<Laser>((nunit).ToString(), @"D:\\SumidaFile\Monitor").Save(laser);
                        }
                        else
                        {
                        }
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

        protected virtual void OnLaserHandler(Laser e)
        {
            LaserHandler?.Invoke(this, e);
        }
    }
}