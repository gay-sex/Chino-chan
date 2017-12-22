using Chino_chan.Models.Language;
using Chino_chan.Models.Settings;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chino_chan.Commands
{
    public class Irc : ModuleBase
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

        private Modules.Irc IrcClient
        {
            get
            {
                return Global.Irc;
            }
        }

        [Command("irc"), Models.Privillages.Owner()]
        public async Task IrcAsync(params string[] Args)
        {
            var Text = Language.IrcHelp;


            if (IrcClient == null)
            {
                Text = Language.IrcDisabled;
            }
            else
            {
                if (Args.Length > 0)
                {
                    switch (Args[0].ToLower())
                    {
                        case "connect":
                            Text = Connect();
                            break;
                        case "disconnect":
                            Text = Disconnected();
                            break;
                        case "join":
                            if (Args.Length > 1)
                            {
                                Text = Join(Tools.ConvertHighlightsBack(string.Join(" ", Args.Skip(1))));
                            }
                            break;
                        case "part":
                            if (Args.Length > 1)
                            {
                                Text = Part(Tools.ConvertHighlightsBack(string.Join(" ", Args.Skip(1))));
                            }
                            break;
                    }
                }
            }


            await Context.Channel.SendMessageAsync(Text);
        }


        private string Connect()
        {
            if (IrcClient.Connected)
            {
                return Language.IrcAlreadyConnected;
            }
            else
            {
                IrcClient.Connect();
                return Language.IrcConnected;
            }
        }
        private string Disconnected()
        {
            if (!IrcClient.Connected)
            {
                return Language.IrcAlreadyDisconnected;
            }
            else
            {
                IrcClient.Disconnect();
                return Language.IrcDisconnected;
            }
        }

        private string Join(string ChannelName)
        {
            if (IrcClient.Join(ChannelName))
            {
                return Language.IrcJoined.Prepare(new Dictionary<string, string>()
                {
                    { "%IRCCHANNEL%", ChannelName }
                });
            }
            else
            {
                return Language.IrcChannelNotFound.Prepare(new Dictionary<string, string>()
                {
                    { "%IRCCHANNEL%", ChannelName }
                });
            }
        }
        private string Part(string ChannelName)
        {
            if (IrcClient.Part(ChannelName))
            {
                return Language.IrcParted.Prepare(new Dictionary<string, string>()
                {
                    { "%IRCCHANNEL%", ChannelName }
                });
            }
            else
            {
                return Language.IrcChannelNotFound.Prepare(new Dictionary<string, string>()
                {
                    { "%IRCCHANNEL%", ChannelName }
                });
            }
        }
    }
}
