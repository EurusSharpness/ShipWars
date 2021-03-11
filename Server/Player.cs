using System;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

namespace Server
{
    public class Player
    {
        public readonly TcpClient Client;
        public readonly int PlayerId;
        private readonly NetworkStream _stream;
        public int HitPoints = 32;

        public Player(int playerId, TcpClient client)
        {
            PlayerId = playerId;
            Client = client;
            _stream = Client.GetStream();
        }

        public override bool Equals(object obj)
        {
            return obj is Player player && PlayerId == player.PlayerId;
        }

        public override int GetHashCode()
        {
            return 2108858624 + PlayerId.GetHashCode();
        }

        
        public void Write(Message message)
        {
            try
            {
                var data = GetBytes(message);
                _stream.Write(data, 0, data.Length);
            }
            catch (Exception)
            {
                //throw new Exception($"Player {PlayerId.ToString()} Diconnected");
            }
        }

        private byte[] GetBytes(Message message)
        {
            return Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(message).ToCharArray());
        }

        public Message Read()
        {
            try
            {
                var data = new byte[2048];
                var len = _stream.Read(data, 0, data.Length);
                var json = Encoding.ASCII.GetString(data, 0, len);
                return JsonConvert.DeserializeObject<Message>(json);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}