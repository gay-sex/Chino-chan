using Chino_chan.Models.Images;
using Chino_chan.Models.Language;
using Chino_chan.Models.Privillages;
using Chino_chan.Models.Settings;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chino_chan.Commands
{
    public class Fun : ModuleBase
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

        [Command("avatar"), Summary("Fun")]
        public async Task GetAvatarAsync(params string[] Args)
        {
            var Ids = Tools.ParseIds(Args, true);
            if (Ids.Count != 0)
            {
                EmbedBuilder Builder = new EmbedBuilder();
                for (int i = 0; i < Ids.Count; i++)
                {
                    IUser User = null;
                    Color Color = new Color(255, 0, 203);

                    if (Context.Guild != null)
                    {
                        if (Global.Client.GetGuild(Context.Guild.Id) is SocketGuild Guild)
                        {
                            if (Guild.GetUser(Ids[i]) is SocketGuildUser SocketUser)
                            {
                                User = SocketUser;
                                for (int j = 0; j < SocketUser.Roles.Count; j++)
                                {
                                    var Role = SocketUser.Roles.ElementAt(j);
                                    if (Role.Color.RawValue != Color.Default.RawValue)
                                    {
                                        Color = Role.Color;
                                    }
                                }
                            }
                        }
                    }

                    if (User == null)
                    {
                        if (Global.Client.GetUser(Ids[i]) is SocketUser ClientUser)
                        {
                            User = ClientUser;
                        }
                    }

                    if (User != null)
                    {
                        var Url = User.GetAvatarUrl(ImageFormat.Png, 1024);

                        Builder.WithAuthor(User);
                        Builder.WithColor(Color);

                        Builder.WithImageUrl(Url);
                        Builder.WithUrl(Url);

                        await Context.Channel.SendMessageAsync("", embed: Builder.Build());
                    }
                }
            }
            else
            {
                await Context.Channel.SendMessageAsync(Language.UserNotFound.Prepare(new Dictionary<string, string>()
                {
                    { "%IDENTIFIER%", string.Join(" ", Args) }
                }));
            }
        }

        [Command("waifu"), Summary("Fun")]
        public async Task WaifuAsync(params string[] Args)
        {
            await Context.Channel.SendMessageAsync("no.");
        }

        [Command("sankaku"), Summary("Fun")]
        public async Task SankakuAsync(params string[] Args)
        {
            SankakuResponse Image;

            if (Args.Length == 0)
            {
                if (!Global.IsNsfwChannel(Settings, Context.Channel.Id))
                    Image = await Global.Sankaku.GetImageAsync(new string[] { "rating:safe" });
                else
                    Image = await Global.Sankaku.GetImageAsync(new string[] { });
            }
            else
            {
                var Tags = new List<string>(Args);
                var Arg = string.Join(" ", Args).ToLower();
                if (!Global.IsNsfwChannel(Settings, Context.Channel.Id))
                {
                    if (Arg.Contains("rating:explicit") || Arg.Contains("rating:questionable"))
                    {
                        await Context.Channel.SendMessageAsync(Language.OnlyNsfw);
                        return;
                    }

                    if (!Arg.Contains("rating:safe"))
                    {
                        Tags.Add("rating:safe");
                    }
                }
                Image = await Global.Sankaku.GetImageAsync(Tags);
            }

            if (Image == null)
            {
                await Context.Channel.SendMessageAsync(Language.NoImages.Prepare(new Dictionary<string, string>()
                {
                    { "%TAGS%", string.Join(" ", Args) }
                }));
            }
            else
            {
                var Builder = new EmbedBuilder
                {
                    Color = new Color(254 << 16 | 217 << 8 | 175),
                    ImageUrl = Image.Url
                };
                await Context.Channel.SendMessageAsync("", embed: Builder.Build());
            }
        }
        
        [Command("danbooru"), Summary("Fun")]
        public async Task DanbooruAsync(params string[] Args)
        {
            DanbooruResponse Image;

            if (Args.Length == 0)
            {
                if (!Global.IsNsfwChannel(Settings, Context.Channel.Id))
                    Image = await Global.Danbooru.GetImageAsync(new string[] { "rating:safe" });
                else
                    Image = await Global.Danbooru.GetImageAsync(new string[] { });
            }
            else
            {
                var Tags = new List<string>(Args);
                var Arg = string.Join(" ", Args).ToLower();
                if (!Global.IsNsfwChannel(Settings, Context.Channel.Id))
                {
                    if (Arg.Contains("rating:explicit") || Arg.Contains("rating:questionable"))
                    {
                        await Context.Channel.SendMessageAsync(Language.OnlyNsfw);
                        return;
                    }

                    if (!Arg.Contains("rating:safe"))
                    {
                        Tags.Add("rating:safe");
                    }
                }
                Image = await Global.Danbooru.GetImageAsync(Tags);
            }
            if (Image == null)
            {
                await Context.Channel.SendMessageAsync(Language.NoImages.Prepare(new Dictionary<string, string>()
                {
                    { "%TAGS%", string.Join(" ", Args) }
                }));
            }
            else
            {
                var Builder = new EmbedBuilder
                {
                    Color = new Color(254 << 16 | 217 << 8 | 175),
                    ImageUrl = Image.Url
                };
                await Context.Channel.SendMessageAsync("", embed: Builder.Build());
            }
        }

        [Command("yandere"), Summary("Fun")]
        public async Task Yandereasync(params string[] Args)
        {
            YandereResponse Image;

            if (Args.Length == 0)
            {
                if (!Global.IsNsfwChannel(Settings, Context.Channel.Id))
                    Image = await Global.Yandere.GetImageAsync(new string[] { "rating:safe" });
                else
                    Image = await Global.Yandere.GetImageAsync(new string[] { });
            }
            else
            {
                var Tags = new List<string>(Args);
                var Arg = string.Join(" ", Args).ToLower();
                if (!Global.IsNsfwChannel(Settings, Context.Channel.Id))
                {
                    if (Arg.Contains("rating:explicit") || Arg.Contains("rating:questionable"))
                    {
                        await Context.Channel.SendMessageAsync(Language.OnlyNsfw);
                        return;
                    }

                    if (!Arg.Contains("rating:s"))
                    {
                        Tags.Add("rating:s");
                    }
                }
                Image = await Global.Yandere.GetImageAsync(Tags);
            }

            if (Image == null)
            {
                await Context.Channel.SendMessageAsync(Language.NoImages.Prepare(new Dictionary<string, string>()
                {
                    { "%TAGS%", string.Join(" ", Args) }
                }));
            }
            else
            {
                var Builder = new EmbedBuilder
                {
                    Color = new Color(254 << 16 | 217 << 8 | 175),
                    ImageUrl = Image.Url
                };
                await Context.Channel.SendMessageAsync("", embed: Builder.Build());
            }
        }

        [Command("gelbooru"), Summary("Fun")]
        public async Task GelbooruAsync(params string[] Args)
        {
            GelbooruResponse Image;

            if (Args.Length == 0)
            {
                if (!Global.IsNsfwChannel(Settings, Context.Channel.Id))
                    Image = await Global.Gelbooru.GetImageAsync(new string[] { "rating:safe" });
                else
                    Image = await Global.Gelbooru.GetImageAsync(new string[] { });
            }
            else
            {
                var Tags = new List<string>(Args);
                var Arg = string.Join(" ", Args).ToLower();
                if (!Global.IsNsfwChannel(Settings, Context.Channel.Id))
                {
                    if (Arg.Contains("rating:explicit") || Arg.Contains("rating:questionable"))
                    {
                        await Context.Channel.SendMessageAsync(Language.OnlyNsfw);
                        return;
                    }

                    if (!Arg.Contains("rating:safe"))
                    {
                        Tags.Add("rating:safe");
                    }
                }
                Image = await Global.Gelbooru.GetImageAsync(Tags);
            }
            if (Image == null)
            {
                await Context.Channel.SendMessageAsync(Language.NoImages.Prepare(new Dictionary<string, string>()
                {
                    { "%TAGS%", string.Join(" ", Args) }
                }));
            }
            else
            {
                var Builder = new EmbedBuilder
                {
                    Color = new Color(254 << 16 | 217 << 8 | 175),
                    ImageUrl = Image.Url
                };
                await Context.Channel.SendMessageAsync("", embed: Builder.Build());
            }
        }

        [Command("images"), Summary("Fun")]
        public async Task ImagesAsync(params string[] Args)
        {
            if (Global.Images.Count == 0)
            {
                await Context.Channel.SendMessageAsync(Language.NoAvailableImageFolder);
            }
            else
            {
                if (Args.Length == 1)
                {
                    var ImageType = Args[0].ToLower();
                    if (Global.Images.Images.ContainsKey(ImageType))
                    {
                        var Count = Global.Images.Images[ImageType].Count;
                        await Context.Channel.SendMessageAsync(Language.ImageCount.Prepare(new Dictionary<string, string>()
                        {
                            { "%IMAGE%", ImageType },
                            { "%COUNT%", Count.ToString() }
                        }));
                        return;
                    }
                }

                await Context.Channel.SendMessageAsync(Language.Images.Prepare(new Dictionary<string, string>()
                {
                    { "%IMAGES%", string.Join(", ", Global.Images.Images.Select(t => t.Key)) }
                }));
            }
        }
        
        private async Task<List<string>> Delete(List<string> Filenames)
        {
            var _Filenames = new List<string>(Filenames);

            IEnumerable<IMessage> Messages = null;
            ulong LastMessageId = 0;
            do
            {
                if (LastMessageId != 0)
                {
                    Messages = await Context.Channel.GetMessagesAsync(LastMessageId, Direction.Before, 100).Flatten();
                }
                else
                {
                    Messages = await Context.Channel.GetMessagesAsync(LastMessageId, Direction.Before, 100).Flatten();
                }
                var LastMessage = Messages.Last();
                if (LastMessage.Id == LastMessageId)
                {
                    break;
                }
                else
                {
                    LastMessageId = LastMessage.Id;
                }
                foreach (var Message in Messages)
                {
                    if (Message.Author.Id != Global.Client.CurrentUser.Id)
                        continue;

                    if (Message.Attachments.Count > 0)
                    {
                        foreach (var Attachment in Message.Attachments)
                        {
                            if (_Filenames.Contains(Attachment.Filename))
                            {
                                _Filenames.Remove(Attachment.Filename);
                                await Message.DeleteAsync();
                                if (_Filenames.Count == 0)
                                {
                                    return null;
                                }
                            }
                        }
                    }
                }
            }
            while (Messages.Count() == 100);

            return _Filenames;
        }
    }
}
