using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chino_chan.WebServer
{
    public class Client
    {
        public delegate void OnDataRecieved(RecievedMessage Message);

        public event Action Disconnected;
        public event OnDataRecieved DataRecieved;

        private Thread ListeningThread { get; set; }

        public Socket Socket { get; private set; }

        public bool Alive { get; private set; }

        public Client(Socket Socket)
        {
            this.Socket = Socket;
            Alive = Socket.Connected;

            ListeningThread = new Thread(Listening);
        }

        private void Listening()
        {
            while (true)
            {
                try
                {
                    var Data = new List<ArraySegment<byte>>();
                    Socket.Receive(Data);

                    var RecievedData = Data.SelectMany(t => t).ToArray();

                    DataRecieved?.Invoke(new RecievedMessage(RecievedData));
                }
                catch (SocketException)
                {
                    Alive = false;
                    ListeningThread.Abort();
                    Disconnected?.Invoke();
                    break;
                }
            }
        }

        public void StartListening()
        {
            ListeningThread.Start();
        }

        public void StopListening()
        {
            ListeningThread.Abort();
            ListeningThread = null;
        }

        public int Send(byte[] Bytes)
        {
            return Socket.Send(Bytes);
        }
    }
}
