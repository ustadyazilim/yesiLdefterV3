using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.IO;

namespace UstadDesktop.WhatsAppIntegration.API
{
    /// <summary>
    /// WhatsApp Cloud API client for sending messages and managing media
    /// Implements WhatsApp Business Cloud API v20.0+ specifications
    /// </summary>
    public class WhatsAppCloudApiClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _version;
        private readonly string _phoneNumberId;
        private bool _disposed;

        public WhatsAppCloudApiClient(string apiBase, string apiVersion, string phoneNumberId, string accessToken, HttpMessageHandler handler = null)
        {
            if (string.IsNullOrEmpty(apiBase))
                throw new ArgumentException("API base URL cannot be null or empty", nameof(apiBase));
            if (string.IsNullOrEmpty(apiVersion))
                throw new ArgumentException("API version cannot be null or empty", nameof(apiVersion));
            if (string.IsNullOrEmpty(phoneNumberId))
                throw new ArgumentException("Phone number ID cannot be null or empty", nameof(phoneNumberId));
            if (string.IsNullOrEmpty(accessToken))
                throw new ArgumentException("Access token cannot be null or empty", nameof(accessToken));

            _httpClient = handler == null ? new HttpClient() : new HttpClient(handler);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.Timeout = TimeSpan.FromSeconds(30);

            _baseUrl = apiBase.TrimEnd('/');
            _version = apiVersion.Trim('/');
            _phoneNumberId = phoneNumberId;
        }

        private string MessagesUrl => $"{_baseUrl}/{_version}/{_phoneNumberId}/messages";
        private string MediaUrl => $"{_baseUrl}/{_version}/{_phoneNumberId}/media";

        /// <summary>
        /// Send a simple text message (within 24-hour window)
        /// </summary>
        /// <param name="toE164">Recipient phone number in E.164 format (e.g., 905551234567)</param>
        /// <param name="text">Message text</param>
        /// <param name="previewUrl">Whether to show URL previews</param>
        /// <returns>API response as JSON string</returns>
        public async Task<string> SendTextAsync(string toE164, string text, bool previewUrl = false)
        {
            if (string.IsNullOrEmpty(toE164))
                throw new ArgumentException("Recipient number cannot be null or empty", nameof(toE164));
            if (string.IsNullOrEmpty(text))
                throw new ArgumentException("Message text cannot be null or empty", nameof(text));

            var payload = new
            {
                messaging_product = "whatsapp",
                to = toE164,
                type = "text",
                text = new { preview_url = previewUrl, body = text }
            };

            return await PostJsonAsync(MessagesUrl, payload);
        }

        /// <summary>
        /// Send a template message (for use outside 24-hour window)
        /// </summary>
        /// <param name="toE164">Recipient phone number in E.164 format</param>
        /// <param name="templateName">WhatsApp approved template name</param>
        /// <param name="languageCode">Template language code (e.g., "tr", "en")</param>
        /// <param name="parameters">Template parameters (optional)</param>
        /// <returns>API response as JSON string</returns>
        public async Task<string> SendTemplateAsync(string toE164, string templateName, string languageCode = "tr", object[] parameters = null)
        {
            if (string.IsNullOrEmpty(toE164))
                throw new ArgumentException("Recipient number cannot be null or empty", nameof(toE164));
            if (string.IsNullOrEmpty(templateName))
                throw new ArgumentException("Template name cannot be null or empty", nameof(templateName));

            var template = new
            {
                name = templateName,
                language = new { code = languageCode }
            };

            // Add parameters if provided
            if (parameters != null && parameters.Length > 0)
            {
                var components = new[]
                {
                    new
                    {
                        type = "body",
                        parameters = parameters
                    }
                };
                template = new
                {
                    name = templateName,
                    language = new { code = languageCode },
                    components = components
                };
            }

            var payload = new
            {
                messaging_product = "whatsapp",
                to = toE164,
                type = "template",
                template = template
            };

            return await PostJsonAsync(MessagesUrl, payload);
        }

        /// <summary>
        /// Send an image message by URL
        /// </summary>
        /// <param name="toE164">Recipient phone number in E.164 format</param>
        /// <param name="imageUrl">Public URL of the image</param>
        /// <param name="caption">Optional image caption</param>
        /// <returns>API response as JSON string</returns>
        public async Task<string> SendImageByUrlAsync(string toE164, string imageUrl, string caption = null)
        {
            if (string.IsNullOrEmpty(toE164))
                throw new ArgumentException("Recipient number cannot be null or empty", nameof(toE164));
            if (string.IsNullOrEmpty(imageUrl))
                throw new ArgumentException("Image URL cannot be null or empty", nameof(imageUrl));

            var image = string.IsNullOrEmpty(caption) 
                ? new { link = imageUrl }
                : new { link = imageUrl, caption = caption };

            var payload = new
            {
                messaging_product = "whatsapp",
                to = toE164,
                type = "image",
                image = image
            };

            return await PostJsonAsync(MessagesUrl, payload);
        }

