using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;

internal class ClassNet
{
    private readonly string email;
    private readonly string password;

    internal ClassNet()
    {
        email = ConfigurationManager.AppSettings["Email"];
        password = ConfigurationManager.AppSettings["Password"];
    }

    internal bool SendEmail(string to, string subject, string content)
    {
        MailAddress fromAddress = new MailAddress(email, "Cazarina Interiors");
        MailAddress toAddress = new MailAddress(to);
        MailMessage msg = new MailMessage(fromAddress, toAddress);
        msg.Subject = subject;
        msg.Body = content;

        var smtpClient = new SmtpClient
        {
            Host = "smtp.gmail.com",
            Port = 587,
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(fromAddress.Address, password)
        };

        try
        {
            smtpClient.Send(msg);
            return true; //Email sent successfully
        }
        catch
        {
            return false; //Error sending email: {ex.Message}
        }
    }
}
