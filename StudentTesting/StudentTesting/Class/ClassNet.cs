using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;

public class ClassNet
{
    private readonly string email;
    private readonly string password;

    public ClassNet()
    {
        email = ConfigurationManager.AppSettings["Email"];
        password = ConfigurationManager.AppSettings["Password"];
    }

    public void SendEmail(string to, string subject, string content)
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
            Console.WriteLine("Email sent successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending email: {ex.Message}");
        }
    }
}