        /// <summary>
        /// Upload media file to WhatsApp servers
        /// </summary>
        /// <param name="fileBytes">File content as byte array</param>
        /// <param name="fileName">Original file name</param>
        /// <param name="mimeType">MIME type of the file</param>
        /// <returns>API response containing media ID</returns>
        public async Task<string> UploadMediaAsync(byte[] fileBytes, string fileName, string mimeType)
        {
            if (fileBytes == null || fileBytes.Length == 0)
                throw new ArgumentException("File bytes cannot be null or empty", nameof(fileBytes));
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("File name cannot be null or empty", nameof(fileName));
            if (string.IsNullOrEmpty(mimeType))
                throw new ArgumentException("MIME type cannot be null or empty", nameof(mimeType));

            using var content = new MultipartFormDataContent();
            
            var fileContent = new ByteArrayContent(fileBytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
            content.Add(fileContent, "file", fileName);
            content.Add(new StringContent("whatsapp"), "messaging_product");

            var response = await _httpClient.PostAsync(MediaUrl, content);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Send an image message using uploaded media ID
        /// </summary>
        /// <param name="toE164">Recipient phone number in E.164 format</param>
        /// <param name="mediaId">Media ID from upload response</param>
        /// <param name="caption">Optional image caption</param>
        /// <returns>API response as JSON string</returns>
        public async Task<string> SendImageByMediaIdAsync(string toE164, string mediaId, string caption = null)
        {
            if (string.IsNullOrEmpty(toE164))
                throw new ArgumentException("Recipient number cannot be null or empty", nameof(toE164));
            if (string.IsNullOrEmpty(mediaId))
                throw new ArgumentException("Media ID cannot be null or empty", nameof(mediaId));

            var image = string.IsNullOrEmpty(caption)
                ? new { id = mediaId }
                : new { id = mediaId, caption = caption };

            var payload = new
            {
                messaging_product = "whatsapp",
                to = toE164,
                type = "image",
                image = image
            };

            return await PostJsonAsync(MessagesUrl, payload);
        }

        /// <summary>
        /// Send a document message using uploaded media ID
        /// </summary>
        /// <param name="toE164">Recipient phone number in E.164 format</param>
        /// <param name="mediaId">Media ID from upload response</param>
        /// <param name="filename">Document filename</param>
        /// <param name="caption">Optional document caption</param>
        /// <returns>API response as JSON string</returns>
        public async Task<string> SendDocumentAsync(string toE164, string mediaId, string filename, string caption = null)
        {
            if (string.IsNullOrEmpty(toE164))
                throw new ArgumentException("Recipient number cannot be null or empty", nameof(toE164));
            if (string.IsNullOrEmpty(mediaId))
                throw new ArgumentException("Media ID cannot be null or empty", nameof(mediaId));

            var document = new { id = mediaId, filename = filename };
            if (!string.IsNullOrEmpty(caption))
            {
                document = new { id = mediaId, filename = filename, caption = caption };
            }

            var payload = new
            {
                messaging_product = "whatsapp",
                to = toE164,
                type = "document",
                document = document
            };

            return await PostJsonAsync(MessagesUrl, payload);
        }

        /// <summary>
        /// Mark a message as read (important for customer service quality metrics)
        /// </summary>
        /// <param name="messageId">WhatsApp message ID</param>
        /// <returns>API response as JSON string</returns>
        public async Task<string> MarkAsReadAsync(string messageId)
        {
            if (string.IsNullOrEmpty(messageId))
                throw new ArgumentException("Message ID cannot be null or empty", nameof(messageId));

            var payload = new
            {
                messaging_product = "whatsapp",
                status = "read",
                message_id = messageId
            };

            return await PostJsonAsync(MessagesUrl, payload);
        }

        /// <summary>
        /// Get media URL from media ID
        /// </summary>
        /// <param name="mediaId">Media ID from webhook</param>
        /// <returns>Media URL response</returns>
        public async Task<string> GetMediaUrlAsync(string mediaId)
        {
            if (string.IsNullOrEmpty(mediaId))
                throw new ArgumentException("Media ID cannot be null or empty", nameof(mediaId));

            var url = $"{_baseUrl}/{_version}/{mediaId}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Download media file from WhatsApp servers
        /// </summary>
        /// <param name="mediaUrl">Media URL from GetMediaUrlAsync</param>
        /// <returns>File content as byte array</returns>
        public async Task<byte[]> DownloadMediaAsync(string mediaUrl)
        {
            if (string.IsNullOrEmpty(mediaUrl))
                throw new ArgumentException("Media URL cannot be null or empty", nameof(mediaUrl));

            var response = await _httpClient.GetAsync(mediaUrl);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadAsByteArrayAsync();
        }

        private async Task<string> PostJsonAsync(string url, object payload)
        {
            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(payload, options);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"WhatsApp API error {response.StatusCode}: {responseText}");
            }

            return responseText;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _httpClient?.Dispose();
                _disposed = true;
            }
        }
    }

    /// <summary>
    /// Response models for WhatsApp API
    /// </summary>
    public class WhatsAppApiResponse
    {
        [JsonPropertyName("messaging_product")]
        public string MessagingProduct { get; set; }

        [JsonPropertyName("contacts")]
        public Contact[] Contacts { get; set; }

        [JsonPropertyName("messages")]
        public Message[] Messages { get; set; }
    }

    public class Contact
    {
        [JsonPropertyName("input")]
        public string Input { get; set; }

        [JsonPropertyName("wa_id")]
        public string WhatsAppId { get; set; }
    }

    public class Message
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
    }

    public class MediaUploadResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
    }

    public class MediaUrlResponse
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("mime_type")]
        public string MimeType { get; set; }

        [JsonPropertyName("sha256")]
        public string Sha256 { get; set; }

        [JsonPropertyName("file_size")]
        public long FileSize { get; set; }
    }
}
