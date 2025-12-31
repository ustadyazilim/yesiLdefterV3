using System.Net.Http;
using System.Text;
using System.Web;

namespace UstadDesktop.WhatsAppIntegration.Notifications
{
    /// <summary>
    /// SMS service for sending password reset and notification messages
    /// Supports NetGSM and other SMS providers
    /// </summary>
    public class SmsService
    {
        private readonly SmsConfiguration _config;
        private readonly HttpClient _httpClient;

        public SmsService(SmsConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        /// <summary>
        /// Send password reset SMS to operator
        /// </summary>
        /// <param name="phoneNumber">Phone number in E.164 format (e.g., 905551234567)</param>
        /// <param name="operatorName">Operator's name</param>
        /// <param name="resetToken">Password reset token</param>
        /// <returns>True if sent successfully</returns>
        public async Task<bool> SendPasswordResetSmsAsync(string phoneNumber, string operatorName, string resetToken)
        {
            try
            {
                var message = $"Merhaba {operatorName}, Ustad WhatsApp sifre sifirlama kodunuz: {resetToken}. Bu kod 1 saat gecerlidir.";
                return await SendSmsAsync(phoneNumber, message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error sending password reset SMS: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Send notification SMS for important events
        /// </summary>
        /// <param name="phoneNumber">Phone number in E.164 format</param>
        /// <param name="message">SMS message content</param>
        /// <returns>True if sent successfully</returns>
        public async Task<bool> SendNotificationSmsAsync(string phoneNumber, string message)
        {
            try
            {
                return await SendSmsAsync(phoneNumber, message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error sending notification SMS: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Send welcome SMS to new operator
        /// </summary>
        /// <param name="phoneNumber">Phone number in E.164 format</param>
        /// <param name="operatorName">Operator's name</param>
        /// <param name="username">Login username</param>
        /// <returns>True if sent successfully</returns>
        public async Task<bool> SendWelcomeSmsAsync(string phoneNumber, string operatorName, string username)
        {
            try
            {
                var message = $"Merhaba {operatorName}, Ustad WhatsApp sistemine hos geldiniz! Kullanici adiniz: {username}. Ilk girisde sifrenizi degistirin.";
                return await SendSmsAsync(phoneNumber, message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error sending welcome SMS: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Send account locked notification SMS
        /// </summary>
        /// <param name="phoneNumber">Phone number in E.164 format</param>
        /// <param name="operatorName">Operator's name</param>
        /// <param name="lockoutMinutes">Lockout duration in minutes</param>
        /// <returns>True if sent successfully</returns>
        public async Task<bool> SendAccountLockedSmsAsync(string phoneNumber, string operatorName, int lockoutMinutes)
        {
            try
            {
                var message = $"Merhaba {operatorName}, hesabiniz guvenlik nedeniyle {lockoutMinutes} dakika kilitlenmistir. Destek icin yoneticinizle iletisime gecin.";
                return await SendSmsAsync(phoneNumber, message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error sending account locked SMS: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> SendSmsAsync(string phoneNumber, string message)
        {
            switch (_config.Provider.ToLower())
            {
                case "netgsm":
                    return await SendNetGsmSmsAsync(phoneNumber, message);
                case "twilio":
                    return await SendTwilioSmsAsync(phoneNumber, message);
                case "3gmesaj":
                case "3gbilisim":
                    return await Send3GBilisimSmsAsync(phoneNumber, message);
                default:
                    throw new NotSupportedException($"SMS provider '{_config.Provider}' is not supported");
            }
        }

        private async Task<bool> Send3GBilisimSmsAsync(string phoneNumber, string message)
        {
            if (string.IsNullOrWhiteSpace(_config.CompanyCode))
            {
                System.Diagnostics.Debug.WriteLine("3G Bilişim SMS error: CompanyCode missing in configuration");
                return false;
            }

            try
            {
                var cleanNumber = CleanPhoneNumber(phoneNumber);
                var smsType = message.Length > 160 ? "5" : "1";
                var sendDate = (_config.ScheduledDate ?? string.Empty) + (_config.ScheduledTime ?? string.Empty);

                var xmlBuilder = new StringBuilder();
                xmlBuilder.Append("<?xml version='1.0' encoding='ISO-8859-9'?>");
                xmlBuilder.Append("<MainmsgBody>");
                xmlBuilder.Append($"<UserName>{_config.Username}</UserName>");
                xmlBuilder.Append($"<PassWord>{_config.Password}</PassWord>");
                xmlBuilder.Append($"<CompanyCode>{_config.CompanyCode}</CompanyCode>");
                xmlBuilder.Append($"<Type>{smsType}</Type>");
                xmlBuilder.Append("<Developer></Developer>");
                xmlBuilder.Append($"<Originator>{_config.Header}</Originator>");
                xmlBuilder.Append("<Version></Version>");
                xmlBuilder.Append($"<Mesgbody><![CDATA[{message}]]></Mesgbody>");
                xmlBuilder.Append($"<Numbers>{cleanNumber}</Numbers>");
                xmlBuilder.Append($"<SDate>{sendDate}</SDate>");
                xmlBuilder.Append("<EDate></EDate>");
                xmlBuilder.Append("</MainmsgBody>");

                var payload = xmlBuilder.ToString();
                var encoding = Encoding.GetEncoding("ISO-8859-9");
                var content = new StringContent(payload, encoding, "application/xml");

                var response = await _httpClient.PostAsync(_config.ApiUrl, content);
                var responseText = await response.Content.ReadAsStringAsync();

                var isSuccess = responseText.StartsWith("ID:", StringComparison.OrdinalIgnoreCase);
                if (!isSuccess)
                {
                    System.Diagnostics.Debug.WriteLine($"3G Bilişim SMS error: {responseText}");
                }

                return isSuccess;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"3G Bilişim SMS exception: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> SendNetGsmSmsAsync(string phoneNumber, string message)
        {
            try
            {
                // Clean phone number - remove + and country code prefixes if present
                var cleanNumber = CleanPhoneNumber(phoneNumber);
                
                // NetGSM API parameters
                var parameters = new Dictionary<string, string>
                {
                    ["usercode"] = _config.Username,
                    ["password"] = _config.Password,
                    ["gsmno"] = cleanNumber,
                    ["message"] = message,
                    ["msgheader"] = _config.Header
                };

                // Add optional parameters
                if (!string.IsNullOrEmpty(_config.StartDate))
                    parameters["startdate"] = _config.StartDate;
                if (!string.IsNullOrEmpty(_config.StopDate))
                    parameters["stopdate"] = _config.StopDate;

                // Build query string
                var queryString = string.Join("&", parameters.Select(p => $"{p.Key}={HttpUtility.UrlEncode(p.Value)}"));
                
                // Send request
                var response = await _httpClient.PostAsync(_config.ApiUrl, 
                    new StringContent(queryString, Encoding.UTF8, "application/x-www-form-urlencoded"));

                var responseText = await response.Content.ReadAsStringAsync();
                
                // NetGSM returns "00 <message_id>" for success, error codes start with other numbers
                var isSuccess = responseText.StartsWith("00");
                
                if (!isSuccess)
                {
                    var errorMessage = GetNetGsmErrorMessage(responseText);
                    System.Diagnostics.Debug.WriteLine($"NetGSM SMS error: {responseText} - {errorMessage}");
                }

                return isSuccess;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"NetGSM SMS exception: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> SendTwilioSmsAsync(string phoneNumber, string message)
        {
            // TODO: Implement Twilio SMS sending
            // This would require Twilio SDK and different authentication
            await Task.Delay(100); // Placeholder
            throw new NotImplementedException("Twilio SMS provider not yet implemented");
        }

        private string CleanPhoneNumber(string phoneNumber)
        {
            // Remove all non-numeric characters
            var cleaned = new string(phoneNumber.Where(char.IsDigit).ToArray());
            
            // Handle Turkish numbers - ensure they start with 90
            if (cleaned.StartsWith("0"))
            {
                cleaned = "90" + cleaned.Substring(1);
            }
            else if (!cleaned.StartsWith("90") && cleaned.Length == 10)
            {
                cleaned = "90" + cleaned;
            }

            return cleaned;
        }

        private string GetNetGsmErrorMessage(string errorCode)
        {
            return errorCode switch
            {
                "20" => "Mesaj metninde ki < > karakterleri engellenmiştir",
                "21" => "SMS gönderim saatleri dışında",
                "22" => "Gönderim esnasında hata oluştu",
                "23" => "Mükerrer gönderim",
                "24" => "Mesaj metni boş",
                "25" => "Gönderilen telefon numarası hatalı",
                "30" => "Geçersiz kullanıcı adı, şifre veya kullanıcınızın API erişim izninin olmama",
                "40" => "Mesaj başlığınız (header) sistemde tanımlı değil",
                "70" => "Hatalı sorgulama",
                _ => $"Bilinmeyen hata kodu: {errorCode}"
            };
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    /// <summary>
    /// SMS configuration settings
    /// </summary>
    public class SmsConfiguration
    {
        public string Provider { get; set; } = "netgsm"; // netgsm, twilio, etc.
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string Header { get; set; } = ""; // SMS header/sender name
        public string ApiUrl { get; set; } = "";
        public string StartDate { get; set; } = ""; // Optional: scheduled sending start date
        public string StopDate { get; set; } = ""; // Optional: scheduled sending stop date
        public bool EnableDeliveryReport { get; set; } = false;
        public string CompanyCode { get; set; } = ""; // 3G Bilişim bayii kodu
        public string? ScheduledDate { get; set; }
        public string? ScheduledTime { get; set; }

        /// <summary>
        /// Validate SMS configuration
        /// </summary>
        public bool IsValid()
        {
            var baseValid = !string.IsNullOrEmpty(Provider) &&
                            !string.IsNullOrEmpty(Username) &&
                            !string.IsNullOrEmpty(Password) &&
                            !string.IsNullOrEmpty(Header) &&
                            !string.IsNullOrEmpty(ApiUrl);

            if (!baseValid)
            {
                return false;
            }

            if (Provider.Equals("3gmesaj", StringComparison.OrdinalIgnoreCase) ||
                Provider.Equals("3gbilisim", StringComparison.OrdinalIgnoreCase))
            {
                return !string.IsNullOrEmpty(CompanyCode);
            }

            return true;
        }

        /// <summary>
        /// Create configuration for NetGSM
        /// </summary>
        public static SmsConfiguration CreateNetGsmConfiguration(string username, string password, string header)
        {
            return new SmsConfiguration
            {
                Provider = "netgsm",
                Username = username,
                Password = password,
                Header = header,
                ApiUrl = "https://api.netgsm.com.tr/sms/send/get/"
            };
        }

        /// <summary>
        /// Create configuration for 3G Bilişim (gateway.3gmesaj.com)
        /// </summary>
        public static SmsConfiguration Create3GBilisimConfiguration(string username, string password, string header, string companyCode, string apiUrl = "http://gateway.3gmesaj.com/SendSmsMany.aspx")
        {
            return new SmsConfiguration
            {
                Provider = "3gmesaj",
                Username = username,
                Password = password,
                Header = header,
                CompanyCode = companyCode,
                ApiUrl = apiUrl
            };
        }

        /// <summary>
        /// Create configuration for Twilio (placeholder)
        /// </summary>
        public static SmsConfiguration CreateTwilioConfiguration(string accountSid, string authToken, string fromNumber)
        {
            return new SmsConfiguration
            {
                Provider = "twilio",
                Username = accountSid,
                Password = authToken,
                Header = fromNumber,
                ApiUrl = "https://api.twilio.com/2010-04-01/Accounts/{accountSid}/Messages.json"
            };
        }
    }

    /// <summary>
    /// SMS sending result with detailed information
    /// </summary>
    public class SmsResult
    {
        public bool IsSuccess { get; set; }
        public string MessageId { get; set; } = "";
        public string ErrorCode { get; set; } = "";
        public string ErrorMessage { get; set; } = "";
        public DateTime SentAt { get; set; }
        public string Provider { get; set; } = "";

        public static SmsResult Success(string messageId, string provider)
        {
            return new SmsResult
            {
                IsSuccess = true,
                MessageId = messageId,
                SentAt = DateTime.UtcNow,
                Provider = provider
            };
        }

        public static SmsResult Failure(string errorCode, string errorMessage, string provider)
        {
            return new SmsResult
            {
                IsSuccess = false,
                ErrorCode = errorCode,
                ErrorMessage = errorMessage,
                SentAt = DateTime.UtcNow,
                Provider = provider
            };
        }
    }
}
