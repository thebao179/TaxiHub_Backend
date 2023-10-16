using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Helper.Models;

namespace Helper
{
    public static class DoStuff
    {
        //2: include alphabet characters, 1: only number
        public static string RandomString(int mode, int len)
        {
            Random rand = new Random();
            string pattern = "qwertyuiopasdfghjklzxcvbnm1234567890";
            if(mode == 1)
            {
                pattern = "0123456789";
            }
            char[] arr = new char[len];
            for (int i = 0; i < len; i++)
            {
                arr[i] = pattern[rand.Next(pattern.Length)];
            }
            return string.Join(string.Empty, arr);
        }

        public static byte[] HashString(string plaintext)
        {
            HashAlgorithm algorithm = HashAlgorithm.Create("SHA-512");
            return algorithm.ComputeHash(Encoding.ASCII.GetBytes(plaintext));
        }

        public static string SendEmails(EmailSender sender, EmailMessage obj)
        {
            using (SmtpClient client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(sender.usr, sender.pwd),
                EnableSsl = true
            })
            {
                try
                {
                    MailMessage message = new MailMessage(new MailAddress(sender.usr, "FakeTaxi"), new MailAddress(obj.EmailTo))
                    {
                        IsBodyHtml = true,
                        Subject = obj.Subject,
                        Body = obj.Content
                    };
                    client.Send(message);
                    return "Send mail success";
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
        }
    }
}
