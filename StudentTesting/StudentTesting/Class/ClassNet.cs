using System;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;

internal class ClassNet
{
    private readonly string email;
    private readonly string password;

    internal ClassNet()
    {
        email = ConfigurationManager.AppSettings["Email"];
        password = ConfigurationManager.AppSettings["Password"];
    }

    internal bool SendEmail(string to, string subject, string code)
    {
        MailAddress fromAddress = new MailAddress(email, "SmartTEST+");
        MailAddress toAddress = new MailAddress(to);

        string projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName;
        string logoPath = Path.Combine(projectDirectory, "Res", "FullLogo.png");
        string htmlBody = @"
<!DOCTYPE html>
<html>
<head>
    <title>SmartTEST+</title>
    <style>
        body {
            font-family: Arial, sans-serif;
            background-color: #f4f4f4;
        }
        .container {
            max-width: 600px;
            margin: 0 auto;
            background-color: white;
            padding: 20px;
            border-radius: 5px;
            box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
        }
        h1 {
            color: #333;
            text-align: center;
            margin-bottom: 30px;
        }
        pre {
            background-color: #f9f9f9;
            padding: 10px;
            border-radius: 3px;
            font-family: Consolas, monospace;
            white-space: pre-wrap;
        }
        .logo {
            display: block;
            margin: 0 auto 20px;
            max-width: 150px;
        }
        .footer {
            text-align: center;
            color: #888;
            margin-top: 30px;
        }
    </style>
</head>
<body>
    <div class=""container"">
        <img class=""logo"" src=""cid:logo"" alt=""Cazarina Interiors Logo"">
        <h1>SmartTEST+</h1>
        <p>Привет,</p>
        <p>Вот код, который вы запросили:</p>
        <pre>" + code + @"</pre>
        <div class=""footer"">
            <p>Благодарим вас за использование наших услуг.</p>
            <p>С наилучшими пожеланиями,<br>SmartTEST Team</p>
        </div>
    </div>
</body>
</html>
";

        MailMessage msg = new MailMessage(fromAddress, toAddress);
        msg.Subject = subject;

        AlternateView htmlView = AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html);
        LinkedResource logoResource = new LinkedResource(logoPath, MediaTypeNames.Image.Jpeg);
        logoResource.ContentId = "logo";
        htmlView.LinkedResources.Add(logoResource);

        msg.AlternateViews.Add(htmlView);

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
            // Log the exception or handle it appropriately
            return false; //Error sending email: {ex.Message}
        }
    }
}

