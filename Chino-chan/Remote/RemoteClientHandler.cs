using Chino_chan.Modules;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Chino_chan.Remote
{
    public class RemoteClientHandler
    {
        private Thread AcceptThread { get; set; }

        public Dictionary<uint, Client> ConnectedClients { get; private set; }
        public TcpListener Server { get; private set; }

        public RemoteClientHandler()
        {
            Server = new TcpListener(GetIPv4(), Global.Settings.WebServerPort);
            ConnectedClients = new Dictionary<uint, Client>();

            AcceptThread = new Thread(() =>
            {
                while (true)
                {
                    var Client = new Client(Server.AcceptSocket(), GetId());

                    Client.DataReceived += (Data) =>
                    {
                        if (Data.Length == 0) Client.Disconnect();
                        MessageType Type = (MessageType)Data[0];
                        if (Type == MessageType.Credentials && !Client.Auth)
                        {
                            
                        }
                        else if (Client.Auth)
                        {
                            if (Type == MessageType.DiscordSendMessage)
                            {

                            }
                            else if (Type == MessageType.Ping)
                            {

                            }
                            else if (Type == MessageType.DiscordSendMessage)
                            {

                            }
                        }
                    };
                    Client.Disconnected += (Id) =>
                    {
                        ConnectedClients.Remove(Id);
                    };

                    ConnectedClients.Add(Client.Id, Client);
                    Client.Send(new byte[] { (byte)MessageType.Auth });
                }
            });
        }

        public void Start()
        {
            Global.Logger.Log(ConsoleColor.Magenta, LogType.Remote, null, "Starting Listener...");
            Server.Start();
            Global.Logger.Log(ConsoleColor.Magenta, LogType.Remote, null, "Listener started! Accepting clients...");
            AcceptThread.Start();
        }


        private IPAddress GetIPv4()
        {
            var Host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var IP in Host.AddressList)
            {
                if (IP.AddressFamily == AddressFamily.InterNetwork)
                {
                    return IP;
                }
            }

            return null;
        }
        private uint GetId()
        {
            uint i = 0;
            while (ConnectedClients.ContainsKey(i)) i++;
            return i;
        }
    }
}
