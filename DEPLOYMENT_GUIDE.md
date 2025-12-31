# 🚀 PRODUCTION DEPLOYMENT GUIDE
## Unified Authentication System - Desktop & Web

---

## 📋 **PRE-DEPLOYMENT CHECKLIST**

### Desktop Application (YesiLdefter)
- [ ] All tests passed (see `AUTHENTICATION_TESTING_GUIDE.md`)
- [ ] Code reviewed and approved
- [ ] Version number updated
- [ ] Release build compiled
- [ ] EXE digitally signed (if applicable)
- [ ] Backup of current production EXE created

### Web Application & APIs
- [ ] All tests passed
- [ ] Code reviewed and approved
- [ ] Environment variables documented
- [ ] Docker images tested locally
- [ ] Database migrations ready (if any)
- [ ] Backup plan ready

---

## 🎯 **DEPLOYMENT ARCHITECTURE**

```
┌─────────────────────────────────────────────────────────────────┐
│                    PRODUCTION ARCHITECTURE                      │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌──────────────────┐                                           │
│  │  Desktop App     │                                           │
│  │  (YesiLdefter)   │─────┐                                     │
│  └──────────────────┘     │                                     │
│                            │                                    │
│  ┌──────────────────┐     │     ┌──────────────────────┐        │
│  │  Web App         │     │     │  Ustad.API           │        │
│  │  (Next.js)       │─────┼────▶│  (.NET Core)        │         │
│  │  Port: 3000      │     │     │  Port: 5000          │        │
│  └──────────────────┘     │     │  /auth/*             │        │
│                           │     │  /UstadFirm/*        │        │
│  ┌──────────────────┐     │     └──────────────────────┘        │
│  │  Mobile Shell    │     │               │                     │
│  │  (React Native)  │─────┘               │                     │
│  └──────────────────┘                     │                     │
│                                           │                     │
│                            ┌─────────────▼───────────────┐      │
│                            │  Go API                     │      │
│                            │  (Gin)                      │      │
│                            │  Port: 8080                 │      │
│                            │  Validates JWT              │      │
│                            └─────────────────────────────┘      │
│                                           │                     │
│                            ┌──────────────▼──────────────┐      │
│                            │  SQL Server                 │      │
│                            │  (UstadCrmDB, ManagerDB)    │      │
│                            │  Port: 1433                 │      │
│                            └─────────────────────────────┘      │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🖥️ **PART 1: DESKTOP APP DEPLOYMENT**

### Step 1.1: Prepare Release Build

```powershell
# Navigate to project directory
cd C:\UstadProjects\yesiLdefter

# Clean previous builds
Remove-Item -Recurse -Force .\bin\Release -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force .\obj\Release -ErrorAction SilentlyContinue

# Build Release configuration
msbuild YesiLdefter.csproj /p:Configuration=Release /p:Platform="Any CPU"

# Verify build succeeded
if (Test-Path ".\bin\Release\YesiLdefter.exe") {
    Write-Host "✅ Build successful!" -ForegroundColor Green
} else {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    exit 1
}
```

### Step 1.2: Version Control

```powershell
# Get current version from EXE
$version = (Get-Item ".\bin\Release\YesiLdefter.exe").VersionInfo.FileVersion
Write-Host "Version: $version"

# Create version tag in git
git tag -a "v$version" -m "Release version $version - Secure Authentication"
```

### Step 1.3: Backup Current Production

```powershell
# On production server
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backupPath = "C:\Backups\YesiLdefter_$timestamp.exe"

Copy-Item "C:\Production\YesiLdefter.exe" $backupPath
Write-Host "✅ Backup created: $backupPath"
```

### Step 1.4: Deploy New Version

```powershell
# Copy new EXE to production
$source = ".\bin\Release\YesiLdefter.exe"
$destination = "C:\Production\YesiLdefter.exe"

# Stop application if running (optional)
# Get-Process "YesiLdefter" -ErrorAction SilentlyContinue | Stop-Process

# Copy new version
Copy-Item $source $destination -Force

