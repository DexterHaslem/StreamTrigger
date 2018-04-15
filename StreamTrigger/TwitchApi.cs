using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
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


        /// <summary>
        /// Asks twitch for the current stream info for a given username
        /// </summary>
        /// <param name="login">stream name, the user login</param>
        /// <returns><see cref="StreamResponseData">data</see> for current stream or null if not live</returns>
        public StreamResponseData GetCurrentStreamInfo(string login)
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

        public StreamResponseData ParseStreamResponseOnline(string jsonPayload)
        {
            StreamsResponse resp = JsonConvert.DeserializeObject<StreamsResponse>(jsonPayload);
            if (resp.Data == null || resp.Data.Length < 1)
                return null;

            // since we specify a user id we should really
            // only get one thing back. at any rate
            // always use first response
            return resp.Data[0];
        }
    }

    public class StreamResponseData
    {
        public string Id { get; set; }
        [JsonProperty(PropertyName = "user_id")]
        public string UserId { get; set; }
        [JsonProperty(PropertyName = "game_id")]
        public string GameId { get; set; }
        [JsonProperty(PropertyName = "community_ids")]
        public Guid[] CommunityIds { get; set; } // TODO: GUIDS?
        public string Type { get; set; }
        public string Title { get; set; }
        [JsonProperty(PropertyName = "viewer_count")]
        public int ViewerCount { get; set; }
        [JsonProperty(PropertyName = "started_at")]
        public DateTime StartedAt { get; set; }
    }

    public class StreamsResponse
    {
        public StreamResponseData[] Data { get; set; }
        // dont care about pagination
    }
}
