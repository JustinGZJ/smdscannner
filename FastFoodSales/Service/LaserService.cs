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
using System.Xml.Linq;
using System.Collections;



namespace DAQ.Service
{
    public interface IIoService
    {
        bool GetInput(uint index);
        void SetOutput(uint index, bool value);
        bool IsConnected { get; }
    }

    public class LaserService :IDisposable
    {
        private const int LASERSCAN_REQ = 0;
        private const int STARTLASER_REQ = 1;
        private const int SCAN_RES_OUT = 2;
        private const int LASERSCAN_RES_OUT = 0;
        private const int LASERSCAN_BUSY_OUT = 7;
        private const int LASERSCAN_DUMPLICATE = 6;
        private const int LASER_BUSY_OUT = 1;
        IEventAggregator Events;
        private readonly FileSaverFactory _factory;
        SimpleTcpClient _laserClient = null;
        SimpleTcpClient _scanner = null;
        LaserRecordsManager LaserRecordsManager;
        FFTester.Tester ts = new FFTester.Tester();

        Settings settings = Settings.Default;
        private IIoService _ioService;
        public event EventHandler<Laser> LaserHandler;

        public LaserService([Inject] IEventAggregator @event, [Inject] FileSaverFactory factory)
        {
            Events = @event;
            _factory = factory;
            _ioService = new IoService("192.168.0.240");
            LaserRecordsManager = new LaserRecordsManager();

        }

        public int GetMarkingNo()
        {
         //   var ret = SaveToMes(DateTime.Now.ToString("yyyy:MM:dd HH:mm:ss"), DateTime.Now.ToString("yyyy:MM:dd HH:mm:ss"), settings.Station, settings.Shift, "1213123123123", settings.LineNo);
          //  Events.PostWarn("SHOPFLOW返回值" + ret.ToString());
            Events.PostMessage($"LASER SEND:FE");
            var m = _laserClient?.WriteLineAndGetReply("FE" + Environment.NewLine, TimeSpan.FromMilliseconds(1000));
            Events.PostMessage($"LASER RECV:{m?.MessageString}");
            //      SetLaserCode();
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
                _laserClient.Delimiter = 0X0D;
                _laserClient.Connect("192.168.0.239", 9004);
                _scanner?.Disconnect();
                _scanner = new SimpleTcpClient();
                _scanner.Delimiter = 0x0d;
                _scanner.Connect("192.168.0.2", 9004);

            }
            catch (Exception EX)
            {
                Events.PostError(EX);
                return;
            }


