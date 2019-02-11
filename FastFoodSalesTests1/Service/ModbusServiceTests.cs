using Microsoft.VisualStudio.TestTools.UnitTesting;
using DAQ.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAQ.Service.Tests
{
    [TestClass()]
    public class ModbusServiceTests
    {
        [TestMethod()]
        public void SwapEndianTest()
        {
            ModbusService service = new ModbusService()
            ;
         var rt  = service.SwapEndian(new ushort[] { 0x1100, 0x0011 });
            Assert.Equals(rt[0], 0x0011);
            

         
        }
    }
}