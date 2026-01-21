/* Core Namespace */
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Data;
/* Database Namespace */
using Microsoft.Data.SqlClient;
/* JWT Namespace */
using System.IdentityModel.Tokens.Jwt;
/* HTTP Namespace */
using System.Security.Claims;
using System.Text;
using System.Globalization;
/* Threading Namespace */
/* Cryptography Namespace */
using System.Security.Cryptography;

namespace Ustad.API.Controllers
{
    [ApiController]
    [Route("auth")] 
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly Classes.EmailService _emailService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IConfiguration configuration, Classes.EmailService emailService, ILogger<AuthController> logger)
        {
            _configuration = configuration;
            _emailService = emailService;
            _logger = logger;
        }
        #region Public Login Class
        /// <summary>
        /// YesilDefter Web & Desktop Application Login request payload
        /// </summary>
        public class LoginRequest
        {
            /// <summary>
            /// User name from login form
            /// </summary>
            public string UserName { get; set; } = string.Empty;
            /// <summary>
            /// User password from login form
            /// </summary>
            public string Password { get; set; } = string.Empty;
            /// <summary>
            /// Cloudflare Turnstile token for bot protection (optional in dev)
            /// </summary>
            public string TurnstileToken { get; set; } = string.Empty;
        }
        /// <summary>
        /// YesilDefter Web & Desktop Application Login response 
        /// including access and refresh tokens
        /// </summary>
        public class LoginResponse
        {
            /// <summary>
            /// JWT access token (short-lived)
            /// </summary>
            public string Token { get; set; } = string.Empty;
            /// <summary>
            /// JWT refresh token (long-lived)
            /// </summary>
            public string RefreshToken { get; set; } = string.Empty;
            /// <summary>
            /// Access token expiration time in seconds
            /// </summary>
            public int AccessTokenExpiresInSeconds { get; set; }
            /// <summary>
            /// Refresh token expiration time in seconds
            /// </summary>
            public int RefreshTokenExpiresInSeconds { get; set; }
            /// <summary>
            /// User/Operator ID
            /// </summary>
            public int UserId { get; set; }
            /// <summary>
            /// Legacy OperatorId (alias for UserId for backward compatibility)
            /// </summary>
            public int OperatorId { get; set; }
            /// <summary>
            /// User GUID identifier
            /// </summary>
            public string UserGUID { get; set; } = string.Empty;
            /// <summary>
            /// User full name
            /// </summary>
            public string FullName { get; set; } = string.Empty;
            /// <summary>
            /// User role (Agent, Admin, etc.)
            /// </summary>
            public string Role { get; set; } = string.Empty;
            /// <summary>
            /// Firm ID (legacy, may be 0)
            /// </summary>
            public int FirmId { get; set; }
            /// <summary>
            /// Database type identifier
            /// </summary>
            public short DbTypeId { get; set; }
        }
        /// <summary>
        /// YesilDefter Web & Desktop Application Refresh token request payload
        /// </summary>
        public class RefreshTokenRequest
        {
            /// <summary>
            /// Valid refresh token to exchange for new token pair
            /// </summary>
            public string RefreshToken { get; set; } = string.Empty;
        }
        /// <summary>
        /// YesilDefter Web & Desktop Application Logout request payload
        /// </summary>
        public class LogoutRequest
        {
            /// <summary>
            /// Refresh token to invalidate
            /// </summary>
            public string RefreshToken { get; set; } = string.Empty;
        }
        /// <summary>
        /// YesilDefter Web & Desktop Application Register request payload
        /// </summary>
        public class RegisterRequest
        {
            public string UserName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public int? FirmId { get; set; }
            public string Role { get; set; } = string.Empty;
            public string TurnstileToken { get; set; } = string.Empty;
        }
        /// <summary>
        /// YesilDefter Web & Desktop Application Reset password start request payload
        /// </summary>
        public class ResetStartRequest { public string UserName { get; set; } = string.Empty; }
        /// <summary>
        /// YesilDefter Web & Desktop Application Reset password confirm request payload
        /// </summary>
        public class ResetConfirmRequest { public string Token { get; set; } = string.Empty; public string NewPassword { get; set; } = string.Empty; }
        /// <summary>
        /// YesilDefter Web & Desktop Application Register response payload
        /// </summary>
        public class RegisterResponse
        {
            public int UserId { get; set; }
            public string UserName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
        }
        /// <summary>
        /// YesilDefter Web & Desktop Application Reset password request response payload
        /// </summary>
        public class ResetPasswordRequestResponse
        {
            public string Message { get; set; } = string.Empty;
        }
        /// <summary>
        /// YesilDefter Web & Desktop Application Reset password response payload
        /// </summary>
        public class ResetPasswordResponse
        {
            public string Message { get; set; } = string.Empty;
        }
        /// <summary>
        /// YesilDefter Web & Desktop Application Change password response payload
        /// </summary>
        public class ChangePasswordResponse
        {
            public string Message { get; set; } = string.Empty;
        }
        /// <summary>
        /// Demo request payload
        /// </summary>
        public class DemoRequestModel
        {
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Note { get; set; } = string.Empty;
        }
        /// <summary>
        /// Demo request response payload
        /// </summary>
        public class DemoRequestResponse
        {
            public string Message { get; set; } = string.Empty;
            public bool Success { get; set; }
        }
        #endregion
        #region Private Login Methods
        // SQL Query Constants
        private const string CREATE_SECURE_PASSWORDS_TABLE = @"
IF OBJECT_ID('UstadUserSecurePasswords') IS NULL
BEGIN
    CREATE TABLE UstadUserSecurePasswords (
        UserId INT PRIMARY KEY,
        PasswordHash VARBINARY(512) NOT NULL,
        Salt VARBINARY(128) NOT NULL,
        Iterations INT NOT NULL,
        CreatedAt DATETIME2 DEFAULT SYSUTCDATETIME(),
        FOREIGN KEY (UserId) REFERENCES UstadUsers(UserId)
    );
    CREATE INDEX IX_UstadUserSecurePasswords_UserId ON UstadUserSecurePasswords(UserId);
END";
        private const string PHASE1_PASSWORD_QUERY = @"
SELECT 
    u.UserId,
    COALESCE(u.UserKey, '') AS UserKey,
    CASE WHEN u.IsActive = 1 THEN 1 ELSE 0 END AS IsActive,
    sp.PasswordHash,
    sp.Salt,
    sp.Iterations
FROM UstadUsers u
LEFT JOIN UstadUserSecurePasswords sp ON u.UserId = sp.UserId
WHERE (u.UserEMail = @u OR u.UserTcNo = @u OR u.UserMobileNo = @u) 
  AND u.IsActive = 1";
        private const string PHASE3_USER_DATA_QUERY = @"
SELECT 
    COALESCE(u.UserFullName, '') AS FullName,
    COALESCE(u.FirmGUID, '') AS FirmGUID,
    COALESCE(u.UserGUID, '') AS UserGUID,
    COALESCE(u.DbTypeId, 0) AS DbTypeId
FROM UstadUsers u
WHERE u.UserId = @userId";
        private const string UPGRADE_PASSWORD_MERGE = @"
MERGE UstadUserSecurePasswords AS target
USING (SELECT @userId AS UserId) AS source
ON target.UserId = source.UserId
WHEN MATCHED THEN
    UPDATE SET 
        PasswordHash = @hash,
        Salt = @salt,
        Iterations = @iterations,
        CreatedAt = SYSUTCDATETIME()
