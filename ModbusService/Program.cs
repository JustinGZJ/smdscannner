using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Modbus.Data;
using Modbus.Device;
using Topshelf;

namespace ModbusService
{

    public class ModbusService : IDisposable
    {
        TcpListener _listener = null;
        ModbusSlave _slave = null;


        public ModbusService()
        {

        }
        public void Start()
        {
            _listener?.Stop();
            _slave?.Dispose();
            _listener = new TcpListener(IPAddress.Any, 502);
            _slave = ModbusTcpSlave.CreateTcp(1, _listener);
            _slave.DataStore = DataStoreFactory.CreateDefaultDataStore();
            _slave.ListenAsync();
        }
        public void Stop()
        {
            _slave?.Dispose();
            _listener?.Stop();
            _listener = null;
        }

        public void Dispose()
        {
            _slave?.Dispose();
            _listener?.Stop();
            _listener = null;
        }
    }

    public class Program
    {
        public static void Main()
        {
            HostFactory.Run(x =>                                 //1
            {
                x.Service<ModbusService>(s =>                        //2
                {
                    s.ConstructUsing(name => new ModbusService());     //3
                    s.WhenStarted(tc => tc.Start());              //4
                    s.WhenStopped(tc => tc.Stop());               //5
                });
                x.RunAsLocalSystem();                            //6

                x.SetDescription("Modbus tcp Service，localhost:502");        //7
                x.SetDisplayName("ModbusTcp");                       //8
                x.SetServiceName("ModbusTcp");                       //9
            });                                                  //10
        }
    }
}
