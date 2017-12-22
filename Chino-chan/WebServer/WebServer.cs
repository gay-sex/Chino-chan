using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Chino_chan.WebServer
{
    // TODO
    public class WebServer
    {
        private Thread AcceptThread { get; set; }

        public List<Client> ConnectedClients { get; private set; }
        public TcpListener Server { get; private set; }

        public WebServer()
        {
            Server = new TcpListener(GetIPv4(), Global.Settings.WebServerPort);
            ConnectedClients = new List<Client>();
        }

        public void Start()
        {
            Server.Start();
            if (AcceptThread == null || !AcceptThread.IsAlive)
            {
                AcceptThread = new Thread(() =>
                {
                    while (true)
                    {
                        var Client = new Client(Server.AcceptSocket());
                        Client.Send(CommunicationHelper.ConcatEnums(MessageType.Auth, AuthType.RequestUserId));

                        ConnectedClients.Add(Client);
                    }
                });
            }

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
    }
}
