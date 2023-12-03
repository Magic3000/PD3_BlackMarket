using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.ConsoleColor;

namespace PD3_BlackMarket
{
    public static class Extentions
    {
        public static void _sout(this object _in, ConsoleColor col = White, bool newStr = true)
        {
            Console.ForegroundColor = col;
            Console.Write($"{_in}{(newStr ? "\n" : "")}");
            Console.ForegroundColor = White;
        }
    }
}
