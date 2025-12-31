# Communication Architecture Documentation

## Overview

The YesilDefter ecosystem uses a unified API-based communication architecture for context-aware synchronization between desktop (Windows Forms), web (Next.js), and mobile (React Native/Expo) platforms. All platforms communicate through the same Ustad.API (.NET) backend.

## Architecture Diagram

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│  Desktop App    │     │   Web App       │     │  Mobile App     │
│  (WinForms)    │     │  (Next.js)      │     │  (React Native) │
│                 │     │                 │     │                 │
│ UstadApiClient  │     │  getSession()   │     │  apiService     │
│  .cs            │     │  API calls      │     │  .ts            │
└────────┬────────┘     └────────┬────────┘     └────────┬────────┘
         │                       │                       │
         │                       │                       │
         └───────────────────────┼───────────────────────┘
                                 │
                    ┌────────────▼────────────┐
                    │    Ustad.API (.NET)     │
                    │   http://localhost:5000│
                    │                         │
                    │  - Authentication       │
                    │  - Firm Management      │
                    │  - User Management       │
                    │  - QR Code Generation   │
                    └─────────────────────────┘
```

## Current Implementation

### 1. Unified API Backend

**Backend**: `Ustad.API` (.NET Core) - `http://localhost:5000`

All three platforms use the same API endpoints:

- **Authentication**:
  - `POST /auth/login` - Username/password login (3-phase authentication)
  - `POST /auth/qr-login` - QR code authentication
  - `POST /auth/select-firm` - Firm selection (returns new JWT with firm claim)
  - `GET /auth/db-connection-info` - Get database connection info (requires JWT)
  
- **Firm Management**:
  - `GET /UstadFirm/user/{userGUID}` - Get user's firms (requires JWT)
  
- **QR Code**:
  - `GET /UstadQR/generate/{firmGUID}/{userGUID}` - Generate QR code for authentication

### 2. Desktop Application (Windows Forms)

**Location**: `YesiLdefter/Tkn/UstadApiClient.cs`

- **Authentication Flow**:
  1. User logs in via `LoginAsync(email, password)`
  2. API returns JWT token and user information
  3. Token is stored in `v.tUser.JwtToken` for subsequent API calls
  4. Token is used for authenticated requests via `SetAuthToken()`

- **QR Code Generation**:
  - Desktop app generates QR codes via `/UstadQR/generate/{firmGUID}/{userGUID}`
  - QR code displayed in `FirmSelectTemplate.html`
  - Mobile/web apps scan QR code to authenticate

- **Endpoints Used**:
  - `POST /auth/login` - User authentication
  - `GET /auth/user/exists?email={email}` - Check if user exists
  - `POST /auth/changepassword` - Change password
  - `GET /auth/db-connection-info` - Get database connection info (requires JWT)
  - `GET /auth/user/firms?userGUID={guid}` - Get user's firms
  - `GET /UstadQR/generate/{firmGUID}/{userGUID}` - Generate QR code

### 3. Web Application (Next.js)

**Location**: `UstadWeb/ustad-web/apps/ustad-web-yesildefter/src/app/page.tsx`

- **Architecture**: Next.js with server-side rendering
- **Session Management**: Uses `getSession()` from `@/lib/services/auth`
- **API Configuration**: `UstadWeb/ustad-web/apps/ustad-web-yesildefter/src/lib/config/api.ts`
  - Base URL: `http://localhost:5000` (Ustad.API)
  - Environment variable: `NEXT_PUBLIC_USTAD_API_URL` or `USTAD_API_URL`
  
- **Key Features**:
  - Server-side session management
  - Firm selection from session
  - Component-based UI architecture
  - Shared components from `@shared/index`

- **Endpoints Used**:
  - `POST /auth/login` - User authentication
  - `GET /UstadFirm/user/{userGUID}` - Get user's firms
  - Session-based authentication (Next.js server-side)

