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
using DAQ.Pages;
using System.Threading.Tasks;
using System.Threading;

namespace DAQ.Service
{
    public class MaterialManager
    {

        public string[] FlyWires { get; set; } = Enumerable.Repeat("",8).ToArray();
        public string[] Tubes { get; set; } = Enumerable.Repeat("", 8).ToArray();

        public MaterialManager Save()
        {
            Settings.Default.Materials = JsonConvert.SerializeObject(this);
            Settings.Default.Save();
            return this;
        }

        public (string, string) GetMaterial(int index)
        {
            if (index > 7 || index < 0)
                throw new OutOfMemoryException("index mush be between 0 and 7");
            return (FlyWires[index], Tubes[index]);
        }

        public static MaterialManager Load()
        {
            MaterialManager m;
            try
            {
                  m = JsonConvert.DeserializeObject<MaterialManager>(Settings.Default.Materials) ?? new MaterialManager();
               // m = new MaterialManager();
            }
            catch (Exception e)
            {
                m = new MaterialManager();
            }
            return m;
        }
    }

    public class ScannerService
    {
        IEventAggregator Events;
        private readonly MaterialManager _materialManager;
        private readonly IIoService ioService;
        private readonly FileSaverFactory _factory;
        SimpleTcpServer _server = null;

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
                Events.PostInfo($"N3 Scanner:{mIndex}" + e.MessageString);
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

                ioService.SetOutput(0, true);
                var settings = Settings.Default;
                var scan = new Scan
                {
                    Bobbin = e.MessageString,
                    Shift = settings.Shift1,
                    ShiftName = settings.ShiftName1,
                    Production = settings.ProductionOrder1,
                    LineNo = settings.LineNo1,
                    MachineNo = settings.MachineNo1,
                    EmployeeNo = settings.EmployeeNo1,
                    SpindleNo=(mIndex+1).ToString(),
                    FlyWireLotNo = this._materialManager.FlyWires[mIndex],
                    TubeLotNo = this._materialManager.Tubes[mIndex]
                };

                Events.PublishOnUIThread(scan);
                _factory.GetFileSaver<Scan>((mIndex + 1).ToString(), settings.SaveRootPath1).Save(scan);
                _factory.GetFileSaver<Scan>((mIndex + 1).ToString(), @"D:\\SumidaFile\Monitor\Scan").Save(scan);
            }
            finally
            {
                ioService.SetOutput(1, true);
                Thread.Sleep(500);
                ioService.SetOutput(1, false);
            }


        }

    }
}