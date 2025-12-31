using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Data.SqlClient;
using System.Text.Json;
using UstadDesktop.WhatsAppIntegration.Security;
using UstadDesktop.WhatsAppIntegration.API;

var builder = WebApplication.CreateBuilder(args);

// Configuration
var configuration = builder.Configuration;

// Services
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// WhatsApp API Client
builder.Services.AddSingleton<WhatsAppCloudApiClient>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    return new WhatsAppCloudApiClient(
        config["WhatsApp:ApiBase"] ?? "https://graph.facebook.com",
        config["WhatsApp:ApiVersion"] ?? "v20.0",
        config["WhatsApp:PhoneNumberId"] ?? throw new InvalidOperationException("WhatsApp:PhoneNumberId not configured"),
        config["WhatsApp:AccessToken"] ?? throw new InvalidOperationException("WhatsApp:AccessToken not configured")
    );
});

// JWT Authentication
var jwtKey = configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key not configured");
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };

        // SignalR JWT support
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken) &&
                    context.HttpContext.Request.Path.StartsWithSegments("/messagehub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Middleware
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// SignalR Hub
app.MapHub<MessageHub>("/messagehub").RequireAuthorization();

// ===== AUTHENTICATION ENDPOINTS =====

app.MapPost("/auth/login", async (HttpContext context) =>
{
    var loginRequest = await context.Request.ReadFromJsonAsync<LoginRequest>();
    if (loginRequest is null)
        return Results.BadRequest("Invalid request");

    try
    {
        using var conn = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        // Get operator with lockout check
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT Id, FirmId, FullName, Role, PasswordHash, Salt, Iterations, 
                   FailedCount, LockoutEnd, IsActive
            FROM WhatsApp_Operators 
            WHERE UserName = @username";
        cmd.Parameters.AddWithValue("@username", loginRequest.UserName);

        using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            await LogAuditEvent(conn, null, "LOGIN_FAILED", "Invalid username", context.Connection.RemoteIpAddress?.ToString());
            return Results.Unauthorized();
        }

        var operatorId = reader.GetInt32("Id");
        var firmId = reader.GetInt32("FirmId");
        var fullName = reader.GetString("FullName");
        var role = reader.GetString("Role");
        var passwordHash = (byte[])reader["PasswordHash"];
        var salt = (byte[])reader["Salt"];
        var iterations = reader.GetInt32("Iterations");
        var failedCount = reader.GetInt32("FailedCount");
        var lockoutEnd = reader.IsDBNull("LockoutEnd") ? null : reader.GetDateTime("LockoutEnd");
        var isActive = reader.GetBoolean("IsActive");

        await reader.CloseAsync();

        // Check if account is active
        if (!isActive)
        {
            await LogAuditEvent(conn, operatorId, "LOGIN_FAILED", "Account disabled", context.Connection.RemoteIpAddress?.ToString());
            return Results.Unauthorized();
        }

        // Check lockout
        if (lockoutEnd.HasValue && lockoutEnd.Value > DateTime.UtcNow)
        {
            await LogAuditEvent(conn, operatorId, "LOGIN_FAILED", "Account locked", context.Connection.RemoteIpAddress?.ToString());
            return Results.Problem("Account is locked. Please try again later.", statusCode: 423);
        }

        // Verify password
        bool passwordValid = SecurePasswordHasher.Verify(loginRequest.Password, passwordHash, salt, iterations);

        if (!passwordValid)
        {
            // Increment failed count and potentially lock account
            await IncrementFailedLogin(conn, operatorId);
            await LogAuditEvent(conn, operatorId, "LOGIN_FAILED", "Invalid password", context.Connection.RemoteIpAddress?.ToString());
            return Results.Unauthorized();
        }

        // Reset failed count on successful login
        await ResetFailedLogin(conn, operatorId);

        // Generate JWT token
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, operatorId.ToString()),
            new Claim(ClaimTypes.Name, fullName),
            new Claim("username", loginRequest.UserName),
            new Claim(ClaimTypes.Role, role),
            new Claim("firmId", firmId.ToString())
        };

        var tokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(int.Parse(configuration["Jwt:ExpiresMinutes"] ?? "480")),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
            Issuer = configuration["Jwt:Issuer"],
            Audience = configuration["Jwt:Audience"]
        };

        var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwt = tokenHandler.WriteToken(token);

        await LogAuditEvent(conn, operatorId, "LOGIN_SUCCESS", "Successful login", context.Connection.RemoteIpAddress?.ToString());

        return Results.Ok(new LoginResponse
        {
            Token = jwt,
            OperatorId = operatorId,
            FullName = fullName,
            Role = role,
            FirmId = firmId
        });
    }
    catch (Exception ex)
    {
        // Log error
        Console.WriteLine($"Login error: {ex.Message}");
        return Results.Problem("Internal server error");
    }
});

