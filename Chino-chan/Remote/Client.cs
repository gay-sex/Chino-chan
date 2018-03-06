using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chino_chan.Remote
{
    public class Client
    {
        public event Action<uint> Disconnected;
        public event Action<byte[]> DataReceived;

        private Thread ListeningThread { get; set; }

        public Socket Socket { get; private set; }

        public bool Alive { get; private set; }
        public uint Id { get; private set; }
        public bool Auth { get; private set; }

        public Client(Socket Socket, uint Id)
        {
            this.Socket = Socket;
            this.Id = Id;

            Auth = false;

            Alive = Socket.Connected;

            ListeningThread = new Thread(Listening);
            ListeningThread.Start();
        }

        private void Listening()
        {
            while (true)
            {
                try
                {
                    var Data = new List<ArraySegment<byte>>();
                    Socket.Receive(Data);

                    var ReceivedData = Data.SelectMany(t => t).ToArray();

                    DataReceived?.Invoke(ReceivedData);
                }
                catch
                {
                    Alive = false;
                    ListeningThread.Abort();
                    Disconnected?.Invoke(Id);
                    break;
                }
            }
        }
        public void Disconnect()
        {
            ListeningThread.Abort();
            Alive = false;
            Socket.Disconnect(false);
            Disconnected?.Invoke(Id);
        }
        public int Send(byte[] Bytes)
        {
            return Socket.Send(Bytes);
        }
    }
}
