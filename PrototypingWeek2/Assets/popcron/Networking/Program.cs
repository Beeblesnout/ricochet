using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Popcron.Networking
{
    // using Console = Console.Console;

    public class Program
    {
        public static void Send(Message message)
        {
            NetworkManager.SendToOtherApp(message);
        }

        public static void WriteLine(string v)
        {
            Console.WriteLine(v);
        }
    }
}