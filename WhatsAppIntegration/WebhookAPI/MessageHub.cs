using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Data.SqlClient;
using UstadDesktop.WhatsAppIntegration.API;

namespace UstadDesktop.WhatsAppIntegration.WebhookAPI
{
    /// <summary>
    /// SignalR Hub for real-time WhatsApp message handling
    /// Supports multi-operator customer service with role-based access
    /// </summary>
    [Authorize]
    public class MessageHub : Hub
    {
        private readonly WhatsAppCloudApiClient _whatsAppClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MessageHub> _logger;

        public MessageHub(WhatsAppCloudApiClient whatsAppClient, IConfiguration configuration, ILogger<MessageHub> logger)
        {
            _whatsAppClient = whatsAppClient;
            _configuration = configuration;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var operatorId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var operatorName = Context.User?.Identity?.Name ?? "Unknown";
            var role = Context.User?.FindFirstValue(ClaimTypes.Role) ?? "Agent";

            _logger.LogInformation("Operator {OperatorName} ({Role}) connected with connection ID {ConnectionId}", 
                operatorName, role, Context.ConnectionId);

            // Add to appropriate groups based on role
            if (role == "Admin")
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
            }
            await Groups.AddToGroupAsync(Context.ConnectionId, "Operators");

            // Log connection event
            if (int.TryParse(operatorId, out var opId))
            {
                await LogAuditEvent(opId, "OPERATOR_CONNECTED", $"SignalR connection established", GetClientInfo());
            }

            // Notify other operators
            await Clients.Others.SendAsync("OperatorStatusChanged", operatorName, "online");

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var operatorId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var operatorName = Context.User?.Identity?.Name ?? "Unknown";

            _logger.LogInformation("Operator {OperatorName} disconnected: {Exception}", 
                operatorName, exception?.Message ?? "Normal disconnect");

            // Log disconnection event
            if (int.TryParse(operatorId, out var opId))
            {
                await LogAuditEvent(opId, "OPERATOR_DISCONNECTED", 
                    exception?.Message ?? "Normal disconnect", GetClientInfo());
            }

