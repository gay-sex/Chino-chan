using Chino_chan.Models.Language;
using Chino_chan.Models.Music;
using Discord;
using Discord.Audio;
using Discord.Commands;
using Google.Apis.YouTube.v3.Data;
using NAudio.Wave;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Models.MediaStreams;

namespace Chino_chan.Modules
{
    public enum PlayerState
    {
        Playing,
        Stopped,
        Paused,
        Next,
        WaitingForDownload
    }

    public class Music
    {
        #region Variables
        public Color Color { get; private set; }

        public IAudioClient Client { get; set; }
        public AudioOutStream PCMStream { get; set; }

        public List<MusicItem> Queue { get; set; }
        public MusicItem NowPlaying { get; set; }

        public PlayerState State { get; set; }

        public float Volume { get; set; }

        public bool Connected
        {
            get
            {
                return Client != null
                    && Client.ConnectionState == ConnectionState.Connected;
            }
        }
        #endregion

        public Music()
        {
            Color = new Color(255, 48, 222);
            State = PlayerState.Stopped;
            Queue = new List<MusicItem>();
        }

        #region Public functions
        public async Task ConnectAsync(ICommandContext Context)
        {
            var Language = Context.GetLanguage();
            if ((Context.User as IGuildUser)?.VoiceChannel is IVoiceChannel Channel)
            {
                Client = await Channel.ConnectAsync();
                await Context.Channel.SendMessageAsync(Language.MusicConnected.Prepare(new Dictionary<string, string>()
                {
                    { "%VOICE%", Channel.Name }
                }));
            }
            else
            {
                await Context.Channel.SendMessageAsync(Language.MusicCannotConnect);
            }
        }
        public async Task DisconnectAsync(ICommandContext Context)
        {
            var Language = Context.GetLanguage();

            if (Connected)
            {
                try
                {
                    await Client.StopAsync();
                }
                catch
                {

                }
                State = PlayerState.Stopped;
                await Context.Channel.SendMessageAsync(Language.MusicDisconnect);
                return;
            }

            await Context.Channel.SendMessageAsync(Language.MusicNotConnected);
        }

        public async Task PlayLocalAsync(ICommandContext Context, string Filename)
        {
            var Duration = GetLength(Filename);
            Queue.Add(new MusicItem()
            {
                Path = Filename,
                Duration = Duration
            });
            Global.GuildSettings.Modify(Context.Guild.Id, Settings =>
            {
                Settings.Music.Queue = Queue;
            });
            if (State != PlayerState.Playing)
            {
                await Play(Context);
            }
        }
        public async Task PlayYoutubeAsync(ICommandContext Context, string Youtube)
        {
            var Language = Context.GetLanguage();
            var YtId = GetYoutubeId(Youtube);
            var Info = GetInformation(YtId);

            if (Info.Type?.ToLower() != "video")
            {
                await Context.Channel.SendMessageAsync(Language.MusicYouTubeNotValidUrl);
                return;
            }

            var MusicItem = new MusicItem()
            {
                Name = Info.Title,
                YouTubeInfo = Info
            };
            
            Queue.Add(MusicItem);
            Global.GuildSettings.Modify(Context.Guild.Id, Settings =>
            {
                Settings.Music.Queue = Queue;
            });

            if (NowPlaying != null)
            {
                await Context.Channel.SendMessageAsync("", embed: new EmbedBuilder()
                {
                    Title = Language.MusicAdded,
                    Description = Language.Title + ": [" + Info.Title + "](" + Info.Url + ")\n"
                            + Language.Uploaded + ": [" + Info.Author + "](" + Info.AuthorUrl + ")\n"
                            + Language.Length + ": " + Info.Duration.ToString(@"hh\:mm\:ss") + "\n"
                            + Language.Index + ": " + (Queue.Count - 1),
                    ThumbnailUrl = Info.Thumbnail,
                    Color = Color

                }.Build());
            }
            else
            {
                await Play(Context);
            }
        }

        public bool RemoveItem(ICommandContext Context, int Id)
        {
            if (Queue.Count <= Id)
            {
                return false;
            }
            else
            {
                Queue.RemoveAt(Id);
                Global.GuildSettings.Modify(Context.Guild.Id, Settings =>
                {
                    Settings.Music.Queue = Queue;
                });
                return true;
            }
        }

        public async Task PlayNextItem(ICommandContext Context)
        {
            if (Queue.Count == 0)
                return;

            await Play(Context);
        }

        public void ChangeVolume(float Volume)
        {
            this.Volume = Volume;
        }

        public void Skip()
        {
            State = PlayerState.Next;
        }

