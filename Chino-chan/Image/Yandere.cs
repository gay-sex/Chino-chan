using Chino_chan.Models.Images;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Threading.Tasks;

namespace Chino_chan.Image
{
    public class Yandere
    {
        private string YanderePostEndpoint = "https://yande.re/post.json";
        private string PageEndpoint = "https://yande.re/post";

        HttpClient Client;

        public Yandere()
        {
            Client = new HttpClient();
        }

        public async Task<List<YandereResponse>> GetImagesAsync(IEnumerable<string> Tags, int Limit = 50, int Page = -1)
        {
            var Endpoint = YanderePostEndpoint
                + "?limit=" + Limit
                + "&tags=" + string.Join("+", Tags).Replace(":", "%3A")
                + (Page != -1 ? ("&page=" + Page) : "");
            var Content = await (await Client.GetAsync(Endpoint)).Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<List<YandereResponse>>(Content);
        }
        public async Task<YandereResponse> GetImageAsync(IEnumerable<string> Tags)
        {
            var Endpoint = PageEndpoint
                + "?tags=" + string.Join("+", Tags).Replace(":", "%3A")
                + "&limit=50";

            var Count = await PageCount(Endpoint);
            var Images = await GetImagesAsync(Tags, 50, Global.Random.Next(-1, Count + 1));


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

            int Pages = 0;

            if (Page.Contains("<div class=\"pagination\">"))
            {
                int Index = Page.IndexOf("<div class=\"pagination\">");
                Page = Page.Substring(Index, Page.IndexOf("</div", Index) - Index);

                var Regex = new Regex("<a href=\".*?\">(\\d*)<\\/a>");
                var Match = Regex.Matches(Page).Cast<Match>().Last();

                Pages = int.Parse(Match.Groups[1].Value);
            }
            
            return Pages;
        }
    }
}
