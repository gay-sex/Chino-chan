﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Chino_chan.Models.Settings;
using Discord;

namespace Chino_chan
{
    public class Entrance
    {
        static Thread CommandThread;
        static ITextChannel Restarted;

        public static void Main(string[] args)
        {
            if (CommandThread == null || !CommandThread.IsAlive)
            {
                CommandThread = new Thread(ManageConsoleCommands);
                CommandThread.Start();
            }
            Global.Setup();

            Global.Client.Ready += async () =>
            {
                await Global.Client.SetStatusAsync(UserStatus.Online);
                await Global.Client.SetGameAsync(Global.Settings.Game);
                await Global.Client.DownloadUsersAsync(Global.Client.Guilds);
                
                if (Restarted != null)
                {
                    await Restarted.SendMessageAsync(Restarted.GetLanguage().Restarted);
                    Restarted = null;
                }
            };

            Global.StartAsync().Wait();
        }

        private static void ManageConsoleCommands()
        {
            while (true)
            {
                var input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input))
                    HandleCommand(input);
            }
        }
        public static void HandleCommand(string Command, ITextChannel Channel = null)
        {
            var Lower = Command.ToLower();
            var Trim = Lower.Trim();
            var Parameter = "";
            var Index = Command.IndexOf(" ");

            if (Index >= 0)
            {
                Lower = Lower.Substring(0, Index);
                Trim = Lower.Trim();
                Parameter = Command.Substring(Index).TrimEnd().TrimStart();
            }
            if (Trim == "gc")
            {
                Clean();
                if (Channel != null)
                {
                    Channel.SendMessageAsync(Channel.GetLanguage().GCDone);
                }
            }
            else if (Trim == "irc")
            {
                var ParamTrim = Parameter.Trim();
                if (ParamTrim == "connect" || ParamTrim == "c")
                {
                    Global.Irc.Connect();
                    if (Channel != null)
                    {
                        Channel.SendMessageAsync(Channel.GetLanguage().IrcConnected);
                    }
                }
                else if (ParamTrim == "disconnect" || ParamTrim == "dc")
                {
                    Global.Irc.Disconnect();
                    if (Channel != null)
                    {
                        Channel.SendMessageAsync(Channel.GetLanguage().IrcDisconnected);
                    }
                }
                else if (ParamTrim == "reconnect" || ParamTrim == "rc")
                {
                    Global.Irc.Reconnect();
                    if (Channel != null)
                    {
                        Channel.SendMessageAsync(Channel.GetLanguage().IrcReconnected);
                    }
                }
                else if (ParamTrim.StartsWith("join"))
                {
                    var ChannelName = Parameter.Substring(Parameter.IndexOf(' ') + 1);
                    var Success = Global.Irc.Join(ChannelName);
                    if (Channel != null)
                    {
                        var Language = Channel.GetLanguage();
                        if (Success)
                        {
                            Channel.SendMessageAsync(Language.IrcJoined.Prepare(new Dictionary<string, string>()
                            {
                                { "%IRCCHANNEL%", ChannelName }
                            }));
                        }
                        else
                        {
                            Channel.SendMessageAsync(Language.IrcChannelNotFound.Prepare(new Dictionary<string, string>()
                            {
                                { "%IRCCHANNEL%", ChannelName }
                            }));
                        }
                    }
                }
                else if (ParamTrim.StartsWith("part"))
                {
                    var ChannelName = Parameter.Substring(Parameter.IndexOf(' ') + 1);
                    var Success = Global.Irc.Part(ChannelName);
                    if (Channel != null)
                    {
                        var Language = Channel.GetLanguage();
                        if (Success)
                        {
                            Channel.SendMessageAsync(Language.IrcJoined.Prepare(new Dictionary<string, string>()
                            {
                                { "%IRCCHANNEL%", ChannelName }
                            }));
                        }
                        else
                        {
                            Channel.SendMessageAsync(Language.IrcChannelNotFound.Prepare(new Dictionary<string, string>()
                            {
                                { "%IRCCHANNEL%", ChannelName }
                            }));
                        }
                    }
                }
            }
            else if (Trim == "quit")
            {
                if (Channel != null)
                    Channel.SendMessageAsync(
                        Global.LanguageHandler.GetLanguage(
                            Global.GuildSettings.GetSettings(Channel.GuildId).LanguageId).Shutdown).Wait();

                Global.StopAsync().Wait();
                Environment.Exit(exitCode: 0);
            }
        }

        private static void Clean()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();

            Global.Logger.Log(ConsoleColor.DarkMagenta, Modules.LogType.GC, null, "Garbage collected!");
        }
        public static void Restart(ITextChannel Channel = null)
        {
            Task.Run(() =>
            {
                Global.StopAsync().Wait();
                Console.Clear();
                Restarted = Channel;
                Main(new string[0]);
            });
        }
    }

    public static class SettingsLanguageIrcExtensions
    {
        public static GuildSetting GetSettings(this Discord.Commands.ICommandContext Context)
        {
            return Global.GuildSettings.GetSettings(Context.Guild != null ? Context.Guild.Id : Context.Channel.Id);
        }
        public static GuildSetting GetSettings(this ITextChannel Channel)
        {
            return Global.GuildSettings.GetSettings(Channel.Guild != null ? Channel.Guild.Id : Channel.Id);
        }
        public static GuildSetting GetSettings(this IGuild Guild)
        {
            return Guild.Id.GetSettings();
        }
        public static GuildSetting GetSettings(this ulong GuildId)
        {
            return Global.GuildSettings.GetSettings(GuildId);
        }

        public static Models.Language.Language GetLanguage(this Discord.Commands.ICommandContext Context)
        {
            return Global.LanguageHandler.GetLanguage(Context.GetSettings().LanguageId);
        }
        public static Models.Language.Language GetLanguage(this ITextChannel Channel)
        {
            return Global.LanguageHandler.GetLanguage(Channel.GetSettings().LanguageId);
        }
        public static Models.Language.Language GetLanguage(this IGuild Guild)
        {
            return Global.LanguageHandler.GetLanguage(Guild.GetSettings().LanguageId);
        }
        public static Models.Language.Language GetLanguage(this ulong GuildId)
        {
            return Global.LanguageHandler.GetLanguage(GuildId.GetSettings().LanguageId);
        }

        public static ITextChannel GetIrcChannel(this string ChannelName)
        {
            ITextChannel Channel;
            var Index = Global.Settings.LastIRCChannels.FindIndex(t => t.ChannelName == ChannelName);
            if (Index < 0)
            {
                return ChannelName.CreateIrcChannel();
            }
            else
            {
                var ValidChannelName = ChannelName.Substring(1);

                var Binding = Global.Settings.LastIRCChannels[Index];

                Channel = Global.Client.GetChannel(Binding.DiscordChannelId) as ITextChannel;
                
                if (Channel == null)
                {
                    Global.Settings.LastIRCChannels.RemoveAt(Index);
                    Channel = ChannelName.CreateIrcChannel();
                }
                Global.SaveSettings();
                return Channel;
            }
        }
        private static ITextChannel CreateIrcChannel(this string ChannelName)
        {
            ITextChannel Channel = null;
            var ValidChannelName = ChannelName.Substring(1);

            var DevGuild = Global.Client.GetGuild(Global.Settings.DevServer.Id);

            if (DevGuild != null)
            {
                var ChannelList = new List<Discord.WebSocket.SocketTextChannel>(DevGuild.TextChannels);
                var ChannelIndex = ChannelList.FindIndex(t => t.Name == ValidChannelName);
                if (ChannelIndex >= 0)
                {
                    Channel = ChannelList[ChannelIndex];
                }
                else
                {
                    Channel = DevGuild.CreateTextChannelAsync(ValidChannelName).Result;
                }
            }

            Global.Settings.LastIRCChannels.Add(new Models.Irc.IrcDiscordChannelBinding()
            {
                ChannelName = ChannelName,
                DiscordChannelId = Channel.Id
            });

            return Channel;
        }
        
        public static string Prepare(this Discord.Commands.ICommandContext Context, string Trivia)
        {
            var Text = Trivia;
            var Settings = Context.GetSettings();

            Text = Text.Replace("%MENTION%", Context.User.Mention);
            Text = Text.Replace("%P%", Settings.Prefix);
            Text = Text.Replace("%PREFIX%", Settings.Prefix);
            Text = Text.Replace("%OWNER%", Global.Settings.OwnerName);
            if (Text.Contains("%LANGUAGES%") || Text.Contains("%LANGS%"))
            {
                var LangNames = string.Join(", ", Global.LanguageHandler.Languages.Select(langPair => langPair.Key + " - " + langPair.Value.Name));
                Text = Text.Replace("%LANGUAGES%", LangNames);
                Text = Text.Replace("%LANGS%", LangNames);
            }

            return Text;
        }
        public static string Prepare(this string Trivia, Dictionary<string, string> Changes, ITextChannel Channel = null, Discord.IGuild Guild = null, Discord.IUser User = null)
        {
            var changes = new Dictionary<string, string>(Changes)
            {
                { "%OWNER%", Global.Settings.OwnerName }
            };

            GuildSetting Settings = null;

            if (Channel != null && Settings == null)
            {
                Settings = Channel.GetSettings();
            }
            else if (Guild != null && Settings == null)
            {
                Settings = Guild.GetSettings();
            }
            else if (User != null && Settings == null)
            {
                Settings = Global.GuildSettings.GetSettings(User.Id);
            }
            
            if (User != null)
            {
                changes.Add("%MENTION%", User.Mention);
            }

            if (Settings != null)
            {
                changes.Add("%P%", Settings.Prefix);
                changes.Add("%PREFIX%", Settings.Prefix);
            }

            var Text = Trivia;

            if (Text.Contains("%LANGUAGES%") || Text.Contains("%LANGS%"))
            {
                var LangNames = string.Join(", ", Global.LanguageHandler.Languages.Select(langPair => langPair.Key + " - " + langPair.Value.Name));
                Text = Text.Replace("%LANGUAGES%", LangNames);
                Text = Text.Replace("%LANGS%", LangNames);
            }

            foreach (var Pair in changes)
            {
                string Key = Pair.Key;

                if (!Key.EndsWith("%"))
                    Key += "%";
                if (!Key.StartsWith("%"))
                    Key = "%" + Key;

                Text = Text.Replace(Key.ToUpper(), Pair.Value);
            }

            return Text;
        }
    }
}
