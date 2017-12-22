using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chino_chan.Models.Settings
{
    public class GuildSetting
    {
        public ulong GuildId { get; set; }

        public string Prefix { get; set; }
        public string LanguageId { get; set; }

        public List<UserCredential> Admins { get; set; }

        public List<BlockedUser> Blocked { get; set; }

        public List<ulong> NsfwChannels { get; set; }

        public int Volume { get; set; }
        public List<string> Query { get; set; }
        public string CurrentPlaying { get; set; }
        public bool FromQuery { get; set; }
        public int MusicPosition { get; set; }
    }
}
