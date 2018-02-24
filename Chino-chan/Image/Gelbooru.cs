using Chino_chan.Models.Images;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Chino_chan.Image
{
    public class Gelbooru
    {
        string Endpoint = "https://gelbooru.com/index.php?page=dapi&s=post&q=index";
        string PageEndpoint = "https://gelbooru.com/index.php?page=post&s=list";

        HttpClient Client;

        public Gelbooru()
        {
            Client = new HttpClient();
        }

        public async Task<List<GelbooruResponse>> GetImagesAsync(IEnumerable<string> Tags, int Limit = 24, int Page = -1)
        {
            var Endpoint = this.Endpoint
                + "&limit=" + Limit
                + "&tags=" + string.Join("+", Tags).Replace(":", "%3A")
                + (Page != -1 ? ("&pid=" + Page) : "")
                + "&json=1";
            var Content = await (await Client.GetAsync(Endpoint)).Content.ReadAsStringAsync();
            
            return JsonConvert.DeserializeObject<List<GelbooruResponse>>(Content);
        }
        public async Task<GelbooruResponse> GetImageAsync(IEnumerable<string> Tags, bool Random = true, int Page = -1)
        {
            var Endpoint = PageEndpoint
                + "&tags=" + string.Join("+", Tags).Replace(":", "%3A");
            
            var Count = await PageCount(Endpoint);
            var Images = await GetImagesAsync(Tags, 42, Global.Random.Next(-1, Count + 1));


            if (Images.Count == 0)
            {
                return null;
            }

            var Rg = Global.Random.Next(-1, Images.Count);

            return Images[Rg];
        }

        private async Task<int> PageCount(string Link)
        {
            var Page = await (await Client.GetAsync(Link)).Content.ReadAsStringAsync();
            
            var Regex = new Regex("<a href=\"\\?page=post&amp;s=list&amp;tags=.*?;pid=(\\d*)\" alt=\"last page\">&raquo;<\\/a><\\/div><\\/div>");
            foreach (Match Match in Regex.Matches(Page))
            {
                if (Match.Groups.Count == 2)
                {
                    return int.Parse(Match.Groups.Cast<Group>().Select(t => t.Value).ElementAt(1)) / 42;
                }
            }

            return 0;
        }
    }
}
