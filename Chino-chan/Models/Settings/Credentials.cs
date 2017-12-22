namespace Chino_chan.Models.Settings
{
    public struct Credentials
    {
        public DiscordCredentials Discord { get; set; }
        public osuCredentials osu { get; set; }
        public YouTubeCredentials Youtube { get; set; }
        public WaifuCloudCredentials WaifuCloud { get; set; }
        public SankakuCredentials Sankaku { get; set; }
        public ImgurCredentials Imgur { get; set; }
    }
}
