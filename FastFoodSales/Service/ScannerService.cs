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
using System.Xml.Linq;
using MongoDB.Bson;

namespace DAQ.Service
{
    public class MaterialManager
    {

        public string[] FlyWires { get; set; } = Enumerable.Repeat("", 8).ToArray();
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
        FFTester.Tester ts = new FFTester.Tester();

        public ScannerService([Inject] IEventAggregator @event, [Inject] MaterialManager materialManager, [Inject] FileSaverFactory factory)
        {
            Events = @event;
            this._materialManager = materialManager;
            this.ioService = new IoService("192.168.0.241");
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

        private int SaveToMes(string starttime, string endtime, string station, string shift, string id, string line, string spindle)
        {
            var batch = new XElement("BATCH");
            batch.SetAttributeValue("TIMESTAMP", starttime);
            batch.SetAttributeValue("SYNTAX_REV", "");
            batch.SetAttributeValue("COMPATIBLE_REV", "");
            var factory = new XElement("FACTORY");
            factory.SetAttributeValue("NAME", "Flextronics DongGuan");
            factory.SetAttributeValue("LINE", line);
            factory.SetAttributeValue("TESTER", station);
            factory.SetAttributeValue("FIXTURE", "");
            factory.SetAttributeValue("SHIFT", shift);
            factory.SetAttributeValue("USER", "admin");
            var product = new XElement("PRODUCT");
            product.SetAttributeValue("NAME", "");
            product.SetAttributeValue("REVISION", "");
            product.SetAttributeValue("FAMILY", "");
            product.SetAttributeValue("CUSTOMER", "");
            var refs = new XElement("REFS");
            refs.SetAttributeValue("SEQ_REF", "");
            refs.SetAttributeValue("FTS_REF", "");
            refs.SetAttributeValue("LIM_REF", "");
            refs.SetAttributeValue("CFG_REF", "");
            refs.SetAttributeValue("CAL_REF", "");
            refs.SetAttributeValue("INSTR_REF", "");
            var panel = new XElement("PANEL");
            panel.SetAttributeValue("ID", id);
            panel.SetAttributeValue("COMMENT", "");
            panel.SetAttributeValue("RUNMODE", "");
            panel.SetAttributeValue("TIMESTAMP", starttime);
            panel.SetAttributeValue("TESTTIME", 1);
            panel.SetAttributeValue("ENDTIME", endtime);
            panel.SetAttributeValue("WAITTIME", "");
            panel.SetAttributeValue("STATUS", "Passed");
            var dut = new XElement("DUT");
            dut.SetAttributeValue("ID", id);
            dut.SetAttributeValue("COMMENT", "");
            dut.SetAttributeValue("PANEL", "");
            dut.SetAttributeValue("SOCKET", "");

            dut.SetAttributeValue("TIMESTAMP", starttime);
            dut.SetAttributeValue("TESTTIME", 1);
            dut.SetAttributeValue("ENDTIME", endtime);
            dut.SetAttributeValue("STATUS", "Passed");
            var group = new XElement("GROUP");
            group.SetAttributeValue("NAME", "SCAN");
            group.SetAttributeValue("STEPGROUP", "");
            group.SetAttributeValue("GROUPINDEX", 0);
            group.SetAttributeValue("LOOPINDEX", "");
            group.SetAttributeValue("TYPE", "");
            group.SetAttributeValue("RESOURCE", "");
            group.SetAttributeValue("MODULETIME", "");
            group.SetAttributeValue("TOTALTIME", "");
            group.SetAttributeValue("TIMESTAMP", starttime);
            group.SetAttributeValue("STATUS", "Passed");

            var test1 = new XElement("TEST");
            test1.SetAttributeValue("NAME", "BOBBIN CODE");
            test1.SetAttributeValue("DESCRIPTION", "BOBBIN CODE");
            test1.SetAttributeValue("UNIT", "");
            test1.SetAttributeValue("VALUE", id);
            test1.SetAttributeValue("HILIM", "");
            test1.SetAttributeValue("LOLIM", "");
            test1.SetAttributeValue("STATUS", "Passed");
            test1.SetAttributeValue("RULE", "");
            test1.SetAttributeValue("TARGET", "");
            test1.SetAttributeValue("DATATYPE", "String");
            var test2 = new XElement("TEST");
            test2.SetAttributeValue("NAME", "SPINDLE");
            test2.SetAttributeValue("DESCRIPTION", "SPINDLE");
            test2.SetAttributeValue("UNIT", "");
            test2.SetAttributeValue("VALUE", spindle);
            test2.SetAttributeValue("HILIM", "");
            test2.SetAttributeValue("LOLIM", "");
            test2.SetAttributeValue("STATUS", "Passed");
            test2.SetAttributeValue("RULE", "");
            test2.SetAttributeValue("TARGET", "");
            test2.SetAttributeValue("DATATYPE", "Number");
            group.Add(test1);
            group.Add(test2);
            dut.Add(group);
            panel.Add(dut);
            batch.Add(factory);
            batch.Add(product);
            batch.Add(refs);
            batch.Add(panel);

            DAQ.wcl.FFTesterServiceClient serviceClient = new wcl.FFTesterServiceClient();
            Events.PostMessage($"PC->SFIS:{id}");

            var res = serviceClient.GetUnitInfo(id, station, "admin", "");
            Events.PostMessage(string.Format("SFIS return value = {0} \n", res.ToJson()));
            if (res.Id != 0)
            {
                Events.PostError(string.Format("SFIS return value = {0} \n", res.Value));
                return -1;
            }
            else
            {
                Events.PostMessage("GetUnitInfo Sucess!");
            }
            serviceClient.Close();
            var ret = ts.SaveResult(batch.ToString());
            Events.PostInfo("pc->sfis:" + batch.ToString());
            Events.PostInfo("sfis->pc:" + ret.ToString());
            if (ret != 0)
            {
                Events.PostError(ts.GetErrMessage(ret));
                return -2;
            }
            //  serviceClient.SaveResult()


            //     Events.PostMessage(string.Format("SFIS return value = {0} \n", res.ToJson()));

            return 0;
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
                    FlyWireLotNo = this._materialManager.FlyWires[mIndex],
                    TubeLotNo = this._materialManager.Tubes[mIndex]
                };

                var ret = SaveToMes(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "FlyWireWindingN5(A)_E201", scan.Shift, scan.Bobbin, "E201", (mIndex + 1).ToString());
                if (ret != 0)
                {
                    ioService.SetOutput(0, false);
                }
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