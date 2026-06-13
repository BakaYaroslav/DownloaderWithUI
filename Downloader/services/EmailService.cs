using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;


namespace Downloader.services
{
    internal class EmailService
    {
        public bool SendMailMessage(string email, int code)
        {
            try
            {
                var mail = new MailMessage();
                mail.From = new MailAddress("noreply.downloader.app@gmail.com");
                mail.To.Add(email);
                mail.Subject = "Downloader - Password Reset Code";
                mail.IsBodyHtml = true;

                mail.Body = $@"

<html lang='en'>
<head>
    <meta name='google' content='notranslate' />
</head>
<body style='margin: 0; padding: 0;'>
    <div style='font-family: Arial, sans-serif; max-width: 400px; margin: 0 auto; padding: 30px; background-color: #f5f5f5; border-radius: 10px;' class='container'>
        <h2 style='color: #ff4444; text-align: center;'>Downloader</h2>
        <p style='color: #222222;' class='text'>Your password reset code:</p>
        <div style='background-color: #e0e0e0; padding: 20px; border-radius: 8px; text-align: center;' class='code-block'>
            <h1 style='color: #ff4444; letter-spacing: 8px; margin: 0; padding-left: 8px;'>{code}</h1>
        </div>
        <p style='color: #666666; font-size: 12px; text-align: center; margin-top: 20px;' class='subtext'>Code expires in 3 minutes</p>
    </div>

    <style>
        @media (prefers-color-scheme: dark) {{
            .container {{
                background-color: #1a1a1a !important;
            }}
            .text {{
                color: #ffffff !important;
            }}
            .code-block {{
                background-color: #2a2a2a !important;
            }}
            .subtext {{
                color: #888888 !important;
            }}
        }}
    </style>
</body>
</html>";

                var smtp = new SmtpClient("smtp.gmail.com");
                smtp.Port = 587;
                smtp.Credentials = new NetworkCredential("noreply.downloader.app@gmail.com", "usgh dmui vidh ysht");
                smtp.EnableSsl = true;
                smtp.Send(mail);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


    }
    
}
