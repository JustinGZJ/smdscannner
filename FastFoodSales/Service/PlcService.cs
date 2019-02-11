using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HslCommunication;
using HslCommunication.Profinet.Omron;
using Stylet;
using StyletIoC;

namespace DAQ.Service
{

    public class EventIO
    {
        public int Index { get; set; }
        public bool Value { get; set; }
    }

    public class PlcService : PropertyChangedBase
    {
        OmronFinsNet omr;
        string addr = "D100";
        public bool IsConnected { get; set; }
        public BindableCollection<short> Datas { get; set; } = new BindableCollection<short>(new short[100]);
        public BindableCollection<bool> Bits { get; set; } = new BindableCollection<bool>(new bool[16]);
        public BindableCollection<string> BitTags { get; set; } = new BindableCollection<string>(new string[16]);

        public IEventAggregator Events { get; set; }

        public bool Connect()
        {
            if (omr != null)
            {
                omr.ConnectClose();
                omr = null;
            }
            omr = new OmronFinsNet(Properties.Settings.Default.PLC_IP, Properties.Settings.Default.PLC_PORT)
            {
                DA1 = 0
            };
            var op = omr.ConnectServer();
            if (op.IsSuccess)
            {
                Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        var rop = omr.ReadInt16(addr, 100);
                        IsConnected = rop.IsSuccess;
                        if (IsConnected)
                        {
                            if (Datas[0] != rop.Content[0])
                            {
                                for (int i = 0; i < 16; i++)
                                {
                                    bool v = (Datas[0] & (1 << i)) > 0;
                                    if (Bits[i] != v)
                                    {
                                        Bits[i] = v;
                                        Events.Publish(new EventIO
                                        {
                                            Index = i,
                                            Value = v
                                        });
                                        Events.Publish(new MsgItem
                                        {
                                            Level = "D",
                                            Time = DateTime.Now,
                                            Value = $"Bit[{i}]:" + (v ? "Rising edge" : "Failing edge")
                                        });
                                    }
                                }
                            }

                            for (int i = 0; i < 100; i++)
                            {
                                if (Datas[i] != rop.Content[i])
                                {
                                    Datas[i] = rop.Content[i];

                                    Events.Publish(new MsgItem
                                    {
                                        Level = "D",
                                        Time = DateTime.Now,
                                        Value = $"DATA[{i}]\t{Convert.ToString(Datas[i], 16)}"
                                    });
                                }
                            }
                        }
                        System.Threading.Thread.Sleep(10);
                    }
                });
            }
            IsConnected = op.IsSuccess;
            return op.IsSuccess;
        }

        public bool WriteBool(int index, bool value)
        {
            var op = omr.Write($"{addr}.{index}", value);
            return op.IsSuccess;
        }

        public bool WriteValue(int index, ushort value)
        {
            var r = int.TryParse(addr.Trim().Substring(1), result: out int v);
            if (r)
            {
                return omr.Write($"D{v + index}", value).IsSuccess;
            }
            return false;
        }
        public void Pulse(int bitIndex, int Delayms = 100)
        {
            Task.Factory.StartNew(() =>
            {
                WriteBool(bitIndex, true);
                System.Threading.Thread.Sleep(Delayms);
                WriteBool(bitIndex, true);
            });
        }

        public bool WriteValue(int index, float value)
        {
            var r = int.TryParse(addr.Trim().Substring(1), result: out int v);
            if (r)
            {
                return omr.Write($"D{v + index}", value).IsSuccess;
            }
            return false;
        }
        public float GetFloat(int byteindex)
        {
            var bytes = new byte[4];
            if (byteindex > Datas.Count - 1)
                throw new Exception("get float value out of index");
            bytes[0] = BitConverter.GetBytes(Datas[byteindex + 1])[0];
            bytes[1] = BitConverter.GetBytes(Datas[byteindex + 1])[1];
            bytes[2] = BitConverter.GetBytes(Datas[byteindex])[0];
            bytes[3] = BitConverter.GetBytes(Datas[byteindex])[1];
            var single = BitConverter.ToSingle(bytes, 0);
            return single;
        }
        public float GetGroupValue(int group, int subidx)
        {
            return GetFloat(group * 4 * 2 + subidx + 1);
        }
        public bool WriteGroupValue(int group, int subidx, float value)
        {
            return WriteValue(group * 4 * 2 + subidx + 1, value);
        }
    }
}
