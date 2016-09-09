using Newtonsoft.Json.Linq;
//using System.Runtime.Serialization.Json; turbolame

namespace StreamTrigger
{
    public class TwitchApi
    {
        private const string streamUrl = "https://api.twitch.tv/kraken/streams/"; // + streamname
        public static bool CheckStreamIsOnline(string streamName)
        {
            string streamJsonString;
            using (var wc = new System.Net.WebClient())
                streamJsonString = wc.DownloadString(streamUrl + streamName);
            JObject streamJsonParsed = JObject.Parse(streamJsonString); 
            return streamJsonParsed["stream"].Type != JTokenType.Null && streamJsonParsed["stream"].Type != JTokenType.None;
        }
    }
}
