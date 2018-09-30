using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace CheaterChat_app
{
    class Client //класс клиента для использования сервером
    {
        public string UserName;
        public Guid ID;

        public TcpClient UserClient; //сокет, обрабатывающий запросы пользователя
        public NetworkStream UserStream; //поток для взаимодействия с сервером
    }
}
