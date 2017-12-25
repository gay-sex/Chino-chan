using Chino_chan.Models.Language;
using Chino_chan.Models.Privillages;
using Chino_chan.Models.Settings;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        [Command("game"), Summary("Owner"), Models.Privillages.Owner()]
        public async Task GameAsync(params string[] Args)
        {
            if (Args.Length == 0)
            {
                if (!string.IsNullOrWhiteSpace(Context.Client.CurrentUser.Activity.Name))
                {
                    await Context.Channel.SendMessageAsync(Language.Game.Prepare(new Dictionary<string, string>()
                    {
                        { "%GAME%", Context.Client.CurrentUser.Activity.Name }
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
                    { "%GAME%", Context.Client.CurrentUser.Activity.Name }
                }));
            }
        }

        [Command("restart"), Summary("Owner"), Models.Privillages.Owner()]
        public async Task ReloadAsync(params string[] Args)
        {
            await Context.Channel.SendMessageAsync(Language.Restarting);
            Entrance.Reload(Context.Channel as ITextChannel);
        }

        [Command("shutdown"), Summary("Owner"), Models.Privillages.Owner()]
        public async Task ShutdownAsync(params string[] Args)
        {
            await Context.Channel.SendMessageAsync(Language.Shutdown);
            await Global.StopAsync();
            Environment.Exit(exitCode: 0);
        }

        [Command("update"), Summary("Owner"), Models.Privillages.Owner()]
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
    }
}
