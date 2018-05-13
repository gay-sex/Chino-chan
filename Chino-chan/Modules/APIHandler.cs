using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Chino_chan.Models.osuAPI;
using Newtonsoft.Json;

namespace Chino_chan.Modules
{
    public class osuApi
    {
        #region API Endpoints
        private string Base
        {
            get
            {
                return "https://osu.ppy.sh/api/";
            }
        }
        private string User
        {
            get
            {
                return Base + "get_user?k=" + Global.Settings.Credentials.osu.Token;
            }
        }
        private string Beatmap
        {
            get
            {
                return Base + "get_beatmaps?k=" + Global.Settings.Credentials.osu.Token;
            }
        }
        private string Scores
        {
            get
            {
                return Base + "get_scores?k=" + Global.Settings.Credentials.osu.Token;
            }
        }
        private string Match
        {
            get
            {
                return Base + "get_match?k=" + Global.Settings.Credentials.osu.Token;
            }
        }
        private string UserBest
        {
            get
            {
                return Base + "get_user_best?k=" + Global.Settings.Credentials.osu.Token;
            }
        }
        private string UserRecent
        {
            get
            {
                return Base + "get_user_recent?k=" + Global.Settings.Credentials.osu.Token;
            }
        }
        #endregion

        HttpClient Client;
        private int CurrentCalls = 0;
        private int CallLimit
        {
            get
            {
                return Global.Settings.OSUAPICallLimit;
            }
        }
        private int ValidState = -1;
        private bool IsValid
        {
            get
            {
                if (ValidState == -1)
                {
                    string Content = "";
                    try
                    {
                        Content = Client.GetStringAsync(User + "&u=2&type=id").Result;
                    }
                    catch
                    {
                        ValidState = 0;
                    }
                    ValidState = Content.Contains("Please provide a valid API key.") ? 0 : 1;
                }
                return ValidState == 1;
            }
        }

        private Timer ResetTimer;

        public osuApi()
        {
            Client = new HttpClient();

            if (!IsValid)
            {
                throw new Exception("Invalid API");
            }

            ResetTimer = new Timer(60000);
            ResetTimer.Elapsed += () =>
            {
                CurrentCalls = 0;
            };
        }

        #region Get Multiplayer
        public async Task<Multiplayer> GetMultiplayer(int ID)
        {
            var Endpoint = Match + "&mp=" + ID;
            return await Call<Multiplayer>(Endpoint);
        }
        #endregion

        #region Get User
        public async Task<User> GetUser(int UserID, Mode Mode = Mode.Standard)
        {
            var Endpoint = User + "&u=" + UserID + "&type=id&m=" + (int)Mode;
            return await Call<User>(Endpoint);
        }
        public async Task<User> GetUser(string UserName, Mode Mode = Mode.Standard)
        {
            var Endpoint = User + "&u=" + UserName + "&type=string&m=" + (int)Mode;
            return await Call<User>(Endpoint);
        }
        #endregion

        #region Get Scores
        public async Task<UserScore> GetUserScoreOn(int UserID, int BeatmapID, Mode Mode = Mode.Standard)
        {
            return (await GetUserScoresOn(UserID, BeatmapID, Mode, 1))[0];
        }
        public async Task<UserScore> GetUserScoreOn(string UserName, int BeatmapID, Mode Mode = Mode.Standard)
        {
            return (await GetUserScoresOn(UserName, BeatmapID, Mode, 1))[0];
        }

        public async Task<UserScore[]> GetUserScoresOn(int UserID, int BeatmapID, Mode Mode = Mode.Standard, int Limit = 50)
        {
            var Endpoint = Scores + "&u=" + UserID + "&type=id&m=" + (int)Mode + "&b=" + BeatmapID + "&limit=" + Limit;
            return await Call<UserScore[]>(Endpoint);
        }
        public async Task<UserScore[]> GetUserScoresOn(string UserName, int BeatmapID, Mode Mode = Mode.Standard, int Limit = 50)
        {
            var Endpoint = Scores + "&u=" + UserName + "&type=string&m=" + (int)Mode + "&b=" + BeatmapID + "&limit=" + Limit;
            return await Call<UserScore[]>(Endpoint);
        }