        public async Task ListAsync(ICommandContext Context)
        {
            var Language = Context.GetLanguage();
            var Builder = new EmbedBuilder()
            {
                Color = Color,
                Title = Language.Queue,
            };
            if (Queue.Count == 0)
            {
                Builder.Description = Language.MusicEmptyQueue;
            }
            else
            {
                for (int i = 0; i < 5 && i < Queue.Count; i++)
                {
                    var NowPlaying = Queue[i];
                    
                    if (!string.IsNullOrWhiteSpace(NowPlaying.YouTubeInfo.Title))
                    {
                        var Info = NowPlaying.YouTubeInfo;
                        Builder.AddField(new EmbedFieldBuilder()
                        {
                            IsInline = false,
                            Name = "#" + i + ": " + Info.Title,
                            Value = "[" + Language.Url + "](" + Info.Url + ")\n" +
                                    Language.Uploaded + ": [" + Info.Author + "](" + Info.AuthorUrl + ")\n" +
                                    Language.Length + ": "
                                    + NowPlaying.YouTubeInfo.Duration.Hours.ToString("00") + ":"
                                    + NowPlaying.YouTubeInfo.Duration.Minutes.ToString("00") + ":" 
                                    + NowPlaying.YouTubeInfo.Duration.Seconds.ToString("00")
                        });
                    }
                    else
                    {
                        Builder.AddField(new EmbedFieldBuilder()
                        {
                            IsInline = false,
                            Name = Path.GetFileNameWithoutExtension(NowPlaying.Path),
                            Value = Language.Length + ": " 
                                    + NowPlaying.Duration.Hours.ToString("00") + ":"
                                    + NowPlaying.Duration.Minutes.ToString("00") + ":" 
                                    + NowPlaying.Duration.Seconds.ToString("00") + "\n"
                        });
                    }
                }
            }
            await Context.Channel.SendMessageAsync("", embed: Builder.Build());
        }
        public async Task SendNowPlayingAsync(ICommandContext Context)
        {
            var Language = Context.GetLanguage();
            
            var Builder = new EmbedBuilder
            {
                Color = Color,
                Title = Language.MusicPlaying
            };

            if (NowPlaying == null)
            {
                Builder.Title = Language.MusicNotPlaying;
            }
            else
            {
                bool yt = !string.IsNullOrWhiteSpace(NowPlaying.YouTubeInfo.Title);

                if (yt)
                {
                    var Info = NowPlaying.YouTubeInfo;
                    Builder.Description = Language.Title + ": [" + Info.Title + "](" + Info.Url + ")\n"
                                + Language.Uploaded + ": [" + Info.Author + "](" + Info.AuthorUrl + ")\n";
                    Builder.ThumbnailUrl = Info.Thumbnail;
                }
                else
                {
                    Builder.Description = Language.Title + ": " + Path.GetFileNameWithoutExtension(NowPlaying.Path) + "\n";
                }

                Builder.Description += Language.Length + ": " +
                    (yt ? NowPlaying.YouTubeInfo.Duration : NowPlaying.Duration).ToString(@"hh\:mm\:ss");
            }

            await Context.Channel.SendMessageAsync("", embed: Builder.Build());
        }

        public async Task<SearchListResponse> SearchAsync(string Keyword)
        {
            if (Global.YouTube != null)
            {
                var Yt = Global.YouTube;
                var Request = Yt.Search.List("snippet");
                Request.Q = Keyword;
                Request.MaxResults = 10;
                Request.Type = "video";

                return await Request.ExecuteAsync();
            }

            return null;
        }
        #endregion
        #region Private functions
        private async Task Play(ICommandContext Context)
        {
            if (Queue.Count == 0)
            {
                State = PlayerState.Stopped;
                return;
            }
            NowPlaying = Queue[0];
            
            var Language = Context.GetLanguage();

            await SendNowPlayingAsync(Context);

            Stream Stream = null;

            State = PlayerState.Playing;
            var PCMStream = Client.CreatePCMStream(AudioApplication.Music);
            var Buffer = new byte[8 * 1024];
            var Count = 0;

            if (string.IsNullOrWhiteSpace(NowPlaying.YouTubeInfo.Title))
            {
                Stream = CreateFFmpegStream(NowPlaying.Path);
            }
            else
            {
                YoutubeClient Client = new YoutubeClient();
                MediaStreamInfoSet InfoSet = await Client.GetVideoMediaStreamInfosAsync(YoutubeClient.ParseVideoId(NowPlaying.YouTubeInfo.Url));

                string Url = "";

                if (InfoSet.Audio.Count > 0)
                {
                    Url = InfoSet.Audio.WithHighestBitrate().Url;
                }
                else
                {
                    Url = InfoSet.Muxed.First().Url;
                }
                Stream = CreateFFmpegStream(Url);
            }

            await Task.Delay(1000);

            if (Stream != null)
            {
                while ((Count = await Stream.ReadAsync(Buffer, 0, Buffer.Length)) != 0)
                {
                    if (State == PlayerState.Paused)
                    {
                        await Context.Channel.SendMessageAsync("", embed: new EmbedBuilder()
                        {
                            Title = Language.MusicPaused,
                            Color = Color
                        }.Build());

                        await PCMStream.FlushAsync();
                        PCMStream.Dispose();

                        while (State == PlayerState.Paused)
                        {
                            Thread.Sleep(100);
                        }

                        PCMStream = Client.CreatePCMStream(AudioApplication.Music);
                        if (State == PlayerState.Playing)
                        {
                            await Context.Channel.SendMessageAsync("", embed: new EmbedBuilder()
                            {
                                Title = Language.MusicResumed,
                                Color = Color
                            }.Build());
                        }
                    }
                    if (State == PlayerState.Stopped || State == PlayerState.Next)
                    {
                        break;
                    }

                    Buffer = ChangeVolume(Buffer, Volume / 100);
                    try
                    {
                        await PCMStream.WriteAsync(Buffer, 0, Count);
                    }
                    catch
                    {
                        break;
                    }
                }
            }
            else
            {
                await Context.Channel.SendMessageAsync("", embed: new EmbedBuilder()
                {
                    Title = Language.MusicCantPlay,
                    Color = Color
                }.Build());
            }

            if (State == PlayerState.Stopped)
            {
                await Context.Channel.SendMessageAsync("", embed: new EmbedBuilder()
                {
                    Title = Language.MusicStopped,
                    Color = Color
                }.Build());
            }
            else if (Queue.Count == 1)
            {
                await Context.Channel.SendMessageAsync("", embed: new EmbedBuilder()
                {
                    Title = Language.MusicFinishedQueue,
                    Color = Color
                }.Build());
            }

            if (State != PlayerState.Stopped)
            {
                RemoveItem(Context, 0);
            }

            NowPlaying = null;

            if (Stream != null)
                Stream.Dispose();

            await PCMStream.FlushAsync();
            PCMStream.Dispose();

            if (State != PlayerState.Stopped)
            {
                await Task.Run(() => Play(Context));
            }

        }
        private Stream CreateFFmpegStream(string PathOrUrl)
        {
            var ffmpeg = new ProcessStartInfo
            {
                FileName = "External\\ffmpeg\\ffmpeg.exe",
                Arguments = $"-i \"{ PathOrUrl }\" -ac 2 -f s16le -ar 48000 pipe:1 -loglevel quiet",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            };
            return Process.Start(ffmpeg).StandardOutput.BaseStream;
        }
        
