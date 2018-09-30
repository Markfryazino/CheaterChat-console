using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace CheaterChat_app
{
    static class BasicMethods //класс базовых методов для использования
    {
        public static string GroupIP = "235.111.111.111";
        public static int groupPort = 2024;
        public static string ReadMessage(NetworkStream netStream) //метод чтения сообщения из потока
        {
            byte[] buffer = new byte[256];
            string message = "";

            do
            {
                netStream.Read(buffer, 0, buffer.Length);
                message += Encoding.Unicode.GetString(buffer);
            }
            while (netStream.DataAvailable);
            if (!message.Contains("$")) throw new Exception();
            message = message.Split('$')[0];
            return message;
        }

        public static void WriteMessage(NetworkStream netStream, string message) //метод записи сообщения в поток
        {
            message += "$заглушка";
            byte[] buffer = Encoding.Unicode.GetBytes(message);
            netStream.Write(buffer, 0, buffer.Length);
        }

        public static void Print(string message) //вывод сообщения на экран
        {
            int indexOfTwoPoints = message.IndexOf(':');
            string userName = "";
            if (indexOfTwoPoints != -1)
            {
                userName = message.Remove(indexOfTwoPoints) + ": ";
                message = message.Substring(indexOfTwoPoints + 2);
            }
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(userName);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.Green;
        }
    }
}