Write-Host "✅ Deployment complete!"
```

### Step 1.5: Verify Deployment

```powershell
# Launch application
Start-Process "C:\Production\YesiLdefter.exe"

# Manual verification checklist:
# 1. Application launches without errors
# 2. Standalone login form appears
# 3. Can login with test credentials
# 4. No pre-auth database connections (verify with SQL Profiler)
# 5. Main application functions normally
```

---

## 🌐 **PART 2: WEB & API DEPLOYMENT (Docker)**

### Step 2.1: Prepare Environment Variables

Create `.env` file on production server:

```bash
# /opt/ustad-web/.env

# ======================
# DATABASE CONFIGURATION
# ======================
DB_HOST=46.101.255.224
DB_PORT=1433
DB_USER=sa
DB_PASS=ustad84352Yazilim
DB_NAME=UstadCrmV1

# Manager Database (optional, defaults to ManagerV1)
DB_MANAGER_NAME=ManagerV1

# ======================
# JWT CONFIGURATION (CRITICAL - Must Match Across All Services)
# ======================
JWT_KEY=UstadSecretKeyForJWTTokenGeneration2026SecureKey32Chars
JWT_SECRET=UstadSecretKeyForJWTTokenGeneration2026SecureKey32Chars
JWT_ISSUER=UstadAuth
JWT_AUDIENCE=UstadClients
JWT_EXPIRES_MINUTES=480
JWT_REFRESH_EXPIRES_MINUTES=20160

# ======================
# NEXTAUTH CONFIGURATION (Web App)
# ======================
NEXTAUTH_SECRET=UstadSecretKeyForJWTTokenGeneration2026SecureKey32Chars
NEXTAUTH_URL=http://143.198.228.153:3000

# ======================
# API URLS
# ======================
USTAD_API_URL=http://ustad-api:5000
GO_API_URL=http://go-api:8080
NEXT_PUBLIC_USTAD_API_URL=http://143.198.228.153:5000
NEXT_PUBLIC_GO_API_URL=http://143.198.228.153:8080

# ======================
# NODE ENVIRONMENT
# ======================
NODE_ENV=production
NEXT_TELEMETRY_DISABLED=1
```

**⚠️ CRITICAL:** Ensure `JWT_KEY`, `JWT_SECRET`, and `NEXTAUTH_SECRET` are identical!

### Step 2.2: Build Applications Locally

```bash
cd /path/to/ustad-web

# 1. Build Next.js App (standalone mode)
cd apps/ustad-web-yesildefter
npm install --legacy-peer-deps
npm run build

# Verify standalone build
if [ -d ".next/standalone" ]; then
    echo "✅ Next.js standalone build successful"
else
    echo "❌ Next.js build failed - standalone output missing"
    exit 1
fi
cd ../..

# 2. Build .NET API
cd ../Ustad.API
dotnet publish -c Release -o ./publish

# Verify publish
if [ -f "./publish/Ustad.API.dll" ]; then
    echo "✅ .NET API publish successful"
else
    echo "❌ .NET API publish failed"
    exit 1
fi
cd ..

# 3. Prepare Go API (no build needed - builds in Docker)
echo "✅ Go API ready (will build in Docker)"
```

### Step 2.3: Prepare Deployment Context

```bash
# Clean previous deployment
rm -rf _deploy
mkdir -p _deploy/{api,go-api,web}

