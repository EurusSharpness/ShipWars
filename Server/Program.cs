using System;
using System.Globalization;
using System.Management.Instrumentation;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Server
{
    internal static class Program
    {
        // ReSharper disable once ObjectCreationAsStatement
        private static void Main() => new TcpServer();
    }

    public class TcpServer
    {
        private readonly TcpListener _listener;
        private const int Port = 8888;
        private const string IpAddress = "127.0.0.1";

        public TcpServer()
        {
            var iPAddress = IPAddress.Parse(IpAddress);
            _listener = new TcpListener(iPAddress, Port);
            StartServer();
        }

        private void StartServer()
        {
            _listener.Start();
            Console.WriteLine($@"Server is listening on {IpAddress}@{Port.ToString()}");
            var p1 = _listener.AcceptTcpClient();
            Console.WriteLine(@"Player 1 is connected");
            var p2 = _listener.AcceptTcpClient();
            Console.WriteLine(@"Player 2 is connected");
            new Thread(RunGame).Start(new Game(p1, p2));
        }

        private static void RunGame(object o)
        {
            if (!(o is Game game))
                return;

            game.StartTheGame();
        }
    }
}