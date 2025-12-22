# Deployment Guide - Ustad.API

This guide provides step-by-step instructions for securely deploying the Ustad.API application.

## 🔒 Security Requirements

**IMPORTANT**: The application requires environment variables for all sensitive credentials. The application will **fail to start** if required credentials are not configured.

---

## 📋 Pre-Deployment Checklist

- [ ] All hardcoded credentials removed from code ✅ (Completed)
- [ ] Environment variables documented
- [ ] Production secrets stored securely (Key Vault, Secrets Manager, etc.)
- [ ] Database credentials rotated (if previously exposed)
- [ ] JWT keys rotated (if previously exposed)
- [ ] Email SMTP credentials rotated (if previously exposed)

---

## 🔑 Required Environment Variables

The application requires the following environment variables to be set in production:

### Database Configuration

```bash
DB_HOST=your-database-server-host-or-ip
DB_PORT=1433
DB_USER=your-database-username
DB_PASS=your-secure-database-password
DB_NAME=your-database-name
```

**Note**: The application also supports `Db:ManagerName` in configuration for the manager database name, or it can be extracted from the `BulutManager` connection string.

### JWT Configuration

```bash
JWT_KEY=your-secure-jwt-key-minimum-32-characters-long
```

**Security Requirements**:
- Minimum 32 characters
- Use cryptographically secure random string
- Example: Generate using `openssl rand -base64 32` or similar

### Optional Environment Variables

```bash
# Cloudflare Turnstile (for bot protection)
TURNSTILE_SECRET=your-cloudflare-turnstile-secret

# JWT Token Expiration (optional, defaults to 480 minutes)
JWT_EXPIRES_MINUTES=480
JWT_REFRESH_EXPIRES_MINUTES=20160
```

---

## 🚀 Deployment Steps

### Option 1: Environment Variables (Recommended)

**For Linux/macOS**:
```bash
export DB_HOST=your-db-host
export DB_PORT=1433
export DB_USER=your-db-user
export DB_PASS=your-secure-password
export DB_NAME=your-db-name
export JWT_KEY=your-secure-32-char-minimum-key

# Run the application
dotnet run
```

**For Windows (PowerShell)**:
```powershell
$env:DB_HOST="your-db-host"
$env:DB_PORT="1433"
$env:DB_USER="your-db-user"
$env:DB_PASS="your-secure-password"
$env:DB_NAME="your-db-name"
$env:JWT_KEY="your-secure-32-char-minimum-key"

# Run the application
dotnet run
```

**For Windows (Command Prompt)**:
```cmd
set DB_HOST=your-db-host
set DB_PORT=1433
set DB_USER=your-db-user
set DB_PASS=your-secure-password
set DB_NAME=your-db-name
set JWT_KEY=your-secure-32-char-minimum-key

# Run the application
dotnet run
```

### Option 2: appsettings.Production.json

1. Copy `appsettings.Production.json` to your deployment location
2. Replace all `YOUR_*` placeholders with actual values
3. Ensure `appsettings.Production.json` is **NOT** committed to source control
4. The application will automatically load `appsettings.Production.json` when `ASPNETCORE_ENVIRONMENT=Production`

**⚠️ WARNING**: If using `appsettings.Production.json`, ensure it is:
- Added to `.gitignore`
- Stored securely (encrypted at rest)
- Not accessible via web server (outside wwwroot)

### Option 3: Azure Key Vault / AWS Secrets Manager (Recommended for Production)

**Azure Key Vault**:
```csharp
// In Program.cs or Startup.cs, add Key Vault configuration
builder.Configuration.AddAzureKeyVault(
    vaultUri: "https://your-keyvault.vault.azure.net/",
    credential: new DefaultAzureCredential()
);
```

**AWS Secrets Manager**:
```csharp
// Add AWS Secrets Manager configuration
builder.Configuration.AddSecretsManager(
    region: RegionEndpoint.USEast1,
    configurator: options => {
        options.SecretFilter = entry => entry.Name.StartsWith("UstadAPI/");
    }
);
```

---

## 🐳 Docker Deployment

### Dockerfile Example

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Ustad.API/Ustad.API.csproj", "Ustad.API/"]
RUN dotnet restore "Ustad.API/Ustad.API.csproj"
COPY . .
WORKDIR "/src/Ustad.API"
RUN dotnet build "Ustad.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Ustad.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ustad.API.dll"]
```

### Docker Run with Environment Variables

```bash
docker run -d \
  -p 5000:5000 \
  -e DB_HOST=your-db-host \
  -e DB_PORT=1433 \
  -e DB_USER=your-db-user \
  -e DB_PASS=your-secure-password \
  -e DB_NAME=your-db-name \
  -e JWT_KEY=your-secure-32-char-minimum-key \
  --name ustad-api \
  ustad-api:latest
