using ClientChat.ClientChat.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
namespace ClientChat.ClientChat
{    
    class Client
    {
        private Socket clientSocket;
        private bool isRun;
        private const int serverPort = 11414;
        private const string serverHost = "192.168.1.2";
        private IPAddress address;
        public Client()
        {            
            address = Dns.GetHostByName(Dns.GetHostName()).AddressList[1];
            this.clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);            
        }
        public void IntitUser()
        {
            this.isRun = true;
            User.id = -1;
            do
            {
                Console.Write("Ввидите имя: ");
                User.name = Console.ReadLine();
            } while (User.name.Length <= 2);
            this.SendUserToServer(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
            
            new Thread(new ThreadStart(InputListener)).Start();
            this.MessageListener();
        }

        private void InputListener()
        {
            while (this.isRun)
            {
                if(ConsoleKey.Enter == Console.ReadKey().Key)
                {
                    Console.Write("ввод: ");
                    this.SendMessage(Console.ReadLine());
                }
            }
        }
        private void SendMessage(string message)
        {
            JObject json = new JObject();
            json.Add("id", User.id);
            json.Add("name", User.name);
            json.Add("ipAddress", address.ToString());
            json.Add("textMessage", message.ToString());
            Socket socket = this.GetSocketConnectedForServer();
            socket.Send(Encoding.UTF8.GetBytes(json.ToString()));
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }

        private Socket GetSocketConnectedForServer()
        {
            tryConnect:
            try
            {
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(new IPEndPoint(IPAddress.Parse(serverHost), serverPort));
                return socket;
            } catch
            {
                Console.WriteLine("подключение..");
                Thread.Sleep(500);
                goto tryConnect;
            }
        }
        private void SendUserToServer(Socket socket)
        {
            JObject json = new JObject();
            json.Add("id", User.id);
            json.Add("name", User.name);
            json.Add("ipAddress", address.ToString());
            json.Add("textMessage", "");
            socket.Connect(new IPEndPoint(IPAddress.Parse(serverHost), serverPort));
            socket.Send(Encoding.UTF8.GetBytes(json.ToString()));
            User.id = int.Parse(this.GetMessage(socket));
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            Console.WriteLine(User.name + " вам выдан id:" + User.id);
        }

        private string GetMessage(Socket socket)
        {
            byte[] data = new byte[256];
            StringBuilder builder = new StringBuilder();
            do
            {
                builder.Append(Encoding.UTF8.GetString(data, 0, socket.Receive(data)));
            } while (socket.Available > 0);

            return builder.ToString();
        }
        private void MessageListener()
        {            
            this.clientSocket.Bind(new IPEndPoint(address, User.id + serverPort));
            this.clientSocket.Listen(10);
            while (this.isRun)
            {
                Console.WriteLine(this.GetMessage(this.clientSocket.Accept()));
            }
        }

    }
}
