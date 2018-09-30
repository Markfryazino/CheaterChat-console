using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CheaterChat_app
{
    class Server
    {
        IPAddress ServerAddress; //адрес и порт сервера
        int ServerPort;
        string ServerName;

        public List<Client> ClientList = new List<Client>(); //список подключенных клиентов
        public TcpListener Listener; //сокет для прослушивания подключений

        public Server(int Port, string Name, ref bool PortUsed) //создание сервера
        {
            try
            {
                PortUsed = false;
                ServerPort = Port;
                ServerName = Name;
                ServerAddress = Dns.GetHostAddresses(Dns.GetHostName())[1]; //получение локального адреса
                Listener = new TcpListener(ServerAddress, ServerPort);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("Сервер создан! Адрес: {0}\nНомер порта: {1}",
                    ServerAddress.ToString(), ServerPort.ToString());
            }

            catch(SocketException)
            {
                PortUsed = true; //возвращаем, что порт уже используется
            }

            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Кажется, произошла ошибка:\n{0}", e.Message);
            }
        }

        public void ListenForConnections() //функция, в которой мы ждем подключения
        {
            Thread givingDirectionThread = new Thread(GiveConnections);
            givingDirectionThread.Start();
            Listener.Start();
            while (true)
            {
                TcpClient newTcpClient = Listener.AcceptTcpClient();
                NetworkStream newStream = newTcpClient.GetStream();
                string newName = BasicMethods.ReadMessage(newStream);
                BasicMethods.WriteMessage(newStream, ServerName);
                Client newClient = new Client //добавляем нового клиента
                {
                    UserName = newName,
                    ID = Guid.NewGuid(),
                    UserClient = newTcpClient,
                    UserStream = newStream
                };
                ClientList.Add(newClient);
                BroadcastMessage(newClient.ID, newName + " присоединился к чату.");
                Thread newClientThread = new Thread(new ParameterizedThreadStart(ListenToClient));
                newClientThread.Start(newClient); //поток, в котором от клиента нам будут приходить сообщения
            }
        }
         
        public void BroadcastMessage(Guid extraID, string message) //распространение сообщения
        {
            BasicMethods.Print(message);
            foreach (var user in ClientList)
                if (user.ID != extraID)
                    BasicMethods.WriteMessage(user.UserStream, message);
        }

        void ListenToClient(object client) //слушаем сообщения клиента
        {
            Client clientToListen = (Client)client;
            try
            {
                while (true)
                {
                    string message = BasicMethods.ReadMessage(clientToListen.UserStream);
                    BroadcastMessage(clientToListen.ID, clientToListen.UserName + ": " + message);
                }
            }
            catch
            {         //если юзер вышел
                WhenClientGone(clientToListen);
            }
        }

        void WhenClientGone(Client client) //функция, удаляющая подключение клиента
        {
            client.UserClient.Close();
            client.UserStream.Close();
            ClientList.Remove(client);
            BroadcastMessage(Guid.NewGuid(), "Похоже, " + client.UserName + " покинул чат.");
        }

        string MyTrim(string s, params char[] c) //функция для обрезания строки, потому что стандартная не работает
        {                                       //эта тоже не работает
            string newString = s;
            for (int i = s.Length - 1; i >= 0; i--)
            {
                bool was = false;
                foreach (char ch in c)
                    if (s[i] == ch)
                    {
                        newString = newString.Remove(newString.Length - 1);
                        was = true;
                        break;
                    }
                if (!was) break;
            }
            return newString;
        }

        void GiveConnections() //функция для раздачи адреса сервера по Udp
        {
            UdpClient udpSender = new UdpClient();
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(BasicMethods.GroupIP), BasicMethods.groupPort);
            IPEndPoint anotherPoint = null;
            UdpClient udpGetter = new UdpClient(BasicMethods.groupPort);
            udpGetter.JoinMulticastGroup(IPAddress.Parse(BasicMethods.GroupIP), 100);
            while (true)
            {
                byte[] message = udpGetter.Receive(ref anotherPoint);
                string sMessage = Encoding.Unicode.GetString(message);
                if (sMessage[0] == '1') continue;
                anotherPoint = null;
                byte[] toSend = Encoding.Unicode.GetBytes(ServerAddress + " " + ServerPort.ToString() + "$");
                udpSender.Send(toSend, toSend.Length, endPoint);
            }
        }
    }
}
