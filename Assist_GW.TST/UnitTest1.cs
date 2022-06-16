using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace AssistCargoRC_GW.TST
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestSendRequest()
        {
            var parms = new Dictionary<string, string>();

            parms.Add("iron:altitude", "0");
            parms.Add("iron:asset", "0");
            parms.Add("iron:battery", "0");
            parms.Add("iron:code", "0");
            parms.Add("iron:course", "0");
            parms.Add("iron:date", "2022-03-04T00:00:00");
            parms.Add("iron:direction", "Santa Ines #34");
            parms.Add("iron:ignition", "TRUE");
            parms.Add("iron:latitude", "19.710250522625174");
            parms.Add("iron:longitude", "-99.067695531216");
            parms.Add("iron:odometer", "0");
            parms.Add("iron:serialNumber", "43535353");
            parms.Add("iron:shipment", "0");
            parms.Add("iron:speed", "0");

            string result = BLL.DataOut.SendRequest(parms, "http://tempuri.org/IRCService/GPSAssetTracking", "Event", false);
            Assert.AreEqual("Algo", result);
        }
    }
}
