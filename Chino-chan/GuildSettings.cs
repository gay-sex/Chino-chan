using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Chino_chan.Models.Settings;

namespace Chino_chan
{
    public class GuildSettings
    {
        private string GuildSettingsPath
        {
            get
            {
                return Global.SettingsFolder + "\\GuildSettings.json";
            }
        }
        
        public Dictionary<ulong, GuildSetting> Settings { get; private set; }

        public GuildSettings()
        {
            Settings = new Dictionary<ulong, GuildSetting>();
            Load();
        }

        public void Save()
        {
            if (System.IO.File.Exists(GuildSettingsPath))
            {
                System.IO.File.Delete(GuildSettingsPath);
            }
            if (!System.IO.Directory.Exists(Global.SettingsFolder))
            {
                System.IO.Directory.CreateDirectory(Global.SettingsFolder);
            }
            System.IO.File.WriteAllText(GuildSettingsPath, JsonConvert.SerializeObject(Settings, Formatting.Indented));
        }
        private void Load()
        {
            if (System.IO.File.Exists(GuildSettingsPath))
            {
                Settings = JsonConvert.DeserializeObject<Dictionary<ulong, GuildSetting>>(System.IO.File.ReadAllText(GuildSettingsPath));
            }
            Save();
        }

        public GuildSetting GetSettings(ulong GuildId)
        {
            if (!Settings.TryGetValue(GuildId, out GuildSetting Setting))
            {
                Setting = new GuildSetting()
                {
                    GuildId = GuildId
                };
                Settings.Add(GuildId, Setting);
                Save();
            }
            return Setting;
        }
        public void Modify(ulong GuildId, Action<GuildSetting> Modification)
        {
            var Setting = GetSettings(GuildId);
            Modification?.Invoke(Setting);
            Settings[GuildId] = Setting;
            Save();
        }
    }
}