            Task.Factory.StartNew(async () =>
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
                    input = _ioService.GetInput(LASERSCAN_REQ);
                    if (input)
                    {
                        Events.PostInfo("开始扫码");
                        GetCode(0);
                        Events.PostInfo("等待扫码信号关闭");
                        SpinWait.SpinUntil(() => _ioService.GetInput(LASERSCAN_REQ) == false);
                        Events.PostInfo("扫码信号关闭");
                    }
                    await Task.Delay(10);
                }
            },TaskCreationOptions.LongRunning);
            Task.Factory.StartNew(async () =>
            {
                bool input;
                //   _ioService.SetOutput(0, false);

                while (true)
                {
                    input = _ioService.GetInput(STARTLASER_REQ);
                 
                    if (input)
                    {
                        Events.PostInfo("N1 开始镭射");
                        if (SetLaserCode() != 0)
                        {
                            Events.PostInfo("N1 设置镭射结果为 0");
                            _ioService.SetOutput(SCAN_RES_OUT, false);
                        }
                        else
                        {
                            Events.PostInfo("N1 设置镭射结果为 1");
                            _ioService.SetOutput(SCAN_RES_OUT, true);
                        }
                        Events.PostInfo("N1 等待镭射请求关闭");
                        SpinWait.SpinUntil(() => _ioService.GetInput(STARTLASER_REQ) == false);
                        Events.PostInfo("N1 镭射完成");
                    }
                    await Task.Delay(10);
                }
            },TaskCreationOptions.LongRunning);
        }

        public int SetLaserCode()
        {
            try
            {
                DAQ.wcl.FFTesterServiceClient serviceClient = new wcl.FFTesterServiceClient();
                string output = "", error = "";
                string[] code = new string[3];
                Events.PostInfo("N1 设置镭射完成信号 0");
                _ioService.SetOutput(LASER_BUSY_OUT, false);
                for (int i = 0; i < 1; i++)
                {
                    var resp = serviceClient.ExecuteGenericFunction("GetTransformerSN", "", settings.Station, settings.EmployeeNo, ref output, ref error);
                    if (resp.Id == 0)
                    {
                        Events.PostWarn("SFIS:" + output);
                        code[i] = output;
                    }
                    else
                    {
                        Events.PostError("SFIS:" + error);
                        return -1;
                    }
                }
                serviceClient.Close();
                //    string data = $"C2,0,0,{code[0]},1,{code[1]},2,{code[2]}";
                string data = $"C2,0,0,{code[0]}";
                Events.PostInfo("PC->LASER:" + data);
                var m = _laserClient.WriteLineAndGetReply(data, TimeSpan.FromSeconds(1));

                if (m != null)
                {
                    Events.PostInfo("LASER:" + m.MessageString);
                    if (!m.MessageString.Contains("C2,0"))
                    {
                        return -2;
                    }
                }
                else
                {
                    return -3;
                }
                data = "WX,StartMarking";
                Events.PostInfo("PC->LASER:" + data);
                m = _laserClient.WriteLineAndGetReply(data, TimeSpan.FromSeconds(2));
                if (m != null)
                {
                    Events.PostInfo("LASER:" + m.MessageString);
                    if (!m.MessageString.Contains("OK"))
                    {
                        return -4;
                    }
                }
                else
                {
                    return -5;
                }
                return 0;
            }
            catch (Exception E)
            {
                Events.PostError(E.Message + Environment.NewLine + E.StackTrace);
                return -9;
            }
            finally
            {
                Events.PostInfo("N1 设置镭射完成信号 1");
                _ioService.SetOutput(LASER_BUSY_OUT, true);
            }
        }

        public void GetCode(int index)
        {
            try
            {
                _ioService.SetOutput(LASERSCAN_RES_OUT, false);
                _ioService.SetOutput(LASERSCAN_DUMPLICATE, false);
                _ioService.SetOutput(LASERSCAN_BUSY_OUT, false);
                string cmd = $"LON";
                Events.PostMessage($"N1 SCANNER SEND:{cmd}");
                var m1 = _scanner.WriteLineAndGetReply(cmd, TimeSpan.FromMilliseconds(3000));
                Events.PostMessage($"N1 SCANNER RECV: {m1.MessageString}");
                if (m1 != null && !m1.MessageString.Contains("ERROR"))
                {
                    Events.PostMessage("N1 设置扫码结果信号为 1");
                    _ioService.SetOutput(LASERSCAN_RES_OUT, true);
                    var laser = new Laser
                    {
                        BobbinCode = m1.MessageString.Trim(),
                        BobbinLotNo = settings.BobbinLotNo,
                        LineNo = settings.LineNo,
                        Shift = settings.Shift,
                        CodeQuality = "NA",
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
                    _factory.GetFileSaver<Laser>((1).ToString()).Save(laser);
                    var qr = LaserRecordsManager.Find(laser.BobbinCode);
                    if (qr != null)
                    {
                        Events.PostWarn($"{qr.BobbinCode} {qr.DateTime} 镭射过了");
                        Events.PostMessage("N1 设置扫码结果信号为 0");
                        _ioService.SetOutput((uint)LASERSCAN_RES_OUT, false);
                        Events.PostMessage("N1 设置扫码重码信号为 1");
                        _ioService.SetOutput((uint)LASERSCAN_DUMPLICATE, true);
                    }
                    else
                    {
                        LaserRecordsManager.Insert(laserpoco);
                    }

                    var ret = SaveToMes(DateTime.Now.ToString("yyyy:MM:dd HH:mm:ss"), DateTime.Now.ToString("yyyy:MM:dd HH:mm:ss"), laser.Station, laser.Shift, laser.BobbinCode, laser.LineNo);
                    Events.PostWarn("N1 SHOPFLOW返回值" + ret.ToString());
                    if (ret != 0)
                    {
                        Events.PostMessage("N1 设置扫码结果信号为 0");
                        _ioService.SetOutput((uint)LASERSCAN_RES_OUT, false);
                    }
                    //  tester.SaveResult(

                }
                else
                {
                    Events.PostMessage("N1 设置扫码结果信号为 0");
                    _ioService.SetOutput(LASERSCAN_RES_OUT, false);
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
                _ioService.SetOutput(LASERSCAN_BUSY_OUT, true);
            }
        }

        private int SaveToMes(string starttime, string endtime, string tester, string shift, string id, string line)
        {
            var batch = new XElement("BATCH");
            batch.SetAttributeValue("TIMESTAMP", starttime);
            batch.SetAttributeValue("SYNTAX_REV", "");
            batch.SetAttributeValue("COMPATIBLE_REV", "");
            var factory = new XElement("FACTORY");
            factory.SetAttributeValue("NAME", "Flextronics DongGuan");
            factory.SetAttributeValue("LINE", line);
            factory.SetAttributeValue("TESTER", tester);
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
            panel.Add(dut);
            batch.Add(factory);
            batch.Add(product);
            batch.Add(refs);
            batch.Add(panel);

            DAQ.wcl.FFTesterServiceClient serviceClient = new wcl.FFTesterServiceClient();
            Events.PostMessage($"PC->SFIS:{id}");
            var res = serviceClient.GetUnitInfo(id, settings.Station, "admin", "");
            Events.PostMessage(string.Format("SFIS return value = {0} \n", res.ToJson()));
            if (res.Id != 0)
            {
                Events.PostError(string.Format("SFIS return value = {0} \n", res.Value));
            }
            else
            {
                Events.PostMessage("GetUnitInfo Sucess!");
            }
            serviceClient.Close();
            var ret = ts.SaveResult(batch.ToString());
            Events.PostInfo("pc->sfis:" + batch.ToString());
            Events.PostInfo("sfis->pc:"+ret.ToString());
            if (ret != 0)
            {
                Events.PostError(ts.GetErrMessage(ret));
                return -2;
            }
          //  serviceClient.SaveResult()

            
     //       Events.PostMessage(string.Format("SFIS return value = {0} \n", res.ToJson()));

            return 0;
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