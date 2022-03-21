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
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace DAQ.Service
{
    public interface IIoService
    {
        bool GetInput(uint index);
        void SetOutput(uint index, bool value);
        bool IsConnected { get; }
    }

    public class LaserService 
    {
        IEventAggregator Events;
        private readonly FileSaverFactory _factory;
        SimpleTcpClient _laserClient = null;
        SimpleTcpClient _scanner = null;
        LaserRecordsManager LaserRecordsManager;
        SimpleTcpServer _server = null;

        Settings settings = Settings.Default;
        private IIoService _ioService;
        public event EventHandler<LaserPoco> LaserHandler;

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
                //_laserClient?.Disconnect();
                //_laserClient = new SimpleTcpClient();
                //_laserClient.Connect("192.168.0.239", 9004);
                //_server?.Stop();
                _server = new SimpleTcpServer().Start(9004, AddressFamily.InterNetwork);
                var ips = _server.GetListeningIPs();
                ips.ForEach(x => Events.Publish(new MsgItem
                { Level = "D", Time = DateTime.Now, Value = "Listening IP: " + x.ToString() + ":9004" }));
                Events.Publish(new MsgItem { Level = "D", Time = DateTime.Now, Value = "Server initialize: " + IPAddress.Any.ToString() + ":9004" });
                _server.Delimiter = 0x0d;
                _server.DelimiterDataReceived -= _server_DelimiterDataReceived;
                _server.DelimiterDataReceived += _server_DelimiterDataReceived;
            }
            catch (Exception EX)
            {
                Events.PostError(EX);
                return;
            }


            //Task.Run(async () =>
            //{
            //    bool input;
            //    //   _ioService.SetOutput(0, false);

            //    while (true)
            //    {
            //        if (!_ioService.IsConnected)
            //        {
            //            Events.PostError("远程IO未连接");
            //            await Task.Delay(500);
            //        }
            //        input = _ioService.GetInput(0);
            //        if (input)
            //        {
            //            GetCode(0);
            //            SpinWait.SpinUntil(() => _ioService.GetInput(0) == false);
            //        }
            //        input = _ioService.GetInput(1);
            //        if (input)
            //        {
            //            GetCode(1);
            //            SpinWait.SpinUntil(() => _ioService.GetInput(1) == false);
            //        }

            //        await Task.Delay(10);
            //    }
            //});
        }

        private void _server_DelimiterDataReceived(object sender, Message e)
        {
            Events.PostMessage($"LASER RECV: {e.MessageString}");
            Regex rgx = new Regex(@"([\w\d]+)(:([\w]))?");
            var match=rgx.Match(e.MessageString);
            string  code="",degree = "E";

            if (match.Success)
            {
                code = match.Groups[1].Value;
                if (match.Groups.Count == 4)
                {
                    degree = match.Groups[3].Value;
                }
                var laser = new Laser
                {
                    BobbinCode = code,
                    BobbinLotNo = settings.BobbinLotNo,
                    LineNo = settings.LineNo,
                    Shift = settings.Shift,
                 //   CodeQuality = degree,
                    ProductionOrder = settings.ProductionOrder,
                    EmployeeNo = settings.EmployeeNo,
                    MachineNo = settings.MachineNo,
                    ShiftName = settings.ShiftName
                };
                var laserpoco = new LaserPoco
                {
                    BobbinCode = laser.BobbinCode,
                    BobbinLotNo = settings.BobbinLotNo,
                    LineNo = settings.LineNo,
                    Shift = settings.Shift,
                    CodeQuality = degree,
                    ProductionOrder = settings.ProductionOrder,
                    EmployeeNo = settings.EmployeeNo,
                    MachineNo = settings.MachineNo,
                    BobbinPartName = settings.BobbinPartName,
                    BobbinCavityNo = settings.BobbinCavityNo,
                    BobbinToolNo = settings.BobbinToolNo,
                    ShiftName = settings.ShiftName
                };
                _factory.GetFileSaver<Laser>((1).ToString()).Save(laser);
                _factory.GetFileSaver<Laser>((1).ToString(), @"D:\\SumidaFile\Monitor").Save(laser);
                OnLaserHandler(laserpoco);
                LaserRecordsManager.Insert(laserpoco);

            }
           
        }



        protected virtual void OnLaserHandler(LaserPoco e)
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