// Password reset endpoints
app.MapPost("/auth/reset/start", async (HttpContext context) =>
{
    var request = await context.Request.ReadFromJsonAsync<PasswordResetStartRequest>();
    if (request is null)
        return Results.BadRequest("Invalid request");

    try
    {
        using var conn = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        // Find operator
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Email, Phone FROM WhatsApp_Operators WHERE UserName = @username AND IsActive = 1";
        cmd.Parameters.AddWithValue("@username", request.UserName);

        using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            return Results.NotFound("User not found");

        var operatorId = reader.GetInt32("Id");
        var email = reader.IsDBNull("Email") ? null : reader.GetString("Email");
        var phone = reader.IsDBNull("Phone") ? null : reader.GetString("Phone");
        await reader.CloseAsync();

        // Generate reset token
        var token = SecurePasswordHasher.GenerateSecureToken();
        var expiresAt = DateTime.UtcNow.AddHours(1);

        using var insertCmd = conn.CreateCommand();
        insertCmd.CommandText = @"
            INSERT INTO WhatsApp_PasswordResetTokens (OperatorId, Token, ExpiresAt)
            VALUES (@operatorId, @token, @expiresAt)";
        insertCmd.Parameters.AddWithValue("@operatorId", operatorId);
        insertCmd.Parameters.AddWithValue("@token", token);
        insertCmd.Parameters.AddWithValue("@expiresAt", expiresAt);
        await insertCmd.ExecuteNonQueryAsync();

        await LogAuditEvent(conn, operatorId, "PASSWORD_RESET_REQUESTED", $"Reset token generated", context.Connection.RemoteIpAddress?.ToString());

        // TODO: Send email/SMS with token
        // For now, return token (in production, only send via secure channel)
        return Results.Ok(new { Token = token, Message = "Reset token generated" });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Password reset start error: {ex.Message}");
        return Results.Problem("Internal server error");
    }
});

