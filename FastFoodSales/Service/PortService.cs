using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAQ.Pages;
using Stylet;
using StyletIoC;


namespace DAQ.Service
{
    public class PortAService : PortService, IHandle<EventIO>
    {

        public PortAService(PlcService plc, IEventAggregator events) : base(plc, events)
        {
            InstName = "RM3545";
            for (int i = 0; i < 4; i++)
            {
                TestSpecs[i].Name = "RESISTANCE " + i.ToString();
            }
        }
        public override string PortName => Properties.Settings.Default.Shift;


        public override void UpdateDatas()
        {
            for (int i = 0; i < 4; i++)
            {
                if (Plc.IsConnected)
                {
                    TestSpecs[i].Lower = Plc.GetGroupValue(i, 0);
                    TestSpecs[i].Upper = Plc.GetGroupValue(i, 1);
                    TestSpecs[i].Value = Plc.GetGroupValue(i, 2);
                    TestSpecs[i].Result = Plc.GetGroupValue(i, 3);
                }
            }
        }

        public override void Handle(EventIO message)
        {
            if (message.Value)
            {
                switch (message.Index)
                {
                    case (int)IO_DEF.READ_RES:
                        Read();
                        Plc.Pulse((int)IO_DEF.READ_RES + 8);
                        break;
                }
            }
        }

        public override void Read()
        {
            base.Read();
            if (Request("SCAN:DATA?", out string reply))
            {
                Events.Publish(new MsgItem
                {
                    Level = "D",
                    Time = DateTime.Now,
                    Value = "RM3545:" + reply
                });
                var values = reply.Split(',');
                if (values.Length > 1)
                {
                    for (int i = 0; i < values.Length; i++)
                    {
                        var a = values[i];
                        if (float.TryParse(a, out float v))
                        {
                            Plc.WriteGroupValue(i, 2, v);
                        }
                        else
                        {
                            Events.Publish(new MsgItem
                            {
                                Level = "E",
                                Time = DateTime.Now,
                                Value = "Resistance value parse fail"
                            });
                        }
                    }
                }
            }

        }

    }
    public class PortBService : PortService
    {
        public override string PortName => Properties.Settings.Default.ShiftName;
        public PortBService(PlcService plc, IEventAggregator @event) : base(plc, @event)
        {
            InstName = "TH";
            for (int i = 0; i < 4; i++)
            {
                TestSpecs[i].Name = "HI-POT " + i.ToString();
            }
        }

        public override void Handle(EventIO message)
        {
            switch (message.Index)
            {
                case (int)IO_DEF.READ_HIP:

                    break;

            }
        }
        public override bool Connect()
        {
            return base.Connect();
        }


        public override void UpdateDatas()
        {
            for (int i = 0; i < 4; i++)
            {
                if (Plc.IsConnected)
                {
                    TestSpecs[i].Lower = Plc.GetGroupValue(i + 4, 0);
                    TestSpecs[i].Upper = Plc.GetGroupValue(i + 4, 1);
                    TestSpecs[i].Value = Plc.GetGroupValue(i + 4, 2);
                    TestSpecs[i].Result = Plc.GetGroupValue(i + 4, 3);
                }
            }
        }
    }
    public class PortService : PropertyChangedBase, IPortService, IHandle<EventIO>
    {
        protected SerialPort port = new SerialPort();
        public virtual string PortName { get; }
        public bool IsConnected { get; set; }
        protected IEventAggregator Events { get; set; }
        protected PlcService Plc { get; set; }

        protected string InstName { get; set; }

        public BindableCollection<TestSpecViewModel> TestSpecs { get; set; }


        public PortService(PlcService plc, IEventAggregator events)
        {
            Events = events;
            Plc = plc;
            Events.Subscribe(this);
            TestSpecs = new BindableCollection<TestSpecViewModel>();
            for (int i = 0; i < 4; i++)
            {
                TestSpecs.Add(new TestSpecViewModel(
                    ));
            }
            Events.Subscribe(this);
            Plc.BindWeak(x => x.Datas, (s, e) =>
            {
                UpdateDatas();
            });
        }

        virtual public void UpdateDatas()
        {
        }

        public virtual bool Connect()
        {
            if (port.IsOpen)
                port.Close();
            try
            {
                port.PortName = PortName;
                port.Open();
                port.WriteLine("*IDN?");
                port.ReadTimeout = 1000;
                string v = port.ReadLine();
                if (v.Length > 0)
                {
                    if (!string.IsNullOrEmpty(InstName))
                    {
                        IsConnected = v.Contains(InstName);
                    }
                    else
                        IsConnected = true;
                    return IsConnected;
                }
                else
                {
                    port.Close();
                    return false;
                }
            }
            catch (Exception EX)
            {

                port.Close();
                return false;
            }
        }

        public bool Request(string cmd, out string reply)
        {
            Events.Publish(new MsgItem
            {
                Level = "D",
                Time = DateTime.Now,
                Value = $"{PortName}\t{cmd}{Environment.NewLine}"
            });
            if (IsConnected)
            {
                port.WriteLine(cmd);
                try
                {
                    reply = port.ReadLine();
                    Events.Publish(new MsgItem
                    {
                        Level = "D",
                        Time = DateTime.Now,
                        Value = $"{PortName}\t{reply}{Environment.NewLine}"
                    });
                    return true;
                }
                catch (Exception ex)
                {
                    reply = ex.Message;
                    Events.Publish(new MsgItem
                    {
                        Level = "E",
                        Time = DateTime.Now,
                        Value = $"{PortName}\t{reply}{Environment.NewLine}"
                    });
                    return false;
                }
            }
            else
            {
                Events.Publish(new MsgItem
                {
                    Level = "E",
                    Time = DateTime.Now,
                    Value = $"{PortName}\t{" port is not connected"}{Environment.NewLine}"
                });
                reply = "port is not connected";
                return false;
            }
        }

        public void DisConnect()
        {
            IsConnected = false;
            port.Close();
        }

        public virtual void Handle(EventIO message)
        {
        }
        public virtual void Read()
        {
            if (!IsConnected)
            {
                Connect();
            }
        }
    }
    public enum IO_DEF
    {
        READ_RES = 0,
        READ_HIP = 1
    }
}
