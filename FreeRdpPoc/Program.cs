using FreeRdpPoc.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreeRdpPoc
{
    internal class Program
    {
        [DllImport("user32.dll")]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        // When you don't want the ProcessId, use this overload and pass IntPtr.Zero for the second parameter
        [DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("User32.dll")]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string strClassName, string strWindowName);


        static void Main(string[] args)
        {
            string _rdpProfile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "rdp_profile.bat");
            string userName = Settings.Default.username;
            string password = Settings.Default.password;
            Process rdpProcess = null;

            Console.WriteLine("==start-process==");

            var processStartInfo = new ProcessStartInfo
            {
                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
                FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wfreerdp.exe"),
                Arguments = $@" config.rdp /u:lightworks /p:Lightwork!@#",
                UseShellExecute = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
            };
            rdpProcess = Process.Start(processStartInfo);

            Console.WriteLine("==end==");

            //AutoRetry(() =>
            //{
            //    var proc = Process.GetProcessesByName("wfreerdp").First();

            //    if(proc != null)
            //    {
            //        //Console.WriteLine($"MainModule : {proc?.MainModule.FileName}");
            //        //Console.WriteLine($"ProcessName : {proc?.ProcessName}");
            //        //Console.WriteLine($"MainWindowTitle : {proc?.MainWindowTitle}");
            //        //Console.WriteLine($"MainWindowHandle : {proc?.MainWindowHandle.ToInt32()}");
            //        //// FindProcess(proc.Id);

            //        HideFrom(proc);
            //    }

            //}, 5, 1000);
            
            void HideFrom(Process info)
            {
                int SW_HIDE = 0;

                if (info == null) Console.WriteLine("process is null");

                IntPtr hWnd = FindWindow(null, info?.MainWindowTitle);
                Console.WriteLine($">> hwnd before process_id: {hWnd.ToInt32()}");

                if (hWnd != IntPtr.Zero)
                {
                    Console.WriteLine($"try to hide {hWnd.ToInt32()}");
                    Console.WriteLine($">> hwnd process_id: {hWnd.ToInt32()}");
                    ShowWindow(hWnd, SW_HIDE);
                }

            }

            void FindProcess(int parentProcessId)
            {
                Thread.Sleep(5000);

                ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                    "SELECT * " +
                    "FROM Win32_Process " +
                    "WHERE ParentProcessId=" + parentProcessId);

                ManagementObjectCollection collection = searcher.Get();
                Console.WriteLine($"child: {collection.Count}");

                if (collection.Count > 0)
                {
                    foreach (var item in collection)
                    {
                        UInt32 childProcessId = (UInt32)item["ProcessId"];
                        if ((int)childProcessId != Process.GetCurrentProcess().Id)
                        {
                            Process childProcess = Process.GetProcessById((int)childProcessId);
                            Console.WriteLine($">> {childProcess.ProcessName}");
                        }
                    }
                }
            }
            Console.ReadLine();
        }

        public static void AutoRetry(Action act, int maxRetryCount, int delayMs, Action<Exception> errorInLoopHandle = null)
        {
            int retryCount = 0;
            do
            {
                retryCount++;
                try
                {
                    act();
                    break;
                }
                catch (Exception ex)
                {
                    errorInLoopHandle?.Invoke(ex);
                    if (retryCount >= maxRetryCount)
                    {
                        throw;
                    }
                    else
                    {
                        Thread.Sleep(delayMs);
                    }
                }
            }
            while (retryCount <= maxRetryCount);
        }
    }
}
