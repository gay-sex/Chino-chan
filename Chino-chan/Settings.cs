using System.Collections.Generic;
using Chino_chan.Models.Settings;
using Chino_chan.Models.Settings.Credentials;

namespace Chino_chan
{
    public class Settings
    {
        public string OwnerName { get; set; } = "ExModify";
        public UserCredential Owner { get; set; } = new UserCredential();

        public List<UserCredential> GlobalAdmins { get; set; } = new List<UserCredential>();

        public string WaifuCloudHostname { get; set; } = "Boltzmann";

        public DevServer DevServer { get; set; }

        public string Game { get; set; } = "with ExMo";

        public List<BlockedUser> GloballyBlocked { get; set; } = new List<BlockedUser>();

        public string InvitationLink { get; set; } = "https://discordapp.com/oauth2/authorize?client_id=271658919443562506&scope=bot&permissions=0";

        public Credentials Credentials { get; set; } = new Credentials();

        public int OSUAPICallLimit { get; set; } = 60;
        
        public int WebServerPort { get; set; } = 2465;
        
        public string[] ImageExtensions { get; set; } = new string[]
        {
            "png",
            "jpg",
            "gif",
            "jpeg",
            "webp"
        };

        public List<Models.Settings.Image> ImagePaths { get; set; } = new List<Models.Settings.Image>();

        public Dictionary<ulong, SayPreferences> SayPreferences { get; set; } = new Dictionary<ulong, SayPreferences>();

        public string GithubLink { get; set; } = "";
    }
}