```

### Docker Compose Example

```yaml
version: '3.8'

services:
  ustad-api:
    image: ustad-api:latest
    ports:
      - "5000:5000"
    environment:
      - DB_HOST=${DB_HOST}
      - DB_PORT=${DB_PORT}
      - DB_USER=${DB_USER}
      - DB_PASS=${DB_PASS}
      - DB_NAME=${DB_NAME}
      - JWT_KEY=${JWT_KEY}
      - ASPNETCORE_ENVIRONMENT=Production
    env_file:
      - .env.production  # Store secrets in .env.production (gitignored)
```

---

## ☁️ Cloud Platform Deployment

### Azure App Service

1. **Set Environment Variables**:
   - Go to Azure Portal → App Service → Configuration → Application Settings
   - Add all required environment variables
   - Use "Key Vault references" for sensitive values

2. **Key Vault Integration** (Recommended):
   ```
   @Microsoft.KeyVault(SecretUri=https://your-keyvault.vault.azure.net/secrets/DB-PASS/)
   ```

### AWS Elastic Beanstalk

1. **Set Environment Variables**:
   - Go to AWS Console → Elastic Beanstalk → Configuration → Software
   - Add environment properties
   - Or use AWS Systems Manager Parameter Store / Secrets Manager

### Google Cloud Run

```bash
gcloud run deploy ustad-api \
  --image gcr.io/your-project/ustad-api \
  --set-env-vars DB_HOST=your-db-host,DB_PORT=1433,DB_USER=your-db-user \
  --set-secrets DB_PASS=db-password:latest,JWT_KEY=jwt-key:latest
```

---

## ✅ Verification

After deployment, verify the application is running securely:

1. **Check Application Startup**:
   ```bash
   # Application should start without errors
   # If credentials are missing, you'll see:
   # InvalidOperationException: DB_HOST environment variable or Db:Host configuration is required
   ```

2. **Test Authentication Endpoint**:
   ```bash
   curl -X POST https://your-api-domain.com/auth/login \
     -H "Content-Type: application/json" \
     -d '{"userName":"test@example.com","password":"testpassword"}'
   ```

3. **Verify No Hardcoded Secrets**:
   - Check application logs for any credential exposure
   - Verify environment variables are set correctly
   - Confirm no secrets in configuration files

---

## 🔄 Credential Rotation

If credentials were previously exposed in source control:

1. **Rotate Database Passwords**:
   - Change database user passwords
   - Update environment variables
   - Restart application

2. **Rotate JWT Keys**:
   - Generate new JWT key (minimum 32 characters)
   - Update `JWT_KEY` environment variable
   - **Note**: All existing tokens will be invalidated

3. **Rotate Email SMTP Passwords**:
   - Generate new app password
   - Update email configuration
   - Test email sending

---

## 📝 Configuration Priority

The application uses the following priority order:

1. **Environment Variables** (highest priority - most secure)
2. **appsettings.Production.json** (if `ASPNETCORE_ENVIRONMENT=Production`)
3. **appsettings.json** (development only)
4. **No fallback** - Application fails if required values are missing ✅

---

## 🆘 Troubleshooting

### Application Fails to Start

**Error**: `InvalidOperationException: DB_HOST environment variable or Db:Host configuration is required`

**Solution**: Set all required environment variables before starting the application.

### JWT Token Validation Fails

**Error**: `InvalidOperationException: JWT key must be at least 32 characters long`

**Solution**: Ensure `JWT_KEY` environment variable is set and is at least 32 characters long.

### Database Connection Fails

**Error**: Database connection timeout or authentication failure

**Solution**: 
- Verify database credentials are correct
- Check network connectivity to database server
- Ensure firewall rules allow connections
- Verify database user has required permissions

---

## 📚 Additional Resources

- [ASP.NET Core Configuration](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [Azure Key Vault](https://azure.microsoft.com/en-us/services/key-vault/)
- [AWS Secrets Manager](https://aws.amazon.com/secrets-manager/)
- [Security Best Practices](Ustad.API/SECURITY_AUDIT_REPORT.md)

---

## ✅ Deployment Checklist

- [ ] All environment variables set
- [ ] Database credentials verified
- [ ] JWT key generated and set (minimum 32 characters)
- [ ] Email SMTP credentials configured
- [ ] Application starts without errors
- [ ] Authentication endpoint tested
- [ ] No secrets in source control
- [ ] Production secrets stored securely
- [ ] Monitoring and logging configured
- [ ] Backup and recovery plan in place

---

**Last Updated**: 2025-01-XX  
**Security Status**: ✅ All critical security issues resolved

