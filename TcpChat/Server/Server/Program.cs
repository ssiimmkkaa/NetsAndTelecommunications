using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace Server
{
    class Program
    {
        static readonly object _lock = new object();
        static readonly Dictionary<int, Socket> _clients = new Dictionary<int, Socket>();

        static void Main(string[] args)
        {
            Connection connection = JsonSerializer.Deserialize<Connection>(File.ReadAllText("app.config.json"));
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(connection.Ip), connection.Port);
            Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            int count = 1;

            try
            {
                listenSocket.Bind(ipPoint);
                listenSocket.Listen(connection.LimitConectors);

                Console.WriteLine($"Сервер {connection.ConnectionString} запущен");

                while (true)
                {
                    Socket client = listenSocket.Accept();
                    lock (_lock) _clients.Add(count, client);

                    Console.WriteLine($"New client #{count} connected");

                    Thread t = new Thread(HandleClients);
                    t.Start(count);
                    count++;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static void HandleClients(object o)
        {
            int id = (int)o;
            Socket client;

            lock (_lock) client = _clients[id];

            while (true)
            {
                try
                {

                    StringBuilder str = new StringBuilder();
                    int bytes;
                    byte[] data = new byte[256];
                    do
                    {
                        bytes = client.Receive(data);
                        str.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (client.Available > 0);

                    Console.WriteLine($"{DateTime.Now} #{id}: {str.ToString()}");
                    Send(data);
                }
                catch (SocketException e)
                {
                    if(e.SocketErrorCode == SocketError.ConnectionReset)
                    {
                        lock (_lock) _clients.Remove(id);
                        client.Shutdown(SocketShutdown.Both);
                        client.Close();
                        Console.WriteLine($"Client #{id} disconnected");
                        Thread.CurrentThread.Join();
                    }

                    Console.WriteLine(e);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            lock (_lock) _clients.Remove(id);
            client.Shutdown(SocketShutdown.Both);
            client.Close();
        }

        public static void Send(byte[] data)
        {
            lock (_lock)
            {
                foreach (Socket c in _clients.Values)
                {
                    c.Send(data);
                }
            }
        }
    }
}