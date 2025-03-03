using Book_Store.View_Models.Email;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;
using System.Text;
using Book_Store.Repository.Interface;

namespace Book_Store.Repository
{
    public class EmailRepo : IEmailRepo
    {
        private readonly EmailConfigureModel _emailconfig;

        public EmailRepo(IOptions<EmailConfigureModel> emailconfig)
        {
            _emailconfig = emailconfig.Value;
        }

        //Send Email Settings
        public async Task SendEmail(UserEmailOptions emailOptions)
        {
            //First Step
            MailMessage message = new MailMessage
            {
                Subject = emailOptions.Subject,
                Body = emailOptions.Body,
                From = new MailAddress(_emailconfig.SendingEmail, _emailconfig.DisplayName),
                IsBodyHtml = _emailconfig.IsBodyHTML,
                BodyEncoding = Encoding.Default
            };

            //If There Any Attachment
            if (emailOptions.Attachment != null)
            {
                message.Attachments.Add(new Attachment(emailOptions.Attachment.OpenReadStream(), emailOptions.Attachment.FileName));
            }

            //Send to Receive Emails 
            foreach (var User in emailOptions.RecieveEmails)
            {
                message.To.Add(User);
            }


            //Second Step
            SmtpClient client = new SmtpClient(_emailconfig.Host, _emailconfig.Port);
            try
            {
                client.Port = _emailconfig.Port;
                client.Host = _emailconfig.Host;
                client.UseDefaultCredentials = _emailconfig.UseDefaultCredentials;
                client.Credentials = new NetworkCredential(_emailconfig.UserName, _emailconfig.PassWord);
                client.EnableSsl = _emailconfig.EnableSSL;

                await client.SendMailAsync(message);
            }
            catch
            {
                client.Dispose();
            }

        }


    }
}
