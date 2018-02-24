using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Chino_chan.Modules
{
    public static class Uploader
    {
        public static string Upload(string File)
        {
            string Endpoint = "https://anonfile.com/api/upload";

            FileStream Stream = new FileStream(File, FileMode.Open);
            string Name = Path.GetFileName(File);

            HttpClient Client = new HttpClient()
            {
                Timeout = TimeSpan.FromHours(1)
            };

            StreamContent StreamContent = new StreamContent(Stream);
            StreamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "file",
                FileName = Name
            };
            StreamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            MultipartFormDataContent Content = new MultipartFormDataContent { StreamContent };
            HttpResponseMessage Message = Client.PostAsync(Endpoint, Content).Result;
            string Json = Message.Content.ReadAsStringAsync().Result;
            
            AnonResponse Anon = JsonConvert.DeserializeObject<AnonResponse>(Json);
            return Anon.Data.File.Url.Short;
        }
    }
    
    public struct AnonResponse
    {
        [JsonProperty("status")]
        public bool Status { get; set; }

        [JsonProperty("data")]
        public AnonData Data { get; set; }

        [JsonProperty("error")]
        public AnonError Error { get; set; }
    }
    public struct AnonData
    {
        [JsonProperty("file")]
        public AnonFile File { get; set; }
    }
    public struct AnonError
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("code")]
        public int Code { get; set; }
    }
    public struct AnonFile
    {
        [JsonProperty("url")]
        public AnonUrl Url { get; set; }

        [JsonProperty("metadata")]
        public AnonMetadata Metadata { get; set; }
    }
    public struct AnonUrl
    {
        [JsonProperty("full")]
        public string Full { get; set; }

        [JsonProperty("short")]
        public string Short { get; set; }
    }
    public struct AnonMetadata
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("size")]
        public AnonSize Size { get; set; }
    }
    public struct AnonSize
    {
        [JsonProperty("bytes")]
        public int Bytes { get; set; }

        [JsonProperty("readable")]
        public string Readable { get; set; }
    }
}
