using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.Utilities
{
    public class MailSender
    {
        public EventHandler SendMailComplete;
        SmtpClient _client;
      
        public MailSender()
        {
           //_client = new SmtpClient("smtp.gmail.com", 25)
           //{
           //    Credentials = new NetworkCredential("Green.Apms.gpms@gmail.com", "Khongcop@55"),
           //    EnableSsl = true,
           //    DeliveryMethod = SmtpDeliveryMethod.Network,
           //};

           //_client.SendCompleted += client_SendCompleted;
        }

        public void SendAsync(string subject, string content, string filePath)
        {
            //MailMessage msg = new MailMessage();
            //msg.From = new MailAddress("Green.Apms.gpms@gmail.com");
            //msg.To.Add("Green.Apms.gpms@gmail.com");
            //msg.Subject = subject;
            //msg.Body = content;

            //if (!string.IsNullOrEmpty(filePath))
            //{
            //    Attachment attachment;
            //    attachment = new System.Net.Mail.Attachment(filePath);
            //    msg.Attachments.Add(attachment);
            //    _client.SendAsync(msg, new object[] { msg, attachment });
            //}
            //else
            //    _client.SendAsync(msg, new object[] { msg });
        }

        public void SendSync(string subject, string content, string filePath)
        {
            //MailMessage msg = new MailMessage();
            //msg.From = new MailAddress("Green.Apms.gpms@gmail.com");
            //msg.To.Add("Green.Apms.gpms@gmail.com");
            //msg.Subject = subject;
            //msg.Body = content;

            //if(!string.IsNullOrEmpty(filePath))
            //{
            //    Attachment attachment;
            //    attachment = new System.Net.Mail.Attachment(filePath);
            //    msg.Attachments.Add(attachment);
            //}

            //_client.Send(msg);
        }

        void client_SendCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            object[] objs = (object[])e.UserState;
            MailMessage msg = (MailMessage)objs[0];
            msg.Dispose();

            if (objs.Length > 1)
            {
                Attachment att = (Attachment)objs[1];
                att.Dispose();
            }

            var handler = SendMailComplete;
            if (handler != null)
                handler(sender, e);
        }
    }
}