            // Notify other operators
            await Clients.Others.SendAsync("OperatorStatusChanged", operatorName, "offline");

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Send a WhatsApp message to a customer
        /// </summary>
        /// <param name="customerNumber">Customer phone number in E.164 format</param>
        /// <param name="message">Message text</param>
        /// <param name="messageType">Type of message (text, template, etc.)</param>
        /// <returns>Task</returns>
        public async Task SendMessage(string customerNumber, string message, string messageType = "text")
        {
            try
            {
                var operatorId = int.Parse(Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var operatorName = Context.User!.Identity!.Name!;
                var role = Context.User!.FindFirstValue(ClaimTypes.Role)!;
                var firmId = int.Parse(Context.User!.FindFirstValue("firmId")!);

                // Validate input
                if (string.IsNullOrWhiteSpace(customerNumber) || string.IsNullOrWhiteSpace(message))
                {
                    await Clients.Caller.SendAsync("Error", "Customer number and message are required");
                    return;
                }

                // Check if operator has permission to send to this conversation
                var conversationId = await GetConversationId(customerNumber, firmId);
                if (conversationId == null)
                {
                    await Clients.Caller.SendAsync("Error", "Conversation not found");
                    return;
                }

                var hasPermission = await CheckConversationPermission(operatorId, conversationId.Value, role);
                if (!hasPermission)
                {
                    await Clients.Caller.SendAsync("Error", "You don't have permission to send messages in this conversation");
                    return;
                }

                // Send message via WhatsApp API
                string apiResponse;
                switch (messageType.ToLower())
                {
                    case "text":
                        apiResponse = await _whatsAppClient.SendTextAsync(customerNumber, message);
                        break;
                    case "template":
                        // For template messages, extract template name from message (format: "template:template_name")
                        var templateName = message.StartsWith("template:") ? message.Substring(9) : message;
                        apiResponse = await _whatsAppClient.SendTemplateAsync(customerNumber, templateName);
                        break;
                    default:
                        throw new ArgumentException($"Unsupported message type: {messageType}");
                }

                // Parse API response to get message ID
                var messageId = ExtractMessageIdFromResponse(apiResponse);

                // Store message in database
                await StoreOutgoingMessage(conversationId.Value, messageId, messageType, message, operatorId);

                // Update conversation last message time
                await UpdateConversationLastMessage(conversationId.Value);

                // Broadcast to all operators
                await Clients.All.SendAsync("MessageSent", new
                {
                    ConversationId = conversationId.Value,
                    CustomerNumber = customerNumber,
                    Message = message,
                    MessageType = messageType,
                    OperatorName = operatorName,
                    OperatorId = operatorId,
                    Timestamp = DateTime.UtcNow,
                    MessageId = messageId
                });

                // Log the action
                await LogAuditEvent(operatorId, "MESSAGE_SENT", 
                    $"Sent {messageType} message to {customerNumber}: {message.Substring(0, Math.Min(50, message.Length))}", 
                    GetClientInfo());

                _logger.LogInformation("Message sent by {OperatorName} to {CustomerNumber}", operatorName, customerNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                await Clients.Caller.SendAsync("Error", $"Failed to send message: {ex.Message}");
            }
        }

        /// <summary>
        /// Assign a conversation to an operator
        /// </summary>
        /// <param name="conversationId">Conversation ID</param>
        /// <param name="assignToOperatorId">Operator ID to assign to (null to unassign)</param>
        /// <returns>Task</returns>
        public async Task AssignConversation(int conversationId, int? assignToOperatorId)
        {
            try
            {
                var operatorId = int.Parse(Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var operatorName = Context.User!.Identity!.Name!;
                var role = Context.User!.FindFirstValue(ClaimTypes.Role)!;

                // Only admins can assign conversations
                if (role != "Admin")
                {
                    await Clients.Caller.SendAsync("Error", "Only administrators can assign conversations");
                    return;
                }

                using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                await conn.OpenAsync();

                // Get assignee name if assigning
                string assigneeName = null;
                if (assignToOperatorId.HasValue)
                {
                    using var nameCmd = conn.CreateCommand();
                    nameCmd.CommandText = "SELECT FullName FROM WhatsApp_Operators WHERE Id = @id";
                    nameCmd.Parameters.AddWithValue("@id", assignToOperatorId.Value);
                    assigneeName = await nameCmd.ExecuteScalarAsync() as string;

                    if (assigneeName == null)
                    {
                        await Clients.Caller.SendAsync("Error", "Assigned operator not found");
                        return;
                    }
                }

                // Update conversation assignment
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    UPDATE WhatsApp_Conversations 
                    SET AssignedOperatorId = @assignToOperatorId, UpdatedAt = GETUTCDATE()
                    WHERE Id = @conversationId";
                cmd.Parameters.AddWithValue("@assignToOperatorId", assignToOperatorId ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@conversationId", conversationId);
                
                var rowsAffected = await cmd.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                {
                    await Clients.Caller.SendAsync("Error", "Conversation not found");
                    return;
                }

                // Get conversation details for broadcast
                var conversationDetails = await GetConversationDetails(conn, conversationId);

                // Broadcast assignment change to all operators
                await Clients.All.SendAsync("ConversationAssigned", new
                {
                    ConversationId = conversationId,
                    CustomerNumber = conversationDetails?.CustomerNumber,
                    AssignedOperatorId = assignToOperatorId,
                    AssignedOperatorName = assigneeName,
                    AssignedBy = operatorName,
                    Timestamp = DateTime.UtcNow
                });

                // Log the assignment
                var action = assignToOperatorId.HasValue ? "CONVERSATION_ASSIGNED" : "CONVERSATION_UNASSIGNED";
                var details = assignToOperatorId.HasValue 
                    ? $"Assigned conversation {conversationId} to {assigneeName}" 
                    : $"Unassigned conversation {conversationId}";
                await LogAuditEvent(operatorId, action, details, GetClientInfo());

                _logger.LogInformation("Conversation {ConversationId} assigned to {AssigneeName} by {OperatorName}", 
                    conversationId, assigneeName ?? "unassigned", operatorName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning conversation");
                await Clients.Caller.SendAsync("Error", $"Failed to assign conversation: {ex.Message}");
            }
        }

        /// <summary>
        /// Mark messages as read
        /// </summary>
        /// <param name="messageIds">Array of message IDs to mark as read</param>
        /// <returns>Task</returns>
        public async Task MarkMessagesAsRead(string[] messageIds)
        {
            try
            {
                var operatorId = int.Parse(Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!);

                using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                await conn.OpenAsync();

                foreach (var messageId in messageIds)
                {
                    // Mark as read in WhatsApp
                    if (!string.IsNullOrEmpty(messageId))
                    {
                        try
                        {
                            await _whatsAppClient.MarkAsReadAsync(messageId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to mark message {MessageId} as read in WhatsApp", messageId);
                        }
                    }

                    // Update in database
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = "UPDATE WhatsApp_Messages SET IsRead = 1 WHERE WhatsAppMessageId = @messageId";
                    cmd.Parameters.AddWithValue("@messageId", messageId);
                    await cmd.ExecuteNonQueryAsync();
                }

                // Broadcast read status to other operators
                await Clients.Others.SendAsync("MessagesMarkedAsRead", messageIds, operatorId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking messages as read");
                await Clients.Caller.SendAsync("Error", $"Failed to mark messages as read: {ex.Message}");
            }
        }

        /// <summary>
        /// Get conversation history for an operator
        /// </summary>
        /// <param name="conversationId">Conversation ID</param>
        /// <param name="pageSize">Number of messages to retrieve</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>Task</returns>
        public async Task GetConversationHistory(int conversationId, int pageSize = 50, int offset = 0)
        {
            try
            {
                var operatorId = int.Parse(Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var role = Context.User!.FindFirstValue(ClaimTypes.Role)!;

                // Check permission
                var hasPermission = await CheckConversationPermission(operatorId, conversationId, role);
                if (!hasPermission)
                {
                    await Clients.Caller.SendAsync("Error", "You don't have permission to view this conversation");
                    return;
                }

                using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                await conn.OpenAsync();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    SELECT m.Id, m.Direction, m.MessageType, m.Body, m.MediaUrl, m.CreatedAt, m.IsRead,
                           o.FullName AS OperatorName
                    FROM WhatsApp_Messages m
                    LEFT JOIN WhatsApp_Operators o ON m.OperatorId = o.Id
                    WHERE m.ConversationId = @conversationId
                    ORDER BY m.CreatedAt DESC
                    OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";
                
                cmd.Parameters.AddWithValue("@conversationId", conversationId);
                cmd.Parameters.AddWithValue("@offset", offset);
                cmd.Parameters.AddWithValue("@pageSize", pageSize);

                var messages = new List<object>();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    messages.Add(new
                    {
                        Id = reader.GetInt64("Id"),
                        Direction = reader.GetString("Direction"),
                        MessageType = reader.GetString("MessageType"),
                        Body = reader.GetString("Body"),
                        MediaUrl = reader.IsDBNull("MediaUrl") ? null : reader.GetString("MediaUrl"),
                        CreatedAt = reader.GetDateTime("CreatedAt"),
                        IsRead = reader.GetBoolean("IsRead"),
                        OperatorName = reader.IsDBNull("OperatorName") ? null : reader.GetString("OperatorName")
                    });
                }

                await Clients.Caller.SendAsync("ConversationHistory", conversationId, messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversation history");
                await Clients.Caller.SendAsync("Error", $"Failed to get conversation history: {ex.Message}");
            }
        }

        /// <summary>
        /// Update conversation status (Open, Closed, Pending)
        /// </summary>
        /// <param name="conversationId">Conversation ID</param>
        /// <param name="status">New status</param>
        /// <returns>Task</returns>
        public async Task UpdateConversationStatus(int conversationId, string status)
        {
            try
            {
                var operatorId = int.Parse(Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var operatorName = Context.User!.Identity!.Name!;
                var role = Context.User!.FindFirstValue(ClaimTypes.Role)!;

                // Validate status
                var validStatuses = new[] { "Open", "Closed", "Pending" };
                if (!validStatuses.Contains(status))
                {
                    await Clients.Caller.SendAsync("Error", "Invalid status. Valid values: Open, Closed, Pending");
                    return;
                }

                // Check permission
                var hasPermission = await CheckConversationPermission(operatorId, conversationId, role);
                if (!hasPermission)
                {
                    await Clients.Caller.SendAsync("Error", "You don't have permission to update this conversation");
                    return;
                }

                using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                await conn.OpenAsync();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    UPDATE WhatsApp_Conversations 
                    SET Status = @status, UpdatedAt = GETUTCDATE()
                    WHERE Id = @conversationId";
                cmd.Parameters.AddWithValue("@status", status);
                cmd.Parameters.AddWithValue("@conversationId", conversationId);
                
                var rowsAffected = await cmd.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                {
                    await Clients.Caller.SendAsync("Error", "Conversation not found");
                    return;
                }

                // Broadcast status change
                await Clients.All.SendAsync("ConversationStatusChanged", new
                {
                    ConversationId = conversationId,
                    Status = status,
                    UpdatedBy = operatorName,
                    Timestamp = DateTime.UtcNow
                });

                // Log the action
                await LogAuditEvent(operatorId, "CONVERSATION_STATUS_CHANGED", 
                    $"Changed conversation {conversationId} status to {status}", GetClientInfo());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating conversation status");
                await Clients.Caller.SendAsync("Error", $"Failed to update conversation status: {ex.Message}");
            }
        }

        // ===== HELPER METHODS =====

        private async Task<int?> GetConversationId(string customerNumber, int firmId)
        {
            using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id FROM WhatsApp_Conversations WHERE CustomerNumber = @number AND FirmId = @firmId";
            cmd.Parameters.AddWithValue("@number", customerNumber);
            cmd.Parameters.AddWithValue("@firmId", firmId);

            return await cmd.ExecuteScalarAsync() as int?;
        }

        private async Task<bool> CheckConversationPermission(int operatorId, int conversationId, string role)
        {
            // Admins have access to all conversations
            if (role == "Admin")
                return true;

            using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT COUNT(*) 
                FROM WhatsApp_Conversations 
                WHERE Id = @conversationId 
                  AND (AssignedOperatorId = @operatorId OR AssignedOperatorId IS NULL)";
            cmd.Parameters.AddWithValue("@conversationId", conversationId);
            cmd.Parameters.AddWithValue("@operatorId", operatorId);

            return (int)await cmd.ExecuteScalarAsync() > 0;
        }

        private async Task StoreOutgoingMessage(int conversationId, string messageId, string messageType, string body, int operatorId)
        {
            using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO WhatsApp_Messages (ConversationId, WhatsAppMessageId, Direction, MessageType, Body, OperatorId, CreatedAt)
                VALUES (@conversationId, @messageId, 'Out', @messageType, @body, @operatorId, GETUTCDATE())";
            cmd.Parameters.AddWithValue("@conversationId", conversationId);
            cmd.Parameters.AddWithValue("@messageId", messageId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@messageType", messageType);
            cmd.Parameters.AddWithValue("@body", body);
            cmd.Parameters.AddWithValue("@operatorId", operatorId);
            await cmd.ExecuteNonQueryAsync();
        }

        private async Task UpdateConversationLastMessage(int conversationId)
        {
            using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE WhatsApp_Conversations SET LastMessageAt = GETUTCDATE() WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", conversationId);
            await cmd.ExecuteNonQueryAsync();
        }

        private async Task<dynamic> GetConversationDetails(SqlConnection conn, int conversationId)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT CustomerNumber, CustomerName FROM WhatsApp_Conversations WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", conversationId);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new
                {
                    CustomerNumber = reader.GetString("CustomerNumber"),
                    CustomerName = reader.IsDBNull("CustomerName") ? null : reader.GetString("CustomerName")
                };
            }
            return null;
        }

        private string ExtractMessageIdFromResponse(string apiResponse)
        {
            try
            {
                var doc = System.Text.Json.JsonDocument.Parse(apiResponse);
                return doc.RootElement
                    .GetProperty("messages")[0]
                    .GetProperty("id")
                    .GetString();
            }
            catch
            {
                return null;
            }
        }

        private async Task LogAuditEvent(int operatorId, string action, string details, string clientInfo)
        {
            try
            {
                using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                await conn.OpenAsync();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO WhatsApp_AuditLogs (FirmId, OperatorId, Action, Details, IpAddress, UserAgent, CreatedAt)
                    VALUES (1, @operatorId, @action, @details, @ipAddress, @userAgent, GETUTCDATE())";
                
                cmd.Parameters.AddWithValue("@operatorId", operatorId);
                cmd.Parameters.AddWithValue("@action", action);
                cmd.Parameters.AddWithValue("@details", details ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ipAddress", Context.GetHttpContext()?.Connection?.RemoteIpAddress?.ToString() ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@userAgent", clientInfo ?? (object)DBNull.Value);
                
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log audit event");
            }
        }

        private string GetClientInfo()
        {
            var httpContext = Context.GetHttpContext();
            return httpContext?.Request?.Headers?.UserAgent.ToString() ?? "Unknown";
        }
    }
}
