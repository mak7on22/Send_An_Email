using System.Net.Mail;
using System.Net;
using System.Text;

namespace TDList.Servises
{
    public class EmailSendler : IEmailSendler
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            var mail = "jl_sp_amg@mail.ru";
            var pw = "mQLVN4jCDtc2B5uEzLPn";

            var client = new SmtpClient("smtp.mail.ru", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(mail, pw),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };
            var mailMessage = new MailMessage(from: mail, to: email, subject, message)
            {
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8
            };

            return client.SendMailAsync(mailMessage);
        }
    }
}
