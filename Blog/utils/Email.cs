using System.Net;
using System.Net.Mail;

namespace Blog.utils
{
    public class Email
    {
        public Email()
        {
        }

        public async Task SendEmailAsync(string toEmail, string title, string message)
        {
            try
            {
                // Set the email sender, recipient, subject, and body
                string senderEmail = "thanhantran21@gmail.com";
                string recipientEmail = toEmail;
                string subject = title;
                string body = message;

                // Create a MailMessage object
                MailMessage mail = new MailMessage(senderEmail, recipientEmail, subject, body);
                mail.IsBodyHtml = true;

                // Set the SMTP server details
                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
                smtpClient.Credentials = new NetworkCredential("thanhantran21@gmail.com", "okifhhhxhjasrioe");
                smtpClient.EnableSsl = true;

                // Send the email
                smtpClient.Send(mail);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while sending the email: " + ex.Message);
            }
        }
    }
}
