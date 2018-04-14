using Newtonsoft.Json.Linq;
//using System.Runtime.Serialization.Json; turbolame

namespace StreamTrigger
{
    /// <summary>
    /// Provides methods for interacting with twitch api
    /// </summary>
    public class TwitchApi
    {
        // beware: Twitch has changed the behavior of responses on 'stable' api versions in the past. woohoo!
        // ##LASTUPDATED 04142018 
        private readonly string _apiRoot = "https://api.twitch.tv/helix/";
        private readonly string _clientId;

        public TwitchApi(string clientId)
        {
            _clientId = clientId;
        }
        
        
        public bool CheckStreamIsOnline(string login)
        {
            // thankfully, twitch added a way to use user login id directly, instead of having to lookup id first
            var getStreamInfoUrl = $"{ _apiRoot}streams?user_login={login}";

            using (var wc = new System.Net.WebClient()) {
                wc.Headers.Add($"Client-ID: {_clientId}");
                var streamJsonString = wc.DownloadString(getStreamInfoUrl);
                // parse this sync, its a small response so doesnt take much time
                return ParseStreamResponseOnline(streamJsonString);
            }
        }

        public bool ParseStreamResponseOnline(string jsonPayload)
        {
            /*
             * // live
                {
                    "data": [
                        {
                            "id": "28327815808",
                            "user_id": "28379957",
                            "game_id": "14304",
                            "community_ids": [
                                "6e940c4a-c42f-47d2-af83-0a2c7e47c421",
                                "cd5c6356-f77d-46e7-918a-17ad16b2f967",
                                "ff1e77af-551d-4993-945c-f8ceaa2a2829"
                            ],
                            "type": "live",
                            "title": "Live - 20+ HRS - MAN VS STREAM HELL DRUID SPEEDRUN",
                            "viewer_count": 990,
                            "started_at": "2018-04-14T13:23:18Z",
                            "language": "en",
                            "thumbnail_url": "https://static-cdn.jtvnw.net/previews-ttv/live_user_mrllamasc-{width}x{height}.jpg"
                        }
                    ],
                    "pagination": {
                        "cursor": "eyJiIjpudWxsLCJhIjp7Ik9mZnNldCI6MX19"
                    }
                }


                // not live

                {
                    "data": [],
                    "pagination": {}
                } */
            JObject streamJsonParsed = JObject.Parse(jsonPayload);
            //JArray rootArray = streamJsonParsed["data"].
            if (streamJsonParsed["data"].Type != JTokenType.Array)
                return false;


            return true;
        }
    }
}
