using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using RestSharp;
using RestSharp.Authenticators;

namespace ServiceWorker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly BackgroundWorker worker = new BackgroundWorker();

        public MainWindow()
        {
            InitializeComponent();
            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            // run all background tasks here
            Console.WriteLine("do work");

            Task.Run(async () =>
            {
                await Alive();
            });

            int next = 0;
            while(next <= 1000)
            {
                Console.WriteLine("write: " + next);
                next++;
            }
        }

        private async Task Alive()
        {
            // robot: 6d5b186f-2d78-4128-94f6-e40362f2c195
            try
            {

                var client = new RestClient("https://dev.lightworktech.com/");
                client.Authenticator = new JwtAuthenticator("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6Imx3YWRtaW4iLCJDdXN0b21lckNvZGUiOiJJSSIsIlVzZXJJZCI6IjEwIiwiVXNlclR5cGUiOiI5IiwibmJmIjoxNjM0NDgzNjgxLCJleHAiOjE2MzQ3NDI4ODEsImlhdCI6MTYzNDQ4MzY4MX0.Il1Yj3PiShAQ-4az6uFgbN6DJ-3pk6i_PBHylp0S_vk");

                var param = new Sample { ipAddress = "127.0.0.1", machineName = "dew", runtimeRobotId = "52bf6586-3cd9-48e5-903a-e6041a5194e9" };
                var request = new RestRequest("/api/v2/AddLogRobotHealthCheck");
                request.AddParameter("id", "1", ParameterType.QueryString);
                request.AddJsonBody(param, "application/json");

                var response = await client.PostAsync<ResultInfo>(request);

                

                Console.WriteLine(response);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //update ui once worker complete his work
            Console.WriteLine("complete");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            worker.RunWorkerAsync();
        }
    }

    public class Sample
    {
        public Sample()
        {

        }

        public Sample(string runtimeRobotId, string ipAddress, string machineName)
        {
            this.runtimeRobotId = runtimeRobotId;
            this.ipAddress = ipAddress;
            this.machineName = machineName;
        }

        public string runtimeRobotId { get; set; }
        public string ipAddress { get; set; }
        public string machineName { get; set; }
    }

    public class ResultInfo
    {
        public bool IsValid { get; set; }

        public string Text { get; set; }

        public int ProcessId { get; set; }

        public string ReturnCode { get; set; }

        public string ReturnValue { get; set; }
    }
}
