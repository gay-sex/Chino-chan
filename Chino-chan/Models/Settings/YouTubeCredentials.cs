using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chino_chan.Models.Settings
{
    public struct YouTubeCredentials
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }

        public string Token { get; set; }
    }
}
