using Microsoft.VisualStudio.TestTools.UnitTesting;
using StreamTrigger;

namespace StreamTriggerTests
{
    [TestClass]
    public class TwitchApiTests
    {
        private TwitchApi _testApi;
        private const string TestClientId = "rqm7hvz8t4im4isn53zogkmi1pvzd5p";

        [TestInitialize]
        public void TestInit()
        {
            _testApi = new TwitchApi(TestClientId);
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