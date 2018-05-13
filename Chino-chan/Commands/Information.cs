using Chino_chan.Models.Language;
using Chino_chan.Models.Settings;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace Chino_chan.Commands
{
    public class Information : ModuleBase
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

        [Command("help"), Summary("Information")]
        public async Task HelpAsync(params string[] args)
        {
            if (args.Length > 0)
            {
                var CommandNames = Global.CommandService.Commands.Select(t => t.Name.ToLower());
                var Message = "";
                bool SendHelp = false;
                foreach (var arg in args)
                {
                    if (!CommandNames.Contains(arg))
                    {
                        SendHelp = true;
                    }
                    else
                    {
                        if (Message == "")
                        {
                            Message += "```css\n";
                        }
                        if (Language.Help.TryGetValue(arg, out string Help))
                        {
                            Message += arg + ": " + Help + "\n";
                        }
                        else
                        {
                            Message += arg + ": " + Language.HelpNotDefined + "\n";
                        }
                    }
                }

                if (SendHelp)
                    await SendHelpAsync();

                if (Message != "")
                {
                    Message += "```";
                    await ReplyAsync(Message);
                }
            }
            else
            {
                await SendHelpAsync();
            }
        }

        [Command("ping"), Summary("Information")]
        public async Task PinkAsync(params string[] Args)
        {
            var Message = await Context.Channel.SendMessageAsync("Pong!");
            await Message.ModifyAsync((MessageProps) =>
            {
                var Time = Message.Timestamp - Context.Message.Timestamp;
                var Seconds = Math.Truncate(Time.TotalMilliseconds / 1000);
                var Ms = Time.TotalMilliseconds - (Seconds * 1000);

                MessageProps.Content = $"Pong! `{ Seconds }s { Ms }ms`";
            });
        }

        [Command("git"), Summary("Information")]
        public async Task SendGitAsync(params string[] Args)
        {
            EmbedBuilder Builder = new EmbedBuilder()
            {
                Color = new Color(255, 0, 203)
            };
            
            if (Context.Guild != null)
            {
                var User = Global.Client.GetGuild(Context.Guild.Id).GetUser(Global.Client.CurrentUser.Id) as IGuildUser;
                Builder.WithAuthor(User);
            }
            else
            {
                Builder.WithAuthor(Global.Client.CurrentUser);
            }
            if (string.IsNullOrWhiteSpace(Global.Settings.GithubLink))
            {
                Builder.WithDescription(Language.HasNoGit);
            }
            else
            {
                Builder.WithDescription(Language.GitDescription);
                Builder.WithUrl(Global.Settings.GithubLink);
            }

            await Context.Channel.SendMessageAsync("", embed: Builder.Build());
        }

        [Command("info"), Summary("Information")]
        public async Task SendInfoAsync(params string[] Args)
        {
            var CurrentProcess = Process.GetCurrentProcess();
            var Owner = Global.Client.GetUser(Global.Settings.Owner.Id);
            int UserCount = 0;
            string[] Names = new string[Global.Client.Guilds.Count];
            for (int i = 0; i < Global.Client.Guilds.Count; i++)
            {
                var Guild = Global.Client.Guilds.ElementAt(i);
                UserCount += Guild.Users.Count;
                Names[i] = Guild.Name;
            }

            var Embed = new EmbedBuilder();

            Embed.WithAuthor(await Context.Guild.GetUserAsync(Context.Client.CurrentUser.Id));
            Embed.WithDescription($"**{ Language.InfoHeader }**\r\n");
            Embed.WithColor(255 << 16 | 050 << 8 | 230);

            #region Memory Usage
            Embed.AddField(new EmbedFieldBuilder()
            {
                IsInline = true,
                Name = Language.MemoryUsageHeader,
                Value = (CurrentProcess.NonpagedSystemMemorySize64 + CurrentProcess.PagedMemorySize64) / 1048576 + "MB"
            });
            #endregion
            #region Discord Library
            Embed.AddField(new EmbedFieldBuilder()
            {
                IsInline = true,
                Name = Language.DiscordLibraryHeader,
                Value = DiscordConfig.Version
            });
            #endregion
            #region Creator
            Embed.AddField(new EmbedFieldBuilder()
            {
                IsInline = true,
                Name = Language.CreatorHeader,
                Value = Owner.Username + "#" + Owner.Discriminator
            });
            #endregion
            #region Users
            Embed.AddField(new EmbedFieldBuilder()
            {
                IsInline = true,
                Name = Language.UsersHeader,
                Value = UserCount
            });
            #endregion
            #region Uptime
            Embed.AddField(new EmbedFieldBuilder()
            {
                IsInline = false,
                Name = Language.UptimeHeader,
                Value = $"{ Global.Uptime.Days } { Language.Days }, { Global.Uptime.Hours } { Language.Hours }, { Global.Uptime.Minutes } { Language.Minutes }, { Global.Uptime.Seconds } { Language.Seconds }"
            });
            #endregion
            #region Joined servers
            Embed.AddField(new EmbedFieldBuilder()
            {
                IsInline = false,
                Name = Language.JoinedServersHeader,
                Value = "**" + string.Join("\r\n- ", Names) + "**"
            });
            #endregion

            await Context.Channel.SendMessageAsync("", embed: Embed.Build());
        }

        [Command("serverinfo"), Summary("Information")]
        public async Task SendServerInfoAsync(params string[] Args)
        {
            var Embed = new EmbedBuilder()
            {
                Color = new Color(255, 50, 230),
                Title = "**" + Language.ServerInformationHeader + "**"
            };

            var Os = Global.SysInfo.OS;
            var CPU = Global.SysInfo.CPU;
            var MemInfo = Global.SysInfo.MemInfo;
            var VideoCard = Global.SysInfo.VideoCardInfo;

            var MemTotal = MemInfo.TotalMemory / 1024 / 1024;
            var MemFree = MemInfo.CurrentMemory;
            var MemUsage = MemTotal - MemFree;

            var Drives = DriveInfo.GetDrives();
            var DrivesText = "";
            foreach (var Drive in Drives)
            {
                if (!Drive.IsReady) continue;
                switch (Drive.DriveType)
                {
                    case DriveType.CDRom:
                    case DriveType.Removable:
                    case DriveType.Unknown:
                        continue;
                }
                var Label = string.IsNullOrWhiteSpace(Drive.VolumeLabel) ? Language.DriveDefaultName : Drive.VolumeLabel;

                var Free = Drive.AvailableFreeSpace / 1073741824;
                var Total = Drive.TotalSize / 1073741824;

                DrivesText += $"- { Label } [{ Drive.DriveFormat }] ({ Drive.Name }): { Total - Free }GB / { Total }GB { Language.Free } { Free }GB\n";
            }
            Embed.AddField(new EmbedFieldBuilder()
            {
                Name = Language.OperatingSystemHeader,
                Value = $"- { Language.NameHeader }: { Os.Name }\n"
                      + $"- { Language.Version }: { Os.Version }\n"
                      + $"- { Language.Architecture }: { Os.Architecture }",
                IsInline = false
            });
            Embed.AddField(new EmbedFieldBuilder()
            {
                Name = Language.ProcessorHeader,
                Value = $"- { Language.NameHeader }: { CPU.Name }\n"
                      + $"- { Language.Socket }: { CPU.Socket }\n"
                      + $"- { Language.Description }: { CPU.Description }\n"
                      + $"- { Language.Speed }: { CPU.Speed }Mhz\n"
                      + $"- { Language.Usage }: { CPU.Percentage }%\n"
                      + $"- { Language.Cache }: L2: { CPU.L2 }KB L3: { CPU.L3 }KB\n"
                      + $"- { Language.Cores }/{ Language.Threads }: { CPU.Cores }/{ CPU.Threads }\n\n",
                IsInline = false
            });
            Embed.AddField(new EmbedFieldBuilder()
            {
                Name = Language.Memory,
                Value = $"- { Language.Used }: { MemUsage } MB / { MemTotal } MB\n"
                      + $"- { Language.Free }: { MemFree } MB",
                IsInline = false
            });
            Embed.AddField(new EmbedFieldBuilder()
            {
                Name = Language.Drives,
                Value = DrivesText,
                IsInline = false
            });
            Embed.AddField(new EmbedFieldBuilder()
            {
                Name = Language.VideoCard,
                Value = VideoCard.Name + " - " + VideoCard.RAM / 1024 / 1024 + "MB",
                IsInline = false
            });
            await Context.Channel.SendMessageAsync("", embed: Embed.Build());
        }

        [Command("invite"), Summary("Information")]
        public async Task SendInviteAsync(params string[] Args)
        {
            if (string.IsNullOrWhiteSpace(Global.Settings.InvitationLink))
            {
                await Context.Channel.SendMessageAsync(Language.HasNoInvitationLink);
            }
            else
            {
                EmbedBuilder Builder = new EmbedBuilder();
                var User = Global.Client.GetGuild(Context.Guild.Id).GetUser(Context.Client.CurrentUser.Id);
                Builder.WithAuthor(User);
                Builder.Url = Global.Settings.InvitationLink;
                Builder.Description = Language.InvitationDescription;
                Builder.Color = new Color(0 << 16 | 255 << 8 | 255);

                await Context.Channel.SendMessageAsync("", embed:Builder.Build());
            }
        }

        private async Task SendHelpAsync()
        {
            string Message = "```css\n";

            var AvailableCommands = new Dictionary<string, List<CommandInfo>>();

            foreach (var Command in Global.CommandService.Commands)
            {
                var Result = await Command.CheckPreconditionsAsync(Context);
                if (Result.IsSuccess)
                {
                    if (AvailableCommands.TryGetValue(Command.Summary, out List<CommandInfo> Infos))
                    {
                        var List = new List<CommandInfo>(Infos)
                        {
                            Command
                        };
                        AvailableCommands[Command.Summary] = List;
                    }
                    else
                    {
                        AvailableCommands.Add(Command.Summary, new List<CommandInfo>() { Command });
                    }
                }
            }

            foreach (var AvailableCommand in AvailableCommands)
            {
                Message += ("#" + AvailableCommand.Key + "\n");
                Message += string.Join(", ", AvailableCommand.Value.Select(t => t.Name));
                Message += "\n\n";
            }
            Message += $"\n{ Language.HelpFootage }```";

            var DMChannel = await Context.User.GetOrCreateDMChannelAsync();
            await DMChannel.SendMessageAsync(Context.Prepare(Message));

            if (Context.Channel.Id != DMChannel.Id)
                await ReplyAsync(Context.Prepare(Language.CheckPrivate));
        }
    }
}
