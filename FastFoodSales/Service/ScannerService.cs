using SimpleTCP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Stylet;
using StyletIoC;
using System.Net.Sockets;
using Modbus.Device;
using Modbus.Utility;
using Modbus.Data;

namespace DAQ.Service
{
    public class ModbusService : IPLCService,IDisposable
    {
        [Inject]
        IEventAggregator Events;
        TcpListener _listener=null;
        ModbusSlave slave = null;
        public ModbusService()
        {
            Initialize();
        }
       
        public void Initialize()
        {
            _listener?.Stop();
            slave?.Dispose();
            _listener = new TcpListener(IPAddress.Any, 502);
            slave = ModbusTcpSlave.CreateTcp(1, _listener);
            slave.DataStore = DataStoreFactory.CreateDefaultDataStore();
            slave.Listen();
        }

        public bool? ReadBool(int index, int bit)
        {
            return (slave?.DataStore.HoldingRegisters[index] & (1 << bit)) > 0;
        }
        public bool Write(int index, bool bit)
        {
            if (slave == null)
                return false;
            if (bit)
                slave.DataStore.HoldingRegisters[index] |= (ushort)(1 << index);
            else
                slave.DataStore.HoldingRegisters[index] &= ((ushort)~(1 << index));
            return true;
        }

        public bool Write(int index, string str)
        {
            if (slave != null)
                return false;
            var bs = Encoding.ASCII.GetBytes(str);
            var mod2 = bs.Length % 2;
            for (int i = 0; i < bs.Length / 2; i++)
            {
                slave.DataStore.HoldingRegisters[index] = (ushort)((bs[2 * i] << 8) & 0xff00 + bs[2 * i + 1]);
            }
            if (mod2 > 0)
                slave.DataStore.HoldingRegisters[index] = (ushort)(slave.DataStore.HoldingRegisters[index] & 0xff00 + bs[bs.Length / 2 + 1]);
            return true;
        }
        public string ReadString(int index, int len)
        {
            ushort[] arr = new ushort[index / 2 + index % 2];
            var r = slave?.DataStore.HoldingRegisters.Skip(index).Take(arr.Length).ToArray();
            if (r == null)
                return null;
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = (ushort)(((r[i] & 0xff00) >> 8) & 0xff + ((r[i] & 0xff) << 8) & 0xff00);
            }
            List<byte> bytes = new List<byte>();
            foreach (var a in arr)
            {
                bytes.AddRange(BitConverter.GetBytes(a));
            }
            return Encoding.ASCII.GetString(bytes.ToArray());
        }

        public bool Write(int index, ushort value)
        {
            if (slave == null)
                return false;
            slave.DataStore.HoldingRegisters[index] = value;
            return true;
        }

        public ushort? ReadUInt16(int index)
        {
            return slave?.DataStore.HoldingRegisters[index];
        }


      
        void IDisposable.Dispose()
        {
            _listener?.Stop();
            slave?.Dispose();
            GC.SuppressFinalize(this);
        }

        public int? ReadInt(int Index)
        {
           var v= slave?.DataStore.HoldingRegisters.Skip(Index).Take(2);
            if(v.Count()==2)
            {
                var vs = SwapEndian(v.ToArray());
                return vs[1]*65536 + (vs[0]);
            }
            return null;
                
        }

       public ushort[] SwapEndian(ushort[] vs)
        {
            if (vs == null)
                return null;
            ushort[] v = new ushort[vs.Count()];
            for(int i=0;i<v.Length;i++)
            {
                v[i] =(ushort) (((vs[i] & 0xff) << 8 )+ ((vs[i] & 0xff00) >> 8) );
            }
            return v;
        }
  
        public bool Write(int index, int value)
        {
            if (slave == null)
                return false;
             var bs=BitConverter.GetBytes(value);
            slave.DataStore.HoldingRegisters[index] = (ushort)(bs[0]*256+bs[1]);
            slave.DataStore.HoldingRegisters[index+1] = (ushort)(bs[3] * 256 + bs[4]);
            return true;
        }
    }

    public class ScannerService : IDisposable
    {
        [Inject]
        IEventAggregator Events;
        SimpleTcpServer _server = null;
      
        public ScannerService([Inject] IEventAggregator @event)
        {
            Events = @event;
            CreateServer();      
        }

    
        public void CreateServer()
        {

            _server?.Stop();
            _server = new SimpleTcpServer();
            _server.Start(9004, AddressFamily.InterNetwork);

            Events.Publish(new MsgItem { Level = "D", Time = DateTime.Now, Value ="Server initialize: "+IPAddress.Any.ToString()+":9004"} );
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
                var addr = ((IPEndPoint)e.TcpClient.Client.RemoteEndPoint).Address.GetAddressBytes()[3];
                var str = e.MessageString.Trim('\r', '\n');
                Events.Publish(str,addr.ToString(),addr.ToString());
                Events.Publish(new MsgItem { Level = "D", Time = DateTime.Now, Value = ((IPEndPoint)e.TcpClient.Client.RemoteEndPoint).Address.ToString()+":"+ str });
            }
            catch (Exception ex)
            {
                Events.Publish(new MsgItem { Level = "E", Time = DateTime.Now, Value = ex.Message });
            }
        }

    }
}