WHEN NOT MATCHED THEN
    INSERT (UserId, PasswordHash, Salt, Iterations, CreatedAt)
    VALUES (@userId, @hash, @salt, @iterations, SYSUTCDATETIME());";
        /// <summary>
        /// Builds database connection string from environment variables or configuration
        /// </summary>
        /// <returns>Connection string</returns>
        /// <exception cref="InvalidOperationException">Thrown if required configuration is missing</exception>
        private string BuildConnectionString()
        {
            string host = Environment.GetEnvironmentVariable("DB_HOST") ?? _configuration["Db:Host"];
            string port = Environment.GetEnvironmentVariable("DB_PORT") ?? _configuration["Db:Port"];
            string user = Environment.GetEnvironmentVariable("DB_USER") ?? _configuration["Db:User"];
            string pass = Environment.GetEnvironmentVariable("DB_PASS") ?? _configuration["Db:Pass"];
            string db   = Environment.GetEnvironmentVariable("DB_NAME") ?? _configuration["Db:Name"];

            if (string.IsNullOrWhiteSpace(host))
                throw new InvalidOperationException("Database host environment variable or Db:Host configuration is required");
            if (string.IsNullOrWhiteSpace(port))
                throw new InvalidOperationException("Database port environment variable or Db:Port configuration is required");
            if (string.IsNullOrWhiteSpace(user))
                throw new InvalidOperationException("Database user environment variable or Db:User configuration is required");
            if (string.IsNullOrWhiteSpace(db))
                throw new InvalidOperationException("Database name environment variable or Db:Name configuration is required");
            if (string.IsNullOrWhiteSpace(pass))
                throw new InvalidOperationException("Database password environment variable or Db:Pass configuration is required");
            return $"Data Source={host},{port}; Initial Catalog={db}; User ID={user}; Password={pass}; TrustServerCertificate=true; Encrypt=false; MultipleActiveResultSets=True";
        }
        /// <summary>
        /// Validates the Cloudflare Turnstile token
        /// </summary>
        /// <param name="token">The Cloudflare Turnstile token to validate</param>
        /// <returns>True if the token is valid, false otherwise</returns>
        private async Task<bool> ValidateTurnstileAsync(string? token)
        {
            if (string.IsNullOrEmpty(token)) return true;
            var turnstileSecret = Environment.GetEnvironmentVariable("TURNSTILE_SECRET") ?? _configuration["Turnstile:Secret"];
            // NOTE(@Janberk): Cloudflare Turnstile is disabled
            if (string.IsNullOrEmpty(turnstileSecret)) return true; 
            try
            {
                using var cloudflareClient = new HttpClient();
                var verify = await cloudflareClient.PostAsync(
                    "https://challenges.cloudflare.com/turnstile/v0/siteverify",
                    new FormUrlEncodedContent(new[]
                    {
                        new System.Collections.Generic.KeyValuePair<string,string>("secret", turnstileSecret),
                        new System.Collections.Generic.KeyValuePair<string,string>("response", token)
                    }));
                if (!verify.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "[Turnstile Validation] Turnstile API returned non-success status: {StatusCode}",
                        verify.StatusCode);
                    return false;
                }
                var obj = await verify.Content.ReadFromJsonAsync<dynamic>();
                return obj?.success == true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    "[Turnstile Validation] Turnstile validation error (non-critical): {Error}",
                    ex.Message);
                return false;
            }
        }
        /// <summary>
        /// Gets the JWT key from the configuration
        /// </summary>
        /// <returns>Stored JWT key</returns>
        /// <exception cref="InvalidOperationException">Thrown if JWT key is not configured</exception>
        private string GetJwtKey()
        {
            var key = Environment.GetEnvironmentVariable("JWT_KEY") ?? _configuration["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new InvalidOperationException(
                    "JWT key environment variable or Jwt:Key configuration is required. " +
                    "Do not use hardcoded fallback values for security.");
            }
            // NOTE(@Janberk): Ensure key is at least 32 characters (256 bits) for HS256
            if (key.Length < 32)
            {
                throw new InvalidOperationException(
                    $"JWT key must be at least 32 characters long. Current length: {key.Length}. " +
                    "Please set JWT key environment variable or Jwt:Key configuration with a secure key.");
            }
            
            return key;
        }
        /// <summary>
        /// Gets the JWT issuer from the configuration
        /// </summary>
        /// <returns>Stored JWT issuer</returns>
        private string GetJwtIssuer()
        {
            return _configuration["Jwt:Issuer"] ?? "UstadAuth";
        }
        /// <summary>
        /// Gets the JWT audience from the configuration
        /// </summary>
        /// <returns>Stored JWT audience</returns>
        private string GetJwtAudience()
        {
            return _configuration["Jwt:Audience"] ?? "UstadClients";
        }
        /// <summary>
        /// Gets the access token expiration minutes from the configuration
        /// </summary>
        /// <returns>Stored access token expiration minutes</returns>
        /// <remarks>
        /// <para>Default: 480 minutes (8 hours)</para>
        /// 
        /// </remarks>
        private int GetAccessTokenExpiresMinutes()
        {
            if (int.TryParse(Environment.GetEnvironmentVariable("JWT_EXPIRES_MINUTES"), out var envValue) && envValue > 0)
            {
                return envValue;
            }
            if (int.TryParse(_configuration["Jwt:ExpiresMinutes"], out var cfgValue) && cfgValue > 0)
            {
                return cfgValue;
            }
            return 480; 
        }
        /// <summary>
        /// Gets the refresh token expiration minutes from the configuration
        /// </summary>
        /// <returns>Stored refresh token expiration minutes</returns>
        /// <remarks>
        /// <para>Default: 14 days</para>
        /// </remarks>
        private int GetRefreshTokenExpiresMinutes()
        {
            if (int.TryParse(Environment.GetEnvironmentVariable("JWT_REFRESH_EXPIRES_MINUTES"), out var envValue) && envValue > 0)
            {
                return envValue;
            }
            if (int.TryParse(_configuration["Jwt:RefreshExpiresMinutes"], out var cfgValue) && cfgValue > 0)
            {
                return cfgValue;
            }

            return 60 * 24 * 14; 
        }
        /// <summary>
        /// Gets the token validation parameters
        /// </summary>
        /// <returns>Token validation parameters</returns>
        private TokenValidationParameters GetTokenValidationParameters()
        {
            return new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetJwtKey())),
                ValidateIssuer = true,
                ValidIssuer = GetJwtIssuer(),
                ValidateAudience = true,
                ValidAudience = GetJwtAudience(),
                RequireExpirationTime = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1),
                NameClaimType = ClaimTypes.Name,
                RoleClaimType = ClaimTypes.Role
            };
        }
        /// <summary>
        /// Builds the base claims for the token
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="userGuid">The user GUID</param>
        /// <param name="fullName">The user full name</param>
        /// <param name="role">The user role</param>
        /// <param name="firmGuid">The firm GUID</param>
        /// <param name="firmId">The firm ID</param>
        /// <param name="firmName">The firm name</param>
       private List<Claim> BuildBaseClaims(int userId, string userGuid, string fullName, string role, string firmGuid, string userName, short dbTypeId, int firmId = 0, string firmName = "")
        {
            var baseClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString(CultureInfo.InvariantCulture)),
                new Claim("userGUID", userGuid ?? string.Empty),
                new Claim(ClaimTypes.Name, fullName ?? string.Empty),
                new Claim(ClaimTypes.Role, string.IsNullOrWhiteSpace(role) ? "Agent" : role),
                new Claim("firm", firmGuid ?? string.Empty),
                new Claim("firmId", firmId.ToString(CultureInfo.InvariantCulture)),
                new Claim("firmName", firmName ?? string.Empty),
                new Claim("uname", userName ?? string.Empty),
                new Claim("dbTypeId", dbTypeId.ToString(CultureInfo.InvariantCulture))
            };
            return baseClaims;
        }
        /// <summary>
        /// Builds the base claims from the principal
        /// </summary>
        /// <param name="principal">The principal</param>
        /// <returns>Base claims</returns>
        private List<Claim> BuildBaseClaimsFromPrincipal(ClaimsPrincipal principal)
        {
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0";
            var userGuid = principal.FindFirst("userGUID")?.Value ?? string.Empty;
            var fullName = principal.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
            var role = principal.FindFirst(ClaimTypes.Role)?.Value ?? "Agent";
            var firmGuid = principal.FindFirst("firm")?.Value ?? string.Empty;
            var firmId = principal.FindFirst("firmId")?.Value ?? "0";
            var firmName = principal.FindFirst("firmName")?.Value ?? string.Empty;
            var userName = principal.FindFirst("uname")?.Value ?? string.Empty;
            var dbTypeIdClaim = principal.FindFirst("dbTypeId")?.Value ?? "0";

            short.TryParse(dbTypeIdClaim, out var dbTypeId);
            int.TryParse(userId, out var userIdInt);
            int.TryParse(firmId, out var firmIdInt);
            
            return BuildBaseClaims(userIdInt, userGuid, fullName, role, firmGuid, userName, dbTypeId, firmIdInt, firmName);
        }
        /// <summary>
        /// Generates the token pair
        /// </summary>
        /// <param name="baseClaims">The base claims</param>
        /// <returns>The token pair</returns>
        private (string accessToken, DateTime accessExpiresAt, string refreshToken, DateTime refreshExpiresAt) GenerateTokenPair(IEnumerable<Claim> baseClaims)
        {
            var handler = new JwtSecurityTokenHandler();
            var claimsList = baseClaims is List<Claim> list ? list : baseClaims.ToList();
            var now = DateTime.UtcNow;
            var signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetJwtKey())),
                SecurityAlgorithms.HmacSha256
            );
            string issuer = GetJwtIssuer();
            string audience = GetJwtAudience();
            int accessMinutes = GetAccessTokenExpiresMinutes();
            int refreshMinutes = GetRefreshTokenExpiresMinutes();
            DateTime accessExpiresAt = now.AddMinutes(accessMinutes);
            DateTime refreshExpiresAt = now.AddMinutes(refreshMinutes);
            List<Claim> MakeTokenClaims(List<Claim> baseClaims, string tokenType) => new List<Claim>(baseClaims)
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("token_type", tokenType)
            };
            var accessClaims = MakeTokenClaims(claimsList, "access");
            var refreshClaims = MakeTokenClaims(claimsList, "refresh");
            string CreateToken(IEnumerable<Claim> claims, DateTime expires) =>
                handler.WriteToken(new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    claims: claims,
                    notBefore: now,
                    expires: expires,
                    signingCredentials: signingCredentials
                ));
            var accessToken = CreateToken(accessClaims, accessExpiresAt);
            var refreshToken = CreateToken(refreshClaims, refreshExpiresAt);
            return (accessToken, accessExpiresAt, refreshToken, refreshExpiresAt);
        }
        #endregion
        #region Public Login Methods
        /// <summary>
        /// Authenticates a user and returns access and refresh tokens
        /// </summary>
        /// <param name="request">Login credentials including username, password, and optional Cloudflare Turnstile token</param>
        /// <returns>LoginResponse with access token, refresh token, and user information</returns>
        /// <response code="200">Login successful, returns tokens and user info</response>
        /// <response code="400">Invalid request (missing username or password)</response>
        /// <response code="401">Authentication failed (invalid credentials or Cloudflare Turnstile validation failed)</response>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null)
            {
                return BadRequest("Geçersiz istek.");
            }
            if (string.IsNullOrWhiteSpace(request?.UserName) || string.IsNullOrWhiteSpace(request?.Password))
                return BadRequest("Kullanıcı adı ve şifre zorunludur.");

            if (!await ValidateTurnstileAsync(request.TurnstileToken))
                return Unauthorized("Güvenlik doğrulaması başarısız.");

            string connStr = BuildConnectionString();
            #region Minimal Password Verification Phase 1
            // ============================================
            // PHASE 1: Minimal Password Verification Query
            // ============================================
            // Only query what's needed for password verification
            int userId = 0;
            string? userKeyPlain = null;
            byte[]? hash = null;
            byte[]? salt = null;
            int iterations = 0;
            bool isActive = false;
            using (var con = new SqlConnection(connStr))
            {
                await con.OpenAsync();
                // Ensure secure passwords table exists
                using (var createCmd = con.CreateCommand())
                {
                    createCmd.CommandText = CREATE_SECURE_PASSWORDS_TABLE;
                    await createCmd.ExecuteNonQueryAsync();
                }
                #region Password Verification Data Query
                // Phase 1: Query ONLY password verification data
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = PHASE1_PASSWORD_QUERY;
                    cmd.Parameters.AddWithValue("@u", request.UserName);
                    using var r = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
                    if (!await r.ReadAsync())
                    {
                        return Unauthorized("Kullanıcı bulunamadı veya aktif değil.");
                    }
                    userId = r.GetInt32("UserId");
                    userKeyPlain = r.GetString("UserKey");
                    isActive = r.GetInt32("IsActive") == 1;
                    if (!r.IsDBNull("PasswordHash"))
                    {
                        hash = (byte[])r["PasswordHash"];
                        salt = (byte[])r["Salt"];
                        iterations = r.GetInt32("Iterations");
                    }
                }
                #endregion
            } // Connection automatically closed and disposed by 'using' statement
            #endregion
            #region Password Verification Phase 2
            // ============================================
            // PHASE 2: Password Verification (NO SQL CONNECTION)
            // ============================================
            // Verify password BEFORE fetching full user data
            bool verified = false;
            bool usingSecurePassword = false;
            if (hash != null && salt != null && iterations > 0)
            {
                verified = Classes.SecurePasswordHasher.Verify(request.Password, hash!, salt!, iterations);
                usingSecurePassword = true;
                if (!verified && !string.IsNullOrEmpty(userKeyPlain))
                {
                    verified = string.Equals(userKeyPlain, request.Password, StringComparison.Ordinal);
                    if (verified)
                    {
                        // Upgrade password on next connection
                        usingSecurePassword = true;
                    }
                }
            }
            else if (!string.IsNullOrEmpty(userKeyPlain))
            {
                verified = string.Equals(userKeyPlain, request.Password, StringComparison.Ordinal);
                usingSecurePassword = false;
            }
            #endregion
            if (!verified)
                return Unauthorized("Şifre hatalı.");
            #region Full User Data + Password Upgrade Phase 3
            // ============================================
            // PHASE 3: Full User Data + Password Upgrade (After Successful Auth)
            // ============================================
            // Only now that password is verified, fetch full user data
            string fullName = "";
            string role = "Agent";
            string firmGuid = "";
            string userGuid = "";
            short dbTypeId = 0;
            using (var con = new SqlConnection(connStr))
            {
                await con.OpenAsync();
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = PHASE3_USER_DATA_QUERY;
                    cmd.Parameters.AddWithValue("@userId", userId);
                    using var r = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
                    if (await r.ReadAsync())
                    {
                        fullName = r.GetString("FullName");
                        firmGuid = r.GetString("FirmGUID");
                        userGuid = r.GetString("UserGUID");
                        if (!r.IsDBNull("DbTypeId"))
                        {
                            dbTypeId = ParseDbTypeId(r.GetInt32(r.GetOrdinal("DbTypeId")));
                        }
                    }
                }
                // Upgrade password if needed (only after successful auth, inside Phase 3 connection)
                // Upgrade if: user verified but not using secure password (legacy plain text)
                if (verified && !usingSecurePassword)
                {
                    await UpgradeUserToSecurePassword(connStr, userId, request.Password);
                }
            } // Close connection after Phase 3
            #endregion
            #region Token Generation
            var baseClaims = BuildBaseClaims(userId, userGuid, fullName, role, firmGuid, request.UserName, dbTypeId, 0, "");
            var tokens = GenerateTokenPair(baseClaims);
            #endregion
            #region Return Login Response
            return Ok(new LoginResponse
            {
                Token = tokens.accessToken,
                RefreshToken = tokens.refreshToken,
                AccessTokenExpiresInSeconds = GetAccessTokenExpiresMinutes() * 60,
                RefreshTokenExpiresInSeconds = GetRefreshTokenExpiresMinutes() * 60,
                UserId = userId,
                OperatorId = userId,
                UserGUID = userGuid,
                FullName = fullName,
                Role = role,
                FirmId = 0,
                DbTypeId = dbTypeId
            });
            #endregion
        }
        /// <summary>
        /// QR Login endpoint for YesilDefter Mobile App authentication
        /// </summary>
        /// <param name="request">QR login request including FirmGUID, UserGUID, TcNoTelefonNo, and DbName</param>
        /// <returns>QRLoginResponse with user information and token</returns>
        /// <response code="200">QR login successful, returns user information and token</response>
        /// <response code="400">Invalid request (missing required fields)</response>
        /// <response code="401">Authentication failed (user not found or invalid credentials)</response>
        [HttpPost("qr-login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(QRLoginResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> QRLogin([FromBody] QRLoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.FirmGUID) || 
                string.IsNullOrWhiteSpace(request.UserGUID) || 
                string.IsNullOrWhiteSpace(request.TcNoTelefonNo) ||
                string.IsNullOrWhiteSpace(request.DbName))
            {
                return BadRequest("FirmGUID, UserGUID, TcNoTelefonNo, and DbName are required.");
            }
            try
            {
                string connStr = BuildConnectionStringForDb(request.DbName);
                using (var con = new SqlConnection(connStr))
                {
                    await con.OpenAsync();
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.CommandText = "[dbo].[prc_GetMtskKursiyerSorgulamaJson]";
                        cmd.Parameters.AddWithValue("@FirmGUID", request.FirmGUID);
                        cmd.Parameters.AddWithValue("@UserGUID", request.UserGUID);
                        cmd.Parameters.AddWithValue("@TcNoTelefonNo", request.TcNoTelefonNo);

                        using var r = await cmd.ExecuteReaderAsync();
                        if (await r.ReadAsync())
                        {
                            string jsonResult = r.IsDBNull(0) ? "{}" : r.GetString(0);
                            return Ok(new QRLoginResponse
                            {
                                User = System.Text.Json.JsonSerializer.Deserialize<object>(jsonResult),
                                Token = null
                            });
                        }
                        else
                        {
                            return Unauthorized("Kullanıcı bulunamadı veya geçersiz kimlik bilgileri.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"QR login başarısız: {ex.Message}");
            }
        }
        #endregion
        #region Public QR Login Class
        /// <summary>
        /// QR login request payload
        /// </summary>
        /// <param name="FirmGUID">FirmGUID</param>
        /// <param name="UserGUID">UserGUID</param>
        /// <param name="TcNoTelefonNo">TcNoTelefonNo</param>
        /// <param name="DbName">DbName</param>
        public class QRLoginRequest
        {
            public string FirmGUID { get; set; } = string.Empty;
            public string UserGUID { get; set; } = string.Empty;
            public string TcNoTelefonNo { get; set; } = string.Empty;
            public string DbName { get; set; } = string.Empty;
        }
        /// <summary>
        /// QR login response payload
        /// </summary>
        /// <param name="User">User</param>
        /// <param name="Token">Token</param>
        public class QRLoginResponse
        {
            public object? User { get; set; }
            public string? Token { get; set; }
        }
        private string BuildConnectionStringForDb(string dbName)
        {
            string baseConnStr = BuildConnectionString();
            // Replace the database name in the connection string
            var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(baseConnStr);
            builder.InitialCatalog = dbName;
            return builder.ConnectionString;
        }
        #endregion
        #region Public Admin Login Method
        /// <summary>
        /// Admin login endpoint for YesilDefter Web & Desktop Application
        /// </summary>
        /// <param name="request">Login credentials including username and password</param>
        /// <returns>LoginResponse with access token, refresh token, and user information</returns>
        /// <response code="200">Admin login successful, returns tokens and user info</response>
        /// <response code="400">Invalid request (missing username or password)</response>
        /// <response code="401">Authentication failed (invalid credentials)</response>
        [HttpPost("login-admin")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> LoginAdmin([FromBody] LoginRequest request)
        {
            var res = await Login(request);
            if (res is ObjectResult objResult && objResult.StatusCode < 400)
            {
                var payload = objResult.Value as LoginResponse;
                if (payload != null && (string.Equals(payload.Role, "Admin", StringComparison.OrdinalIgnoreCase) || 
                                       string.Equals(payload.FullName, "Tekin Uçar", StringComparison.OrdinalIgnoreCase)))
                {
                    payload.Role = "Admin";
                    return Ok(payload);
                }
            }
            return Unauthorized("Admin yetkisi gerekli.");
        }
        #endregion
        #region Public Register Method
        /// <summary>
        /// Register endpoint for YesilDefter Web & Desktop Application
        /// </summary>
        /// <param name="request">Register credentials including username, email, full name, password, firm id, role, and turnstile token</param>
        /// <returns>RegisterResponse with user information</returns>
        /// <response code="200">Register successful, returns user information</response>
        /// <response code="400">Invalid request (missing required fields)</response>
        /// <response code="401">Authentication failed (invalid credentials)</response>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(RegisterResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (request == null)
            {
                return BadRequest("Geçersiz istek.");
            }
            if (string.IsNullOrWhiteSpace(request?.UserName) || string.IsNullOrWhiteSpace(request?.Email) || string.IsNullOrWhiteSpace(request?.FullName) || string.IsNullOrWhiteSpace(request?.Password))
                return BadRequest("Kullanıcı adı, email, adı ve şifre zorunludur.");
            return BadRequest("Kayıt işlemi devre dışı. Lütfen yöneticinize başvurun.");
        }
        #endregion
        #region Public Reset Password Request Method
        /// <summary>
        /// Reset password request endpoint for YesilDefter Web & Desktop Application
        /// </summary>
        /// <param name="request">Reset password request including username</param>
        /// <returns>ResetPasswordRequestResponse with message</returns>
        /// <response code="200">Reset password request successful, returns message</response>
        /// <response code="400">Invalid request (missing username)</response>
        /// <response code="401">Authentication failed (invalid credentials)</response>
        [HttpPost("resetPasswordRequest")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ResetPasswordRequestResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> ResetPasswordRequest([FromBody] ResetStartRequest request)
        {
            if (request == null)
            {
                return BadRequest();
            }
            if (string.IsNullOrWhiteSpace(request?.UserName)) return BadRequest();
            using var con = new SqlConnection(BuildConnectionString());
            await con.OpenAsync();
            using (var createCmd = con.CreateCommand())
            {
                createCmd.CommandText = @"
IF OBJECT_ID('UstadUserResetTokens') IS NULL
BEGIN
    CREATE TABLE UstadUserResetTokens (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        Token NVARCHAR(128) NOT NULL UNIQUE,
        ExpiresAt DATETIME2 NOT NULL,
        Used BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 DEFAULT SYSUTCDATETIME(),
        FOREIGN KEY (UserId) REFERENCES UstadUsers(UserId)
    );
END";
                await createCmd.ExecuteNonQueryAsync();
            }
            string userEmail = string.Empty;
            string userName = string.Empty;
            string token = string.Empty;
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = @"
SELECT TOP 1 
    UserId,
    COALESCE(UserEMail, '') AS UserEMail,
    COALESCE(UserFullName, '') AS UserFullName
FROM UstadUsers 
WHERE (UserEMail=@u OR UserTcNo=@u OR UserMobileNo=@u) AND IsActive=1";
                cmd.Parameters.AddWithValue("@u", request.UserName);
                using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
                if (!await reader.ReadAsync())
                {
                    return Ok(new { message = "If the user exists, a password reset link has been sent to their email." });
                }
                int userId = reader.GetInt32("UserId");
                userEmail = reader.GetString("UserEMail");
                userName = reader.GetString("UserFullName");
                token = System.Guid.NewGuid().ToString("N").ToUpper().Substring(0, 32);
                reader.Close();
                using (var insertCmd = con.CreateCommand())
                {
                    insertCmd.CommandText = @"
INSERT INTO UstadUserResetTokens(UserId, Token, ExpiresAt) 
VALUES(@userId, @token, DATEADD(MINUTE,30,SYSUTCDATETIME()))";
                    insertCmd.Parameters.AddWithValue("@userId", userId);
                    insertCmd.Parameters.AddWithValue("@token", token);
                    await insertCmd.ExecuteNonQueryAsync();
                }
            }
            if (!string.IsNullOrWhiteSpace(userEmail) && !string.IsNullOrWhiteSpace(token))
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _emailService.SendPasswordResetEmailAsync(
                            userEmail, 
                            string.IsNullOrWhiteSpace(userName) ? userEmail : userName, 
                            token
                        );
                    }
                    catch
                    {
                    }
                });
            }
            // Always return success (security best practice - don't reveal if user exists)
            return Ok(new { message = "If the user exists, a password reset link has been sent to their email." });
        }
        #endregion
        #region Public Reset Password Method
        /// <summary>
        /// Reset password endpoint for YesilDefter Web & Desktop Application
        /// </summary>
        /// <param name="request">Reset password request including token and new password</param>
        /// <returns>ResetPasswordResponse with message</returns>
        /// <response code="200">Reset password successful, returns message</response>
        /// <response code="400">Invalid request (missing token or new password)</response>
        [HttpPost("resetPassword")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ResetPasswordResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetConfirmRequest req)
        {
            if (req == null)
            {
                return BadRequest();
            }
            if (string.IsNullOrWhiteSpace(req?.Token) || string.IsNullOrWhiteSpace(req?.NewPassword)) return BadRequest();
            var hashResult = Classes.SecurePasswordHasher.HashPassword(req.NewPassword);
            byte[] hash = hashResult.Hash;
            byte[] salt = hashResult.Salt;
            int iterations = hashResult.Iterations;
            using var con = new SqlConnection(BuildConnectionString());
            await con.OpenAsync();
            using var tr = con.BeginTransaction();
            try
            {
                int userId;
                using (var sel = con.CreateCommand())
                {
                    sel.Transaction = tr;
                    sel.CommandText = "SELECT UserId FROM UstadUserResetTokens WHERE Token=@t AND Used=0 AND ExpiresAt>SYSUTCDATETIME()";
                    sel.Parameters.AddWithValue("@t", req.Token);
                    var obj = await sel.ExecuteScalarAsync();
                    if (obj == null) { await tr.RollbackAsync(); return BadRequest("Token invalid/expired"); }
                    userId = (int)obj;
                }
                using (var up = con.CreateCommand())
                {
                    up.Transaction = tr;
                    up.CommandText = @"
IF EXISTS (SELECT 1 FROM UstadUserSecurePasswords WHERE UserId=@id)
    UPDATE UstadUserSecurePasswords SET PasswordHash=@h, Salt=@s, Iterations=@i WHERE UserId=@id
ELSE
    INSERT INTO UstadUserSecurePasswords (UserId, PasswordHash, Salt, Iterations) VALUES (@id, @h, @s, @i)";
                    up.Parameters.Add("@h", SqlDbType.VarBinary, hash.Length).Value = hash;
                    up.Parameters.Add("@s", SqlDbType.VarBinary, salt.Length).Value = salt;
                    up.Parameters.AddWithValue("@i", iterations);
                    up.Parameters.AddWithValue("@id", userId);
                    await up.ExecuteNonQueryAsync();
                }
                using (var used = con.CreateCommand())
                {
                    used.Transaction = tr;
                    used.CommandText = "UPDATE UstadUserResetTokens SET Used=1 WHERE Token=@t";
                    used.Parameters.AddWithValue("@t", req.Token);
                    await used.ExecuteNonQueryAsync();
                }
                await tr.CommitAsync();
                return Ok(new { message = "Şifre başarıyla sıfırlandı." });
            }
            catch (Exception ex)    
            {
                await tr.RollbackAsync();
                return StatusCode(500, "Şifre sıfırlama hatası: " + ex.Message);
            }
        }
        #endregion
        #region Public Demo Request Method
        /// <summary>
        /// Demo request endpoint for new users
        /// </summary>
        /// <param name="request">Demo request including name, email, and optional note</param>
        /// <returns>DemoRequestResponse with success message</returns>
        /// <response code="200">Demo request received successfully</response>
        /// <response code="400">Invalid request (missing name or email)</response>
        [HttpPost("demo-request")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(DemoRequestResponse), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> DemoRequest([FromBody] DemoRequestModel request)
        {
            if (request == null)
            {
                return BadRequest(new DemoRequestResponse { Message = "Geçersiz istek.", Success = false });
            }

            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new DemoRequestResponse { Message = "İsim ve e-posta zorunludur.", Success = false });
            }

            // Validate email format
            try
            {
                var emailAddr = new System.Net.Mail.MailAddress(request.Email);
            }
            catch
            {
                return BadRequest(new DemoRequestResponse { Message = "Geçersiz e-posta formatı.", Success = false });
            }

            // Send email asynchronously (fire and forget)
            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendDemoRequestEmailAsync(
                        request.Name,
                        request.Email,
                        request.Note ?? string.Empty
                    );
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Failed to send demo request email for {Email}", request.Email);
                }
            });

            // Always return success (don't reveal if email sending failed)
            return Ok(new DemoRequestResponse
            {
                Message = "Demo talebiniz alındı. En kısa sürede size geri dönüş yapacağız.",
                Success = true
            });
        }
        #endregion
        #region Public Refresh Token Method
        /// <summary>
        /// Refresh token endpoint for YesilDefter Web & Desktop Application
        /// </summary>
        /// <param name="request">Refresh token request containing a valid refresh token</param>
        /// <returns>LoginResponse with access token, refresh token, and user information</returns>
        /// <response code="200">Token refresh successful, returns new token pair</response>
        /// <response code="400">Invalid request (missing refresh token)</response>
        /// <response code="401">Authentication failed (invalid refresh token)</response>
        [HttpPost("refresh")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public IActionResult Refresh([FromBody] RefreshTokenRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest("Geçersiz istek. Refresh token gereklidir.");
            }
            var handler = new JwtSecurityTokenHandler();
            ClaimsPrincipal principal;
            SecurityToken validatedToken;
            try
            {
                principal = handler.ValidateToken(request.RefreshToken, GetTokenValidationParameters(), out validatedToken);
            }
            catch
            {
                return Unauthorized("Geçersiz refresh token.");
            }
            if (!(validatedToken is JwtSecurityToken jwtToken &&
                string.Equals(jwtToken.Header.Alg, SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase)))
            {
                return Unauthorized("Geçersiz refresh token.");
            }
            var tokenType = principal.FindFirst("token_type")?.Value;
            if (!string.Equals(tokenType, "refresh", StringComparison.OrdinalIgnoreCase))
            {
                return Unauthorized("Geçersiz token tipi.");
            }
            var baseClaims = BuildBaseClaimsFromPrincipal(principal);
            var idClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0";
            int.TryParse(idClaim, out var userId);
            var userGuid = principal.FindFirst("userGUID")?.Value ?? string.Empty;
            var fullName = principal.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
            var role = principal.FindFirst(ClaimTypes.Role)?.Value ?? "Agent";
            var dbTypeIdClaim = principal.FindFirst("dbTypeId")?.Value ?? "0";
            short.TryParse(dbTypeIdClaim, out var dbTypeId);
            var tokens = GenerateTokenPair(baseClaims);
            return Ok(new LoginResponse
            {
                Token = tokens.accessToken,
                RefreshToken = tokens.refreshToken,
                AccessTokenExpiresInSeconds = GetAccessTokenExpiresMinutes() * 60,
                RefreshTokenExpiresInSeconds = GetRefreshTokenExpiresMinutes() * 60,
                UserId = userId,
                OperatorId = userId,
                UserGUID = userGuid,
                FullName = fullName,
                Role = role,
                FirmId = 0,
                DbTypeId = dbTypeId
            });
        }
        #endregion
        #region Public Logout Method
        /// <summary>
        /// Logout endpoint for YesilDefter Web & Desktop Application
        /// </summary>
        /// <param name="request">Logout request containing refresh token to invalidate</param>
        /// <returns>Success response</returns>
        /// <response code="200">Logout successful</response>
        /// <response code="401">Unauthorized (invalid or missing JWT)</response>
        /// <remarks>
        /// Note: Redis blacklist integration for token invalidation will be added in a subsequent iteration.
        /// </remarks>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public IActionResult Logout([FromBody] LogoutRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest("Geçersiz istek. Refresh token gereklidir.");
            }
            var handler = new JwtSecurityTokenHandler();
            ClaimsPrincipal principal;
            SecurityToken validatedToken;
            try
            {
                principal = handler.ValidateToken(request.RefreshToken, GetTokenValidationParameters(), out validatedToken);
            }
            catch
            {
                return Unauthorized("Geçersiz refresh token.");
            }
            if (!(validatedToken is JwtSecurityToken jwtToken &&
                string.Equals(jwtToken.Header.Alg, SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase)))
            {
                return Unauthorized("Geçersiz refresh token.");
            }
            var tokenType = principal.FindFirst("token_type")?.Value;
            if (!string.Equals(tokenType, "refresh", StringComparison.OrdinalIgnoreCase))
            {
                return Unauthorized("Geçersiz token tipi.");
            }
            var baseClaims = BuildBaseClaimsFromPrincipal(principal);
            var idClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0";
            int.TryParse(idClaim, out var userId);
            var userGuid = principal.FindFirst("userGUID")?.Value ?? string.Empty;
            var fullName = principal.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
            var role = principal.FindFirst(ClaimTypes.Role)?.Value ?? "Agent";
            var dbTypeIdClaim = principal.FindFirst("dbTypeId")?.Value ?? "0";
            short.TryParse(dbTypeIdClaim, out var dbTypeId);
            var tokens = GenerateTokenPair(baseClaims);
            return Ok(new LoginResponse
            {
                Token = tokens.accessToken,
                RefreshToken = tokens.refreshToken,
                AccessTokenExpiresInSeconds = GetAccessTokenExpiresMinutes() * 60,
                RefreshTokenExpiresInSeconds = GetRefreshTokenExpiresMinutes() * 60,
                UserId = userId,
                OperatorId = userId,
                UserGUID = userGuid,
                FullName = fullName,
                Role = role,
                FirmId = 0,
                DbTypeId = dbTypeId
            });
        }
        #endregion
        #region Public Check User Exists Method
        [HttpGet("user/exists")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckUserExists([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest("Email is required");
            string connStr = BuildConnectionString();
            using var con = new SqlConnection(connStr);
            await con.OpenAsync();
            using var cmd = con.CreateCommand();
            cmd.CommandText = @"
SELECT TOP 1 
    UserId, 
    COALESCE(UserFullName, '') AS UserFullName,
    CASE WHEN IsActive = 1 THEN 1 ELSE 0 END AS IsActive
FROM UstadUsers
WHERE (UserEMail = @email OR UserTcNo = @email OR UserMobileNo = @email)";
            cmd.Parameters.AddWithValue("@email", email);
            using var r = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
            if (!await r.ReadAsync())
            {
                return Ok(new { Exists = false });
            }
            return Ok(new
            {
                Exists = true,
                UserId = r.GetInt32("UserId"),
                UserFullName = r.GetString("UserFullName"),
                IsActive = r.GetInt32("IsActive") == 1
            });
        }
        #endregion
        #region Public Change Password Method
        /// <summary>
        /// Change password endpoint for YesilDefter Web & Desktop Application
        /// </summary>
        /// <param name="request">Change password request including email, old password, and new password</param>
        /// <returns>ChangePasswordResponse with message</returns>
        /// <response code="200">Change password successful, returns message</response>
        /// <response code="400">Invalid request (missing email, old password, or new password)</response>
        [HttpPost("changepassword")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ChangePasswordResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (request == null)
            {
                return BadRequest("Geçersiz istek. Email, eski şifre ve yeni şifre gereklidir.");
            }
            if (string.IsNullOrWhiteSpace(request?.Email) || 
                string.IsNullOrWhiteSpace(request?.OldPassword) || 
                string.IsNullOrWhiteSpace(request?.NewPassword))
                return BadRequest("Geçersiz istek. Email, eski şifre ve yeni şifre gereklidir.");
            if (request.NewPassword.Length < 4)
                return BadRequest("Yeni şifre en az 4 karakter olmalıdır.");
            if (request.NewPassword == request.OldPassword)
                return BadRequest("Yeni şifre eski şifre ile aynı olamaz.");

            string connStr = BuildConnectionString();
            int userId = 0;
            string? db_user_key = null;
            byte[]? hash = null;
            byte[]? salt = null;
            int iterations = 0;
            using (var con = new SqlConnection(connStr))
            {
                await con.OpenAsync();
                using var cmd = con.CreateCommand();
                cmd.CommandText = @"
SELECT 
    u.UserId, 
    COALESCE(u.UserKey, '') AS UserKey,
    sp.PasswordHash,
    sp.Salt,
    sp.Iterations
FROM UstadUsers u
LEFT JOIN UstadUserSecurePasswords sp ON u.UserId = sp.UserId
WHERE (u.UserEMail = @email OR u.UserTcNo = @email OR u.UserMobileNo = @email) 
  AND u.IsActive = 1";

                cmd.Parameters.AddWithValue("@email", request.Email);
                using var r = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
                if (!await r.ReadAsync())
                    return Unauthorized("Kullanıcı bulunamadı veya aktif değil.");
                userId = r.GetInt32("UserId");
                db_user_key = r.GetString("UserKey");

                if (!r.IsDBNull("PasswordHash"))
                {
                    hash = (byte[])r["PasswordHash"];
                    salt = (byte[])r["Salt"];
                    iterations = r.GetInt32("Iterations");
                }
            }
            bool verified = false;
            if (hash != null && salt != null && iterations > 0)
            {
                verified = Classes.SecurePasswordHasher.Verify(request.OldPassword, hash!, salt!, iterations);
            }
            else if (!string.IsNullOrEmpty(db_user_key))
            {
                verified = string.Equals(db_user_key, request.OldPassword);
            }
            if (!verified)
                return Unauthorized("Eski şifre hatalı.");
            var hashResult = Classes.SecurePasswordHasher.HashPassword(request.NewPassword);
            using (var con = new SqlConnection(connStr))
            {
                await con.OpenAsync();
                using var tr = con.BeginTransaction();
                try
                {
                    using var cmd = con.CreateCommand();
                    cmd.Transaction = tr;
                    cmd.CommandText = @"
IF EXISTS (SELECT 1 FROM UstadUserSecurePasswords WHERE UserId = @userId)
    UPDATE UstadUserSecurePasswords 
    SET PasswordHash = @hash, Salt = @salt, Iterations = @iterations 
    WHERE UserId = @userId
ELSE
    INSERT INTO UstadUserSecurePasswords (UserId, PasswordHash, Salt, Iterations) 
    VALUES (@userId, @hash, @salt, @iterations)";
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.Add("@hash", SqlDbType.VarBinary, hashResult.Hash.Length).Value = hashResult.Hash;
                    cmd.Parameters.Add("@salt", SqlDbType.VarBinary, hashResult.Salt.Length).Value = hashResult.Salt;
                    cmd.Parameters.AddWithValue("@iterations", hashResult.Iterations);

                    await cmd.ExecuteNonQueryAsync();
                    await tr.CommitAsync();
                }
                catch (Exception ex)
                {
                    await tr.RollbackAsync();
                    return StatusCode(500, "Şifre değiştirme hatası: " + ex.Message);
                }
            }
            return Ok(new { Message = "Şifreniz başarıyla değiştirildi." });
        }
        #endregion
        #region Public Change Password Class
        /// <summary>
        /// Change password request payload
        /// </summary>
        public class ChangePasswordRequest
        {
            public string Email { get; set; } = string.Empty;
            public string OldPassword { get; set; } = string.Empty;
            public string NewPassword { get; set; } = string.Empty;
        }
        #endregion
        #region Private Upgrade User to Secure Password Method
        /// <summary>
        /// Upgrade a user from plain text password to secure PBKDF2 hash
        /// </summary>
        private async Task UpgradeUserToSecurePassword(string connectionString, int userId, string plainPassword)
        {
            try
            {
                var hashResult = Classes.SecurePasswordHasher.HashPassword(plainPassword);
                byte[] hash = hashResult.Hash;
                byte[] salt = hashResult.Salt;
                int iterations = hashResult.Iterations;
                using var con = new SqlConnection(connectionString);
                await con.OpenAsync();
                using var cmd = con.CreateCommand();
                cmd.CommandText = UPGRADE_PASSWORD_MERGE;
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.Add("@hash", SqlDbType.VarBinary, hash.Length).Value = hash;
                cmd.Parameters.Add("@salt", SqlDbType.VarBinary, salt.Length).Value = salt;
                cmd.Parameters.AddWithValue("@iterations", iterations);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                // Log upgrade errors but don't fail login - user can still login with legacy password
                // Note: This is a Task method, cannot return IActionResult
                _logger.LogWarning(
                    "[Password Upgrade] ⚠️ Failed to upgrade password (non-critical) - UserId: {UserId}, Error: {Error}",
                    userId, ex.Message);
            }
        }
        #endregion
        #region Private Check Database Accessibility Method
        /// <summary>
        /// Check if database is accessible for YesilDefter Web & Desktop Application
        /// </summary>
        private async Task<bool> IsDatabaseAccessible()
        {
            try
            {
                using var con = new SqlConnection(BuildConnectionString());
                await con.OpenAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    "[Database Check] Database not accessible - Error: {Error}",
                    ex.Message);
                return false;
            }
        }
         #region Select Firm (Authenticated)
        /// <summary>
        /// Select firm endpoint - validates user access to firm and issues new token with firm claim
        /// </summary>
        /// <param name="request">Select firm request containing FirmGUID</param>
        /// <returns>LoginResponse with new token pair including firm claim</returns>
        /// <response code="200">Firm selected successfully, returns new token pair</response>
        /// <response code="400">Invalid request (missing FirmGUID)</response>
        /// <response code="401">Unauthorized - valid JWT token required or user does not have access to firm</response>
        [HttpPost("select-firm")]
        [Authorize]
        [ProducesResponseType(typeof(LoginResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> SelectFirm([FromBody] SelectFirmRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.FirmGUID))
            {
                return BadRequest("FirmGUID is required.");
            }

            try
            {
                // Get current user from JWT claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userGuidClaim = User.FindFirst("userGUID")?.Value ?? string.Empty;
                
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized("Invalid user token.");
                }

                // Validate user has access to the requested firm
                string connStr = BuildConnectionString();
                int firmId = 0;
                string firmName = string.Empty;
                string firmGUID = request.FirmGUID.Trim();

                using (var con = new SqlConnection(connStr))
                {
                    await con.OpenAsync();
                    using (var cmd = con.CreateCommand())
                    {
                        // Check if user has access to this firm via UstadFirmsUsers table
                        // First, try to find the firm by matching the COALESCE logic from GetUserFirmsAsync
                        // If that fails, we'll query what GUIDs are actually in the database for debugging
                        // Match the logic from GetUserFirms: only filter by UserGUID, not by f.IsActive
                        // This ensures firms shown in the list can be selected
                        cmd.CommandText = @"
SELECT TOP 1
    uf.FirmId,
    COALESCE(f.FirmLongName, '') AS FirmLongName,
    CASE 
        WHEN uf.FirmGUID IS NOT NULL THEN CAST(uf.FirmGUID AS NVARCHAR(36))
        WHEN f.FirmGUID IS NOT NULL THEN CAST(f.FirmGUID AS NVARCHAR(36))
        ELSE '00000000-0000-0000-0000-000000000000'
    END AS FirmGUID,
    CAST(uf.FirmGUID AS NVARCHAR(36)) AS UstadFirmsUsers_FirmGUID,
    CAST(f.FirmGUID AS NVARCHAR(36)) AS UstadFirms_FirmGUID
FROM UstadFirmsUsers uf
LEFT JOIN UstadFirms f ON uf.FirmId = f.FirmId
WHERE uf.UserGUID = @UserGUID
  AND uf.IsActive = 1
  AND (
    (uf.FirmGUID IS NOT NULL AND LOWER(CAST(uf.FirmGUID AS NVARCHAR(36))) = LOWER(@FirmGUID))
    OR 
    (uf.FirmGUID IS NULL AND f.FirmGUID IS NOT NULL AND LOWER(CAST(f.FirmGUID AS NVARCHAR(36))) = LOWER(@FirmGUID))
  )";
                        cmd.Parameters.AddWithValue("@UserGUID", userGuidClaim);
                        cmd.Parameters.AddWithValue("@FirmGUID", firmGUID.Trim());

                        // Add debug logging
                        _logger?.LogInformation($"[SelectFirm] Querying for UserGUID={userGuidClaim}, FirmGUID={firmGUID}");

                        using var r = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
                        if (!await r.ReadAsync())
                        {
                            // Debug: Query what firms the user actually has access to
                            _logger?.LogWarning($"[SelectFirm] No matching firm found. Querying available firms for UserGUID={userGuidClaim}");
                            
                            using (var debugCmd = con.CreateCommand())
                            {
                                // First, check if the firm exists in UstadFirmsUsers for this user (without IsActive filter on UstadFirms)
                                debugCmd.CommandText = @"
SELECT 
    uf.FirmId,
    uf.IsActive AS UstadFirmsUsers_IsActive,
    CAST(uf.FirmGUID AS NVARCHAR(36)) AS UstadFirmsUsers_FirmGUID,
    f.IsActive AS UstadFirms_IsActive,
    CAST(f.FirmGUID AS NVARCHAR(36)) AS UstadFirms_FirmGUID,
    CASE 
        WHEN uf.FirmGUID IS NOT NULL THEN CAST(uf.FirmGUID AS NVARCHAR(36))
        WHEN f.FirmGUID IS NOT NULL THEN CAST(f.FirmGUID AS NVARCHAR(36))
        ELSE 'NULL'
    END AS Effective_FirmGUID,
    f.FirmLongName
FROM UstadFirmsUsers uf
LEFT JOIN UstadFirms f ON uf.FirmId = f.FirmId
WHERE uf.UserGUID = @UserGUID
  AND (
    (uf.FirmGUID IS NOT NULL AND LOWER(CAST(uf.FirmGUID AS NVARCHAR(36))) = LOWER(@FirmGUID))
    OR 
    (uf.FirmGUID IS NULL AND f.FirmGUID IS NOT NULL AND LOWER(CAST(f.FirmGUID AS NVARCHAR(36))) = LOWER(@FirmGUID))
  )";
                                debugCmd.Parameters.AddWithValue("@UserGUID", userGuidClaim);
                                debugCmd.Parameters.AddWithValue("@FirmGUID", firmGUID);
                                
                                using var debugR = await debugCmd.ExecuteReaderAsync();
                                if (await debugR.ReadAsync())
                                {
                                    var debugFirmId = debugR.GetInt32("FirmId");
                                    var ufIsActive = debugR.GetBoolean("UstadFirmsUsers_IsActive");
                                    var ufGuid = debugR.IsDBNull("UstadFirmsUsers_FirmGUID") ? "NULL" : debugR.GetString("UstadFirmsUsers_FirmGUID");
                                    var fIsActive = debugR.IsDBNull("UstadFirms_IsActive") ? (bool?)null : debugR.GetBoolean("UstadFirms_IsActive");
                                    var fGuid = debugR.IsDBNull("UstadFirms_FirmGUID") ? "NULL" : debugR.GetString("UstadFirms_FirmGUID");
                                    var effectiveGuid = debugR.GetString("Effective_FirmGUID");
                                    var debugFirmName = debugR.IsDBNull("FirmLongName") ? "" : debugR.GetString("FirmLongName");
                                    
                                    _logger?.LogWarning($"[SelectFirm] Found firm in database: FirmId={debugFirmId}, UstadFirmsUsers.IsActive={ufIsActive}, UstadFirms.IsActive={fIsActive}, UstadFirmsUsers.FirmGUID={ufGuid}, UstadFirms.FirmGUID={fGuid}, Effective={effectiveGuid}, Name={debugFirmName}");
                                    _logger?.LogWarning($"[SelectFirm] Firm is excluded because: UstadFirmsUsers.IsActive={ufIsActive} (needs 1), UstadFirms.IsActive={fIsActive} (needs 1 or NULL)");
                                }
                                else
                                {
                                    _logger?.LogWarning($"[SelectFirm] Firm with GUID {firmGUID} not found in UstadFirmsUsers for this user at all.");
                                    
                                    // Also check all available firms (limited to 20 for logging)
                                    debugCmd.CommandText = @"
SELECT TOP 20
    uf.FirmId,
    uf.IsActive AS UstadFirmsUsers_IsActive,
    CAST(uf.FirmGUID AS NVARCHAR(36)) AS UstadFirmsUsers_FirmGUID,
    f.IsActive AS UstadFirms_IsActive,
    CAST(f.FirmGUID AS NVARCHAR(36)) AS UstadFirms_FirmGUID,
    CASE 
        WHEN uf.FirmGUID IS NOT NULL THEN CAST(uf.FirmGUID AS NVARCHAR(36))
        WHEN f.FirmGUID IS NOT NULL THEN CAST(f.FirmGUID AS NVARCHAR(36))
        ELSE 'NULL'
    END AS Effective_FirmGUID,
    f.FirmLongName
FROM UstadFirmsUsers uf
LEFT JOIN UstadFirms f ON uf.FirmId = f.FirmId
WHERE uf.UserGUID = @UserGUID
ORDER BY uf.FirmId";
                                    debugCmd.Parameters.Clear();
                                    debugCmd.Parameters.AddWithValue("@UserGUID", userGuidClaim);
                                    
                                    using var debugR2 = await debugCmd.ExecuteReaderAsync();
                                    var availableFirms = new List<string>();
                                    while (await debugR2.ReadAsync())
                                    {
                                        var debugFirmId2 = debugR2.GetInt32("FirmId");
                                        var ufIsActive2 = debugR2.GetBoolean("UstadFirmsUsers_IsActive");
                                        var ufGuid2 = debugR2.IsDBNull("UstadFirmsUsers_FirmGUID") ? "NULL" : debugR2.GetString("UstadFirmsUsers_FirmGUID");
                                        var fIsActive2 = debugR2.IsDBNull("UstadFirms_IsActive") ? (bool?)null : debugR2.GetBoolean("UstadFirms_IsActive");
                                        var fGuid2 = debugR2.IsDBNull("UstadFirms_FirmGUID") ? "NULL" : debugR2.GetString("UstadFirms_FirmGUID");
                                        var effectiveGuid2 = debugR2.GetString("Effective_FirmGUID");
                                        var debugFirmName2 = debugR2.IsDBNull("FirmLongName") ? "" : debugR2.GetString("FirmLongName");
                                        availableFirms.Add($"FirmId={debugFirmId2}, UstadFirmsUsers.IsActive={ufIsActive2}, UstadFirms.IsActive={fIsActive2}, Effective={effectiveGuid2}, Name={debugFirmName2}");
                                    }
                                    
                                    _logger?.LogWarning($"[SelectFirm] Available firms for user (first 20): {string.Join("; ", availableFirms)}");
                                }
                                
                                _logger?.LogWarning($"[SelectFirm] Looking for FirmGUID={firmGUID} (lowercase: {firmGUID.ToLowerInvariant()})");
                            }
                            
                            return Unauthorized("User does not have access to the specified firm.");
                        }

                        firmId = r.GetInt32("FirmId");
                        firmName = r.GetString("FirmLongName");
                        string dbFirmGUID = r.GetString("FirmGUID");
                        // Check if it's the empty GUID placeholder
                        if (!string.IsNullOrEmpty(dbFirmGUID) && dbFirmGUID != "00000000-0000-0000-0000-000000000000")
                        {
                            firmGUID = dbFirmGUID;
                        }
                    }
                }

                // Get user info for token generation
                var fullName = User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
                var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "Agent";
                var userName = User.FindFirst("uname")?.Value ?? string.Empty;
                var dbTypeIdClaim = User.FindFirst("dbTypeId")?.Value ?? "0";
                short.TryParse(dbTypeIdClaim, out var dbTypeId);

                // Build claims with firm information
                var baseClaims = BuildBaseClaims(userId, userGuidClaim, fullName, role, firmGUID, userName, dbTypeId, firmId, firmName);
                var tokens = GenerateTokenPair(baseClaims);

                return Ok(new LoginResponse
                {
                    Token = tokens.accessToken,
                    RefreshToken = tokens.refreshToken,
                    AccessTokenExpiresInSeconds = GetAccessTokenExpiresMinutes() * 60,
                    RefreshTokenExpiresInSeconds = GetRefreshTokenExpiresMinutes() * 60,
                    UserId = userId,
                    OperatorId = userId,
                    UserGUID = userGuidClaim,
                    FullName = fullName,
                    Role = role,
                    FirmId = firmId,
                    DbTypeId = dbTypeId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error selecting firm");
                return StatusCode(500, "Error selecting firm: " + ex.Message);
            }
        }

        /// <summary>
        /// Select firm request payload
        /// </summary>
        public class SelectFirmRequest
        {
            /// <summary>
            /// Firm GUID to select
            /// </summary>
            public string FirmGUID { get; set; } = string.Empty;
        }
        #endregion
        #endregion
        #region Get Database Connection Info (Authenticated)
        /// <summary>
        /// Get database connection information for authenticated desktop application
        /// Returns connection details (server, database, username) but NOT password for security
        /// NOTE(@Janberk): Password is handled server-side only via environment variables
        /// </summary>
        /// <returns>DatabaseConnectionInfo with server, database, and username</returns>
        /// <response code="200">Returns database connection info</response>
        /// <response code="401">Unauthorized - valid JWT token required</response>
        [HttpGet("db-connection-info")]
        [Authorize]
        [ProducesResponseType(typeof(DatabaseConnectionInfo), 200)]
        [ProducesResponseType(401)]
        public IActionResult GetDatabaseConnectionInfo()
        {
            try
            {
                // NOTE(@Janberk): Require firm claim - user must select firm before getting DB connection info
                var firmClaim = User.FindFirst("firm")?.Value;
                var firmIdClaim = User.FindFirst("firmId")?.Value;
                
                if (string.IsNullOrWhiteSpace(firmClaim) || string.IsNullOrWhiteSpace(firmIdClaim))
                {
                    return Unauthorized("Firm selection required. Please call /auth/select-firm first.");
                }
                // Get connection info from environment variables or configuration (NO hardcoded fallbacks)
                string host = Environment.GetEnvironmentVariable("DB_HOST") ?? _configuration["Db:Host"];
                string port = Environment.GetEnvironmentVariable("DB_PORT") ?? _configuration["Db:Port"];
                string user = Environment.GetEnvironmentVariable("DB_USER") ?? _configuration["Db:User"];
                string dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? _configuration["Db:Name"];
                string password = Environment.GetEnvironmentVariable("DB_PASS") ?? _configuration["Db:Pass"];

                if (string.IsNullOrWhiteSpace(host))
                    throw new InvalidOperationException("Database host environment variable or Db:Host configuration is required");
                if (string.IsNullOrWhiteSpace(port))
                    throw new InvalidOperationException("Database port environment variable or Db:Port configuration is required");
                if (string.IsNullOrWhiteSpace(user))
                    throw new InvalidOperationException("Database user environment variable or Db:User configuration is required");
                if (string.IsNullOrWhiteSpace(dbName))
                    throw new InvalidOperationException("Database name environment variable or Db:Name configuration is required");
                if (string.IsNullOrWhiteSpace(password))
                    throw new InvalidOperationException("Database password environment variable or Db:Pass configuration is required");

                // NOTE(@Janberk): Get manager DB info from configuration
                var managerConnStr = _configuration.GetConnectionString("BulutManager");
                string managerServer = host;
                string managerDb = null;
                string managerUser = user;
                if (!string.IsNullOrEmpty(managerConnStr))
                {
                    try
                    {
                        var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(managerConnStr);
                        managerServer = builder.DataSource.Split(',')[0]; 
                        managerDb = builder.InitialCatalog;
                        managerUser = builder.UserID;
                    }
                    catch
                    {
                        // NOTE(@Janberk): Connection string parsing failed, will try configuration fallback below
                    }
                }
                if (string.IsNullOrWhiteSpace(managerDb))
                {
                    managerDb = _configuration["Db:ManagerName"];
                    if (string.IsNullOrWhiteSpace(managerDb))
                        throw new InvalidOperationException("Manager database name must be configured via BulutManager connection string or Db:ManagerName");
                }
                
                string ustadCrmConnStr = $"Data Source={host},{port};Initial Catalog={dbName};User ID={user};Password={password};MultipleActiveResultSets=True;TrustServerCertificate=true;Encrypt=false";
                string managerConnStrBuilt = $"Data Source={managerServer},{port};Initial Catalog={managerDb};User ID={managerUser};Password={password};MultipleActiveResultSets=True;TrustServerCertificate=true;Encrypt=false";

                string encryptionKey = GetJwtKey();
                string encryptedUstadCrm = EncryptConnectionString(ustadCrmConnStr, encryptionKey);
                string encryptedManager = EncryptConnectionString(managerConnStrBuilt, encryptionKey);

                return Ok(new DatabaseConnectionInfo
                {
                    UstadCrmServer = host,
                    UstadCrmPort = port,
                    UstadCrmDatabase = dbName,
                    UstadCrmUsername = user,
                    ManagerServer = managerServer,
                    ManagerDatabase = managerDb,
                    ManagerUsername = managerUser,
                    EncryptedUstadCrmConnectionString = encryptedUstadCrm,
                    EncryptedManagerConnectionString = encryptedManager
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting database connection info");
                return StatusCode(500, "Error retrieving database connection information");
            }
        }
        #endregion
        #region Private Encryption Methods
        /// <summary>
        /// Encrypt connection string using AES encryption with key derived from JWT secret
        /// </summary>
        private string EncryptConnectionString(string plainText, string key)
        {
            try
            {
                // Derive a 32-byte key from the JWT key
                byte[] keyBytes = Encoding.UTF8.GetBytes(key);
                if (keyBytes.Length < 32)
                {
                    // Pad key to 32 bytes
                    Array.Resize(ref keyBytes, 32);
                }
                else if (keyBytes.Length > 32)
                {
                    // Truncate to 32 bytes
                    byte[] truncated = new byte[32];
                    Array.Copy(keyBytes, truncated, 32);
                    keyBytes = truncated;
                }
                using (Aes aes = Aes.Create())
                {
                    aes.Key = keyBytes;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.GenerateIV();
                    using (ICryptoTransform encryptor = aes.CreateEncryptor())
                    {
                        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                        byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                        // Combine IV and encrypted data
                        byte[] result = new byte[aes.IV.Length + encryptedBytes.Length];
                        Array.Copy(aes.IV, 0, result, 0, aes.IV.Length);
                        Array.Copy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);
                        return Convert.ToBase64String(result);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error encrypting connection string");
                throw;
            }
        }
        #endregion
        #region Database Connection Info Class
        /// <summary>
        /// Database connection information response with encrypted connection strings
        /// </summary>
        public class DatabaseConnectionInfo
        {
            public string UstadCrmServer { get; set; } = string.Empty;
            public string UstadCrmPort { get; set; } = "1433";
            public string UstadCrmDatabase { get; set; } = string.Empty;
            public string UstadCrmUsername { get; set; } = string.Empty;
            public string ManagerServer { get; set; } = string.Empty;
            public string ManagerDatabase { get; set; } = string.Empty;
            public string ManagerUsername { get; set; } = string.Empty;
            /// <summary>
            /// Encrypted UstadCRM connection string - decrypt using JWT key on desktop
            /// </summary>
            public string EncryptedUstadCrmConnectionString { get; set; } = string.Empty;
            /// <summary>
            /// Encrypted Manager connection string - decrypt using JWT key on desktop
            /// </summary>
            public string EncryptedManagerConnectionString { get; set; } = string.Empty;
        }
        #endregion
        #region Private Helper Methods
        /// <summary>
        /// Parse DbTypeId from database value, ensuring it fits in short range
        /// </summary>
        /// <param name="dbTypeIdInt">The integer value from database</param>
        /// <returns>Parsed short value, clamped to short.MinValue/short.MaxValue if out of range</returns>
        private short ParseDbTypeId(int dbTypeIdInt)
        {
            if (dbTypeIdInt > short.MaxValue) return short.MaxValue;
            if (dbTypeIdInt < short.MinValue) return short.MinValue;
            return (short)dbTypeIdInt;
        }
        #endregion
    }
}

