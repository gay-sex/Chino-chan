using Chino_chan.Models.Language;
using Chino_chan.Models.Settings;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Chino_chan.Commands
{
    public class Owner : ModuleBase
    {
        public GuildSetting Settings
        {
            get
            {
                return Context.GetSettings();
            }
        }
        public Language Language
        {
            get
            {
                return Context.GetLanguage();
            }
        }

        [Command("game"), Summary("Owner"), Models.Privileges.Owner()]
        public async Task GameAsync(params string[] Args)
        {
            if (Args.Length == 0)
            {
                if (Context.Client.CurrentUser.Game.HasValue)
                {
                    await Context.Channel.SendMessageAsync(Language.Game.Prepare(new Dictionary<string, string>()
                    {
                        { "%GAME%", Context.Client.CurrentUser.Game.Value.Name }
                    }));
                }
                else
                {
                    await Context.Channel.SendMessageAsync(Language.NoGame);
                }
            }
            else
            {
                await Global.Client.SetGameAsync(string.Join(" ", Args));
                await Context.Channel.SendMessageAsync(Language.Game.Prepare(new Dictionary<string, string>()
                {
                    { "%GAME%", Context.Client.CurrentUser.Game.Value.Name }
                }));
            }
        }

        [Command("restart"), Summary("Owner"), Models.Privileges.Owner()]
        public async Task ReloadAsync(params string[] Args)
        {
            await Context.Channel.SendMessageAsync(Language.Restarting);
            Entrance.Reload(Context.Channel as ITextChannel);
        }

        [Command("shutdown"), Summary("Owner"), Models.Privileges.Owner()]
        public async Task ShutdownAsync(params string[] Args)
        {
            await Context.Channel.SendMessageAsync(Language.Shutdown);
            await Global.StopAsync();
            Environment.Exit(exitCode: 0);
        }

        [Command("update"), Summary("Owner"), Models.Privileges.Owner()]
        public async Task UpdateAsync(params string[] Args)
        {
            if (Global.Updater.Update())
            {
                await Context.Channel.SendMessageAsync(Language.Updated);
                await Global.StopAsync();
                Environment.Exit(exitCode: 0);
            }
            else
            {
                await Context.Channel.SendMessageAsync(Language.UpdateError);
            }
        }

        [Command("set"), Summary("Owner"), Models.Privileges.Owner()]
        public async Task SetAsync(params string[] Args)
        {
            if (Args.Length >= 2)
            {
                var Type = Global.Settings.GetType();
                var Properties = Type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (var Property in Properties)
                {
                    if (Property.Name.ToLower() == Args[0].ToLower())
                    {
                        Property.SetValue(Global.Settings, string.Join(" ", Args.Skip(1)));
                        await Context.Channel.SendMessageAsync($"{ Property.Name }: `{ Property.GetValue(Global.Settings) }`");
                        break;
                    }
                }
            }
        }

        [Command("get"), Summary("Owner"), Models.Privileges.Owner()]
        public async Task GetAsync(params string[] Args)
        {
            if (Args.Length == 1)
            {
                var Type = typeof(Settings);
                var Properties = Type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (var Property in Properties)
                {
                    if (Property.Name.ToLower() == Args[0].ToLower())
                    {
                        var Content = JsonConvert.SerializeObject(Property.GetValue(Global.Settings), Formatting.Indented);

                        await Context.Channel.SendMessageAsync($"{ Property.Name }: `{ Content }`");
                        break;
                    }
                }
            }
        }
    }
}