# Copy .NET API
cp -r ../Ustad.API/publish/* _deploy/api/
cp ../Ustad.API/Dockerfile _deploy/api/
cp ../Ustad.API/appsettings.json _deploy/api/

# Copy Go API source
cp apps/ustad-web-api/Dockerfile _deploy/go-api/ 2>/dev/null || true
cp apps/ustad-web-api/go.mod _deploy/go-api/
cp apps/ustad-web-api/go.sum _deploy/go-api/
cp -r apps/ustad-web-api/cmd _deploy/go-api/
cp -r apps/ustad-web-api/internal _deploy/go-api/
cp -r apps/ustad-web-api/sql _deploy/go-api/ 2>/dev/null || true

# Copy Next.js App
mkdir -p _deploy/web/dist/apps/ustad-web-yesildefter/.next
cp -r apps/ustad-web-yesildefter/.next/standalone _deploy/web/dist/apps/ustad-web-yesildefter/.next/
cp -r apps/ustad-web-yesildefter/.next/static _deploy/web/dist/apps/ustad-web-yesildefter/.next/
cp -r apps/ustad-web-yesildefter/public _deploy/web/dist/apps/ustad-web-yesildefter/
cp apps/ustad-web-yesildefter/Dockerfile _deploy/web/

# Copy docker-compose
cp docker-compose.yml _deploy/

echo "✅ Deployment context prepared"
```

### Step 2.4: Transfer to Production Server

```bash
# Variables
REMOTE_USER="root"
REMOTE_HOST="143.198.228.153"
REMOTE_DIR="/opt/ustad-web"

# Create remote directory
ssh $REMOTE_USER@$REMOTE_HOST "mkdir -p $REMOTE_DIR"

# Transfer files
echo "📤 Transferring files to server..."
scp -r _deploy/* $REMOTE_USER@$REMOTE_HOST:$REMOTE_DIR/

echo "✅ Files transferred"
```

### Step 2.5: Deploy on Server

```bash
# SSH to server
ssh $REMOTE_USER@$REMOTE_HOST

# Navigate to deployment directory
cd /opt/ustad-web

# Stop existing containers
docker compose down

# Remove old images (optional - saves disk space)
docker system prune -f

# Build and start new containers
docker compose up -d --build

# Watch logs
docker compose logs -f
```

### Step 2.6: Verify Deployment

```bash
# Check container status
docker compose ps

# Expected output:
# NAME                  STATUS    PORTS
# ustad-web            Up        0.0.0.0:3000->3000/tcp
# ustad-api            Up        0.0.0.0:5000->5000/tcp
# go-api               Up        0.0.0.0:8080->8080/tcp

# Check logs for errors
docker compose logs ustad-api | grep ERROR
docker compose logs ustad-web | grep ERROR
docker compose logs go-api | grep ERROR

# Test API endpoints
curl http://localhost:5000/health
curl http://localhost:8080/health

# Test authentication endpoint
curl -X POST http://localhost:5000/auth/login \
  -H "Content-Type: application/json" \
  -d '{"UserName":"test@example.com","Password":"TestPassword123"}'
```

---

## 🔍 **POST-DEPLOYMENT VERIFICATION**

### Desktop App Verification

```
✅ Checklist:
[ ] Application launches
[ ] Standalone login form appears
[ ] Can login with valid credentials
[ ] NO database connections before auth (verify with SQL Profiler)
[ ] Database connections established after successful auth
[ ] Main application functions normally
[ ] Form layouts load correctly
[ ] No errors in Windows Event Viewer
```

### Web App Verification

```bash
# Test Web App
curl http://143.198.228.153:3000

# Test API Authentication
curl -X POST http://143.198.228.153:5000/auth/login \
  -H "Content-Type: application/json" \
  -d '{"UserName":"test@example.com","Password":"TestPassword123"}'

# Test JWT Validation (Go API)
# Get token from login above, then:
curl http://143.198.228.153:8080/protected-endpoint \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### Browser Verification

```
Open browser: http://143.198.228.153:3000

✅ Checklist:
[ ] Website loads
[ ] Login page appears
[ ] Can login with valid credentials
[ ] Session persists across page refreshes
[ ] Protected routes require authentication
[ ] Logout works correctly
```

---

## 🚨 **ROLLBACK PROCEDURE**

### If Desktop App Fails

```powershell
# On production server
# Find latest backup
Get-ChildItem C:\Backups\YesiLdefter_*.exe | Sort-Object LastWriteTime -Descending | Select-Object -First 1

# Restore backup
$backup = "C:\Backups\YesiLdefter_TIMESTAMP.exe"
Copy-Item $backup "C:\Production\YesiLdefter.exe" -Force

# Restart application
Start-Process "C:\Production\YesiLdefter.exe"
```

### If Web/API Fails

```bash
# SSH to server
ssh root@143.198.228.153

cd /opt/ustad-web

# Stop current containers
docker compose down

# Restore previous version from git
git checkout PREVIOUS_VERSION

# Rebuild and restart
docker compose up -d --build

# Monitor logs
docker compose logs -f
```

---

## 📊 **MONITORING & HEALTH CHECKS**

### Desktop App Monitoring

```
Monitor:
1. Windows Event Viewer (Application logs)
2. SQL Server Profiler (connection monitoring)
3. User login success/failure rates
4. Application crash reports
```

### Web/API Monitoring

```bash
# Check container health
docker compose ps

# Monitor logs
docker compose logs -f --tail=100

# Check resource usage
docker stats

# Monitor API response times
while true; do
  time curl -s http://localhost:5000/health > /dev/null
  sleep 5
done

# Check authentication success rate
docker compose logs ustad-api | grep "Login successful" | wc -l
docker compose logs ustad-api | grep "Login failed" | wc -l
```

### Database Monitoring

```sql
-- Monitor active connections
SELECT 
    DB_NAME(dbid) as DatabaseName,
    COUNT(dbid) as TotalConnections,
    loginame
FROM sys.sysprocesses
WHERE dbid > 0
GROUP BY DB_NAME(dbid), loginame
ORDER BY TotalConnections DESC;

-- Monitor failed login attempts
SELECT 
    *
FROM UstadCrmV1.dbo.UstadUsers
WHERE LastLoginAttempt > DATEADD(hour, -1, GETDATE())
AND LastLoginSuccess IS NULL;
```

---

## 🔒 **SECURITY VERIFICATION**

### Post-Deployment Security Checks

```
✅ Checklist:
[ ] No hardcoded passwords in deployed binaries
[ ] JWT secrets match across all services
[ ] Database passwords not exposed in logs
[ ] HTTPS enabled (production)
[ ] Firewall rules configured
[ ] SQL Server remote access restricted
[ ] Environment variables secured
[ ] Docker images scanned for vulnerabilities
[ ] API endpoints require authentication
[ ] Rate limiting configured (if applicable)
```

---

## 📞 **SUPPORT & TROUBLESHOOTING**

### Common Issues

**Issue:** Desktop app can't connect to API
```
Solution:
1. Check API is running: curl http://localhost:5000/health
2. Check firewall allows port 5000
3. Verify API URL in registry
4. Check network connectivity
```

**Issue:** "JWT token invalid" error
```
Solution:
1. Verify JWT_KEY matches between .NET and Go API
2. Check JWT_ISSUER and JWT_AUDIENCE match
3. Verify token hasn't expired
4. Check clock synchronization
```

**Issue:** Database connection fails after login
```
Solution:
1. Verify database server accessible
2. Check credentials in environment variables
3. Verify /auth/db-connection-info endpoint works
4. Check JWT key for decryption matches
```

---

## ✅ **DEPLOYMENT SIGN-OFF**

### Desktop App
- **Deployed by:** _________________
- **Date:** _________________
- **Version:** _________________
- **Status:** [ ] Success [ ] Failed
- **Rollback Plan:** [ ] Tested [ ] Not Tested

### Web & APIs
- **Deployed by:** _________________
- **Date:** _________________
- **Containers:**
  - Next.js: [ ] Running [ ] Failed
  - .NET API: [ ] Running [ ] Failed
  - Go API: [ ] Running [ ] Failed
- **Status:** [ ] Success [ ] Failed
- **Rollback Plan:** [ ] Tested [ ] Not Tested

### Verification
- [ ] All health checks passed
- [ ] Authentication tested
- [ ] Database connections verified
- [ ] Security checks completed
- [ ] Monitoring configured
- [ ] Documentation updated

---

**Deployment Status:** ⏳ **READY FOR EXECUTION**  
**Next Action:** Execute deployment steps in sequence  
**Support:** Contact DevOps team for deployment assistance

