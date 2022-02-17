using System.Threading.Tasks;
using HslCommunication.ModBus;
using System.Linq;
using System.Threading;

namespace DAQ.Service
{
    public class IoService : IIoService
    {
        private ModbusTcpNet modbusTcp;
        private bool[] _inputs = new bool[24];
        private bool[] _outputs = new bool[8] { false,false,false,false,false,false,false,false};
        public bool IsConnected { get; private set; }

        public IoService()
        {
            
            modbusTcp = new ModbusTcpNet { IpAddress = "192.168.0.240", Port = 502};
           var con= modbusTcp.ConnectServer();
            if (con.IsSuccess)
            {
                Task.Run(() =>
                {
                    while (true)
                    {
                        var r1 = modbusTcp.ReadCoil("0", 24);
                        IsConnected = r1.IsSuccess;
                        if (r1.IsSuccess)
                        {
                            for (var i = 0; i < 24; i++)
                            {
                                _inputs[i] = r1.Content[i];
                            }
                            var r2 = modbusTcp.WriteCoil("16", _outputs);
                        }
                    }
                });
            }
            else
            {
            //    throw new System.Exception(con.ToMessageShowString()) ;
            }
  
        }

        public bool GetInput(uint index)
        {
            return index < 24 && _inputs[index];
        }

        public void SetOutput(uint index, bool value)
        {
            if (index <= (_outputs.Length - 1))
                _outputs[index] = value;
        }
    }
}