using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwitchCaseSample
{
    internal class Program
    {
        const string TEST1 = "TEST1";
        const string TEST2 = "TEST2";
        const string TEST3 = "TEST3";

        static void Main(string[] args)
        {
            var tmp = "TEST3";

            switch (tmp)
            {
                case TEST1:
                    Console.WriteLine(tmp);
                    break;
                case TEST2:
                case TEST3:
                    Console.WriteLine($">> {tmp}");
                    break;
                default:
                    break;
            }

            Console.ReadKey();
        }
    }
}
