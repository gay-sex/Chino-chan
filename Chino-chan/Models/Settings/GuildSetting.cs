using Chino_chan.Models.Music;
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

        public string Prefix { get; set; } = ";";
        public string LanguageId { get; set; } = "en";

        public List<UserCredential> Admins { get; set; } = new List<UserCredential>();

        public List<BlockedUser> Blocked { get; set; } = new List<BlockedUser>();

        public List<ulong> NsfwChannels { get; set; } = new List<ulong>();

        public Music Music { get; set; } = new Music();
    }

    public class Music
    {
        public int Volume { get; set; } = 50;
        public List<MusicItem> Queue { get; set; } = new List<MusicItem>();
    }
}
