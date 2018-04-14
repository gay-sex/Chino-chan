using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chino_chan.Models.Music
{
    public class MusicItem
    {
        public string Name { get; set; }
        
        public YouTubeResponse YouTubeInfo { get; set; }

        public string Path { get; set; }

        public TimeSpan Duration { get; set; }
    }
}
