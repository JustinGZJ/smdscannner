using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using HslCommunication.Core;
using Modbus.Data;
using Modbus.Device;
using Stylet;
using StyletIoC;

namespace DAQ.Service
{
    public class ModbusService : IPLCService, IDisposable
    {
        TcpListener _listener = null;
        ModbusSlave _slave = null;
        [Inject] private IByteTransform Transformer { get; set; }

        public ModbusService()
        {
            Initialize();
        }

        public void Initialize()
        {
            _listener?.Stop();
            _slave?.Dispose();
            _listener = new TcpListener(IPAddress.Any, 502);
            _slave = ModbusTcpSlave.CreateTcp(1, _listener);
            _slave.DataStore = DataStoreFactory.CreateDefaultDataStore();
            _slave.ListenAsync();
        }

        public bool? ReadBool(int index, int bit)
        {
            if (_slave != null)
            {
                ushort s;
                lock (_slave.DataStore.SyncRoot)
                {
                    s = _slave.DataStore.HoldingRegisters[index];
                }
                var bytes = Transformer.TransByte(s);
                return Transformer.TransBool(bytes, 0, 2)[bit];
            }
            return null;
        }
        public bool Write(int index, int bit, bool value)
        {

            if (_slave == null)
                return false;
            var mark = (ushort)((1 << index) & ushort.MaxValue);
            var bytes = Transformer.TransByte(mark);
            var markT = BitConverter.ToUInt16(bytes, 0);
            lock (_slave.DataStore.SyncRoot)
            {
                if (value)
                    _slave.DataStore.HoldingRegisters[index] |= markT;
                else
                    _slave.DataStore.HoldingRegisters[index] &= (ushort)~markT;
            }
            return true;
        }

        public bool Write(int index, string str)
        {
            if (_slave == null)
                return false;
            var bs = Encoding.UTF8.GetBytes(str);
            var shorts = Transformer.TransUInt16(bs, 0, bs.Length / 2 + bs.Length % 2);
            lock (_slave.DataStore.SyncRoot)
            {
                for (int i = 0; i < shorts.Length; i++)
                {
                    _slave.DataStore.HoldingRegisters[i + index] = shorts[i];
                }
            }
            return true;
        }
        public string ReadString(int index, int len)
        {
            ushort[] arr = new ushort[index / 2 + index % 2];
            if (_slave == null)
                return null;
            ushort[] r;
            lock (_slave.DataStore.SyncRoot)
            {
                r = _slave.DataStore.HoldingRegisters.Skip(index).Take(arr.Length).ToArray();
            }
            var bytes = Transformer.TransByte(r);
            return Encoding.UTF8.GetString(bytes.ToArray());
        }

        public bool Write(int index, ushort value)
        {
            if (_slave == null)
                return false;
            var v = Transformer.TransUInt16(BitConverter.GetBytes(value), 0);
            lock (_slave.DataStore.SyncRoot)
            {
                _slave.DataStore.HoldingRegisters[index] = v;
            }
            return true;
        }

        public ushort? ReadUInt16(int index)
        {
            if (_slave == null)
                return null;
            lock (_slave.DataStore.SyncRoot)
            {
                var v = _slave.DataStore.HoldingRegisters[index];
                return Transformer.TransUInt16(BitConverter.GetBytes(v), 0);
            }
        }



        void IDisposable.Dispose()
        {
            _listener?.Stop();
            _slave?.Dispose();
            GC.SuppressFinalize(this);
        }

        public int? ReadInt(int Index)
        {
            if (_slave != null)
            {
                ushort[] v = null;
                lock (_slave.DataStore.SyncRoot)
                {
                    v = _slave.DataStore.HoldingRegisters.Skip(Index).Take(2).ToArray();
                }
                if (v?.Length != 2) return null;
                var bs = Transformer.TransByte(v.ToArray());
                return BitConverter.ToInt32(bs, 0);
            }
            else
            {
                return null;
            }
        }

        public bool Write(int index, int value)
        {
            if (_slave == null)
                return false;
            var bs = BitConverter.GetBytes(value);
            var v = Transformer.TransInt32(bs, 0);
            lock (_slave.DataStore.SyncRoot)
            {
                _slave.DataStore.HoldingRegisters[index] = (ushort)(v & ushort.MaxValue);
                _slave.DataStore.HoldingRegisters[index + 1] = (ushort)((v >> 16) & ushort.MaxValue);
                return true;
            }
        }
    }
}