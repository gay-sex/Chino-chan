namespace Chino_chan.Models.Settings.Credentials
{
    public struct Credentials
    {
        public DiscordCredentials Discord { get; set; }
        public osuCredentials osu { get; set; }
        public GoogleCredentials Google { get; set; }
        public WaifuCloudCredentials WaifuCloud { get; set; }
        public SankakuCredentials Sankaku { get; set; }
        public ImgurCredentials Imgur { get; set; }
    }
}
