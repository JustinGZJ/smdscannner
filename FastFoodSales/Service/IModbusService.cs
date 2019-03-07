using System;
using StyletIoC;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using Stylet;

namespace DAQ.Service
{
    public interface IPLCService
    {
        void Initialize();
        bool? ReadBool(int index, int bit);
        string ReadString(int index, int len);
        ushort? ReadUInt16(int index);
        int? ReadInt(int Index);
        bool Write(int index, int bit,bool value);
        bool Write(int index, string str);
        bool Write(int index, ushort value);
        bool Write(int index, int value);
    }
    public class VAR : PropertyChangedBase
    {
        public string Name { get; set; }
        public DateTime Time { get; set; }
        public object Value { get; set; }
        public TYPE Type { get; set; }
        public int StartIndex { get; set; }
        public int Tag { get; set; }
        public string Index
        {
            get
            {
                if (this.Type == TYPE.BOOL)
                {
                    return $"{StartIndex}:{Tag}";
                }
                else if (this.Type == TYPE.STRING)
                {
                    return $"{StartIndex}^{Tag}";
                }
                else
                    return StartIndex.ToString();
            }
        }

        public int GetLength()
        {
            if (Type == TYPE.FLOAT ||Type == TYPE.INT)
            {
                return 2;
            }
            else if (Type == TYPE.STRING)
            {
                return (Tag/ 2 + Tag % 2);
            }
            else
            {
                return 1;
            }
        }
    }
    public enum TYPE
    {
        BOOL, STRING, SHORT, INT, FLOAT
    }
}