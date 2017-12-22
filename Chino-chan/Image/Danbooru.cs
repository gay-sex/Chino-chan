using Chino_chan.Models.Images;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Chino_chan.Image
{
    public class Danbooru
    {
        string DanbooruEndpoint = "https://danbooru.donmai.us/posts.json";
        HttpClient Client;

        public Danbooru()
        {
            Client = new HttpClient();
        }

        public async Task<List<DanbooruResponse>> GetImagesAsync(IEnumerable<string> Tags, int Limit = 50, bool Random = true)
        {
            var Endpoint = DanbooruEndpoint
                + "?limit=" + Limit
                + "&tags=" + string.Join("+", Tags).Replace(":", "%3A")
                + "&random=" + Random.ToString().ToLower();

            var Content = await (await Client.GetAsync(Endpoint)).Content.ReadAsStringAsync();
            
            return JsonConvert.DeserializeObject<List<DanbooruResponse>>(Content);
        }
        public async Task<DanbooruResponse> GetImageAsync(IEnumerable<string> Tags, bool Random = true, int Page = -1)
        {
            return (await GetImagesAsync(Tags, 1, Random))[0];
        }
    }
}