        private byte[] ChangeVolume(byte[] AudioSamples, float Volume)
        {
            Contract.Requires(AudioSamples != null);
            Contract.Requires(AudioSamples.Length % 2 == 0);
            Contract.Requires(Volume >= 0f && Volume <= 1f);

            var Output = new byte[AudioSamples.Length];
            if (Math.Abs(Volume - 1f) < 0.0001f)
            {
                Buffer.BlockCopy(AudioSamples, 0, Output, 0, AudioSamples.Length);
                return Output;
            }

            int VolumeFixed = (int)Math.Round(Volume * 65536d);

            for (var i = 0; i < Output.Length; i += 2)
            {
                int Sample = (short)((AudioSamples[i + 1] << 8) | AudioSamples[i]);
                int Processed = (Sample * VolumeFixed) >> 16;

                Output[i] = (byte)Processed;
                Output[i + 1] = (byte)(Processed >> 8);
            }

            return Output;
        }

        private string GetYoutubeId(string Link)
        {
            var Rx = new Regex("(http:\\/\\/|https:\\/\\/)?(www\\.)?(youtu\\.be|youtube\\.com)\\/(watch\\?v=)?(\\S*)?");
            if (Rx.IsMatch(Link))
            {
                var Match = Rx.Match(Link);
                var Query = Match.Groups[5].Value;

                if (string.IsNullOrWhiteSpace(Query))
                {
                    return null;
                }

                if (Query.Contains('?'))
                {
                    Query = Query.Substring(0, Query.IndexOf("?"));
                }
                if (Query.Contains('&'))
                {
                    Query = Query.Substring(0, Query.IndexOf("&"));
                }

                return Query;
            }

            return Link;
        }
        private YouTubeResponse GetInformation(string Id)
        {
            WebClient HelperClient = new WebClient();

            var Url = "https://www.youtube.com/watch?v=" + Id;
            
            try
            {
                string Data = HelperClient.DownloadString("https://www.youtube.com/oembed?url=" + Url + "&format=json");

                if (Data != "Not Found")
                {
                    var Parsed = JsonConvert.DeserializeObject<YouTubeResponse>(Data);
                    Parsed.Url = Url;

                    Data = HelperClient.DownloadString(Url);

                    Regex Regex = new Regex("\"length_seconds\":\"(\\d*)\"");
                    if (Regex.IsMatch(Data))
                    {
                        Match Match = Regex.Match(Data);
                        Parsed.Duration = TimeSpan.FromSeconds(int.Parse(Match.Groups[1].Value));
                    }

                    return Parsed;
                }
            }
            catch { }
            
            return default(YouTubeResponse);
        }
        private TimeSpan GetLength(string Path)
        {
            if (File.Exists(Path))
            {
                var Stream = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var File = new Mp3FileReader(Stream);

                var Time = File.TotalTime;

                File.Dispose();
                Stream.Dispose();
                return Time;
            }
            return TimeSpan.FromSeconds(0);
        }
        #endregion
    }
    
}
