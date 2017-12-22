using Chino_chan.Models.Irc;
using Chino_chan.Models.Settings;
using Meebey.SmartIrc4net;
using System;
using System.Text;
using System.Threading;

namespace Chino_chan.Modules
{
    public class Irc
    {
        private osuCredentials Credentials
        {
            get
            {
                return Global.Settings.Credentials.osu;
            }
        }
        private IrcClient Client;
        
        public event Action<IrcMessage> MessageReceived;
        public bool Connected
        {
            get
            {
                return Client.IsConnected;
            }
        }

        public Irc()
        {
            Client = new IrcClient()
            {
                AutoReconnect = true,
                AutoRetry = true,
                AutoRelogin = true,
                AutoRetryDelay = 100,
                Encoding = Encoding.UTF8,
                AutoJoinOnInvite = true,
                ActiveChannelSyncing = true
            };
            Client.OnConnecting += (s, e) =>
            {
                Global.Logger.Log(ConsoleColor.Green, LogType.IRC, "Bancho", "Connecting...");
            };
            Client.OnConnected += (s, e) =>
            {
                Global.Logger.Log(ConsoleColor.Green, LogType.IRC, "Bancho", "Connected!");
            };
            Client.OnConnectionError += (s, e) =>
            {
                Global.Logger.Log(ConsoleColor.Green, LogType.IRC, "Bancho", "Connection error!");
            };
            Client.OnDisconnecting += (s, e) =>
            {
                Global.Logger.Log(ConsoleColor.Green, LogType.IRC, "Bancho", "Disconnecting...");
            };
            Client.OnDisconnected += (s, e) =>
            {
                Global.Logger.Log(ConsoleColor.Green, LogType.IRC, "Bancho", "Disconnected!");
            };
            Client.OnError += (s, e) =>
            {
                Global.Logger.Log(ConsoleColor.Green, LogType.IRC, "Error", e.ErrorMessage);
            };
            Client.OnJoin += (s, e) =>
            {
                if (e.Who == Global.Settings.Credentials.osu.Username)
                {
                    Global.Logger.Log(ConsoleColor.Green, LogType.IRC, e.Channel, e.Who + " joined!");

                    var Channel = e.Channel.GetIrcChannel();

                    Channel.SendMessageAsync("--Connected to " + e.Channel + "!--");
                }
            };
            Client.OnKick += (s, e) =>
            {
                var KickText = e.Who + " has been kicked by " + e.Whom + " because " + e.KickReason;
                Global.Logger.Log(ConsoleColor.Green, LogType.IRC, e.Channel, KickText);
                var Channel = e.Channel.GetIrcChannel();
                Channel.SendMessageAsync(KickText);
            };
            Client.OnPart += (s, e) =>
            {
                if (e.Who == Global.Settings.Credentials.osu.Username)
                {
                    Global.Logger.Log(ConsoleColor.Green, LogType.IRC, e.Channel, e.Who + " left!");

                    var Channel = e.Channel.GetIrcChannel();
                    Channel.SendMessageAsync("--Left from " + e.Channel + "!--");
                }
            };

            Client.OnRawMessage += (sender, Message) =>
            {
                if (Message.Data.Type == ReceiveType.ChannelMessage)
                {
                    var Msg = new IrcMessage()
                    {
                        Channel = Message.Data.Channel,
                        Content = Message.Data.Message,
                        From = Message.Data.Nick,
                        Time = DateTime.Now
                    };
                    MessageReceived?.Invoke(Msg);
                    var Channel = Message.Data.Channel.GetIrcChannel();
                    Channel.SendMessageAsync(Msg.From + ": " + Msg.Content);
                }
            };
        }

        public void Connect()
        {
            if (Client.IsConnected)
                return;

            new Thread(() =>
            {
                Client.Connect(new string[] { "irc.ppy.sh" }, 6667);
                Client.Login(Credentials.Username, Credentials.Username, 0, Credentials.Username, Credentials.Password);

                Client.Listen();
                if (Client.IsConnected)
                    Client.Disconnect();
            }).Start();
        }
        public void Disconnect()
        {
            Client.Disconnect();
        }
        public void Reconnect()
        {
            Client.Reconnect(true, true);
        }

        public bool SendMessage(string ChannelName, string Message)
        {
            if (Client.IsJoined(ChannelName))
            {
                Client.SendMessage(SendType.Message, ChannelName, Message);
                return true;
            }
            return false;
        }
        public bool Join(string ChannelName)
        {
            if (!Client.IsJoined(ChannelName))
            {
                Client.RfcJoin(ChannelName);
            }
            return Client.JoinedChannels.Contains(ChannelName);
        }
        public bool Part(string ChannelName)
        {
            bool Joined = Client.IsJoined(ChannelName);
            if (Joined)
            {
                Client.RfcPart(ChannelName);
            }
            return Joined && !Client.IsJoined(ChannelName);
        }
    }
}
