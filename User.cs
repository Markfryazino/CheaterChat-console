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
    class boolean //класс для использования в таймере 
    {
        public bool b = false;
    }

    static class User //класс с функциями, которые будут использоваться клиентом
    {
        /*static string BeginningOfAddress;
        static CancellationToken token;
        public static async void TryGetServersAsync(CancellationTokenSource cts)
        {
            token = cts.Token;
            string[] addressParts = Dns.GetHostAddresses(Dns.GetHostName()).ToString().Split('.');
            BeginningOfAddress = addressParts[0] + "." + addressParts[1] + "." + addressParts[2] + ".";
            Console.WriteLine("Начинаем поиск работающих серверов...");

        }                           КОГДА ОСНОВНУЮ РАБОТУ СДЕЛАЕМ, ТОГДА И БУДЕМ ТУТ ЗАМОРАЧИВАТЬСЯ

        static Task TryServer(int end)
        {
            token.ThrowIfCancellationRequested();
            try
            {
                TcpClient tryClient = new TcpClient(BeginningOfAddress + end.ToString(), 1984);

            }
        }*/

        public static int Port
        {
            get
            {
                Random rnd = new Random();
                return rnd.Next(1000, 10000);
            }
        }

        static TcpClient TryConnectFirstVersion(string ip, int port, string name) //подключение первой версии (часть)
        {
            TcpClient rezult = null;
            try
            {
                rezult = new TcpClient(ip, port);
                NetworkStream ns = rezult.GetStream();
                BasicMethods.WriteMessage(ns, name);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("Ты успешно подключился к серверу!");
            }
            catch
            {
                //Console.ForegroundColor = ConsoleColor.Gray;
                //Console.WriteLine("К сожалению, данный адрес и порт не задействован ни одним сервером. Попробуй еще.");
            }
            return rezult;
        }

        public static TcpClient TryConnect(string name) //подключение первой версии (полное)
        {
            TcpClient rezult = null;
            while (rezult == null)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("Чтобы подключиться, введи IP-адрес сервера (в первую очередь лучше проверить " +
                    "192.168.1.122): ");
                Console.ForegroundColor = ConsoleColor.Green;
                string ip = Console.ReadLine();
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("Теперь введи номер порта (обычно это 1984): ");
                Console.ForegroundColor = ConsoleColor.Green;
                string sPort = Console.ReadLine();
                if (ip == "выйти" || sPort == "выйти") throw new ReturnException();
                int port = Int32.Parse(sPort);
                rezult = TryConnectFirstVersion(ip, port, name);
            }
            return rezult;
        }
        
        public static TcpClient TryConnect2(string name) //вторая версия подключения
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Пробуем подключиться...");
            TcpClient rez1 = TryConnectFirstVersion(Dns.GetHostAddresses(Dns.GetHostName())[1].ToString(), 1984, name);
            if (rez1 != null) return rez1;
            UdpClient udpSender = new UdpClient();
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(BasicMethods.GroupIP), BasicMethods.groupPort);
            IPEndPoint anotherPoint = null;
            byte[] messageToSend = Encoding.Unicode.GetBytes("Я тут$");
            UdpClient udpGetter = new UdpClient(BasicMethods.groupPort);
            udpGetter.JoinMulticastGroup(IPAddress.Parse(BasicMethods.GroupIP), 100);
            Timer timer = null;
            Thread timerThread = new Thread(() =>
            {
                TimerCallback callback = new TimerCallback((object o) =>
                {
                    Console.WriteLine("К сожалению, запущенных серверов нет, тебе придется создать свой. " +
                        "Перенаправляем обратно...");
                    byte[] stop = Encoding.Unicode.GetBytes("Стоп$э");
                    udpSender.Send(stop, stop.Length, endPoint);
                });
                timer = new Timer(callback, null, 5000, 5000);
            });
            timerThread.Start();
            udpSender.Send(messageToSend, messageToSend.Length, endPoint);
            while (true)
            {
                byte[] message = udpGetter.Receive(ref anotherPoint);
                anotherPoint = null;
                string sMessage = Encoding.Unicode.GetString(message).Split('$')[0];
                if (sMessage == "Стоп")
                {
                    if (timer != null) timer.Dispose();
                    timerThread.Abort();
                    udpGetter.Close();
                    udpSender.Close();
                    return null;
                }
                if (sMessage[0] != 'Я')
                {
                    string ip = sMessage.Split()[0];
                    int port = Convert.ToInt32(sMessage.Split()[1]);
                    TcpClient server = TryConnectFirstVersion(ip, port, name);
                    timer.Dispose();
                    timerThread.Abort();
                    udpSender.Close();
                    udpGetter.Close();
                    return server;
                }
            }
        }
    }
}
