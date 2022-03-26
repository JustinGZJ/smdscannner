using SimpleTCP;
using System;
using System.IO;
using System.Net;
using Stylet;
using StyletIoC;
using System.Net.Sockets;
using System.Diagnostics;
using System.Windows.Media;
using DAQ.Properties;
using DAQ.Pages;
using System.Threading.Tasks;
using System.Threading;

namespace DAQ.Service
{

    public class ScannerService
    {
        IEventAggregator Events;
        private readonly MaterialManager _materialManager;
        private readonly IIoService ioService;
        private readonly FileSaverFactory _factory;
        SimpleTcpServer _server = null;
        LaserRecordsManager LaserRecordsManager = new LaserRecordsManager();

        public ScannerService([Inject] IEventAggregator @event, [Inject] MaterialManager materialManager, [Inject]IIoService ioService, [Inject] FileSaverFactory factory)
        {
            Events = @event;
            this._materialManager = materialManager;
            this.ioService = ioService;
            this._factory = factory;
            CreateServer();
        }




        public void CreateServer()
        {

            _server?.Stop();
            _server = new SimpleTcpServer().Start(9005, AddressFamily.InterNetwork);

            var ips = _server.GetListeningIPs();
            ips.ForEach(x => Events.Publish(new MsgItem
            { Level = "D", Time = DateTime.Now, Value = "Listening IP: " + x.ToString() + ":9005" }));
            Events.Publish(new MsgItem { Level = "D", Time = DateTime.Now, Value = "Server initialize: " + IPAddress.Any.ToString() + ":9005" });
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
                int mIndex = -1;

                for (int i = 0; i < 6; i++)
                {
                    if (ioService.GetInput((uint)i))
                    {
                        mIndex = i;
                    }
                }
                Events.PostInfo($"N5 Scanner:{mIndex}" + e.MessageString);
                if (mIndex == -1)
                {
                    Events.PostError("G4 轴号未指定");
                    ioService.SetOutput(0, false);

                    return;
                }
                if (e.MessageString.Contains("ERROR"))
                {
                    Events.PostError("扫码错误");
                    ioService.SetOutput(0, false);
                    return;
                }
                LaserPoco data = LaserRecordsManager.Find(e.MessageString);
                if (data!= null)
                {
                    Events.PostError($"{data.BobbinCode} :{data.DateTime:G} 扫过了。 ");
                    ioService.SetOutput(0, false);
                    return;
                }

                ioService.SetOutput(0, true);
                var settings = Settings.Default;
                var scan = new Scan
                {
                    Bobbin = e.MessageString,
                    Shift = settings.Shift1,
                    ShiftName = settings.ShiftName1,
                    Station=settings.Station1,
                    Production = settings.ProductionOrder1,
                    LineNo = settings.LineNo1,
                    MachineNo = settings.MachineNo1,
                    EmployeeNo = settings.EmployeeNo1,
                    SpindleNo=(mIndex+1).ToString(),
                    FlyWireLotNo = this._materialManager.FlyWires[mIndex],
                    TubeLotNo = this._materialManager.Tubes[mIndex]
                };
                LaserRecordsManager.Insert(new LaserPoco() { BobbinCode = scan.Bobbin });
                Events.PublishOnUIThread(scan);
                _factory.GetFileSaver<Scan>((mIndex + 1).ToString(), settings.SaveRootPath1).Save(scan);
                _factory.GetFileSaver<Scan>((mIndex + 1).ToString(), @"D:\\SumidaFile\Monitor\Scan").Save(scan);
            }
            finally
            {
                ioService.SetOutput(1, true);
                SpinWait.SpinUntil(()=>ioService.GetInput(7));
                 ioService.SetOutput(1, false);
            }


        }

    }
}