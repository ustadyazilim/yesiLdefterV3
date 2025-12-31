using System.Net;
using System.Net.Mail;

namespace UstadDesktop.WhatsAppIntegration.Notifications
{
    /// <summary>
    /// Email service for sending password reset and notification emails
    /// Supports both Gmail and Office365 SMTP configurations
    /// </summary>
    public class EmailService
    {
        private readonly EmailConfiguration _config;

        public EmailService(EmailConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Send password reset email to operator
        /// </summary>
        /// <param name="toEmail">Recipient email address</param>
        /// <param name="operatorName">Operator's full name</param>
        /// <param name="resetToken">Password reset token</param>
        /// <returns>True if sent successfully</returns>
        public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string operatorName, string resetToken)
        {
            try
            {
                var subject = "Ustad WhatsApp - Şifre Sıfırlama";
                var body = CreatePasswordResetEmailBody(operatorName, resetToken);

                return await SendEmailAsync(toEmail, subject, body);
            }
            catch (Exception ex)
            {
                // Log error
                System.Diagnostics.Debug.WriteLine($"Error sending password reset email: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Send notification email for important events
        /// </summary>
        /// <param name="toEmail">Recipient email address</param>
        /// <param name="subject">Email subject</param>
        /// <param name="message">Email message</param>
        /// <returns>True if sent successfully</returns>
        public async Task<bool> SendNotificationEmailAsync(string toEmail, string subject, string message)
        {
            try
            {
                var body = CreateNotificationEmailBody(message);
                return await SendEmailAsync(toEmail, subject, body);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error sending notification email: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Send welcome email to new operator
        /// </summary>
        /// <param name="toEmail">Recipient email address</param>
        /// <param name="operatorName">Operator's full name</param>
        /// <param name="username">Login username</param>
        /// <param name="temporaryPassword">Temporary password</param>
        /// <returns>True if sent successfully</returns>
        public async Task<bool> SendWelcomeEmailAsync(string toEmail, string operatorName, string username, string temporaryPassword)
        {
            try
            {
                var subject = "Ustad WhatsApp - Hoş Geldiniz";
                var body = CreateWelcomeEmailBody(operatorName, username, temporaryPassword);

                return await SendEmailAsync(toEmail, subject, body);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error sending welcome email: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            using var client = new SmtpClient(_config.SmtpHost, _config.SmtpPort);
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(_config.SmtpUsername, _config.SmtpPassword);

            using var message = new MailMessage();
            message.From = new MailAddress(_config.FromEmail, _config.FromName);
            message.To.Add(toEmail);
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;
            message.Priority = MailPriority.Normal;

            await client.SendMailAsync(message);
            return true;
        }

        private string CreatePasswordResetEmailBody(string operatorName, string resetToken)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 0; padding: 20px; background-color: #f5f7f9; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: white; border-radius: 8px; padding: 30px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .logo {{ color: #295C00; font-size: 24px; font-weight: bold; margin-bottom: 10px; }}
        .title {{ color: #1F2937; font-size: 20px; font-weight: 600; }}
        .content {{ color: #374151; line-height: 1.6; }}
        .token-box {{ background-color: #F3F4F6; border: 2px solid #295C00; border-radius: 6px; padding: 15px; margin: 20px 0; text-align: center; }}
        .token {{ font-family: 'Courier New', monospace; font-size: 18px; font-weight: bold; color: #295C00; letter-spacing: 2px; }}
        .warning {{ background-color: #FEF3C7; border-left: 4px solid #F59E0B; padding: 15px; margin: 20px 0; }}
        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #E5E7EB; color: #6B7280; font-size: 14px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo'>Ustad WhatsApp</div>
            <div class='title'>Şifre Sıfırlama Talebi</div>
        </div>
        
        <div class='content'>
            <p>Merhaba <strong>{operatorName}</strong>,</p>
            
            <p>Hesabınız için şifre sıfırlama talebinde bulundunuz. Aşağıdaki kodu kullanarak yeni şifrenizi belirleyebilirsiniz:</p>
            
            <div class='token-box'>
                <div class='token'>{resetToken}</div>
            </div>
            
            <div class='warning'>
                <strong>Önemli Güvenlik Bilgileri:</strong>
                <ul>
                    <li>Bu kod <strong>1 saat</strong> süreyle geçerlidir</li>
                    <li>Kod yalnızca <strong>bir kez</strong> kullanılabilir</li>
                    <li>Bu talebi siz yapmadıysanız, bu e-postayı görmezden gelin</li>
                    <li>Güvenliğiniz için kodu kimseyle paylaşmayın</li>
                </ul>
            </div>
            
            <p>Yeni şifrenizi belirlemek için WhatsApp uygulamasındaki ""Şifremi Unuttum"" bölümünden bu kodu girin.</p>
            
            <p>Herhangi bir sorunuz olursa, lütfen sistem yöneticinizle iletişime geçin.</p>
        </div>
        
        <div class='footer'>
            <p>Bu e-posta Ustad WhatsApp sistemi tarafından otomatik olarak gönderilmiştir.</p>
            <p>© 2024 Ustad Yazılım. Tüm hakları saklıdır.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string CreateWelcomeEmailBody(string operatorName, string username, string temporaryPassword)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 0; padding: 20px; background-color: #f5f7f9; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: white; border-radius: 8px; padding: 30px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .logo {{ color: #295C00; font-size: 24px; font-weight: bold; margin-bottom: 10px; }}
        .title {{ color: #1F2937; font-size: 20px; font-weight: 600; }}
        .content {{ color: #374151; line-height: 1.6; }}
        .credentials-box {{ background-color: #F3F4F6; border-radius: 6px; padding: 20px; margin: 20px 0; }}
        .credential-row {{ display: flex; justify-content: space-between; margin: 10px 0; }}
        .credential-label {{ font-weight: 600; }}
        .credential-value {{ font-family: 'Courier New', monospace; background-color: white; padding: 5px 10px; border-radius: 4px; }}
        .warning {{ background-color: #FEF3C7; border-left: 4px solid #F59E0B; padding: 15px; margin: 20px 0; }}
        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #E5E7EB; color: #6B7280; font-size: 14px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo'>Ustad WhatsApp</div>
            <div class='title'>Hoş Geldiniz!</div>
        </div>
        
        <div class='content'>
            <p>Merhaba <strong>{operatorName}</strong>,</p>
            
            <p>Ustad WhatsApp müşteri hizmetleri sistemine hoş geldiniz! Hesabınız başarıyla oluşturulmuştur.</p>
            
            <div class='credentials-box'>
                <h3>Giriş Bilgileriniz:</h3>
                <div class='credential-row'>
                    <span class='credential-label'>Kullanıcı Adı:</span>
                    <span class='credential-value'>{username}</span>
                </div>
                <div class='credential-row'>
                    <span class='credential-label'>Geçici Şifre:</span>
                    <span class='credential-value'>{temporaryPassword}</span>
                </div>
            </div>
            
            <div class='warning'>
                <strong>Güvenlik İçin Önemli:</strong>
                <ul>
                    <li>İlk girişinizde <strong>mutlaka</strong> şifrenizi değiştirin</li>
                    <li>Güçlü bir şifre seçin (en az 8 karakter, büyük/küçük harf, rakam ve özel karakter)</li>
                    <li>Şifrenizi kimseyle paylaşmayın</li>
                    <li>Bu e-postayı güvenli bir yerde saklayın</li>
                </ul>
            </div>
            
            <h3>Sistem Özellikleri:</h3>
            <ul>
                <li>Müşteri mesajlarını gerçek zamanlı görüntüleme</li>
                <li>Hızlı yanıt şablonları</li>
                <li>Konuşma geçmişi ve raporlama</li>
                <li>Dosya ve medya paylaşımı</li>
                <li>Takım arkadaşlarıyla işbirliği</li>
            </ul>
            
            <p>Sorularınız için sistem yöneticinizle iletişime geçebilirsiniz.</p>
            
            <p>İyi çalışmalar dileriz!</p>
        </div>
        
        <div class='footer'>
            <p>Bu e-posta Ustad WhatsApp sistemi tarafından otomatik olarak gönderilmiştir.</p>
            <p>© 2024 Ustad Yazılım. Tüm hakları saklıdır.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string CreateNotificationEmailBody(string message)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 0; padding: 20px; background-color: #f5f7f9; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: white; border-radius: 8px; padding: 30px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .logo {{ color: #295C00; font-size: 24px; font-weight: bold; margin-bottom: 10px; }}
        .content {{ color: #374151; line-height: 1.6; }}
        .message-box {{ background-color: #F9FAFB; border-radius: 6px; padding: 20px; margin: 20px 0; }}
        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #E5E7EB; color: #6B7280; font-size: 14px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo'>Ustad WhatsApp</div>
        </div>
        
        <div class='content'>
            <div class='message-box'>
                {message}
            </div>
        </div>
        
        <div class='footer'>
            <p>Bu e-posta Ustad WhatsApp sistemi tarafından otomatik olarak gönderilmiştir.</p>
            <p>© 2024 Ustad Yazılım. Tüm hakları saklıdır.</p>
        </div>
    </div>
</body>
</html>";
        }
    }

    /// <summary>
    /// Email configuration settings
    /// </summary>
    public class EmailConfiguration
    {
        public string SmtpHost { get; set; } = "";
        public int SmtpPort { get; set; } = 587;
        public string SmtpUsername { get; set; } = "";
        public string SmtpPassword { get; set; } = "";
        public string FromEmail { get; set; } = "";
        public string FromName { get; set; } = "";
        public bool EnableSsl { get; set; } = true;
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Validate email configuration
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(SmtpHost) &&
                   SmtpPort > 0 &&
                   !string.IsNullOrEmpty(SmtpUsername) &&
                   !string.IsNullOrEmpty(SmtpPassword) &&
                   !string.IsNullOrEmpty(FromEmail);
        }

        /// <summary>
        /// Create configuration for Gmail
        /// </summary>
        public static EmailConfiguration CreateGmailConfiguration(string username, string appPassword, string fromName = "Ustad WhatsApp")
        {
            return new EmailConfiguration
            {
                SmtpHost = "smtp.gmail.com",
                SmtpPort = 587,
                SmtpUsername = username,
                SmtpPassword = appPassword,
                FromEmail = username,
                FromName = fromName,
                EnableSsl = true
            };
        }

        /// <summary>
        /// Create configuration for Office365
        /// </summary>
        public static EmailConfiguration CreateOffice365Configuration(string username, string password, string fromName = "Ustad WhatsApp")
        {
            return new EmailConfiguration
            {
                SmtpHost = "smtp.office365.com",
                SmtpPort = 587,
                SmtpUsername = username,
                SmtpPassword = password,
                FromEmail = username,
                FromName = fromName,
                EnableSsl = true
            };
        }
    }
}