### 4. Mobile Application (React Native/Expo)

**Location**: `UstadWeb/ustad-web/apps/ustad-mobile-shell/services/api.ts`

- **Architecture**: React Native with Expo
- **API Service**: Singleton `apiService` instance
- **Storage**: AsyncStorage for token persistence
- **Platform Support**: Android, iOS, Web (via Expo)

- **Authentication Flow**:
  1. Regular login: `login(userName, password)` → `POST /auth/login`
  2. QR login: `qrLogin(payload)` → `POST /auth/qr-login`
  3. Token stored in AsyncStorage
  4. Token used for authenticated requests

- **Firm Management**:
  - `getUserFirms(userGUID)` → `GET /UstadFirm/user/{userGUID}`
  - `selectFirm(firmGUID)` → `POST /auth/select-firm` (returns new JWT)
  - Firm GUID stored in AsyncStorage

- **QR Code Support**:
  - `parseQRData(qrString)` - Parse QR code JSON
  - `qrLogin(payload)` - Authenticate via QR code
  - QR payload includes: `firmGUID`, `userGUID`, `tcNoTelefonNo`, `dbName`

- **API Configuration**:
  - Ustad.API: `http://localhost:5000` (default)
  - Go API fallback: `http://localhost:8080`
  - Platform-specific localhost resolution (Android: `10.0.2.2`, iOS: `localhost`)
  - Environment variables: `USTAD_API_URL`, `API_URL`

- **Key Methods**:
  - `login(userName, password)` - Regular authentication
  - `qrLogin(payload)` - QR code authentication
  - `getUserFirms(userGUID?)` - Get user's firms
  - `selectFirm(firmGUID)` - Select firm and get new token
  - `validateToken()` - Validate JWT token
  - `getUserGUID()` - Extract userGUID from JWT token

### 5. QR Code Authentication (Cross-Platform)

**Location**: 
- Desktop: `YesiLdefter/Forms/Templates/FirmSelectTemplate.html`
- Mobile: `UstadWeb/ustad-web/apps/ustad-mobile-shell/services/api.ts`

- **Purpose**: Enable mobile/web authentication via QR code scanning
- **Flow**:
  1. Desktop application generates QR code via `/UstadQR/generate/{firmGUID}/{userGUID}`
  2. QR code contains JSON with:
     - `firmGUID` / `firmId`
     - `userGUID` / `userId`
     - `tcNoTelefonNo`
     - `SectorTypeId`
     - `IsActive`
     - `dbName`
  3. Mobile/web app scans QR code
  4. Mobile/web app calls `POST /auth/qr-login` with QR payload
  5. API returns JWT token (if available) and user context
  6. Mobile/web app stores token and firm context

- **API Endpoints**:
  - `GET /UstadQR/generate/{firmGUID}/{userGUID}` - Generate QR code (Desktop)
  - `POST /auth/qr-login` - Authenticate via QR code (Mobile/Web)

- **QR Code Library**: Uses `qrcode.js` library loaded from CDN (Desktop)

### 6. Firm Selection & Context Sharing

**Location**: `YesiLdefter/Forms/ms_UserFirmSelect.cs`, `YesiLdefter/Forms/Templates/FirmSelectTemplate.html`

- **Purpose**: Allow user to select firm and share context across platforms
- **Features**:
  - WebView2-based firm selection UI
  - QR code generation for mobile authentication
  - Firm selection triggers database connection setup
  - Context (firm, user) is shared via API

### 4. Configuration Management

**Location**: `YesiLdefter/Tkn/tApiConfig.cs`

- **API Base URL Configuration**:
  - Priority: Registry > Settings Files > Environment Variable > Default
  - Default: `http://143.198.228.153:5000` (production server)
  - Supports `appsettings.Production.json` and `appsettings.json`
  - Environment variable: `USTAD_API_BASE_URL`

