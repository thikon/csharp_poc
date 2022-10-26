using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTrigger
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int[] timeTrigger = { 5, 10, 15, 60 };

            // license database
            List<TmpLicense> licenseList = new List<TmpLicense>();
            licenseList.Add(new TmpLicense { ApplicationType = "CC", Concurrence = 1, ExpireTime = new DateTime(2022, 3, 1) });
            licenseList.Add(new TmpLicense { ApplicationType = "CC", Concurrence = 3, ExpireTime = new DateTime(2022, 2, 19) });
            licenseList.Add(new TmpLicense { ApplicationType = "CC", Concurrence = 2, ExpireTime = new DateTime(2022, 2, 24) });
            licenseList.Add(new TmpLicense { ApplicationType = "CC", Concurrence = 10, ExpireTime = new DateTime(2022, 1, 5) });
            licenseList.Add(new TmpLicense { ApplicationType = "CC", Concurrence = 4, ExpireTime = new DateTime(2022, 1, 1) });

            // compare with current day
            var current = DateTime.Now;

            foreach (var item in licenseList)
            {
                // input value for RULES
                // use subtract for compare between current day
                int diffDate = current.Subtract(item.ExpireTime).Days;

                // RULES
                // contain with range from config or runtime collection
                if (timeTrigger.Contains(diffDate))
                {
                    // alert with display message
                    Console.WriteLine($"{item.Concurrence}'s concurrence at {item.ExpireTime} will expired in {diffDate} days before.");
                }
                else
                {
                    Console.WriteLine($"{item.Concurrence}'s concurrence at {item.ExpireTime} not expire");
                }
            }

            Console.ReadKey();
        }

        class TmpLicense
        {
            public int Concurrence { get; set; }
            public DateTime ExpireTime { get; set; }
            public string ApplicationType { get; set; }
        }
    }
}
