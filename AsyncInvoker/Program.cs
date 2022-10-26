using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncInvoker
{
    public class Program
    {
        static void Main(string[] args)
        {
            MainClass mainClass = new MainClass();
            mainClass.Test();

            Console.ReadKey();
        }
    }

    public class SubClass
    {
        public void Call(Action<string> action)
        {
            Console.WriteLine("SubClass Call");
            action?.Invoke("call xxxx");
        }
    }

    public class MainClass
    {
        public void Test()
        {
            Console.WriteLine("AA");
            Console.WriteLine("BB");

            SubClass subClass = new SubClass();
            subClass.Call(Invoke1);

            Console.WriteLine("CC");
            Console.WriteLine("DD");
        }

        public void Invoke1(string msg)
        {
            Console.WriteLine($"from invoke1: {msg}");
        }
    }
}
