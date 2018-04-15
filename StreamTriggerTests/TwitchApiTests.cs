using Microsoft.VisualStudio.TestTools.UnitTesting;
using StreamTrigger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamTrigger.Tests
{
    [TestClass()]
    public class TwitchApiTests
    {
        private TwitchApi _testApi;
        private readonly string _testClientId = "rqm7hvz8t4im4isn53zogkmi1pvzd5p";

        [TestInitialize]
        public void TestInit()
        {
            _testApi = new TwitchApi(_testClientId);
        }

        [TestMethod]
        public void ParseStreamResponseOfflineTest()
        {
            var offlinePayload = System.IO.File.ReadAllText("offline.json");
            var offlinePayloadResult = _testApi.ParseStreamResponseOnline(offlinePayload);
            Assert.IsNull(offlinePayloadResult);
        }

        [TestMethod]
        public void ParseStreamResponseOnlineTest()
        {
            var onlinePayload = System.IO.File.ReadAllText("online.json");
            var onlinePayloadResult = _testApi.ParseStreamResponseOnline(onlinePayload);
            Assert.IsNotNull(onlinePayloadResult);
        }
    }
}