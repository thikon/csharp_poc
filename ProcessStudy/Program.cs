using Lightwork.License.Package.EnumType;
using Lightwork.License.Package.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace ProcessStudy
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //var tmp = new TestProcess();
            //var tmp2 = tmp.CheckConcurrence("RobotClient");

            //Console.WriteLine("=== Current Owner ===");
            //tmp2.ForEach(item =>
            //{
            //    Console.WriteLine(item);
            //});

            //Console.WriteLine($"All process: {tmp2.Count}");

            IdentityHelper identityHelper = new IdentityHelper();
            var tmp1 = identityHelper.ValidateConcurrence(ApplicationType.Robot);
            Console.WriteLine($"validate concurrence: {tmp1}");

            RegistryService registry = new RegistryService();
            var tmp2 = registry.CheckConcurrence(ApplicationType.Robot).Result;
            Console.WriteLine("=== Current Owner ===");
            tmp2.ForEach(item =>
            {
                Console.WriteLine(item);
            });

            Console.ReadKey();
        }
    }

    class TestProcess
    {
        public List<string> CheckConcurrence(string processName)
        {
            if (string.IsNullOrEmpty(processName))
            {
                return new List<string>();
            }

            Process[] processes = Process.GetProcessesByName(processName);
            List<string> userList = new List<string>();
            Process[] array = processes;
            foreach (Process process in array)
            {
                string currentUser = GetProcessOwner(process.Id);
                if (!userList.Any((string x) => x.Equals(currentUser)))
                {
                    userList.Add(currentUser);
                }
            }

            return userList;
        }

        public List<string> CheckConcurrence2(string processName)
        {
            if (string.IsNullOrEmpty(processName))
            {
                return new List<string>();
            }

            Process[] processes = Process.GetProcessesByName(processName);
            List<string> userList = new List<string>();
            Process[] array = processes;
            foreach (Process process in array)
            {
                string currentUser = GetProcessOwner(process.Id);
                if (!userList.Any((string x) => x.Equals(currentUser)))
                {
                    userList.Add(currentUser);
                }
            }

            return userList;
        }

        private string GetProcessOwner(int processId)
        {
            string queryString = "Select * From Win32_Process Where ProcessID = " + processId;
            ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(queryString);
            ManagementObjectCollection managementObjectCollection = managementObjectSearcher.Get();
            foreach (ManagementObject item in managementObjectCollection)
            {
                string[] array = new string[2]
                {
                    string.Empty,
                    string.Empty
                };
                object[] args = array;
                if (Convert.ToInt32(item.InvokeMethod("GetOwner", args)) == 0)
                {
                    return array[1] + "\\" + array[0];
                }
            }

            return string.Empty;
        }
    }
}
