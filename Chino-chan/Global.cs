using Chino_chan.Image;
using Chino_chan.Modules;
using Chino_chan.Models.Settings;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Troschuetz.Random;
using Troschuetz.Random.Generators;
using Google.Apis.Drive.v2;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;

namespace Chino_chan
{
    public class CPUInfo
    {
        private ManagementObjectSearcher Searcher { get; set; }

        public string Name { get; private set; }
        public string Socket { get; private set; }
        public string Description { get; private set; }
        public uint Speed { get; private set; }
        public uint L2 { get; private set; }
        public uint L3 { get; private set; }
        public uint Cores { get; private set; }
        public uint Threads { get; private set; }
        public ulong Percentage
        {
            get
            {
                var CPUPref = Searcher.Get().Cast<ManagementBaseObject>().FirstOrDefault();
                return (ulong)CPUPref["PercentProcessorTime"];
            }
        }

        public CPUInfo(ManagementBaseObject Info)
        {
            Socket = (string)Info["SocketDesignation"];
            Name = ((string)Info["Name"]).Replace("  ", " ");
            Description = (string)Info["Caption"];
            Speed = (uint)Info["MaxClockSpeed"];
            L2 = (uint)Info["L2CacheSize"];
            L3 = (uint)Info["L3CacheSize"];
            Cores = (uint)Info["NumberOfCores"];
            Threads = (uint)Info["NumberOfLogicalProcessors"];

            Searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PerfFormattedData_PerfOS_Processor");
        }
    }
    public class OsInfo
    {
        public string Name { get; private set; }
        public string Version { get; private set; }
        public string Architecture { get; private set; }

        public OsInfo(ManagementBaseObject Info)
        {
            Name = ((string)Info["Caption"]).Trim();
            Version = (string)Info["Version"];
            Architecture = (string)Info["OSArchitecture"];
        }
    }
    public class MemInfo
    {
        private PerformanceCounter Counter { get; set; }

        public ulong TotalMemory { get; private set; }
        public float CurrentMemory
        {
            get
            {
                return Counter.NextValue();
            }
        }

        public MemInfo(ManagementBaseObject Info)
        {
            TotalMemory = (ulong)Info["TotalPhysicalMemory"];
            Counter = new PerformanceCounter("Memory", "Available MBytes", true);
        }
    }
    public class VideoCardInfo
    {
        public string Name { get; private set; }
        public uint RAM { get; private set; }

        public VideoCardInfo(ManagementBaseObject Info)
        {
            Name = (string)Info["Name"];
            RAM = (uint)Info["AdapterRAM"];
        }
    }
    public static class Global
    {
        private static TimeSpan StartedTime;
        public static TimeSpan Uptime
        {
            get
            {
                return new TimeSpan(DateTime.Now.Ticks) - StartedTime;
            }
        }

        public static string SettingsFolder
        {
            get
            {
                return "Data";
            }
        }
        private static string SettingsPath
        {
            get
            {
                return SettingsFolder + "\\Settings.json";
            }
        }
        
