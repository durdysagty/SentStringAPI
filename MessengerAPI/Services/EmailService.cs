using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using System.Threading.Tasks;

namespace MessengerAPI.Services
{
    public static class EmailService
    {
        public static async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("sentstring.com", "no-reply@sentstring.com"));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(TextFormat.Html)
            {
                Text = htmlMessage
            };

            using var client = new SmtpClient();
            await client.ConnectAsync("smtp.mail.ru", 465, true);
            await client.AuthenticateAsync("no-reply@sentstring.com", "Ws87ujh[-45AmNb");
            await client.SendAsync(emailMessage);

            await client.DisconnectAsync(true);
        }
    }
}
