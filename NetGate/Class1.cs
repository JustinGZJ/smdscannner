using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace NetGate.Address
{
    interface IAddressTranslate
    {
        /// <summary>
        /// 转换成通用地址
        /// </summary>
        /// <param name="Address"></param>
        /// <returns></returns>
        DeviceAddress Translate(string Address);
        /// <summary>
        /// 通用地址转换成PLC地址
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        string TranslateBack(DeviceAddress address);
    }
    public enum DataType : byte
    {
        NONE = 0,
        BOOL = 1,
        BYTE = 3,
        SHORT = 4,
        WORD = 5,
        DWORD = 6,
        INT = 7,
        FLOAT = 8,
        SYS = 9,
        STR = 11
    }
    [Flags]
    public enum ByteOrder : byte
    {
        None = 0,
        BigEndian = 1,
        LittleEndian = 2,
        Network = 4,
        Host = 8
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct DeviceAddress : IComparable<DeviceAddress>
    {
        public int Area;
        public int Start;
        public ushort DBNumber;
        public ushort DataSize;
        public ushort CacheIndex;
        public byte Bit;
        public DataType VarType;
        public ByteOrder ByteOrder;

        public DeviceAddress(int area, ushort dbnumber, ushort cIndex, int start, ushort size, byte bit, DataType type, ByteOrder order = ByteOrder.None)
        {
            Area = area;
            DBNumber = dbnumber;
            CacheIndex = cIndex;
            Start = start;
            DataSize = size;
            Bit = bit;
            VarType = type;
            ByteOrder = order;
        }

        public static readonly DeviceAddress Empty = new DeviceAddress(0, 0, 0, 0, 0, 0, DataType.NONE);

        public int CompareTo(DeviceAddress other)
        {
            return this.Area > other.Area ? 1 :
                this.Area < other.Area ? -1 :
                this.DBNumber > other.DBNumber ? 1 :
                this.DBNumber < other.DBNumber ? -1 :
                this.Start > other.Start ? 1 :
                this.Start < other.Start ? -1 :
                this.Bit > other.Bit ? 1 :
                this.Bit < other.Bit ? -1 : 0;
        }
    }
}
