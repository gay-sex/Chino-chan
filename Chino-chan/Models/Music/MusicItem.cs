using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chino_chan.Models.Music
{
    public enum DownloadState
    {
        Added,
        Downloading,
        Downloaded
    }
    public class MusicItem
    {
        public string Name { get; set; }

        public MusicYouTubeData YouTubeData { get; set; }

        public string Path { get; set; }

        public TimeSpan Duration { get; set; }
    }

    public class MusicYouTubeData
    {
        public event Action Downloaded;

        public YouTubeResponse YouTubeInfo { get; set; }

        [JsonIgnore]
        private DownloadState _State { get; set; }
        public DownloadState State
        {
            get
            {
                return _State;
            }
            set
            {
                _State = value;
                if (value == DownloadState.Downloaded)
                {
                    Downloaded?.Invoke();
                }
            }
        }
    }
}
