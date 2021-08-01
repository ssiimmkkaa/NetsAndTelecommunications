using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Client
{
    class Program
    {
        public static readonly string _ip = "127.0.0.1";
        public static readonly int _port = 3000;

        static void Main(string[] args)
        {
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(_ip), _port);
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.Connect(ipPoint);

            Console.WriteLine($"Connected to {_ip}:{_port}");

            Thread thread = new Thread(ReceiveData);
            thread.Start(clientSocket);

            string s;
            while (!string.IsNullOrEmpty(s = Console.ReadLine()))
            {
                byte[] data = Encoding.Unicode.GetBytes(s);
                clientSocket.Send(data);
            }

            clientSocket.Shutdown(SocketShutdown.Send);
            clientSocket.Close();
            thread.Join();
            
            Console.WriteLine($"disconnect from {_ip}:{_port}");
            Console.ReadKey();
        }

        static void ReceiveData(object socket)
        {
            Socket clientSocket = (Socket)socket;
            byte[] data = new byte[256];
            StringBuilder str = new StringBuilder();
            int bytes;
            do
            {
                bytes = clientSocket.Receive(data, data.Length, 0);
                str.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (clientSocket.Available > 0);
            Console.WriteLine(str.ToString());
        }
    }
}