- **JWT Key Configuration**:
  - Priority: Registry > Settings Files > Environment Variable > Default
  - Default: `UstadSecretKeyForJWTTokenGeneration2026SecureKey32Chars`
  - Environment variable: `USTAD_JWT_KEY`
  - Must match API's JWT key for encryption/decryption

## Communication Patterns

### 1. Request/Response Pattern
- All communication uses HTTP REST API
- JSON request/response format
- JWT Bearer token authentication
- Error handling with status codes

### 2. Token-Based Authentication
- JWT tokens for session management
- Token stored in memory (`v.tUser.JwtToken`)
- Token included in Authorization header for authenticated requests
- Token refresh not yet implemented (TODO in code)

### 3. Context Synchronization
- User context (userGUID, userId) shared via API
- Firm context (firmGUID, firmId) shared via API
- QR code enables cross-platform context sharing
- Database connection info retrieved via API (encrypted)

## API Client Implementation

**File**: `YesiLdefter/Tkn/UstadApiClient.cs`

**Key Methods**:
- `LoginAsync(email, password)` - Authenticate user
- `GetUserFirmsAsync(userGUID)` - Get user's firms
- `GetDbConnectionInfoAsync(firmGUID, userGUID)` - Get encrypted DB connection
- `SelectFirmAsync(firmGUID, userGUID)` - Select firm and get updated token
- `SetAuthToken(token)` - Set JWT token for authenticated requests
- `RequestPasswordResetAsync(email)` - Request password reset

**Error Handling**:
- Network errors caught and wrapped
- HTTP status codes included in exceptions
- User-friendly error messages

## Cross-Platform Communication Flow

### Desktop → Mobile/Web (QR Code Flow)

1. **Desktop**: User logs in and selects firm
2. **Desktop**: Generates QR code via `/UstadQR/generate/{firmGUID}/{userGUID}`
3. **Mobile/Web**: Scans QR code
4. **Mobile/Web**: Parses QR data (firmGUID, userGUID, tcNoTelefonNo, dbName)
5. **Mobile/Web**: Calls `POST /auth/qr-login` with QR payload
6. **API**: Validates QR data and returns JWT token + user context
7. **Mobile/Web**: Stores token and firm context
8. **Mobile/Web**: Ready to use authenticated endpoints

### Web/Mobile → Desktop (Context Sync)

**Current Status**: One-way (Desktop → Mobile/Web via QR)

- Desktop generates QR code for mobile/web to scan
- No reverse flow (mobile/web → desktop) currently implemented
- Future: Webhooks or real-time sync could enable bidirectional communication

## Real-Time Communication

**Current Status**: Not implemented

- No SignalR/WebSocket infrastructure detected
- All communication is request/response (REST API)
- Future: Webhooks or SignalR could enable real-time updates
- All communication is request/response based
- No real-time updates or push notifications

## Future Enhancements (If Needed)

### 1. Real-Time Updates
- **Option**: Implement SignalR for real-time synchronization
- **Use Cases**: 
  - Live data updates across platforms
  - Notification delivery
  - Session synchronization

### 2. Token Refresh
- **Current**: Token stored but no refresh mechanism
- **Enhancement**: Implement refresh token flow
- **Benefit**: Longer sessions without re-authentication

### 3. Offline Support
- **Current**: All operations require API connection
- **Enhancement**: Cache API responses, queue operations
- **Benefit**: Work offline, sync when online

## Production Readiness

### ✅ Implemented
- API-based authentication
- JWT token handling
- QR code generation
- Firm selection and context sharing
- Configuration management (registry, settings files, env vars)
- Error handling

### ⚠️ Needs Attention
- Token refresh mechanism (currently tokens don't expire gracefully)
- Real-time updates (if required for production)
- Offline support (if required for production)

## Notes

- Communication architecture is API-centric
- QR code enables mobile/web authentication
- Context sharing works via API endpoints
- No additional implementation needed for basic production release
- Real-time features can be added later if requirements emerge

