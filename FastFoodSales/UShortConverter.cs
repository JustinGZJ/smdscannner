using System;
using System.Collections.Generic;

namespace DAQ
{
    public class UShortConverter
    {
        public static byte[] GetBytes(ushort[] ushorts)
        {
            List<byte> bytes = new List<byte>();
            foreach (var vUshort in ushorts)
            {
              bytes.AddRange(BitConverter.GetBytes(vUshort));  ;
            }
            return bytes.ToArray();
        }

        public static ushort[] GetUshorts(byte[] bytes)
        {
            List<ushort> ushorts = new List<ushort>();
            if (bytes?.Length>1)
            {
                for (int i = 0; i < bytes.Length; i =i+2)
                {
                   ushorts.Add(BitConverter.ToUInt16(bytes, i)); ;
                }       
            }
            return ushorts.ToArray();
        }
    }
}