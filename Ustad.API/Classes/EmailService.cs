using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Ustad.API.Classes
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly bool _useSsl;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly string _baseUrl;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            
            // Get SMTP settings from environment variables or appsettings
            _smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST") ?? 
                       configuration["Email:Smtp:Host"] ?? 
                       "smtp.gmail.com";
            
            _smtpPort = int.TryParse(
                Environment.GetEnvironmentVariable("SMTP_PORT") ?? 
                configuration["Email:Smtp:Port"], 
                out var port) ? port : 587;
            
            _smtpUsername = Environment.GetEnvironmentVariable("SMTP_USERNAME") ?? 
                           configuration["Email:Smtp:Username"] ?? "";
            
            _smtpPassword = Environment.GetEnvironmentVariable("SMTP_PASSWORD") ?? 
                          configuration["Email:Smtp:Password"] ?? "";
            
            _useSsl = bool.TryParse(
                Environment.GetEnvironmentVariable("SMTP_USE_SSL") ?? 
                configuration["Email:Smtp:UseSsl"], 
                out var ssl) ? ssl : true;
            
            _fromEmail = Environment.GetEnvironmentVariable("EMAIL_FROM") ?? 
                        configuration["Email:From:Address"] ?? 
                        "noreply@ustad.com";
            
            _fromName = Environment.GetEnvironmentVariable("EMAIL_FROM_NAME") ?? 
                       configuration["Email:From:Name"] ?? 
                       "Ustad yesiLdefter";
            
            _baseUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") ?? 
                      configuration["Email:BaseUrl"] ?? 
                      "http://localhost:3002";
        }

        /// <summary>
        /// Send password reset email
        /// </summary>
        public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string userName, string resetToken)
        {
            try
            {
                var resetLink = $"{_baseUrl}/auth/reset-password?token={resetToken}";
                
                var subject = "yesiLdefter - Şifre Sıfırlama";
                var body = BuildPasswordResetEmailBody(userName, resetLink);

                return await SendEmailAsync(toEmail, subject, body);
            }
            catch (Exception ex)
            {
                // Log error (you can add logging here)
                System.Diagnostics.Debug.WriteLine($"Email send error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Send demo request email to internal team and confirmation to requester
        /// </summary>
        public async Task<bool> SendDemoRequestEmailAsync(string requesterName, string requesterEmail, string note)
        {
            try
            {
                var subject = $"yesiLdefter - Yeni Demo Talebi: {System.Net.WebUtility.HtmlEncode(requesterName)}";
                var internalBody = BuildDemoRequestInternalEmailBody(requesterName, requesterEmail, note);
                
                // Send to internal team (both addresses)
                var internalEmails = new[] { "tekinucar70@hotmail.com", "ustadyazilim@gmail.com" };
                var allSent = true;

                foreach (var email in internalEmails)
                {
                    var sent = await SendEmailAsync(email, subject, internalBody);
                    if (!sent) allSent = false;
                }

                // Send confirmation to requester
                var confirmationSubject = "yesiLdefter - Demo Talebiniz Alındı";
                var confirmationBody = BuildDemoRequestConfirmationEmailBody(requesterName);
                var confirmationSent = await SendEmailAsync(requesterEmail, confirmationSubject, confirmationBody);

                return allSent && confirmationSent;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Demo request email error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Generic email sending method
        /// </summary>
        public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody, string? plainTextBody = null)
        {
            // If SMTP is not configured, skip sending (useful for development)
            if (string.IsNullOrWhiteSpace(_smtpUsername) || string.IsNullOrWhiteSpace(_smtpPassword))
            {
                System.Diagnostics.Debug.WriteLine($"Email not sent (SMTP not configured): To: {toEmail}, Subject: {subject}");
                return true; // Return true to not break the flow in development
            }

            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_fromName, _fromEmail));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = htmlBody;
                if (!string.IsNullOrWhiteSpace(plainTextBody))
                {
                    bodyBuilder.TextBody = plainTextBody;
                }
                else
                {
                    // Generate plain text from HTML (simple version)
                    bodyBuilder.TextBody = htmlBody.Replace("<br>", "\n")
                                                   .Replace("<br/>", "\n")
                                                   .Replace("<br />", "\n")
                                                   .Replace("<p>", "")
                                                   .Replace("</p>", "\n")
                                                   .Replace("<strong>", "")
                                                   .Replace("</strong>", "")
                                                   .Replace("<a", "")
                                                   .Replace("</a>", "");
                }

                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    // For .NET Core, we need to trust certificates
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    
                    await client.ConnectAsync(_smtpHost, _smtpPort, _useSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
                    
                    if (!string.IsNullOrWhiteSpace(_smtpUsername))
                    {
                        await client.AuthenticateAsync(_smtpUsername, _smtpPassword);
                    }
                    
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Email send error: {ex.Message}");
                return false;
            }
        }

        private string BuildPasswordResetEmailBody(string userName, string resetLink)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <style>
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f5f5f5;
        }}
        .container {{
            background-color: #ffffff;
            border-radius: 8px;
            padding: 30px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }}
        .header {{
            text-align: center;
            margin-bottom: 30px;
        }}
        .logo {{
            font-size: 24px;
            font-weight: bold;
            color: #4CAF50;
            margin-bottom: 10px;
        }}
        .content {{
            margin-bottom: 30px;
        }}
        .button {{
            display: inline-block;
            padding: 12px 30px;
            background-color: #4CAF50;
            color: #ffffff;
            text-decoration: none;
            border-radius: 5px;
            font-weight: bold;
            margin: 20px 0;
        }}
        .button:hover {{
            background-color: #45a049;
        }}
        .link {{
            color: #4CAF50;
            word-break: break-all;
        }}
        .footer {{
            margin-top: 30px;
            padding-top: 20px;
            border-top: 1px solid #eeeeee;
            font-size: 12px;
            color: #666;
            text-align: center;
        }}
        .warning {{
            background-color: #fff3cd;
            border-left: 4px solid #ffc107;
            padding: 12px;
            margin: 20px 0;
            border-radius: 4px;
            font-size: 14px;
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <div class=""logo"">yesiLdefter</div>
        </div>
        
        <div class=""content"">
            <p>Merhaba <strong>{System.Net.WebUtility.HtmlEncode(userName)}</strong>,</p>
            
            <p>yesiLdefter hesabınız için şifre sıfırlama talebi aldık. Şifrenizi sıfırlamak için aşağıdaki bağlantıya tıklayın:</p>
            
            <div style=""text-align: center; margin: 30px 0;"">
                <a href=""{System.Net.WebUtility.HtmlEncode(resetLink)}"" class=""button"">Şifremi Sıfırla</a>
            </div>
            
            <p>Veya aşağıdaki bağlantıyı tarayıcınıza kopyalayıp yapıştırabilirsiniz:</p>
            <p><a href=""{System.Net.WebUtility.HtmlEncode(resetLink)}"" class=""link"">{System.Net.WebUtility.HtmlEncode(resetLink)}</a></p>
            
            <div class=""warning"">
                <strong>⚠ Önemli:</strong> Bu bağlantı 30 dakika süreyle geçerlidir. Eğer şifre sıfırlama talebinde bulunmadıysanız, bu e-postayı görmezden gelebilirsiniz.
            </div>
        </div>
        
        <div class=""footer"">
            <p>Bu e-posta yesiLdefter sisteminden otomatik olarak gönderilmiştir.</p>
            <p>© {DateTime.UtcNow.Year} Ustad. Tüm hakları saklıdır.</p>
        </div>
            </div>
</body>
</html>";
        }

        private string BuildDemoRequestInternalEmailBody(string requesterName, string requesterEmail, string note)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <style>
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f5f5f5;
        }}
        .container {{
            background-color: #ffffff;
            border-radius: 8px;
            padding: 30px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }}
        .header {{
            text-align: center;
            margin-bottom: 30px;
        }}
        .logo {{
            font-size: 24px;
            font-weight: bold;
            color: #4CAF50;
            margin-bottom: 10px;
        }}
        .content {{
            margin-bottom: 30px;
        }}
        .info-box {{
            background-color: #f0f7ff;
            border-left: 4px solid #2196F3;
            padding: 15px;
            margin: 20px 0;
            border-radius: 4px;
        }}
        .info-label {{
            font-weight: bold;
            color: #1976D2;
            margin-bottom: 5px;
        }}
        .info-value {{
            color: #333;
        }}
        .note-box {{
            background-color: #fff9e6;
            border-left: 4px solid #ffc107;
            padding: 15px;
            margin: 20px 0;
            border-radius: 4px;
        }}
        .footer {{
            margin-top: 30px;
            padding-top: 20px;
            border-top: 1px solid #eeeeee;
            font-size: 12px;
            color: #666;
            text-align: center;
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <div class=""logo"">yesiLdefter</div>
            <h2>Yeni Demo Talebi</h2>
        </div>
        
        <div class=""content"">
            <p>Yeni bir demo talebi alındı:</p>
            
            <div class=""info-box"">
                <div class=""info-label"">İsim:</div>
                <div class=""info-value"">{System.Net.WebUtility.HtmlEncode(requesterName)}</div>
            </div>
            
            <div class=""info-box"">
                <div class=""info-label"">E-posta:</div>
                <div class=""info-value""><a href=""mailto:{System.Net.WebUtility.HtmlEncode(requesterEmail)}"">{System.Net.WebUtility.HtmlEncode(requesterEmail)}</a></div>
            </div>
            
            {(string.IsNullOrWhiteSpace(note) ? "" : $@"
            <div class=""note-box"">
                <div class=""info-label"">Not:</div>
                <div class=""info-value"">{System.Net.WebUtility.HtmlEncode(note)}</div>
            </div>")}
            
            <p style=""margin-top: 30px;"">
                <strong>İşlem:</strong> Lütfen bu demo talebini değerlendirin ve ilgili kişiye geri dönüş yapın.
            </p>
        </div>
        
        <div class=""footer"">
            <p>Bu e-posta yesiLdefter sisteminden otomatik olarak gönderilmiştir.</p>
            <p>© {DateTime.UtcNow.Year} Ustad. Tüm hakları saklıdır.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string BuildDemoRequestConfirmationEmailBody(string requesterName)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <style>
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f5f5f5;
        }}
        .container {{
            background-color: #ffffff;
            border-radius: 8px;
            padding: 30px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }}
        .header {{
            text-align: center;
            margin-bottom: 30px;
        }}
        .logo {{
            font-size: 24px;
            font-weight: bold;
            color: #4CAF50;
            margin-bottom: 10px;
        }}
        .content {{
            margin-bottom: 30px;
        }}
        .success-box {{
            background-color: #d4edda;
            border-left: 4px solid #28a745;
            padding: 15px;
            margin: 20px 0;
            border-radius: 4px;
        }}
        .footer {{
            margin-top: 30px;
            padding-top: 20px;
            border-top: 1px solid #eeeeee;
            font-size: 12px;
            color: #666;
            text-align: center;
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <div class=""logo"">yesiLdefter</div>
        </div>
        
        <div class=""content"">
            <p>Merhaba <strong>{System.Net.WebUtility.HtmlEncode(requesterName)}</strong>,</p>
            
            <div class=""success-box"">
                <strong>✓ Demo talebiniz başarıyla alındı!</strong>
            </div>
            
            <p>Demo talebiniz ekibimize iletildi. En kısa sürede size geri dönüş yapacağız.</p>
            
            <p>yesiLdefter hakkında daha fazla bilgi için web sitemizi ziyaret edebilirsiniz.</p>
        </div>
        
        <div class=""footer"">
            <p>Bu e-posta yesiLdefter sisteminden otomatik olarak gönderilmiştir.</p>
            <p>© {DateTime.UtcNow.Year} Ustad. Tüm hakları saklıdır.</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}

