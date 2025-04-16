using System.Net;
using System.Net.Mail;
using System.Text;

namespace OnlineMusicStore.Helpers
{
    public static class EmailHelper
    {
        public static void SendOrderConfirmation(string toEmail, string subject, string bodyHtml)
        {
            var fromAddress = new MailAddress("yourmail@gmail.com", "Online Music Store");
            var toAddress = new MailAddress(toEmail);
            var smtp = new SmtpClient();

            var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = bodyHtml,
                IsBodyHtml = true
            };

            smtp.Send(message);
        }
    }
}