        public static bool OSUAPIEnabled
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Settings.Credentials.osu.Token) && Settings.OSUAPICallLimit > 0;
            }
        }

        public static Settings Settings { get; private set; }
        public static GuildSettings GuildSettings { get; private set; }
        public static LanguageHandler LanguageHandler { get; private set; }
        public static DiscordSocketClient Client { get; private set; }
        public static CommandService CommandService { get; private set; }
        public static BaseLogger Logger { get; private set; }
        public static TRandom Random { get; private set; }

        public static ImageHandler Images { get; private set; }
        
        public static Danbooru Danbooru { get; private set; }
        public static Gelbooru Gelbooru { get; private set; }
        public static Yandere Yandere { get; private set; }
        public static Sankaku Sankaku { get; private set; }

        public static DriveService GoogleDrive { get; private set; }

        public static Image.Imgur Imgur { get; private set; }
        
        public static CPUInfo CPU { get; private set; }
        public static OsInfo OS { get; private set; }
        public static MemInfo MemInfo { get; private set; }
        public static VideoCardInfo VideoCardInfo { get; private set; }

        public static SocketTextChannel JunkChannel
        {
            get
            {
                var Channel = Client.GetChannel(Settings.DevServer.JunkChannelId);

                if (Channel != null)
                    return Channel as SocketTextChannel;

                return null;
            }
        }

        public static Updater Updater { get; private set; }
        
        public static osuApi osuAPI { get; private set; }

        public static void Setup()
        {
            StartedTime = new TimeSpan(DateTime.Now.Ticks);
            Random = new TRandom(new NR3Generator(TMath.Seed()));
            Logger = new BaseLogger();
            Logger.Log(ConsoleColor.Cyan, LogType.NoDisplay, "Info", "Welcome to Chino-chan!");

            LoadSettings();

            Gelbooru = new Gelbooru();
            Danbooru = new Danbooru();
            Yandere = new Yandere();
            
            if (!string.IsNullOrWhiteSpace(Settings.Credentials.Sankaku.Username)
             && !string.IsNullOrWhiteSpace(Settings.Credentials.Sankaku.Password))
            {
                Sankaku = new Sankaku(Settings.Credentials.Sankaku.Username, Settings.Credentials.Sankaku.Password);
            }
            if (!string.IsNullOrWhiteSpace(Settings.Credentials.Imgur.ClientId)
             && !string.IsNullOrWhiteSpace(Settings.Credentials.Imgur.ClientSecret))
            {
                Imgur = new Image.Imgur();
            }

            if (!string.IsNullOrWhiteSpace(Settings.Credentials.Google.ClientSecret)
             && !string.IsNullOrWhiteSpace(Settings.Credentials.Google.ClientId))
            {
                Logger.Log(ConsoleColor.Green, LogType.GoogleDrive, null, "Logging into GoogleDrive...");
                var Credential = GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets()
                {
                    ClientId = Settings.Credentials.Google.ClientId,
                    ClientSecret = Settings.Credentials.Google.ClientSecret
                }, new string[] { DriveService.Scope.Drive }, "user", CancellationToken.None).Result;
                
                GoogleDrive = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = Credential,
                    ApplicationName = "Chino-chan"
                });
                Logger.Log(ConsoleColor.Green, LogType.GoogleDrive, null, "Logged in!");
            }
            
            LanguageHandler = new LanguageHandler();
            GuildSettings = new GuildSettings();
            
            Updater = new Updater();

            Client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                AlwaysDownloadUsers = true,
                DefaultRetryMode = RetryMode.AlwaysRetry,
                LargeThreshold = 250,
                LogLevel = LogSeverity.Verbose
            });

            Client.Log += (Log) =>
            {
                Logger.Log(ConsoleColor.White, LogType.Discord, Log.Severity.ToString(), Log.Message);
                return Task.CompletedTask;
            };
            Client.Ready += () =>
            {
                new Task(() =>
                {
                    Logger.Log(ConsoleColor.DarkYellow, LogType.Sankaku, "Login", "Logging in...");
                    var LoggedIn = Sankaku.Login(out bool TooManyRequests);
                    if (LoggedIn)
                    {
                        Logger.Log(ConsoleColor.DarkYellow, LogType.Sankaku, "Login", "Logged in!");
                    }
                    else
                    {
                        Logger.Log(ConsoleColor.Red, LogType.Sankaku, "Login", "Couldn't log in due to " + (TooManyRequests ? "rate limitation!" : "wrong credentials!"));
                    }
                }).Start();
                
                return Task.CompletedTask;
            };
            
            if (OSUAPIEnabled)
            {
                osuAPI = new osuApi();
            }

            CommandService = new CommandService(new CommandServiceConfig()
            {
                CaseSensitiveCommands = false,
                LogLevel = LogSeverity.Verbose,
                DefaultRunMode = RunMode.Async
            });

            Logger.Log(ConsoleColor.Cyan, LogType.Updater, null, "Loading Commands...");
            CommandService.AddModulesAsync(Assembly.GetEntryAssembly()).ContinueWith((ModuleInfo) =>
            {
                Logger.Log(ConsoleColor.Cyan, LogType.Updater, null, "Loaded Commands!");
                var Modules = ModuleInfo.Result;
                var Text = "";
                var HelpNotFound = new List<string>();

                var English = LanguageHandler.GetLanguage("en");

                foreach (var Module in Modules)
                {
                    Text += Environment.NewLine + "- " + Module.Name + Environment.NewLine;
                    foreach (var Command in Module.Commands)
                    {
                        Text += "-- " + Command.Name + Environment.NewLine;
                        if (!English.Help.ContainsKey(Command.Name))
                        {
                            HelpNotFound.Add(Command.Name);
                        }
                    }
                }
                Logger.Log(ConsoleColor.Cyan, LogType.Updater, null, "Available modules and commands: " + Text);
                if (HelpNotFound.Count != 0)
                {
                    Logger.Log(ConsoleColor.Cyan, LogType.Updater, "Warning", "Help not found for these commands: " + string.Join(", ", HelpNotFound));
                }
            });

            Client.MessageReceived += async (ReceivedMessage) =>
            {
                var Message = ReceivedMessage as SocketUserMessage;
                if (Message == null)
                {
                    return;
                }
                
                var Context = new CommandContext(Client, Message);

                var Settings = GuildSettings.GetSettings(Context.Guild != null ? Context.Guild.Id : Context.User.Id);

                if (Global.Settings.SayPreferences.ContainsKey(Context.User.Id) && !Message.Content.StartsWith(Settings.Prefix + "say"))
                {
                    var Prefs = Global.Settings.SayPreferences[Context.User.Id];
                    if (Prefs.Listening.ContainsKey(Context.Channel.Id))
                    {
                        var Channel = Client.GetChannel(Prefs.Listening[Context.Channel.Id]) as ITextChannel;
                        if (Channel == null)
                        {
                            Global.Settings.SayPreferences[Context.User.Id].Listening.Remove(Context.Channel.Id);
                            SaveSettings();
                        }
                        else
                        {
                            await Channel.SendMessageAsync(Message.Content);

                            if (Prefs.AutoDel)
                            {
                                var Dm = await Context.User.GetOrCreateDMChannelAsync();
                                if (Dm.Id != Context.Channel.Id)
                                {
                                    await Message.DeleteAsync();
                                }
                            }
                        }
                    }
                }


                if (Message.Content == "/gamerescape")
                {
                    var Name = Message.Author.Username;
                    if (!Context.IsPrivate)
                    {
                        await Message.DeleteAsync();
                        Name = (await Context.Guild.GetUserAsync(Message.Author.Id)).Nickname ?? Name;
                    }
                    await Message.Channel.SendMessageAsync($"{ Name } ¯\\_(ツ)_/¯");
                    return;
                }
                else if (Message.Content == "/lenny")
                {
                    var Name = Message.Author.Username;
                    if (!Context.IsPrivate)
                    {
                        await Message.DeleteAsync();
                        Name = (await Context.Guild.GetUserAsync(Message.Author.Id)).Nickname ?? Name;
                    }
                    await Message.Channel.SendMessageAsync($"{ Name } ( ͡° ͜ʖ ͡°)");
                    return;
                }
                
                int Position = 0;

                if (!(Message.HasStringPrefix(Settings.Prefix, ref Position)
                    || Message.HasMentionPrefix(Client.CurrentUser, ref Position)))
                    return;

                var MessageCommand = Message.Content.Substring(Position).ToLower();

                if (Images.Images.ContainsKey(MessageCommand))
                {
                    new Task(async () =>
                    {
                        var Pair = Images.Images[MessageCommand];
                        if (Pair.IsNsfw && !(Context.IsPrivate || IsNsfwChannel(Settings, Message.Channel.Id)))
                        {
                            await Message.Channel.SendMessageAsync(LanguageHandler.GetLanguage(Settings.LanguageId).OnlyNsfw);
                            return;
                        }
                        bool Success = false;
                        var File = "";
                        do
                        {
                            if (!string.IsNullOrWhiteSpace(File))
                            {
                                Images.Images[MessageCommand].Files.Remove(File);

                                if (Images.Images[MessageCommand].Files.Count == 0)
                                {
                                    await Message.Channel.SendMessageAsync(LanguageHandler.GetLanguage(Settings.LanguageId).CantUploadImage);
                                    break;
                                }
                            }
                            File = Pair.RandomFile();
                            Success = await SendImageAsync(File, Context.Channel, Pair.TitleIncludeName ? Pair.Name : null);
                        }
                        while (!Success);
                    }).Start();
                    return;
                }

                var Result = await CommandService.ExecuteAsync(Context, Position);
                if (!Result.IsSuccess)
                {
                    switch (Result.Error)
                    {
                        case CommandError.BadArgCount:
                            break;
                        case CommandError.ParseFailed:
                            break;
                        case CommandError.UnmetPrecondition:
                            if (Result.ErrorReason == "Owner")
                            {
                                await Context.Channel.SendMessageAsync(Context.GetLanguage().NoOwnerPermission);
                            }
                            else
                            {
                                await Context.Channel.SendMessageAsync(Context.GetLanguage().NoPermission);
                            }
                            break;
                        case CommandError.UnknownCommand:
                            if (IsOwner(Context.User.Id))
                            {
                                var Command = Tools.ConvertHighlightsBack(Message.Content.Substring(Position));

                                new Thread(() => Entrance.HandleCommand(Command, Context.Channel as ITextChannel)).Start();
                            }
                            break;
                        default:
                            await Context.Channel.SendMessageAsync($"```css\nAn error happened! Please report this to { Global.Settings.OwnerName }!\n\nError type: { Result.Error }\nReason: { Result.ErrorReason }```");
                            break;
                    }
                }
                else
                {
                    Logger.Log(ConsoleColor.DarkYellow, LogType.Updater, Context.Guild != null ? Context.Guild.Name : Context.Channel.Name, $"#{ Context.Channel.Name } { Context.User.Username } executed { Context.Message.Content }");
                }
            };
            
            Stopwatch Watch = new Stopwatch();
            Watch.Start();
            var Searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");

            CPU = new CPUInfo(Searcher.Get().Cast<ManagementBaseObject>().FirstOrDefault());
            Logger.Log(ConsoleColor.Cyan, LogType.WMI, null, "Loading Processor data took " + Watch.ElapsedMilliseconds + "ms");

            Watch.Restart();
            Searcher.Query = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
            OS = new OsInfo(Searcher.Get().Cast<ManagementBaseObject>().FirstOrDefault());
            Logger.Log(ConsoleColor.Cyan, LogType.WMI, null, "Loading Operating System data took " + Watch.ElapsedMilliseconds + "ms");

            Watch.Restart();
            Searcher.Query = new ObjectQuery("SELECT * FROM Win32_ComputerSystem");
            MemInfo = new MemInfo(Searcher.Get().Cast<ManagementBaseObject>().FirstOrDefault());
            Logger.Log(ConsoleColor.Cyan, LogType.WMI, null, "Loading Computer System data took " + Watch.ElapsedMilliseconds + "ms");
            
            Watch.Restart();
            Searcher.Query = new ObjectQuery("SELECT * FROM Win32_VideoController");
            VideoCardInfo = new VideoCardInfo(Searcher.Get().Cast<ManagementBaseObject>().FirstOrDefault());
            Logger.Log(ConsoleColor.Cyan, LogType.WMI, null, "Loading Video card data took " + Watch.ElapsedMilliseconds + "ms");
            Watch.Stop();

            Watch = null;
        }

        public static async Task StartAsync()
        {
            await Client.LoginAsync(TokenType.Bot, Settings.Credentials.Discord.Token);
            await Client.StartAsync();
            
        }

        public static async Task StopAsync()
        {
            await Client.StopAsync();

            Client.Dispose();
            Client = null;
        }
        
        public static async Task<bool> SendImageAsync(string File, IMessageChannel Channel, string Title = null, string Description = null)
        {
            EmbedBuilder Builder = new EmbedBuilder
            {
                Color = new Color(0, 255, 255),
                Title = string.IsNullOrWhiteSpace(Title) ? "" : Title,
                Description = string.IsNullOrWhiteSpace(Description) ? "" : Description
            };

            var Stream = new FileStream(File, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            var Url = "";

            if (Stream.Length / 1024 / 1024 > 8192)
            {
                if (Imgur == null)
                {
                    return false;
                }
                else
                {
                    Url = await Imgur.UploadImage(Stream);
                }
            }
            else
            {
                var Message = await JunkChannel.SendFileAsync(Stream, Path.GetFileName(File), "");
                Url = Message.Attachments.ElementAt(0).Url;
            }
            
            Builder.WithImageUrl(Url);

            await Channel.SendMessageAsync("", embed: Builder.Build());

            return true;
        }

        private static void LoadSettings()
        {
            if (File.Exists(SettingsPath))
            {
                Settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(SettingsPath));

                if (Settings == null)
                    Settings = new Settings();

                Logger.Log(ConsoleColor.Yellow, LogType.Settings, null, "Settings read, checking..");
            }
            else
            {
                Settings = new Settings();
                SaveSettings();
            }

            var HasToQuit = false;
            
            if (string.IsNullOrWhiteSpace(Settings.Credentials.Discord.Token))
            {
                Logger.Log(ConsoleColor.Red, LogType.Discord, null, "Please insert the Discord token!");
                HasToQuit = true;
            }
            if (Settings.Owner?.Id == 0)
            {
                Logger.Log(ConsoleColor.Red, LogType.Discord, null, "Please insert your Discord Id!");
                HasToQuit = true;
            }
            if (string.IsNullOrWhiteSpace(Settings.Owner?.Password))
            {
                Logger.Log(ConsoleColor.Red, LogType.Discord, null, "Please insert your custom password! This can be anything, you have to define it to access to the console!");
                HasToQuit = true;
            }
            if (Settings.WebServerPort < 1 || Settings.WebServerPort > 65535)
            {
                Logger.Log(ConsoleColor.Red, LogType.Discord, null, "Please define the WebServer port between 0 and 65535!");
                HasToQuit = true;
            }

            if (HasToQuit)
            {
                Logger.Log(ConsoleColor.Cyan, LogType.NoDisplay, null, "You can find the Settings.json in \"Data\" folder!");

                Console.ReadLine();
                Environment.Exit(exitCode: 0);
            }

            Images = new ImageHandler();

            SaveSettings();
            
            Logger.Log(ConsoleColor.Yellow, LogType.Settings, null, "Settings Loaded!");
        }
        
        public static void SaveSettings()
        {
            if (File.Exists(SettingsPath))
            {
                File.Delete(SettingsPath);
            }
            if (!Directory.Exists(SettingsFolder))
            {
                Directory.CreateDirectory(SettingsFolder);
            }
            File.WriteAllText(SettingsPath, JsonConvert.SerializeObject(Settings, Formatting.Indented));
        }

        public static bool IsNsfwChannel(ulong GuildId, ulong ChannelId)
        {
            return IsNsfwChannel(GuildSettings.GetSettings(GuildId), ChannelId);
        }
        public static bool IsNsfwChannel(GuildSetting GuildSettings, ulong ChannelId)
        {
            return GuildSettings.NsfwChannels.Contains(ChannelId)
                || (bool)(Client.GetChannel(ChannelId) as ITextChannel)?.IsNsfw;
        }
        
        public static bool IsAdminOrHigher(ulong Id, ulong GuildId)
        {
            var Settings = GuildSettings.GetSettings(GuildId);
            var Guild = Client.GetGuild(GuildId);
            return IsAdminOrHigher(Id, Guild, Settings);
        }
        public static bool IsAdminOrHigher(ulong Id, IGuild Guild, GuildSetting Settings)
        {
            var res = IsAdmin(Id, Settings)
                || IsServerOwner(Id, Guild)
                || IsGlobalAdmin(Id)
                || IsOwner(Id);
            return res;
        }
        public static bool IsServerOwnerOrHigher(ulong Id, ulong GuildId)
        {
            var Guild = Client.GetGuild(GuildId);
            return IsServerOwnerOrHigher(Id, Guild);
        }
        public static bool IsServerOwnerOrHigher(ulong Id, IGuild Guild)
        {
            return (IsServerOwner(Id, Guild) || Guild == null)
                || IsGlobalAdmin(Id)
                || IsOwner(Id);
        }
        public static bool IsGlobalAdminOrHigher(ulong Id)
        {
            return IsGlobalAdmin(Id) || IsOwner(Id);
        }

        public static bool IsAdmin(ulong Id, ulong GuildId)
        {
            var Settings = GuildSettings.GetSettings(GuildId);
            return IsAdmin(Id, Settings);
        }
        public static bool IsAdmin(ulong Id, GuildSetting Settings)
        {
            return Settings.Admins.Select(t => t.Id).Contains(Id);
        }
        public static bool IsServerOwner(ulong Id, ulong GuildId)
        {
            if (GuildId == 0)
                return true;

            return IsServerOwner(Id, Client.GetGuild(GuildId));
        }
        public static bool IsServerOwner(ulong Id, IGuild Guild)
        {
            if (Guild == null)
                return true;
            return Guild.OwnerId == Id;
        }
        public static bool IsGlobalAdmin(ulong Id)
        {
            return Settings.GlobalAdmins.Select(t => t.Id).Contains(Id);
        }
        public static bool IsOwner(ulong Id)
        {
            return Settings.Owner.Id == Id;
        }

        public static List<string> AdminsAt(ulong Id)
        {
            List<string> AdminNames = new List<string>(AdminIdsAt(Id).Select(t => Client.GetUser(t).Username));
            return AdminNames;
        }
        public static List<ulong> AdminIdsAt(ulong Id)
        {
            List<ulong> AdminIds = new List<ulong>();
            IGuild Guild;
            if (Id == 0)
            {
                foreach (var Setting in GuildSettings.Settings)
                {
                    Setting.Value.Admins.ForEach(AdminCredential =>
                    {
                        if (Client.GetUser(AdminCredential.Id) is IUser User)
                        {
                            AdminIds.Add(User.Id);
                        }
                    });
                    if ((Guild = Client.GetGuild(Setting.Key)) != null)
                    {
                        if (!IsGlobalAdminOrHigher(Guild.OwnerId))
                            AdminIds.Add(Guild.OwnerId);
                    }
                }
            }
            else
            {
                var Setting = GuildSettings.GetSettings(Id);
                Setting.Admins.ForEach(AdminCredential =>
                {
                    if (Client.GetUser(AdminCredential.Id) is IUser User)
                    {
                        AdminIds.Add(User.Id);
                    }
                });
                if ((Guild = Client.GetGuild(Setting.GuildId)) != null)
                {
                    if (!IsGlobalAdminOrHigher(Guild.OwnerId))
                        AdminIds.Add(Guild.OwnerId);
                }
            }
            return AdminIds;
        }

        public static List<string> GlobalAdmins()
        {
            var GlobalAdmins = new List<string>();
            foreach (var Admin in Settings.GlobalAdmins)
            {
                if (Client.GetUser(Admin.Id) is IUser User)
                {
                    GlobalAdmins.Add(User.Username);
                }
            }
            return GlobalAdmins;
        }
        public static string OwnerName()
        {
            return Client.GetUser(Settings.Owner.Id).Username;
        }

        public static bool IsBlocked(ulong Id, ulong GuildOrDMId = 0)
        {
            GuildSetting Setting = null;

            if (GuildOrDMId != 0)
                Setting = GuildOrDMId.GetSettings();

            if (Setting != null)
            {
                var Index = Setting.Blocked.FindIndex(p => p.Id == Id);
                if (Index < 0)
                {
                    Index = Settings.GloballyBlocked.FindIndex(p => p.Id == Id);
                }
                return Index > -1;
            }
            else
            {
                var Index = Settings.GloballyBlocked.FindIndex(p => p.Id == Id);
                return Index > -1;
            }
        }
        public static List<ulong> Blocked(ulong Id = 0)
        {
            GuildSetting Setting = null;

            if (Id != 0)
                Setting = Id.GetSettings();

            if (Setting != null)
            {
                return Setting.Blocked.Select(t => t.Id).ToList();
            }
            else
            {
                return Settings.GloballyBlocked.Select(t => t.Id).ToList();
            }
        }
    }
}