        public async Task<UserScore[]> GetBeatmapTopScores(int BeatmapID, Mode Mode = Mode.Standard, int Limit = 50)
        {
            var Endpoint = Scores + "&m=" + (int)Mode + "&b=" + BeatmapID + "&limit=" + Limit;
            return await Call<UserScore[]>(Endpoint);
        }
        #endregion
        #region Get Best Scores
        public async Task<BestScore[]> GetUserBestScores(string UserName, Mode Mode = Mode.Standard, int Limit = 50)
        {
            var Endpoint = UserBest + "&u=" + UserName + "&type=string&m=" + (int)Mode + "&limit=" + Limit;
            return await Call<BestScore[]>(Endpoint);
        }
        public async Task<BestScore[]> GetUserBestScores(int UserID, Mode Mode = Mode.Standard, int Limit = 50)
        {
            var Endpoint = UserBest + "&u=" + UserID + "&type=id&m=" + (int)Mode + "&limit=" + Limit;
            return await Call<BestScore[]>(Endpoint);
        }

        public async Task<BestScore> GetUserBestScore(string UserName, Mode Mode = Mode.Standard)
        {
            return (await GetUserBestScores(UserName, Mode, 1))[0];
        }
        public async Task<BestScore> GetUserBestScore(int UserID, Mode Mode = Mode.Standard)
        {
            return (await GetUserBestScores(UserID, Mode, 1))[0];
        }
        #endregion
        #region Get Recent
        public async Task<RecentScore[]> GetUserRecentScores(string UserName, Mode Mode = Mode.Standard, int Limit = 10)
        {
            var Endpoint = UserRecent + "&u=" + UserName + "&type=string&m=" + (int)Mode + "&limit=" + Limit;
            return await Call<RecentScore[]>(Endpoint);
        }
        public async Task<RecentScore[]> GetUserRecentScores(int UserID, Mode Mode = Mode.Standard, int Limit = 10)
        {
            var Endpoint = UserRecent + "&u=" + UserID + "&type=id&m=" + (int)Mode + "&limit=" + Limit;
            return await Call<RecentScore[]>(Endpoint);
        }

        public async Task<RecentScore> GetUserRecentScore(string UserName, Mode Mode = Mode.Standard)
        {
            return (await GetUserRecentScores(UserName, Mode, 1))[0];
        }
        public async Task<RecentScore> GetUserRecentScore(int UserID, Mode Mode = Mode.Standard)
        {
            return (await GetUserRecentScores(UserID, Mode, 1))[0];
        }
        #endregion
        #region Get Beatmaps
        public async Task<Beatmap> GetBeatmapAsync(int BeatmapID, Mode Mode = Mode.Standard)
        {
            var Endpoint = Beatmap + "&b=" + BeatmapID + "&m=" + (int)Mode;
            return (await Call<Beatmap[]>(Endpoint))[0];
        }
        public async Task<Beatmap[]> GetBeatmapsAsync(int BeatmapSetID, Mode Mode = Mode.Standard)
        {
            var Endpoint = Beatmap + "&s=" + BeatmapSetID + "&m=" + (int)Mode;
            return await Call<Beatmap[]>(Endpoint);
        }
        #endregion

        private async Task<T> Call<T>(string Endpoint)
        {
            ResetTimer.Start();

            if (CallLimit >= CurrentCalls)
            {
                await Task.Delay(ResetTimer.LastStartTime.AddSeconds(60) - DateTime.Now);
            }

            var Content = await Client.GetStringAsync(Endpoint);
            CurrentCalls++;

            if (Content == "")
            {
                return default(T);
            }
            else
            {
                return JsonConvert.DeserializeObject<T>(Content);
            }
        }
    }
    public enum Mode
    {
        Standard = 0,
        Taiko = 1,
        CatchTheBeat = 2,
        Mania = 3
    }
}
