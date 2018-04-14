using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chino_chan.Remote
{
    public enum ClientType
    {
        Remote,
        WebBrowser,
        Unknown
    }
    public class Client
    {
        public event Action<uint> Disconnected;
        public event Action<byte[]> DataReceived;
        public event Action<string> WebBrowserDataReceived;

        private Thread ListeningThread { get; set; }

        public ClientType Type { get; private set; }

        public TcpClient TcpClient { get; private set; }

        public bool Alive
        {
            get
            {
                return TcpClient.Connected;
            }
        }
        public uint Id { get; private set; }
        public bool Auth { get; private set; }

        public Client(TcpClient TcpClient, uint Id)
        {
            this.TcpClient = TcpClient;
            this.Id = Id;

            Auth = false;
            Type = ClientType.Unknown;

            ListeningThread = new Thread(Listening);
            ListeningThread.Start();
        }

        private void Listening()
        {
            while (true)
            {
                try
                {
                    NetworkStream Stream = TcpClient.GetStream();

                    var Data = new byte[TcpClient.Available];

                    Stream.Read(Data, 0, Data.Length);
                    if (Type == ClientType.Unknown)
                    {
                        string Content = Encoding.UTF8.GetString(Data);
                        if (Content.StartsWith("{") && Content.EndsWith("}"))
                        {
                            Type = ClientType.Remote;
                        }
                        else
                        {
                            Type = ClientType.WebBrowser;
                        }
                    }

                    if (Type == ClientType.Remote)
                    {
                        DataReceived?.Invoke(Data);
                    }
                    else
                    {
                        WebBrowserDataReceived?.Invoke(TcpClient.Client.RemoteEndPoint.ToString());
                    }
                }
                catch
                {
                    Disconnect();
                }
            }
        }
        public void Disconnect()
        {
            ListeningThread.Abort();
            if (Alive)
                TcpClient.Close();
            Disconnected?.Invoke(Id);
        }

        public void Send(byte[] Bytes)
        {
            using (NetworkStream Stream = TcpClient.GetStream())
            {
                Stream.Write(Bytes, 0, Bytes.Length);
            }
        }
    }
}
