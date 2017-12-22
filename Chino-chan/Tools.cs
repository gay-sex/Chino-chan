using Chino_chan.Models.Settings;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Chino_chan
{
    public static class Tools
    {
        public static string ConvertHighlightsBack(string Input)
        {
            var Base = Input;
            var Regex = GetMentionFinderRegex();
            foreach (Match Match in Regex.Matches(Input))
            {
                var Name = GetName(GetId(Match.Value));
                Base = Base.Replace(Match.Value, Match.Value.Substring(1, 1) + Name);
            }
            return Base;
        }

        public static List<string> GetNames(string Input)
        {
            var Ids = GetIds(Input);
            var Names = new List<string>();

            foreach (var Id in Ids)
            {
                if (Global.Client.GetChannel(Id) is IGuildChannel Channel)
                {
                    Names.Add(Channel.Name);
                }
                else if (Global.Client.GetUser(Id) is IUser User)
                {
                    if (User is IGuildUser GuildUser)
                    {
                        Names.Add(GuildUser.Nickname ?? GuildUser.Username);
                    }
                    else
                    {
                        Names.Add(User.Username);
                    }
                }
                else if (Global.Client.GetGuild(Id) is IGuild Guild)
                {
                    Names.Add(Guild.Name);
                }
            }

            return Names;
        }
        public static List<string> GetNames(List<ulong> Ids)
        {
            var List = new List<string>();
            for (var i = 0; i < Ids.Count; i++)
            {
                List.Add(GetName(Ids[i]));
            }
            return List;
        }
        public static List<string> GetNames(List<UserCredential> Credentials)
        {
            var List = new List<string>();
            for (var i = 0; i < Credentials.Count; i++)
            {
                List.Add(GetName(Credentials[i].Id));
            }
            return List;
        }

        public static List<ulong> GetIds(string Input)
        {
            List<ulong> Ids = new List<ulong>();
            var Result = GetMentionFinderRegex().Matches(Input);
            foreach (Match Match in Result)
            {
                if (Match.Success)
                {
                    Ids.Add(GetId(Match.Value));
                }
            }
            return Ids;
        }

        public static string GetName(ulong Id)
        {
            if (Global.Client.GetChannel(Id) is IGuildChannel Channel)
            {
                return Channel.Name;
            }
            else if (Global.Client.GetUser(Id) is IUser User)
            {
                if (User is IGuildUser GuildUser)
                {
                    return GuildUser.Nickname ?? GuildUser.Username;
                }
                else
                {
                    return User.Username;
                }
            }
            else if (Global.Client.GetGuild(Id) is IGuild Guild)
            {
                return Guild.Name;
            }
            return "<#" + Id.ToString() + ">";
        }

        public static ulong GetId(string Section)
        {
            return ulong.Parse(Section.Substring(2, Section.Length - 3));
        }

        public static Regex GetMentionFinderRegex()
        {
            return new Regex(@"<[@|#]\d*>");
        }

        public static List<ulong> ParseIds(string[] Value, bool SearchGlobally, ICommandContext Context = null)
        {
            return ParseUsers(Value, SearchGlobally, Context).Select(t => t.Id).ToList();
        }
        public static List<IGuildUser> ParseUsers(string[] Value, bool SearchGlobally, ICommandContext Context = null)
        {
            var ReturnedUsers = new List<IGuildUser>();
            var Part = "";
            var Users = new List<IGuildUser>();

            if (SearchGlobally)
            {
                foreach (var Guild in Global.Client.Guilds)
                {
                    Users.AddRange(Guild.Users);
                }
            }
            else
            {
                if (Context != null)
                {
                    if (Context.Guild != null)
                    {
                        Users.AddRange(Context.Guild.GetUsersAsync().Result);
                    }
                }
            }

            for (int i = 0; i < Value.Length; i++)
            {
                if (Part != "")
                    Part += " ";

                Part += Value[i].ToLower();

                foreach (var User in Users)
                {
                    if (ulong.TryParse(Part, out ulong ParsedId))
                    {
                        if (User.Id == ParsedId)
                        {
                            ReturnedUsers.Add(User as IGuildUser);
                            Part = "";
                            continue;
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(User.Nickname))
                    {
                        if (User.Nickname.ToLower() == Part)
                        {
                            ReturnedUsers.Add(User);
                            Part = "";
                            continue;
                        }
                    }
                    if (User.Username.ToLower() == Part)
                    {
                        ReturnedUsers.Add(User);
                        Part = "";
                        continue;
                    }
                }
            }
            return ReturnedUsers;
        }
    }
}
