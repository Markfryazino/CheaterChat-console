using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheaterChat_app
{
    class ReturnException : Exception
    {
        new string Message;
        public ReturnException()
        {
        }
    }
}
