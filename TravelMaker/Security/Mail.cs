using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace TravelMaker.Security
{
    /// <summary>
    ///     寄信
    /// </summary>
    public class Mail
    {
        public static bool SendGmail(string fromAddress, string toAddress, string Subject, string MailBody)
        {
            MailMessage mailMessage = new MailMessage(fromAddress, toAddress);
            mailMessage.Subject = Subject;
            mailMessage.IsBodyHtml = true;
            mailMessage.Body = MailBody;

            // SMTP Server
            SmtpClient mailSender = new SmtpClient("smtp.gmail.com");
            System.Net.NetworkCredential basicAuthenticationInfo =
                new System.Net.NetworkCredential(fromAddress, ConfigurationManager.AppSettings["emailPassword"]);
            mailSender.Credentials = basicAuthenticationInfo;
            mailSender.Port = 587;
            mailSender.EnableSsl = true;
            try 
            {
                mailSender.Send(mailMessage);
                mailMessage.Dispose();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
