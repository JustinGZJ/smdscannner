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
using MongoDB.Driver;
using MongoDB.Bson;
using System.Linq;


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
        SimpleTcpClient _scanner = null;
        LaserRecordsManager LaserRecordsManager;

        Settings settings = Settings.Default;
        private IIoService _ioService;
        public event EventHandler<Laser> LaserHandler;

        public LaserService([Inject] IEventAggregator @event, [Inject] IIoService ioService, [Inject] FileSaverFactory factory)
        {
            Events = @event;
            _factory = factory;
            _ioService = ioService;
            LaserRecordsManager = new LaserRecordsManager();
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
                _scanner?.Disconnect();
                _scanner = new SimpleTcpClient();
                _scanner.Delimiter = 0x0d;
                _scanner.Connect("192.168.0.3", 9004);
                Events.Publish(new MsgItem { Level = "D", Time = DateTime.Now, Value = "Server initialize: " + IPAddress.Any.ToString() + ":9004" });
            }
            catch (Exception EX)
            {
                Events.PostError(EX);
                return;
            }


            Task.Run(async () =>
            {
                bool input;
                //   _ioService.SetOutput(0, false);

                while (true)
                {
                    if (!_ioService.IsConnected)
                    {
                        Events.PostError("远程IO未连接");
                        await Task.Delay(500);
                    }
                    input = _ioService.GetInput(0);
                    if (input)
                    {
                        GetCode(0);
                        SpinWait.SpinUntil(() => _ioService.GetInput(0) == false);
                    }
                    input = _ioService.GetInput(1);
                    if (input)
                    {
                        GetCode(1);
                        SpinWait.SpinUntil(() => _ioService.GetInput(1) == false);
                    }

                    await Task.Delay(10);
                }
            });
        }

        public void GetCode(int index)
        {
            try
            {
                if (index == 0)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        _ioService.SetOutput((uint)(i), false);
                    }
                }
                else
                {
                    for (int i = 0; i < 3; i++)
                    {
                        _ioService.SetOutput((uint)(i + 3 * index), false);
                    }
                }
                Events.PostMessage($"第{index + 1}组扫码");
                _ioService.SetOutput(7, false);
                string cmd = $"LON";
                Events.PostMessage($"SCANNER SEND:{cmd}");
                var m1 = _scanner.WriteLineAndGetReply(cmd, TimeSpan.FromMilliseconds(3000));
                Events.PostMessage($"LASER RECV: {m1.MessageString}");
                var splits = m1?.MessageString.Split(',');
                for (int i = 0; i < splits.Length; i++)
                {
                    var CodeWithQuality = splits[i];
                    if (CodeWithQuality.Contains("ERROR"))
                    {
                        _ioService.SetOutput((uint)(i + 3 * index), false);
                        continue;
                    }
                    var CodeWithQualitySplits = CodeWithQuality.Split(':');
                    var code = CodeWithQualitySplits[0];
                    var judgecode = CodeWithQualitySplits[1].Split('/')[0];
                    var judgeSplits = CodeWithQualitySplits[1].Split('/').Take(1);
                    _ioService.SetOutput((uint)(i + 3 * index), true);
                    var laser = new Laser
                    {
                        BobbinCode = code,
                        BobbinLotNo = settings.BobbinLotNo,
                        LineNo = settings.LineNo,
                        Shift = settings.Shift,
                        CodeQuality = judgecode,
                        ProductionOrder = settings.ProductionOrder,
                        BobbinPartName = settings.BobbinPartName,
                        EmployeeNo = settings.EmployeeNo,
                        MachineNo = settings.MachineNo,
                        BobbinCavityNo = settings.BobbinCavityNo,
                        BobbinToolNo = settings.BobbinToolNo,
                        ShiftName = settings.ShiftName
                    };
                    var laserpoco = new LaserPoco
                    {
                        BobbinCode = laser.BobbinCode,
                        BobbinLotNo = settings.BobbinLotNo,
                        LineNo = settings.LineNo,
                        Shift = settings.Shift,
                        CodeQuality = laser.CodeQuality,
                        ProductionOrder = settings.ProductionOrder,
                        EmployeeNo = settings.EmployeeNo,
                        MachineNo = settings.MachineNo,
                        BobbinPartName = settings.BobbinPartName,
                        BobbinCavityNo = settings.BobbinCavityNo,
                        BobbinToolNo = settings.BobbinToolNo,
                        ShiftName = settings.ShiftName
                    };
                    OnLaserHandler(laser);
                    _factory.GetFileSaver<Laser>((i + 3 * index + 1).ToString()).Save(laser);
                    _factory.GetFileSaver<Laser>((i + 3 * index + 1).ToString(), @"D:\\SumidaFile\Monitor").Save(laser);
                    var qr = LaserRecordsManager.Find(laser.BobbinCode);
                    if (qr != null)
                    {
                        Events.PostWarn($"{qr.BobbinCode} {qr.DateTime} 镭射过了");
                        _ioService.SetOutput((uint)(i + 3 * index), false);
                        _ioService.SetOutput((uint)(6), true);
                    }
                    LaserRecordsManager.Insert(laserpoco);
                }
            }
            catch (Exception ex)
            {
                Events.PostError(ex);
                //   throw;
            }
            finally
            {
                Thread.Sleep(200);
                _ioService.SetOutput(7, true);
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


    public class LaserRecordsManager
    {
        private readonly string connStr;
        MongoClient client;
        IMongoDatabase database;
        IMongoCollection<LaserPoco> collection;
        public LaserRecordsManager(string connStr = "mongodb://127.0.0.1:27017")
        {
            this.connStr = connStr;
            client = new MongoClient(connStr);
            database = client.GetDatabase("smd");
            collection = database.GetCollection<LaserPoco>("laser");
        }

        public void Insert(LaserPoco laser)
        {
            collection.InsertOne(laser);
        }

        public LaserPoco Find(string code)
        {
            return collection.AsQueryable().FirstOrDefault(x => x.BobbinCode == code);
        }
    }
}