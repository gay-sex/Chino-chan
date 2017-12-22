using Chino_chan.Models.Language;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Chino_chan.Modules
{
    public class LanguageHandler
    {
        private string LanguageFolder
        {
            get
            {
                return Global.SettingsFolder + "\\Languages";
            }
        }
        public Dictionary<string, Language> Languages { get; private set; }

        public LanguageHandler()
        {
            Languages = new Dictionary<string, Language>();
            LoadLanguages();
        }

        public void LoadLanguages()
        {
            Languages.Clear();
            GenEn();

            if (!System.IO.Directory.Exists(LanguageFolder))
            {
                System.IO.Directory.CreateDirectory(LanguageFolder);
            }

            var LanguageFiles = System.IO.Directory.EnumerateFiles(LanguageFolder, "*.json");

            foreach (var File in LanguageFiles)
            {
                var Language = JsonConvert.DeserializeObject<Language>(System.IO.File.ReadAllText(File));
                Global.Logger.Log(ConsoleColor.Magenta, LogType.Language, null, Language.Name + " language loaded!");
                Languages.Add(Language.Id, Language);
            }
        }

        private void GenEn()
        {
            var Language = new Language();

            if (System.IO.File.Exists(LanguageFolder + "\\en.json"))
                System.IO.File.Delete(LanguageFolder + "\\en.json");

            System.IO.File.WriteAllText(LanguageFolder + "\\en.json", JsonConvert.SerializeObject(Language, Formatting.Indented));
        }

        public Language GetLanguage(string Id)
        {
            if (Languages.TryGetValue(Id, out Language Value))
            {
                return Value;
            }
            else
            {
                return Languages["en"];
            }
        }
    }
}
