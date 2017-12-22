using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chino_chan.Models.Settings
{
    public class UserCredential
    {
        [JsonProperty("id")]
        public ulong Id { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }
    }
}
