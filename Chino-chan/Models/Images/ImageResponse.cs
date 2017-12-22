using Newtonsoft.Json;

namespace Chino_chan.Models.Images
{
    public class GelbooruResponse
    {
        [JsonProperty("tags")]
        public string Tags { get; set; }
        [JsonProperty("file_url")]
        public string Url { get; set; }
        [JsonProperty("rating")]
        public string Rating { get; set; }
    }
    public class DanbooruResponse
    {
        [JsonProperty("tag_string")]
        public string Tags { get; set; }
        [JsonProperty("large_file_url")]
        public string Uri { get; set; }
        [JsonIgnore]
        public string Url
        {
            get
            {
                return "https://danbooru.donmai.us" + Uri;
            }
        }
        [JsonProperty("rating")]
        public string Rating { get; set; }
    }
    public class YandereResponse
    {
        [JsonProperty("tags")]
        public string Tags { get; set; }
        [JsonProperty("file_url")]
        public string Url { get; set; }
        [JsonProperty("rating")]
        public string Rating { get; set; }
    }
    public class SankakuResponse
    {
        public string Tags { get; set; }
        public string Url { get; set; }
        public string Rating { get; set; }
    }
}
