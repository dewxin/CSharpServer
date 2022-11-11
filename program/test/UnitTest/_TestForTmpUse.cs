using Microsoft.VisualStudio.TestTools.UnitTesting;
using Protocol.Param;
using SqlDataCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest
{
    [TestClass]
    public class _TestForTmpUse
    {
        [TestMethod]
        public void TestMethod1()
        {
            var ip = IPAddress.Parse("192.168.1.1");

            IPEndPoint iPEndPoint = new IPEndPoint(ip, 23);

            var addr = iPEndPoint.Address.ToString();
            Console.WriteLine(addr);

        }
    }
}
