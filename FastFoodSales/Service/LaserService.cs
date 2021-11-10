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
        LaserRecordsManager LaserRecordsManager;

        private bool laserFinish = false;
        Properties.Settings settings = Settings.Default;
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
                Events.Publish(new MsgItem { Level = "D", Time = DateTime.Now, Value = "Server initialize: " + IPAddress.Any.ToString() + ":9004" });
            }
            catch (Exception EX)
            {
                Events.PostError(EX);
                return;
            }


            Task.Run(async () =>
            {
                //   _ioService.SetOutput(0, false);
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
            var locations = new string[] { settings.LaserLoc1, settings.LaserLoc2, settings.LaserLoc3, settings.LaserLoc4, settings.LaserLoc5, settings.LaserLoc6 };
            for (uint i = 0; i < 8; i++)
            {
                _ioService.SetOutput(i, false);
            }
            _ioService.SetOutput(7, true);


            try
            {
                for (int i = 0; i < 6; i++)
                {
                    int ngCount=0;  //ABB模式  OK X X PASS | NG OK OK PASS | NG NG X FAIL'
                    int okCount=0;
                    for (int j = 0; j < 3; j++)
                    {
                        string cmd = $"WX,Check2DCode=1,{locations[i]}{Environment.NewLine}";
                        Events.PostMessage($"LASER SEND:{cmd}");
                        var m1 = _laserClient.WriteLineAndGetReply(cmd, TimeSpan.FromMilliseconds(3000));
                        if (m1 != null)
                        {
                            Events.PostMessage($"LASER RECV: {m1.MessageString}");
                            var splits = m1.MessageString.Split(',');
                            var nunit = i + 1;
                            
                            if (splits.Length >= 3)
                            {
                                if (m1.MessageString.ToUpper().Contains("WX,OK"))
                                {
                                    if (!(splits[2].Contains("A") || splits[2].Contains("B")))
                                    {   
                                        ngCount++;
                                        Events.PostMessage($"扫码等级不合格：{ngCount}");
                                    }
                                    else
                                    {
                                        okCount++;
                                    }
                                    Events.PostMessage($"OK次数{okCount},NG次数{ngCount}");
                                    bool judge=false;
                                    if (ngCount == 1)
                                    {
                                        if(okCount<2)
                                        {
                                            Events.PostMessage($"重新扫码");
                                            continue;
                                        }
                                        else
                                        {
                                            judge=true;
                                        }
                                    }
                                    if (ngCount > 1)
                                    {
                                        judge=false;
                                    }
                                    if (ngCount == 0 && okCount > 0)
                                    {
                                        judge=true;
                                    }  
                                    Events.PostMessage($"扫码判定：{judge}");
                                    _ioService.SetOutput((uint)(i), judge);  
                                    var laser = new Laser
                                    {
                                        BobbinCode = splits[3].Trim('\r', '\n'),
                                        BobbinLotNo = settings.BobbinLotNo,
                                        LineNo = settings.LineNo,
                                        Shift = settings.Shift,
                                        CodeQuality = splits[2],
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
     

                                    if (judge)
                                    {
                           
                                        var qr = LaserRecordsManager.Find(laser.BobbinCode);
                                        if (qr != null)
                                        {
                                            Events.PostWarn($"{qr.BobbinCode} {qr.DateTime} 镭射过了");
                                            _ioService.SetOutput((uint)(i), false);
                                            _ioService.SetOutput((uint)(6), true);
                                        }
                                        else
                                        {
                                            OnLaserHandler(laser);
                                            _factory.GetFileSaver<Laser>((nunit).ToString()).Save(laser);
                                            _factory.GetFileSaver<Laser>((nunit).ToString(), @"D:\\SumidaFile\Monitor").Save(laser);
                                        }
                                        LaserRecordsManager.Insert(laserpoco);
                                    }

                                    break;
                                }
                                else
                                {
                                    cmd = $"UY,{settings.MarkingNo.ToString().PadLeft(3, '0')},{(nunit - 1).ToString().PadLeft(3, '0')},0{Environment.NewLine}";
                                    Events.PostMessage($"LASER SEND: {cmd}");
                                    var reply = _laserClient.WriteLineAndGetReply(cmd, TimeSpan.FromMilliseconds(2000));
                                    if (reply == null) return;
                                    Events.PostMessage($"LASER RECV: {reply.MessageString}");
                                    SaveLaserLog2(reply, nunit);
                                    break;
                                }
                            }
                            else
                            {
                                Events.PostError(new Exception("Format error " + splits[2]));
                                break;
                            }
                        }
                    }



                }
            }
            catch (Exception ex)
            {
                Events.PostError(ex.Message + "\n" + ex.StackTrace);
            }
            _ioService.SetOutput(7, false);

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