app.MapPost("/auth/reset/confirm", async (HttpContext context) =>
{
    var request = await context.Request.ReadFromJsonAsync<PasswordResetConfirmRequest>();
    if (request is null)
        return Results.BadRequest("Invalid request");

    try
    {
        using var conn = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        // Verify token
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT rt.OperatorId
            FROM WhatsApp_PasswordResetTokens rt
            WHERE rt.Token = @token 
              AND rt.Used = 0 
              AND rt.ExpiresAt > GETUTCDATE()";
        cmd.Parameters.AddWithValue("@token", request.Token);

        var operatorId = await cmd.ExecuteScalarAsync() as int?;
        if (!operatorId.HasValue)
        {
            return Results.BadRequest("Invalid or expired token");
        }

        // Validate new password
        if (!SecurePasswordHasher.IsPasswordStrong(request.NewPassword))
        {
            return Results.BadRequest("Password does not meet security requirements");
        }

        // Hash new password
        var (hash, salt, iterations) = SecurePasswordHasher.HashPassword(request.NewPassword);

        // Update password and mark token as used
        using var transaction = conn.BeginTransaction();
        try
        {
            // Update password
            using var updateCmd = conn.CreateCommand();
            updateCmd.Transaction = transaction;
            updateCmd.CommandText = @"
                UPDATE WhatsApp_Operators 
                SET PasswordHash = @hash, Salt = @salt, Iterations = @iterations,
                    FailedCount = 0, LockoutEnd = NULL, UpdatedAt = GETUTCDATE()
                WHERE Id = @operatorId";
            updateCmd.Parameters.Add("@hash", System.Data.SqlDbType.VarBinary, hash.Length).Value = hash;
            updateCmd.Parameters.Add("@salt", System.Data.SqlDbType.VarBinary, salt.Length).Value = salt;
            updateCmd.Parameters.AddWithValue("@iterations", iterations);
            updateCmd.Parameters.AddWithValue("@operatorId", operatorId.Value);
            await updateCmd.ExecuteNonQueryAsync();

            // Mark token as used
            using var tokenCmd = conn.CreateCommand();
            tokenCmd.Transaction = transaction;
            tokenCmd.CommandText = "UPDATE WhatsApp_PasswordResetTokens SET Used = 1 WHERE Token = @token";
            tokenCmd.Parameters.AddWithValue("@token", request.Token);
            await tokenCmd.ExecuteNonQueryAsync();

            await transaction.CommitAsync();
            
            await LogAuditEvent(conn, operatorId.Value, "PASSWORD_RESET_SUCCESS", "Password reset completed", context.Connection.RemoteIpAddress?.ToString());

            return Results.Ok(new { Message = "Password reset successfully" });
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Password reset confirm error: {ex.Message}");
        return Results.Problem("Internal server error");
    }
});

// ===== WHATSAPP WEBHOOK ENDPOINTS =====

app.MapGet("/webhook", (HttpContext context) =>
{
    var mode = context.Request.Query["hub.mode"];
    var token = context.Request.Query["hub.verify_token"];
    var challenge = context.Request.Query["hub.challenge"];

    var verifyToken = configuration["WhatsApp:VerifyToken"];

    if (mode == "subscribe" && token == verifyToken)
    {
        Console.WriteLine("Webhook verified successfully");
        return Results.Content(challenge!, "text/plain");
    }

    Console.WriteLine($"Webhook verification failed: mode={mode}, token={token}");
    return Results.Unauthorized();
});

app.MapPost("/webhook", async (HttpContext context, IHubContext<MessageHub> hubContext) =>
{
    try
    {
        using var reader = new StreamReader(context.Request.Body);
        var body = await reader.ReadToEndAsync();
        
        Console.WriteLine($"Webhook received: {body}");

        // Parse WhatsApp webhook payload
        var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        if (!root.TryGetProperty("entry", out var entries) || entries.ValueKind != JsonValueKind.Array)
            return Results.Ok();

        foreach (var entry in entries.EnumerateArray())
        {
            if (!entry.TryGetProperty("changes", out var changes) || changes.ValueKind != JsonValueKind.Array)
                continue;

            foreach (var change in changes.EnumerateArray())
            {
                if (!change.TryGetProperty("value", out var value))
                    continue;

                // Handle incoming messages
                if (value.TryGetProperty("messages", out var messages) && messages.ValueKind == JsonValueKind.Array)
                {
                    foreach (var message in messages.EnumerateArray())
                    {
                        await ProcessIncomingMessage(message, hubContext, configuration);
                    }
                }

                // Handle message status updates
                if (value.TryGetProperty("statuses", out var statuses) && statuses.ValueKind == JsonValueKind.Array)
                {
                    foreach (var status in statuses.EnumerateArray())
                    {
                        await ProcessMessageStatus(status, hubContext, configuration);
                    }
                }
            }
        }

        return Results.Ok();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Webhook processing error: {ex.Message}");
        return Results.Ok(); // Always return 200 to WhatsApp
    }
});

// ===== ADMIN ENDPOINTS =====

app.MapGet("/admin/operators", async (HttpContext context) =>
{
    // TODO: Implement admin endpoints for operator management
    return Results.Ok(new { Message = "Admin endpoints coming soon" });
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.Run("http://0.0.0.0:5000");

// ===== HELPER METHODS =====

static async Task ProcessIncomingMessage(JsonElement message, IHubContext<MessageHub> hubContext, IConfiguration config)
{
    try
    {
        var from = message.GetProperty("from").GetString();
        var messageId = message.GetProperty("id").GetString();
        var timestamp = message.GetProperty("timestamp").GetString();
        var type = message.GetProperty("type").GetString();

        string messageBody = "";
        string mediaUrl = null;

        switch (type)
        {
            case "text":
                messageBody = message.GetProperty("text").GetProperty("body").GetString();
                break;
            case "image":
                var image = message.GetProperty("image");
                messageBody = image.TryGetProperty("caption", out var caption) ? caption.GetString() : "[Image]";
                mediaUrl = image.TryGetProperty("id", out var mediaId) ? mediaId.GetString() : null;
                break;
            case "document":
                var document = message.GetProperty("document");
                messageBody = $"[Document: {document.GetProperty("filename").GetString()}]";
                mediaUrl = document.GetProperty("id").GetString();
                break;
            default:
                messageBody = $"[{type}]";
                break;
        }

        // Store message in database and broadcast to operators
        using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        // Get or create conversation
        var conversationId = await GetOrCreateConversation(conn, from, 1); // TODO: Get FirmId from context

        // Insert message
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO WhatsApp_Messages (ConversationId, WhatsAppMessageId, Direction, MessageType, Body, MediaUrl, CreatedAt)
            VALUES (@conversationId, @messageId, 'In', @type, @body, @mediaUrl, GETUTCDATE())";
        cmd.Parameters.AddWithValue("@conversationId", conversationId);
        cmd.Parameters.AddWithValue("@messageId", messageId ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@type", type ?? "text");
        cmd.Parameters.AddWithValue("@body", messageBody ?? "");
        cmd.Parameters.AddWithValue("@mediaUrl", mediaUrl ?? (object)DBNull.Value);
        await cmd.ExecuteNonQueryAsync();

        // Broadcast to all connected operators
        await hubContext.Clients.All.SendAsync("CustomerMessageReceived", from, messageBody, type, conversationId);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error processing incoming message: {ex.Message}");
    }
}

static async Task ProcessMessageStatus(JsonElement status, IHubContext<MessageHub> hubContext, IConfiguration config)
{
    try
    {
        var messageId = status.GetProperty("id").GetString();
        var statusValue = status.GetProperty("status").GetString();
        var timestamp = status.GetProperty("timestamp").GetString();

        // Update message status in database
        using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE WhatsApp_Messages 
            SET IsRead = CASE WHEN @status = 'read' THEN 1 ELSE IsRead END
            WHERE WhatsAppMessageId = @messageId";
        cmd.Parameters.AddWithValue("@messageId", messageId);
        cmd.Parameters.AddWithValue("@status", statusValue);
        await cmd.ExecuteNonQueryAsync();

        // Broadcast status update
        await hubContext.Clients.All.SendAsync("MessageStatusUpdated", messageId, statusValue);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error processing message status: {ex.Message}");
    }
}

static async Task<int> GetOrCreateConversation(SqlConnection conn, string customerNumber, int firmId)
{
    // Try to get existing conversation
    using var selectCmd = conn.CreateCommand();
    selectCmd.CommandText = "SELECT Id FROM WhatsApp_Conversations WHERE CustomerNumber = @number AND FirmId = @firmId";
    selectCmd.Parameters.AddWithValue("@number", customerNumber);
    selectCmd.Parameters.AddWithValue("@firmId", firmId);
    
    var existingId = await selectCmd.ExecuteScalarAsync();
    if (existingId != null)
    {
        // Update last message time
        using var updateCmd = conn.CreateCommand();
        updateCmd.CommandText = "UPDATE WhatsApp_Conversations SET LastMessageAt = GETUTCDATE() WHERE Id = @id";
        updateCmd.Parameters.AddWithValue("@id", existingId);
        await updateCmd.ExecuteNonQueryAsync();
        
        return (int)existingId;
    }

    // Create new conversation
    using var insertCmd = conn.CreateCommand();
    insertCmd.CommandText = @"
        INSERT INTO WhatsApp_Conversations (FirmId, CustomerNumber, Status, LastMessageAt, CreatedAt, UpdatedAt)
        OUTPUT INSERTED.Id
        VALUES (@firmId, @number, 'Open', GETUTCDATE(), GETUTCDATE(), GETUTCDATE())";
    insertCmd.Parameters.AddWithValue("@firmId", firmId);
    insertCmd.Parameters.AddWithValue("@number", customerNumber);
    
    return (int)await insertCmd.ExecuteScalarAsync();
}

static async Task IncrementFailedLogin(SqlConnection conn, int operatorId)
{
    using var cmd = conn.CreateCommand();
    cmd.CommandText = @"
        UPDATE WhatsApp_Operators 
        SET FailedCount = FailedCount + 1,
            LockoutEnd = CASE 
                WHEN FailedCount + 1 >= 5 THEN DATEADD(MINUTE, 15, GETUTCDATE())
                ELSE LockoutEnd 
            END
        WHERE Id = @id";
    cmd.Parameters.AddWithValue("@id", operatorId);
    await cmd.ExecuteNonQueryAsync();
}

static async Task ResetFailedLogin(SqlConnection conn, int operatorId)
{
    using var cmd = conn.CreateCommand();
    cmd.CommandText = "UPDATE WhatsApp_Operators SET FailedCount = 0, LockoutEnd = NULL WHERE Id = @id";
    cmd.Parameters.AddWithValue("@id", operatorId);
    await cmd.ExecuteNonQueryAsync();
}

static async Task LogAuditEvent(SqlConnection conn, int? operatorId, string action, string details, string ipAddress)
{
    using var cmd = conn.CreateCommand();
    cmd.CommandText = @"
        INSERT INTO WhatsApp_AuditLogs (FirmId, OperatorId, Action, Details, IpAddress, CreatedAt)
        VALUES (1, @operatorId, @action, @details, @ipAddress, GETUTCDATE())";
    cmd.Parameters.AddWithValue("@operatorId", operatorId ?? (object)DBNull.Value);
    cmd.Parameters.AddWithValue("@action", action);
    cmd.Parameters.AddWithValue("@details", details ?? (object)DBNull.Value);
    cmd.Parameters.AddWithValue("@ipAddress", ipAddress ?? (object)DBNull.Value);
    await cmd.ExecuteNonQueryAsync();
}

// ===== DATA MODELS =====

public record LoginRequest(string UserName, string Password);
public record LoginResponse
{
    public string Token { get; init; }
    public int OperatorId { get; init; }
    public string FullName { get; init; }
    public string Role { get; init; }
    public int FirmId { get; init; }
}

public record PasswordResetStartRequest(string UserName);
public record PasswordResetConfirmRequest(string Token, string NewPassword);
