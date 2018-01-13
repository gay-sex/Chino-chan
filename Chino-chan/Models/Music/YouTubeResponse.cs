using Newtonsoft.Json;

namespace Chino_chan.Models.Music
{
    public struct YouTubeResponse
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("thumbnail_url")]
        public string Thumbnail { get; set; }

        [JsonProperty("author_name")]
        public string Author { get; set; }

        [JsonProperty("author_url")]
        public string AuthorUrl { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }

}