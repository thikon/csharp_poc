using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace EmailSending
{
    internal class Program
    {
        static bool mailSent = false;
        static void Main(string[] args)
        {
            int port = 587;
            string host = "smtp.gmail.com";
            string username = "developer@lightworkai.com";
            string password = "";
            string mailFrom = "developer@lightworkai.com";
            string mailTo = "";
            string mailTitle = "Testtitle";
            string mailMessage = "Testmessage";

            password = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));

            MailAddress from = new MailAddress(mailFrom);
            MailMessage message = new MailMessage
            {
                From = from
            };
            message.To.Add(mailTo);
            message.Subject = mailTitle;
            message.Body = mailMessage;
            message.IsBodyHtml = true;

            SmtpClient client = new SmtpClient(host,port);
            client.EnableSsl = true;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(username, password);
            client.SendCompleted += Client_SendCompleted;
            string userState = "test message1";
            client.SendAsync(message, userState);

            Console.WriteLine("Sending message... press c to cancel mail. Press any other key to exit.");
            string answer = Console.ReadLine();
            // If the user canceled the send, and mail hasn't been sent yet,
            // then cancel the pending operation.
            if (answer.StartsWith("c") && mailSent == false)
            {
                client.SendAsyncCancel();
            }
            // Clean up.
            message.Dispose();
            Console.WriteLine("Goodbye.");
        }

        private static void Client_SendCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            // Get the unique identifier for this asynchronous operation.
            String token = (string)e.UserState;

            if (e.Cancelled)
            {
                Console.WriteLine("[{0}] Send canceled.", token);
            }
            if (e.Error != null)
            {
                Console.WriteLine("[{0}] {1}", token, e.Error.ToString());
            }
            else
            {
                Console.WriteLine("Message sent.");
            }
            mailSent = true;
        }
    }
}
