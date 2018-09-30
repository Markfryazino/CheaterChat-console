using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;

namespace CheaterChat_app
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("Добро пожаловать в CheaterChat - наш собственный мессенджер с блэкджеком и серверами" +
                    "!\nМожешь подключиться к серверу или создать свой собственный." +
                    "\nНабери 'создать', чтобы создать сервер, или 'подключиться', чтобы... ну... подключиться: ");
                Console.ForegroundColor = ConsoleColor.Green;
                string action = Console.ReadLine();
                if (action == "создать") CreateServer();
                else if (action == "подключиться") Connect();
            }

        }

        static void Connect() //выполняем, если юзер решает подключиться
        {
            TcpClient client = null;
            NetworkStream netStream = null;
            Thread listeningThread = null;
            try
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("\nВведи свое имя: ");
                Console.ForegroundColor = ConsoleColor.Green;
                string name = Console.ReadLine();
                Console.ForegroundColor = ConsoleColor.Gray;
                /*Console.WriteLine("Хорошо. Сначала необходимо ввести координаты сервера. В любой момент можно ввести " +
                    "'выйти', чтобы вернуться (и словить ошибку программы, которую у меня не получается обработать." +
                    " UPD: получилось).");
                client = User.TryConnect(name);*/ //подключение
                client = User.TryConnect2(name);
                if (client == null) throw new ReturnException();
                netStream = client.GetStream();
                string serverName = BasicMethods.ReadMessage(netStream);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"Сервер '{serverName}' приветствует тебя!\n");
                listeningThread = new Thread(() => //поток, в котором принимаем сообщения
                { 
                    while (true)
                    {
                        string message = BasicMethods.ReadMessage(netStream);
                        Console.SetCursorPosition(0, Console.CursorTop);
                        BasicMethods.Print(message);
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.Write(name + ": ");
                        Console.ForegroundColor = ConsoleColor.Green;
                    }
                });
                listeningThread.Start();

                Console.WriteLine("Теперь ты можешь писать в чат, просто написав текст сообщения и нажав Enter.\n");
                while (true)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.Write(name + ": ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    string message = Console.ReadLine();    //пишем сообщения
                    if (message == "выйти") throw new ReturnException();
                    else BasicMethods.WriteMessage(netStream, message);
                }

            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("\nК сожалению, плохой код творца привел к ужасающей ошибке: \n" + e.Message + "\n" +
                    "Программа сделает все, что сможет, чтобы стабилизировать ситуацию.");
            }
            finally
            {
                if (listeningThread != null) listeningThread.Abort();
                if (client != null) client.Close();
                if (netStream != null) netStream.Close();
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("Нажми любую клавишу, чтобы продолжить.");
                Console.ReadLine();
            }
        }

        static void CreateServer() //создание сервера
        {
            TcpClient probableServer = User.TryConnect2("Американский шпион");
            if (probableServer != null)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("Кто-то уже запустил сервер, а это значит, что все, что тебе остается, это подключиться " +
                    "к нему или выйти. Нам очень жаль.\n");
                probableServer.Close();
                return;
            }
            Server server; //сервер, который будем использовать
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("Хорошо. Введи имя для сервера: ");
            Console.ForegroundColor = ConsoleColor.Green;
            string NameOfServer = Console.ReadLine();
            int Port = 1984;
            bool PortUsed = false;

            server = new Server(Port, NameOfServer, ref PortUsed);  //дальше идет обработка случая, когда порт '1984' занят

            while (PortUsed) //цикл, в котором мы добиваемся, наконец, свободного порта
            {
                Port++;
                server = new Server(Port, NameOfServer, ref PortUsed);
            }

            try
            {
                Console.WriteLine("Ждем подключений...\n");
                Thread listeningThread = new Thread(server.ListenForConnections); //запускаем прослушивание
                listeningThread.Start();
                Console.Read();
            }

            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Кажется, произошла ошибка:\n{0}", e.Message);
            }

            finally
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                server.BroadcastMessage(Guid.NewGuid(), "Сервер завершает свою работу. Чат закрывается.");
                if (server.Listener != null) server.Listener.Stop(); //закрытие всех сокетов
                foreach (var user in server.ClientList)
                {
                    user.UserClient.Close();
                    user.UserStream.Close();
                }
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("Сервер завершает работу.");
                Console.ReadLine();
            }
        }
    }                //ВЫВОДИТЬ ТУ ЧАСТЬ СВОЕГО СООБЩЕНИЯ, КОТОРАЯ УЖЕ НАПИСАНА
                    //ВЫВОДИТЬ ИМЯ СЕРВЕРА
}                   //ОБРАБОТАТЬ ВОЗНИКАЮЩИЕ ПРИ ВЫХОДЕ ИЛИ ЗАКРЫТИИ СЕРВЕРА ИСКЛЮЧЕНИЯ 
