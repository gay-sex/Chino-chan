using Chino_chan.Models.Language;
using Chino_chan.Models.Privileges;
using Chino_chan.Models.Settings;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Chino_chan.Commands
{
    public class Management : ModuleBase
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
        
        [Command("purgedm"), Summary("Management")]
        public async Task PurgeDMAsync(params string[] NoError)
        {
            var DmChannel = await Context.User.GetOrCreateDMChannelAsync();
            if (Context.Channel.Id == DmChannel.Id)
            {
                var MessageCount = 0;
                var Limit = 100;
                ulong LastMessageId = 0;
                IEnumerable<IMessage> Messages;
                do
                {
                    do
                    {
                        if (LastMessageId != 0)
                            Messages = await DmChannel.GetMessagesAsync(LastMessageId, Direction.Before, Limit).Flatten();
                        else
                            Messages = await DmChannel.GetMessagesAsync(Limit).Flatten();

                        if (LastMessageId == Messages.Last().Id)
                        {
                            Limit = 0;
                            break;
                        }
                        LastMessageId = Messages.Last().Id;
                    }
                    while (!Messages.Select(t => t.Author.Id).Contains(Context.Client.CurrentUser.Id));

                    if (Limit == 0)
                        break;

                    MessageCount = Messages.Count();

                    foreach (var Message in Messages)
                    {
                        if (Message.Author.Id == Context.Client.CurrentUser.Id)
                        {
                            await Message.DeleteAsync();
                        }
                        else
                        {
                            LastMessageId = Message.Id;
                        }
                    }
                }
                while (Limit == MessageCount);
            }
        }

        [Command("prefix"), Summary("Management"), Admin()]
        public async Task ChangePrefixAsync(params string[] Prefix)
        {
            if (Prefix.Length == 0)
            {
                await ReplyAsync(Context.Prepare(Language.CurrentPrefix));
            }
            else
            {
                Global.GuildSettings.Modify(Context.Guild.Id, Modi =>
                {
                    Modi.Prefix = string.Join(" ", Prefix);
                });
                await ReplyAsync(Context.Prepare(Language.PrefixChanged));
            }
        }

        [Command("lang"), Summary("Management"), Admin()]
        public async Task ChangeLanguageAsync(params string[] Language)
        {
            if (Language.Length == 0)
            {
                await base.ReplyAsync(Context.Prepare(this.Language.LanguageHelp));
            }
            else
            {
                var Id = string.Join(" ", Language).ToLower();
                if (Global.LanguageHandler.Languages.ContainsKey(Id))
                {
                    Global.GuildSettings.Modify(Context.Guild.Id, Modi =>
                    {
                        Modi.LanguageId = Id;
                    });
                    await base.ReplyAsync(this.Language.LanguageChanged);
                }
                else
                {
                    await base.ReplyAsync(Context.Prepare(this.Language.LanguageHelp));
                }
            }
        }

        [Command("purge"), Summary("Management"), ServerOwner()]
        public async Task PurgeChannelAsync(params string[] NoError)
        {
            var DMChannel = await Context.User.GetOrCreateDMChannelAsync();
            if (DMChannel.Id == Context.Channel.Id)
            {
                await ReplyAsync(Language.CannotPurgeDM);
            }
            else
            {
                IEnumerable<IMessage> OriginalMessages;
                List<IMessage> Messages;
                do
                {
                    OriginalMessages = await Context.Channel.GetMessagesAsync(50).Flatten();
                    Messages = (OriginalMessages).ToList();

                    var BulkableMessages = Messages.Where(t => t.CreatedAt > DateTime.Now.AddDays(-13));

                    foreach (var Bulked in BulkableMessages)
                        Messages.Remove(Bulked);

                    if (BulkableMessages.Count() > 0)
                        await (Context.Channel as ITextChannel).DeleteMessagesAsync(BulkableMessages);
                    
                    for (int i = 0; i < Messages.Count; i++)
                    {
                        var Message = Messages[i];
                        if (Message.CreatedAt < DateTime.Now.AddDays(-13))
                        {
                            Messages.Remove(Message);
                            await Message.DeleteAsync();
                            await Task.Delay(500);
                            i--;
                        }
                    }
                }
                while (OriginalMessages.Count() == 50);

                await ReplyAsync(Language.ChannelPurged);
            }
        }

        [Command("rlang"), Summary("Management"), Models.Privileges.Owner()]
        public async Task ReloadLanguages(params string[] NoError)
        {
            Global.LanguageHandler.LoadLanguages();
            await ReplyAsync(Language.LanguagesReloaded);
        }

        [Command("admin"), Summary("Management")]
        public async Task AdminAsync(params string[] Args)
        {
            var IsGlobalAdmin = Global.IsGlobalAdmin(Context.User.Id);
            var IsOwner = Global.IsOwner(Context.User.Id);
            var GlobalOrOwner = IsGlobalAdmin || IsOwner;

            var Property = "";
            string[] Value = new string[0];
            if (Args.Length > 1)
            {
                Property = Args[0].ToLower();
                Value = Args.Skip(1).ToArray();
            }
            if (Value.Length > 0)
            {
                if (GlobalOrOwner)
                {
                    var Ids = Tools.ParseIds(Value, Property.Contains("global") && IsOwner, Context);
                    
                    if (Ids.Count == 0)
                    {
                        await ReplyAsync(Language.UserNotFoundAdmin.Prepare(new Dictionary<string, string>()
                        {
                            { "%IDENTIFIER%", string.Join(" ", Value) }
                        }));
                    }
                    else
                    {
                        if (Property == "add")
                        {
                            await AddAdminsAsync(false, Ids);
                        }
                        else if (Property == "remove")
                        {
                            await RemoveAdminsAsync(false, Ids);
                        }
                        else if (IsOwner)
                        {
                            if (Property == "addglobal")
                            {
                                await AddAdminsAsync(true, Ids);
                            }
                            else if (Property == "removeglobal")
                            {
                                await RemoveAdminsAsync(true, Ids);
                            }
                            else
                            {
                                await SendAdminsAsync(IsGlobalAdmin, IsOwner);
                            }
                        }
                        else
                        {
                            await SendAdminsAsync(IsGlobalAdmin, IsOwner);
                        }
                    }
                }
                else
                {
                    await SendAdminsAsync(IsGlobalAdmin, IsOwner);
                }
            }
            else
            {
                await SendAdminsAsync(IsGlobalAdmin, IsOwner);
            }
        }
        
        [Command("block"), Summary("Management")]
        public async Task BlockAsync(params string[] Args)
        {
            var IsAdmin = Global.IsAdminOrHigher(Context.User.Id, Context.Guild, Settings);
            var IsGlobalAdmin = Global.IsGlobalAdminOrHigher(Context.User.Id);

            var OriginalProperty = "";
            var Property = "";
            string[] Value = new string[0];
            if (Args.Length > 1)
            {
                OriginalProperty = Args[0];
                Property = Args[0].ToLower();
                Value = Args.Skip(1).ToArray();
            }
            if (Value.Length > 0)
            {
                if (IsAdmin)
                {
                    var Ids = Tools.ParseIds(Value, Property.Contains("global") && IsGlobalAdmin, Context);

                    if (Ids.Count == 0)
                    {
                        await ReplyAsync(Language.UserNotFoundAdmin.Prepare(new Dictionary<string, string>()
                        {
                            { "%IDENTIFIER%", string.Join(" ", Value) }
                        }));
                    }
                    else
                    {

                        if (Property == "add")
                        {
                            await AddAdminsAsync(false, Ids);
                        }
                        else if (Property == "remove")
                        {
                            await RemoveAdminsAsync(false, Ids);
                        }
                        else if (IsGlobalAdmin)
                        {
                            if (Property == "addglobal")
                            {
                                await AddAdminsAsync(true, Ids);
                            }
                            else if (Property == "removeglobal")
                            {
                                await RemoveAdminsAsync(true, Ids);
                            }
                            else
                            {
                                await SendBlockedAsync(IsAdmin, IsGlobalAdmin);
                            }
                        }
                        else
                        {
                            var Parsed = Tools.ParseIds(new string[1] { OriginalProperty }, true);
                            if (Parsed.Count > 0)
                            {
                                BlockedUser BlockedUser = null;

                                var Id = Parsed[0];
                                var Index = Global.Settings.GloballyBlocked.FindIndex(p => p.Id == Id);
                                
                                if (Index > 0)
                                {
                                    BlockedUser = Global.Settings.GloballyBlocked[Index];
                                }
                                else
                                {
                                    Index = Settings.Blocked.FindIndex(p => p.Id == Id);
                                    if (Index > 0)
                                    {
                                        BlockedUser = Settings.Blocked[Index];
                                    }
                                    else
                                    {
                                        await SendBlockedAsync(IsAdmin, IsGlobalAdmin);
                                    }
                                }

                                if (BlockedUser != null)
                                {
                                    var BlockedInfo = Language.BlockedInfo.Prepare(new Dictionary<string, string>()
                                    {
                                        { "%NAME%", Tools.GetName(BlockedUser.Id) },
                                        { "%ID%", BlockedUser.Id.ToString() },
                                        { "%BLOCKER%", Tools.GetName(BlockedUser.Who) },
                                        { "%BLOCKERID%", BlockedUser.Who.ToString() },
                                        { "%REASON", BlockedUser.Reason }
                                    });
                                    await Context.Channel.SendMessageAsync($"```css\n{ BlockedInfo }```");
                                }
                            }
                            else
                            {
                                await SendBlockedAsync(IsAdmin, IsGlobalAdmin);
                            }

                        }
                    }
                }
                else
                {
                    await SendBlockedAsync(IsAdmin, IsGlobalAdmin);
                }
            }
            else
            {
                await SendBlockedAsync(IsAdmin, IsGlobalAdmin);
            }
        }

        [Command("delete"), Summary("Management"), Admin()]
        public async Task DeleteAsync(params string[] Args)
        {
            var SendNotification = true;
            var RemoveMessage = false;

            int MessageCount = 0;
            var Ids = new List<ulong>();

            for (int i = 0; i < Args.Length; i++)
            {
                var Arg = Args[i];
                var ArgLower = Arg.ToLower();
                if (ArgLower.StartsWith("notify:") && Arg.Length > 7)
                {
                    var Result = ArgLower.Split(':')[1];

                    if (Result == "false" || Result == "no" || Result == "off")
                    {
                        SendNotification = false;
                    }
                }
                else if (ArgLower.StartsWith("count:") && Arg.Length > 6)
                {
                    var Result = Arg.Split(':')[1];
                    int.TryParse(Result, out MessageCount);
                }
                else if (ArgLower.StartsWith("ids:") && Arg.Length > 4)
                {
                    if (Arg.Length > 4)
                    {
                        var IdsRaw = Arg.Split(':')[1].Split(',');
                        foreach (var IdRaw in IdsRaw)
                        {
                            if (ulong.TryParse(IdRaw, out ulong Id))
                            {
                                Ids.Add(Id);
                            }
                        }
                    }
                }
                else if (ArgLower.StartsWith("selfremove:"))
                {
                    if (ArgLower.Length > 11)
                    {
                        var Result = ArgLower.Split(':')[1];
                        if (Result == "true" || Result == "yes" || Result == "on")
                        {
                            RemoveMessage = true;
                        }
                    }
                }
            }

            if (RemoveMessage)
            {
                await Context.Message.DeleteAsync();
            }

            if (Ids.Count > 0)
            {
                var Deleted = await DeleteMessages(Context.Channel as ITextChannel, Ids);
                if (SendNotification)
                {
                    var Message = "```css\n";
                    if (Deleted.Item1.Count != 0)
                    {
                        Message += Language.MessageIdsDeleted.Prepare(new Dictionary<string, string>()
                        {
                            { "%IDS", "[" + string.Join(", ", Deleted.Item1) + "]" }
                        });
                    }
                    if (Deleted.Item2.Count > 0)
                    {
                        if (Deleted.Item1.Count > 0)
                        {
                            Message += "\n";
                        }
                        Message += Language.MessageIdsNotDeleted.Prepare(new Dictionary<string, string>()
                        {
                            { "%IDS", "[" + string.Join(", ", Deleted.Item2) + "]" }
                        });
                    }
                    Message += "```";
                    await Context.Channel.SendMessageAsync(Message);
                }
            }
            if (MessageCount == 0 && Ids.Count == 0)
            {
                await Context.Channel.SendMessageAsync(Context.Prepare(Language.MessageDeleteHelp));
            }
            else
            {
                if (MessageCount == 0)
                {
                    return;
                }
                await DeleteMessages(Context.Channel as ITextChannel, MessageCount);
                if (SendNotification)
                {
                    await Context.Channel.SendMessageAsync(Language.MessageCountDeleted.Prepare(new Dictionary<string, string>()
                    {
                        { "%COUNT%", MessageCount.ToString() }
                    }));
                }
            }
        }

        [Command("highlight"), Summary("Management")]
        public async Task HighlightAsync(params string[] Args)
        {
            var UserMessages = new List<IMessage>();
            
            IGuildUser User = null;

            string Arg = string.Join(" ", Args);
            bool IdParsed = false;

            if (Arg == "")
            {
                await Context.Channel.SendMessageAsync(Context.Prepare(Language.HighlightProvideArgs));
            }
            else
            {
                IdParsed = ulong.TryParse(Arg, out ulong Id);
                if (IdParsed)
                {
                    var Message = await Context.Channel.GetMessageAsync(Id);
                    if (Message == null)
                    {
                        User = await Context.Guild.GetUserAsync(Id);
                        if (User == null)
                        {
                            await Context.Channel.SendMessageAsync(Context.Prepare(Language.HighlightProvideArgs));
                            return;
                        }
                    }
                    else
                    {
                        UserMessages.Add(Message);
                    }
                }
                else
                {
                    var Users = Tools.ParseUsers(Args, false, Context);
                    if (Users.Count == 1)
                    {
                        User = Users[0];
                    }
                }
                
                if (User != null)
                {
                    IEnumerable<IMessage> Messages = null;
                    ulong LastId = 0;
                    do
                    {
                        if (LastId != 0)
                            Messages = await Context.Channel.GetMessagesAsync(LastId, Direction.Before, 100).Flatten();
                        else
                            Messages = await Context.Channel.GetMessagesAsync(100).Flatten();

                        int i;

                        for (i = 0; i < Messages.Count(); i++)
                        {
                            var Message = Messages.ElementAt(i);
                            if (Message.Author.Id == User.Id)
                            {
                                UserMessages.Add(Message);
                            }
                            else if (UserMessages.Count > 0)
                            {
                                break;
                            }
                        }
                        if (i != Messages.Count() - 1 || Messages.Last().Id != UserMessages.Last().Id)
                        {
                            break;
                        }
                    }
                    while (Messages.Count() == 100);
                }

                if (UserMessages.Count == 0)
                {
                    if (User != null)
                    {
                        await Context.Channel.SendMessageAsync(Language.HighlightNotFound.Prepare(new Dictionary<string, string>()
                        {
                            { "%USER%", User.Nickname ?? User.Username }
                        }));
                    }
                    else
                    {
                        if (IdParsed)
                        {
                            await Context.Channel.SendMessageAsync(Language.HighlightUserOrMessageNotFound.Prepare(new Dictionary<string, string>()
                            {
                                { "%ID%", Arg }
                            }));
                        }
                        else
                        {
                            await Context.Channel.SendMessageAsync(Language.HighlightUserNotFound.Prepare(new Dictionary<string, string>()
                            {
                                { "%PROPERTY%", Arg }
                            }));
                        }
                    }
                }
                else
                {
                    User = Global.Client.GetGuild(Context.Guild.Id).GetUser(UserMessages[0].Author.Id);
                    if (UserMessages.Count == 1)
                        await Context.Channel.SendMessageAsync("", embed: CreateEmbed(User, UserMessages[0]));
                    else
                        await Context.Channel.SendMessageAsync("", embed:CreateStackedEmbed(User, UserMessages.OrderBy(t => t.CreatedAt)));
                }
            }

        }

        [Command("setnsfw"), Summary("Management"), Admin()]
        public async Task SetNsfwAsync(params string[] Args)
        {
            if (Context.Guild == null)
            {
                await Context.Channel.SendMessageAsync(Language.DmIsNsfw);
                return;
            }
            if (Args.Length == 1 && Global.IsAdminOrHigher(Context.User.Id, Context.Guild, Settings))
            {
                bool SetToNsfw = false;
                
                var CheckableText = Args[0].ToLower();
                if (CheckableText == "1"
                 || CheckableText == "yes"
                 || CheckableText == "true"
                 || CheckableText == "ya")
                    SetToNsfw = true;

                Global.GuildSettings.Modify(Settings.GuildId, Modify =>
                {
                    if (SetToNsfw)
                    {
                        if (!Modify.NsfwChannels.Contains(Context.Channel.Id))
                        {
                            Modify.NsfwChannels.Add(Context.Channel.Id);
                        }
                    }
                    else
                    {
                        if (Modify.NsfwChannels.Contains(Context.Channel.Id))
                        {
                            Modify.NsfwChannels.Remove(Context.Channel.Id);
                        }
                    }
                });

                await Context.Channel.SendMessageAsync(Language.NsfwStateChanged.Prepare(new Dictionary<string, string>()
                {
                    { "%STATE%", SetToNsfw ? Language.Nsfw : Language.Sfw }
                }));
            }
            else
            {
                await Context.Channel.SendMessageAsync(Language.NsfwStateChanged.Prepare(new Dictionary<string, string>()
                {
                    { "%STATE%", Global.IsNsfwChannel(Settings, Context.Channel.Id) ? Language.Nsfw : Language.Sfw }
                }));
            }
        }

        [Command("say"), Summary("Management"), Admin()]
        public async Task SayAsync(params string[] Args)
        {
            if (Args.Length > 0)
            {
                var Option = Args[0].ToLower();
                var Value = "";
                
                if (Args.Length > 1)
                    Value = string.Join(" ", Args.Skip(1));

                string Message = null;
                ITextChannel txtChannel = Context.Channel as ITextChannel;
                
                if (Option == "listen")
                {
                    if (ulong.TryParse(Value, out ulong ChannelId))
                    {
                        if (Global.Client.GetChannel(ChannelId) is ITextChannel Channel)
                        {
                            if (Global.Settings.SayPreferences.ContainsKey(Context.User.Id))
                            {
                                Global.Settings.SayPreferences[Context.User.Id].Listening.Add(Context.Channel.Id, ChannelId);
                            }
                            else
                            {
                                Global.Settings.SayPreferences.Add(Context.User.Id, new SayPreferences()
                                {
                                    UserId = Context.User.Id,
                                    Listening = new Dictionary<ulong, ulong>()
                                    {
                                        { Context.Channel.Id, ChannelId }
                                    }
                                });
                            }

                            await Context.Channel.SendMessageAsync(Language.SayListening.Prepare(new Dictionary<string, string>()
                            {
                                { "%CHANNEL%", Channel.Name }
                            }, User: Context.User));

                            Global.SaveSettings();
                            return;
                        }
                    }
                    else if (Value == "stop")
                    {
                        if (Global.Settings.SayPreferences.ContainsKey(Context.User.Id))
                        {
                            if (Global.Settings.SayPreferences[Context.User.Id].Listening.ContainsKey(Context.Channel.Id))
                            {
                                Global.Settings.SayPreferences[Context.User.Id].Listening.Remove(Context.Channel.Id);
                                await Context.Channel.SendMessageAsync(Language.SayNotListening);
                                return;
                            }
                        }
                    }
                }
                else if (Option == "autodel")
                {
                    Value = Value.ToLower();
                    if (Value == "true" || Value == "yes" || Value == "ya" || Value == "1")
                    {
                        if (Global.Settings.SayPreferences.ContainsKey(Context.User.Id))
                        {
                            Global.Settings.SayPreferences[Context.User.Id].AutoDel = true;
                        }
                        else
                        {
                            Global.Settings.SayPreferences.Add(Context.User.Id, new SayPreferences()
                            {
                                AutoDel = true,
                                UserId = Context.User.Id
                            });
                        }
                        await Context.Channel.SendMessageAsync(Language.SayAutoDelChanged.Prepare(new Dictionary<string, string>()
                        {
                            { "%STATE%", Language.On }
                        }));
                        Global.SaveSettings();
                        return;
                    }
                    else if (Value == "false" || Value == "no" || Value == "nai" || Value == "0")
                    {
                        if (Global.Settings.SayPreferences.ContainsKey(Context.User.Id))
                        {
                            Global.Settings.SayPreferences[Context.User.Id].AutoDel = false;
                        }
                        else
                        {
                            Global.Settings.SayPreferences.Add(Context.User.Id, new SayPreferences()
                            {
                                AutoDel = false,
                                UserId = Context.User.Id
                            });
                        }
                        await Context.Channel.SendMessageAsync(Language.SayAutoDelChanged.Prepare(new Dictionary<string, string>()
                        {
                            { "%STATE%", Language.Off }
                        }));
                        Global.SaveSettings();
                        return;
                    }
                }
                else if (ulong.TryParse(Option, out ulong Id))
                {
                    if (Global.Client.GetChannel(Id) is ITextChannel Channel)
                    {
                        txtChannel = Channel;
                        Message = string.Join(" ", Args.Skip(1));
                    }
                    else
                    {
                        Message = string.Join(" ", Args);
                    }
                }
                else
                {
                    Message = string.Join(" ", Args);
                }

                if (Message != null)
                {
                    var Regex = new Regex("");
                    await txtChannel.SendMessageAsync(Message);
                    await SayAutoDeleteAsync();
                    return;
                }
            }
            await Context.Channel.SendMessageAsync(Context.Prepare(Language.SayHelp));
        }

        #region Help
        private async Task SendAdminsAsync(bool IsGlobalAdmin, bool IsOwner)
        {
            var Admins = new Dictionary<string, List<string>>()
            {
                { Language.Admin, Global.AdminsAt(Context.Guild != null ? Context.Guild.Id : 0) },
                { Language.GlobalAdmin, Global.GlobalAdmins() },
                { Language.Owner, new List<string>() { Global.OwnerName() } }
            };
            var Message = "```css\n";
            foreach (var AdminType in Admins)
            {
                if (AdminType.Value.Count != 0)
                {
                    Message += $"--[{ AdminType.Key }]--\n";
                    Message += string.Join(", ", AdminType.Value) + "\n\n";
                }
            }
            if (Message == "```css\n")
            {
                Message += Context.Prepare(Language.HasNoAdmin) + "\n\n";
            }

            if (IsGlobalAdmin || IsOwner)
            {
                Message += Context.Prepare(Language.GlobalAdminHelp) + "\n";
            }
            if (IsOwner)
            {
                Message += "\n" + Context.Prepare(Language.OwnerAdminHelp) + "\n";
            }
            if (IsGlobalAdmin || IsOwner)
            {
                Message += "\n\n" + Context.Prepare(Language.NameSafety);
            }

            Message += "```";
            await ReplyAsync(Message);
        }
        private async Task SendBlockedAsync(bool IsAdmin, bool IsGlobalAdmin)
        {
            var BlockedUsers = new Dictionary<string, List<string>>();

            var Id = Context.Guild != null ? Context.Guild.Id : 0;
            if (Id == 0)
            {
                BlockedUsers.Add(Language.GloballyBlocked, Tools.GetNames(Global.Blocked()));
            }
            else
            {
                BlockedUsers.Add(Language.BlockedAtGuild, Tools.GetNames(Global.Blocked(Id)));
                BlockedUsers.Add(Language.GloballyBlocked, Tools.GetNames(Global.Blocked()));
            }
            
            var Message = "```css\n";
            foreach (var BlockedUser in BlockedUsers)
            {
                if (BlockedUser.Value.Count != 0)
                {
                    Message += $"--[{ BlockedUser.Key }]--\n";
                    Message += string.Join(", ", BlockedUser.Value) + "\n\n";
                }
            }
            if (Message == "```css\n")
            {
                Message += Context.Prepare(Language.NooneBlocked) + "\n\n";
            }

            if (IsAdmin)
            {
                Message += Context.Prepare(Language.BlockHelp) + "\n";
            }
            if (IsGlobalAdmin)
            {
                Message += "\n" + Context.Prepare(Language.GlobalBlockHelp) + "\n";
            }
            if (IsAdmin)
            {
                Message += "\n\n" + Context.Prepare(Language.NameSafety);
            }

            Message += "```";
            await ReplyAsync(Message);
        }
        #endregion
        #region Admin Management
        private async Task AddAdminsAsync(bool Globally, List<ulong> Ids)
        {
            var ToAdd = new List<UserCredential>();
            var AlreadyAdmin = new List<string>();
            
            var Message = "```css";

            foreach (var Id in Ids)
            {
                UserCredential Credential = null;
                if (Global.IsAdminOrHigher(Id, Context.Guild, Settings))
                {
                    if (Global.IsGlobalAdminOrHigher(Id) || (!Globally && Global.IsAdmin(Id, Settings)))
                    {
                        AlreadyAdmin.Add(Tools.GetName(Id));
                        continue;
                    }
                    else
                    {
                        Global.GuildSettings.Modify(Context.Guild.Id, Setting =>
                        {
                            var Index = Settings.Admins.FindIndex(t => t.Id == Id);
                            Credential = Settings.Admins[Index];
                            Settings.Admins.RemoveAt(Index);
                        });
                    }
                }
                if (Credential == null)
                {
                    Credential = new UserCredential()
                    {
                        Id = Id,
                        Password = GeneratePassword()
                    };
                }
                ToAdd.Add(Credential);
            }

            if (AlreadyAdmin.Count > 0)
            {
                Message += $"\n--[{ Language.AlreadyAdminsTag }]--\n";
                Message += string.Join(", ", AlreadyAdmin) + "\n";
            }
            if (ToAdd.Count > 0)
            {
                Message += $"\n--[{ Language.NewAdminsTag }]--\n";
                var UsernameList = Tools.GetNames(ToAdd);
                Message += string.Join(", ", UsernameList) + "\n";

                if (Globally)
                {
                    Global.Settings.GlobalAdmins.AddRange(ToAdd);
                    Global.SaveSettings();
                }
                else
                {
                    Global.GuildSettings.Modify(Context.Guild.Id, Setting =>
                    {
                        Setting.Admins.AddRange(ToAdd);
                    });
                }
            }

            Message += "```";
            await Context.Channel.SendMessageAsync(Message);
        }
        private async Task RemoveAdminsAsync(bool Globally, List<ulong> Ids)
        {
            var Removed = new List<string>();
            var NotAdmin = new List<string>();

            for (int i = 0; i < Ids.Count; i++)
            {
                var Id = Ids[i];
                if (Globally)
                {
                    if (Global.IsGlobalAdmin(Id))
                    {
                        var Index = Global.Settings.GlobalAdmins.FindIndex(t => t.Id == Id);
                        Removed.Add(Tools.GetName(Id));
                        Global.Settings.GlobalAdmins.RemoveAt(Index);
                    }
                    else
                    {
                        NotAdmin.Add(Tools.GetName(Id));
                    }
                }
                else
                {
                    if (Global.IsAdmin(Id, Settings))
                    {
                        Global.GuildSettings.Modify(Id, Settings =>
                        {
                            var Index = Settings.Admins.FindIndex(t => t.Id == Id);
                            Settings.Admins.RemoveAt(Index);
                        });
                        Removed.Add(Tools.GetName(Id));
                    }
                    else
                    {
                        NotAdmin.Add(Tools.GetName(Id));
                    }
                }
            }
            Global.SaveSettings();

            var Message = "```css";

            if (NotAdmin.Count > 0)
            {
                Message += $"\n--[{ Language.NotAdminsTag }]--\n";
                Message += string.Join(", ", NotAdmin) + "\n";
            }
            if (Removed.Count > 0)
            {
                Message += $"\n--[{ Language.RemovedAdminsTag }]--\n";
                Message += string.Join(", ", Removed) + "\n";
            }

            Message += "```";
            await Context.Channel.SendMessageAsync(Message);

        }
        #endregion
        #region Block Management
        private async Task BlockUserAsync(bool Globally, ulong Id, ulong Who, string Reason = "")
        {
            var Message = "```css\n";
            
            if (Global.IsBlocked(Id, Context.Guild.Id))
            {
                Message += Language.AlreadyBlocked.Prepare(new Dictionary<string, string>()
                {
                    { "%NAME%", Tools.GetName(Id) }
                });
            }
            else
            {
                var Blocked = new BlockedUser()
                {
                    Id = Id,
                    Who = Who,
                    Reason = Reason
                };

                if (Globally)
                {
                    Global.Settings.GloballyBlocked.Add(Blocked);
                    Global.SaveSettings();
                }
                else
                {
                    Global.GuildSettings.Modify(Settings.GuildId, (Settings) =>
                    {
                        Settings.Blocked.Add(Blocked);
                    });
                }

                Message += Language.NewBlocked.Prepare(new Dictionary<string, string>()
                {
                    { "%NAME%", Tools.GetName(Id) }
                });
            }

            Message += "```";
            await Context.Channel.SendMessageAsync(Message);
        }
        private async Task RemoveAdminsAsync(bool Globally, ulong Id)
        {
            var Message = "```css\n";
            var SettingsId = Context.Guild == null ? Context.Channel.Id : Context.Guild.Id;

            if (Global.IsBlocked(Id, (Globally ? 0 : SettingsId)))
            {
                if (Globally)
                {
                    var Index = Global.Settings.GloballyBlocked.FindIndex(p => p.Id == Id);
                    Global.Settings.GloballyBlocked.RemoveAt(Index);
                    Global.SaveSettings();
                }
                else
                {
                    Global.GuildSettings.Modify(SettingsId, (Settings) =>
                    {
                        var Index = Settings.Blocked.FindIndex(p => p.Id == Id);
                        Settings.Blocked.RemoveAt(Index);
                    });
                }

                Message += Language.FreeBlock.Prepare(new Dictionary<string, string>()
                {
                    { "%NAME%", Tools.GetName(Id) }
                });
            }
            else
            {
                Message += Language.NotBlocked.Prepare(new Dictionary<string, string>()
                {
                    { "%NAME%", Tools.GetName(Id) }
                });
            }

            Message += "```";
            await Context.Channel.SendMessageAsync(Message);
        }
        #endregion
        #region Highlight
        private Embed CreateStackedEmbed(IGuildUser User, IEnumerable<IMessage> Messages)
        {
            var EmbedBuilder = new EmbedBuilder();
            EmbedBuilder.WithAuthor(User.Nickname ?? User.Username, User.GetAvatarUrl(ImageFormat.Png, 2048));
            EmbedBuilder.WithColor(GetHighestRoleColor(User));

            foreach (var Message in Messages)
            {
                EmbedBuilder.Description += Message.Content;
                for (int i = 0; i < Message.Attachments.Count; i++)
                {
                    var Attachment = Message.Attachments.ElementAt(i);
                    if (string.IsNullOrWhiteSpace(EmbedBuilder.ImageUrl))
                    {
                        var Extension = Attachment.Filename.Split('.').Last();
                        if (Global.Settings.ImageExtensions.Contains(Extension.ToLower()))
                        {
                            EmbedBuilder.WithImageUrl(Attachment.Url);
                            continue;
                        }
                    }
                    if (EmbedBuilder.Description.Trim() != "")
                        EmbedBuilder.Description += "\n";

                    EmbedBuilder.Description += $"#{ i + 1 }: [{ Attachment.Filename }]({ Attachment.Url }) ({ Attachment.Size / 1024 }KB)";
                }
                EmbedBuilder.Description += "\n";
            }

            var Last = Messages.Last();

            var Date = $"{ Format(Last.CreatedAt.Month) }/{ Format(Last.CreatedAt.Day) }/{ Last.CreatedAt.Year } { Format(Last.CreatedAt.Hour) }:{ Format(Last.CreatedAt.Minute) }:{ Format(Last.CreatedAt.Second) }";

            EmbedBuilder.WithFooter("in #" + Last.Channel.Name + " at: " + Date);

            return EmbedBuilder.Build();
        }
        private Embed CreateEmbed(IGuildUser User, IMessage Message)
        {
            var EmbedBuilder = new EmbedBuilder();
            EmbedBuilder.WithAuthor(User.Nickname ?? User.Username, User.GetAvatarUrl(ImageFormat.Png, 2048));
            EmbedBuilder.WithColor(GetHighestRoleColor(User));

            EmbedBuilder.WithDescription(Message.Content);
            for (int i = 0; i < Message.Embeds.Count; i++)
            {
                var Embed = Message.Embeds.ElementAt(i);

                if (EmbedBuilder.Description.Trim() != "")
                    EmbedBuilder.Description += "\n\n";

                var AttachUrl = "";

                if (string.IsNullOrWhiteSpace(EmbedBuilder.ImageUrl))
                {
                    if (Embed.Image.HasValue)
                    {
                        EmbedBuilder.WithImageUrl(Embed.Image.Value.Url);
                    }
                }
                if (string.IsNullOrWhiteSpace(EmbedBuilder.ThumbnailUrl))
                {
                    if (Embed.Thumbnail.HasValue)
                    {
                        EmbedBuilder.WithThumbnailUrl(Embed.Thumbnail.Value.Url);
                    }
                }
                if (string.IsNullOrWhiteSpace(EmbedBuilder.Url))
                {
                    if (!string.IsNullOrWhiteSpace(Embed.Url))
                    {
                        EmbedBuilder.WithUrl(Embed.Url);
                    }
                }

                foreach (var Field in Embed.Fields)
                {
                    EmbedBuilder.AddField(Field.Name, Field.Value, Field.Inline);
                }

                if (!string.IsNullOrWhiteSpace(Embed.Title) || !string.IsNullOrWhiteSpace(Embed.Description))
                {
                    var First = "";
                    var Second = "";
                    var Third = "";

                    if (!string.IsNullOrWhiteSpace(Embed.Title))
                    {
                        var Title = AttachUrl == "" ? Embed.Title : $"[{ Embed.Title }]({ AttachUrl })";
                        First = $"\n{ Language.HighlightTitle }: { Title }";
                    }
                    if (!string.IsNullOrWhiteSpace(Embed.Description))
                    {
                        Second = $"\n{ Language.HighlightDescription }: { Embed.Description }";
                    }
                    if (Embed.Footer.HasValue)
                    {
                        Third = $"\n{ Language.HighlightFooter }: { Embed.Footer.Value.Text }";
                    }

                    if (Message.Embeds.Count == 1)
                    {
                        EmbedBuilder.Description += First + Second + Third;
                    }
                    else
                    {
                        EmbedBuilder.Description += $"#{ i + 1 } { Language.HighlightEmbed }:{ First }{ Second }{ Third }";

                    }
                }
            }
            for (int i = 0; i < Message.Attachments.Count; i++)
            {
                var Attachment = Message.Attachments.ElementAt(i);
                if (string.IsNullOrWhiteSpace(EmbedBuilder.ImageUrl))
                {
                    var Extension = Attachment.Filename.Split('.').Last();
                    if (Global.Settings.ImageExtensions.Contains(Extension.ToLower()))
                    {
                        EmbedBuilder.WithImageUrl(Attachment.Url);
                        continue;
                    }
                }
                if (EmbedBuilder.Description.Trim() != "")
                    EmbedBuilder.Description += "\n";

                EmbedBuilder.Description += $"#{ i + 1 }: [{ Attachment.Filename }]({ Attachment.Url }) ({ Attachment.Size / 1024 }KB)";
            }
            
            var Date = $"{ Format(Message.CreatedAt.Month) }/{ Format(Message.CreatedAt.Day) }/{ Message.CreatedAt.Year } { Format(Message.CreatedAt.Hour) }:{ Format(Message.CreatedAt.Minute) }:{ Format(Message.CreatedAt.Second) }";

            EmbedBuilder.WithFooter("in #" + Message.Channel.Name + " at: " + Date);

            return EmbedBuilder.Build();
        }
        private string Format(int Number)
        {
            if (Number < 10)
                return "0" + Number;

            return Number + "";
        }
        private uint GetHighestRoleColor(IGuildUser User)
        {
            uint RawColor = Color.Default.RawValue;

            foreach (var RoleId in User.RoleIds.Reverse())
            {
                var Role = User.Guild.GetRole(RoleId);

                if (Role.Color.RawValue != Color.Default.RawValue)
                {
                    RawColor = Role.Color.RawValue;
                    break;
                }
            }

            return RawColor;
        }
        #endregion
        #region Delete
        private async Task SayAutoDeleteAsync()
        {
            if (Global.Settings.SayPreferences.ContainsKey(Context.User.Id))
            {
                if (Global.Settings.SayPreferences[Context.User.Id].AutoDel)
                {
                    var Dm = await Context.User.GetOrCreateDMChannelAsync();
                    if (Dm.Id != Context.Channel.Id)
                    {
                        await Context.Message.DeleteAsync();
                    }
                }
            }
        }

        private async Task DeleteMessages(ITextChannel Channel, int Count)
        {
            var Messages = await Channel.GetMessagesAsync(Count).Flatten();
            foreach (var Message in Messages)
            {
                await Message.DeleteAsync();
            }
        }
        private async Task<Tuple<List<ulong>, List<ulong>>> DeleteMessages(ITextChannel Channel, IEnumerable<ulong> Ids)
        {
            var Deleted = new List<ulong>();
            var NonDeleted = new List<ulong>();

            foreach (var Id in Ids)
            {
                var Message = await Channel.GetMessageAsync(Id);
                if (Message != null)
                {
                    await Message.DeleteAsync();
                    Deleted.Add(Id);
                }
                else
                {
                    NonDeleted.Add(Id);
                }
            }
            return new Tuple<List<ulong>, List<ulong>>(Deleted, NonDeleted);
        }
        #endregion
        #region Untility
        private string GeneratePassword(int Length = 10)
        {
            string Password = "";
            for (var i = 0; i < Length; i++)
            {
                var Char = (char)Global.Random.Next(32, 127);
                Password += Char;
            }
            return Password;
        }
        #endregion
    }
}
