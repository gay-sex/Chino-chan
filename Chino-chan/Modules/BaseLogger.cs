using Chino_chan.Models.osuAPI;
using Discord;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Chino_chan.Modules
{
    public struct LogMessage
    {
        public ConsoleColor Color { get; set; }
        public LogType Type { get; set; }
        public string Severity { get; set; }
        public string Message { get; set; }
        public DateTime Time { get; set; }

        public LogMessage(ConsoleColor Color, LogType Type, string Severity, string Message, DateTime Time)
        {
            this.Color = Color;
            this.Type = Type;
            this.Severity = Severity;
            this.Message = Message;
            this.Time = Time;
        }
    }
    public enum LogType 
    {
        Discord,
        osuApi,
        Language,
        Settings,
        Updater,
        Commands,
        GC,
        NoDisplay,
        WMI,
        Images,
        Sankaku,
        Imgur,
        GoogleDrive,
        ExternalModules,
        YouTubeAPI
    }
    public class BaseLogger
    {
        Timer SaveTimer { get; set; }
        string Path { get; set; }
        List<string> SavedLog { get; set; }
        List<string> Logs { get; set; }

        List<string> DiscordLog { get; set; }

        ObservableCollection<LogMessage> LogQuery { get; set; }
        bool Logging { get; set; } = false;

        public BaseLogger()
        {
            SaveTimer = new Timer(200)
            {
                Repeat = false
            };

            SavedLog = new List<string>();
            Logs = new List<string>();

            if (!System.IO.Directory.Exists("Log"))
                System.IO.Directory.CreateDirectory("Log");

            Path = "Log\\log." + System.IO.Directory.EnumerateFiles("Log", "log.*.log").Count() + ".log";
            
            SaveTimer.Elapsed += () =>
            {
                if (!System.IO.File.Exists(Path))
                {
                    if (SavedLog.Count > 0)
                    {
                        System.IO.File.WriteAllText(Path, string.Join(Environment.NewLine, SavedLog));
                    }
                    else
                    {
                        System.IO.File.WriteAllText(Path, "");
                    }
                }
                if (Logs.Count > 0)
                {
                    System.IO.File.AppendAllText(Path, string.Join(Environment.NewLine, Logs));
                    SavedLog.AddRange(Logs);
                    Logs.Clear();
                }
            };

            LogQuery = new ObservableCollection<LogMessage>();
            LogQuery.CollectionChanged += (sender, Args) =>
            {
                if (Args.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    if (!Logging)
                    {
                        Logging = true;
                        StartLogging();
                    }
                }
            };
        }
        
        private void StartLogging()
        {
            for (int i = 0; i < LogQuery.Count; i++)
            {
                var Message = LogQuery[i];
                Log(Message.Color, Message.Type, Message.Severity, Message.Message, Message.Time);
                LogQuery.Remove(Message);
                i--;
            }
            Logging = false;
        }

        public void Log(ConsoleColor Color, LogType Type, string Severity, string Message, DateTime? Time = null)
        {
            LogQuery.Add(new LogMessage(Color, Type, Severity, Message, Time ?? DateTime.Now));
        }

        private void Log(ConsoleColor Color, LogType Type, string Severity, string Message, DateTime Time)
        {
            if (!Message.Contains("DELETE")
                && !Message.Contains("POST")
                && !Message.Contains("GET")
                && !Message.Contains("PUT")
                && !Message.Contains("PATCH"))
            {
                string TimeBlock = "[" + GetTime(Time) + "] ";
                var TypeBlock = "";
                if (Type != LogType.NoDisplay)
                    TypeBlock = "[" + Type.ToString() + "] ";

                string SeverityBlock = Severity;

                string LogLine = TimeBlock + TypeBlock;

                if (!string.IsNullOrWhiteSpace(SeverityBlock))
                {
                    SeverityBlock = "[" + SeverityBlock + "] ";
                    LogLine += SeverityBlock;
                }
                LogLine += Message;

                Logs.Add(LogLine);

                SaveTimer.Start();

                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write(TimeBlock);

                Console.ForegroundColor = Color;
                Console.Write(TypeBlock);

                if (!string.IsNullOrWhiteSpace(SeverityBlock))
                {
                    Console.Write(SeverityBlock);
                }

                Console.ResetColor();

                Console.Write(Message + Environment.NewLine);
            }
        }

        private String GetTime(DateTime? Time)
        {
            var time = Time ?? DateTime.Now;
            return $"{ time.Year }. { Format(time.Month) }. { Format(time.Day) }. { Format(time.Hour) }:{ Format(time.Minute) }:{ Format(time.Second) }";
        }

        private string Format(int Value)
        {
            if (Value < 10)
                return "0" + Value;

            return Value.ToString();
        }
    }
}
