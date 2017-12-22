using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Chino_chan.Models.Settings;
using Chino_chan.Models.Irc;

namespace Chino_chan
{
    public class Settings
    {
        public List<IrcDiscordChannelBinding> LastIRCChannels { get; set; }

        public string OwnerName { get; set; }
        public UserCredential Owner { get; set; }

        public List<UserCredential> GlobalAdmins { get; set; }

        public string WaifuCloudHostname { get; set; }

        public DevServer DevServer { get; set; }

        public string Game { get; set; }

        public List<BlockedUser> GloballyBlocked { get; set; }
        
        public string InvitationLink { get; set; }
        
        public Credentials Credentials { get; set; }

        public int OSUAPICallLimit { get; set; }
        
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

        public Dictionary<ulong, SayPreferences> SayPreferences { get; set; } = new Dictionary<ulong, Models.Settings.SayPreferences>();

        public string GithubLink { get; set; }
        
    }
}
