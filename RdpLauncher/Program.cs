using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RdpLauncher
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Process rdpProcess = null;

            Console.WriteLine("==launcher start-process==");

            var processStartInfo = new ProcessStartInfo
            {
                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
                FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FreeRdpPoc.exe"),
                UseShellExecute = true,
            };
            rdpProcess = Process.Start(processStartInfo);

            Console.WriteLine("==launcher end==");
            Console.ReadLine();
        }
    }
}
