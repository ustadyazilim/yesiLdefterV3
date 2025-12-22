# Technical Roadmap - Next Steps Implementation

## Overview

This document provides a technical implementation plan for the next steps, organized into 4-hour work blocks. Tasks are linked by dependencies - complete prerequisite tasks before starting dependent ones.

## Work Block 1: Quick Wins for Presentation (Priority: High)

### Task 1.1: Database Script Feature Analysis
**Goal**: Create a feature list from database scripts that can be demonstrated or promised for tomorrow's presentation.

**Dependencies**: None (can start immediately)

**Technical Steps**:
1. **Analyze Script Categories** (`DATABASE_SCRIPTS_INVENTORY.md` already created)
   - Review `UST/Mtsk/` scripts (158 files) - MTSK module features
   - Review `UST/OnMuhasebe/` scripts (135 files) - Accounting features
   - Review `UST/SRC/` scripts (58 files) - SRC module features
   - Review `UST/MsV3/` scripts (50 files) - MS V3 features

2. **Extract Feature Names from Scripts**:
   - Parse table names to infer features (e.g., `MtskAday*.txt` → Candidate Management)
   - Parse procedure names to infer workflows (e.g., `prc_MtskAdayBorclandir.txt` → Billing)
   - Document lookup tables as configuration options

3. **Create Feature Matrix**:
   - File: `YesiLdefter/Scripts/FEATURE_MATRIX.md`
   - Columns: Feature Name, Module, Status (Implemented/Planned), Script Files, Description
   - Group by: MTSK, Accounting, SRC, Web, etc.

4. **Identify Quick-Demo Features**:
   - Features that can be demonstrated with existing UI
   - Features that can be promised (scripts exist, UI pending)
   - Features that show system capabilities

**Deliverable**: `YesiLdefter/Scripts/FEATURE_MATRIX.md` with categorized feature list

**Estimated Effort**: 2-3 hours

---

### Task 1.2: DevExpress Design Token System (Foundation)
**Goal**: Create infrastructure to load design tokens from a single file and apply to DevExpress.

**Dependencies**: None (can start in parallel with 1.1)

**Technical Steps**:
1. **Create Design Token File Structure**:
   - File: `YesiLdefter/Forms/Templates/shared/design-tokens.json`
   - Structure:
     ```json
     {
       "colors": {
         "brand": { "primary": "#295c00", "primaryLight": "#8e9c78", ... },
         "background": { "primary": "#f8f9fa", "secondary": "#ffffff", ... },
         "text": { "primary": "#111827", "secondary": "#374151", ... }
       },
       "typography": { "fontFamily": "Inter Tight", "fontSizes": {...} },
       "spacing": { "base": 4, "scale": [4, 8, 12, 16, 24, 32, 48] },
       "borders": { "radius": { "sm": 8, "md": 12, "lg": 16 } },
       "shadows": { "sm": "...", "md": "...", "lg": "..." }
     }
     ```

2. **Create Design Token Loader**:
   - File: `YesiLdefter/Tkn/tDesignTokenLoader.cs`
   - Methods:
     - `LoadDesignTokens(string jsonPath)` - Load from JSON file
     - `GetColor(string path)` - Get color by path (e.g., "colors.brand.primary")
     - `GetValue<T>(string path)` - Generic value getter
   - Cache loaded tokens in memory

3. **Integrate with Existing Theme**:
   - Modify `YesiLdefter/Tkn/tDevExpressTheme.cs`
   - Add method: `LoadFromDesignTokens(DesignTokenLoader loader)`
   - Map design token values to `tDevExpressTheme` static properties
   - Maintain backward compatibility with hardcoded values

**Deliverable**: 
- `YesiLdefter/Forms/Templates/shared/design-tokens.json`
- `YesiLdefter/Tkn/tDesignTokenLoader.cs`
- Updated `YesiLdefter/Tkn/tDevExpressTheme.cs` with token loading

**Estimated Effort**: 2-3 hours

---

## Work Block 2: DevExpress Skin Implementation (Depends on 1.2)

### Task 2.1: DevExpress Custom Skin Generator
**Goal**: Generate DevExpress skin from design tokens.

**Dependencies**: Task 1.2 (Design Token System)

**Technical Steps**:
1. **Research DevExpress Skin Creation**:
   - Review DevExpress skin architecture
   - Identify skin file format (XML-based or programmatic)
   - Check if custom skin can be created at runtime

2. **Create Skin Generator Class**:
   - File: `YesiLdefter/Tkn/tDevExpressSkinGenerator.cs`
   - Methods:
     - `GenerateSkinFromTokens(DesignTokenLoader tokens)` - Create skin definition
     - `ApplySkinToApplication()` - Apply generated skin
     - `RegisterCustomSkin()` - Register with DevExpress

3. **Map Design Tokens to DevExpress Appearance**:
   - Map colors to `AppearanceObject` properties
   - Map typography to font settings
   - Map spacing to padding/margin
   - Map borders to border options
   - Map shadows to appearance effects

4. **Integration Point**:
   - Modify `YesiLdefter/main.cs` line 250: `UserLookAndFeel.Default.SetSkinStyle(...)`
   - Replace with: `tDevExpressSkinGenerator.ApplySkinFromTokens()`
   - Ensure skin applies on application startup

**Deliverable**: 
- `YesiLdefter/Tkn/tDevExpressSkinGenerator.cs`
- Updated `YesiLdefter/main.cs` to use generated skin
- Documentation on skin customization

**Estimated Effort**: 3-4 hours

**Note**: If DevExpress doesn't support runtime skin generation, create a programmatic appearance system that applies tokens to all controls.

---

## Work Block 3: Real-Time Communication (Can Start After Block 1)

### Task 3.1: Webhook Infrastructure Design
**Goal**: Design webhook system for real-time sync between web and mobile.

**Dependencies**: None (can start after Block 1)

**Technical Steps**:
1. **Review Existing API Architecture**:
   - File: `YesiLdefter/COMMUNICATION_ARCHITECTURE.md`
   - Identify API endpoints that need webhook notifications
   - Document current request/response patterns

2. **Design Webhook Payload Structure**:
   - File: `YesiLdefter/WEBHOOK_ARCHITECTURE.md`
   - Define webhook event types:
     - `user.login` - User logged in
     - `firm.selected` - Firm selected
     - `data.updated` - Data changed
     - `session.expired` - Session expired
   - Define payload format (JSON)
   - Define authentication (JWT or webhook secret)

3. **Design Webhook Receiver (Desktop)**:
   - File: `YesiLdefter/Tkn/tWebhookReceiver.cs`
   - Methods:
     - `StartWebhookListener(int port)` - Start HTTP listener
     - `RegisterWebhookHandler(string eventType, Action<WebhookPayload> handler)`
     - `ProcessWebhookRequest(HttpListenerContext context)` - Process incoming webhooks
   - Use `HttpListener` for local webhook endpoint

4. **Design Webhook Sender (API Side)**:
   - Document API-side webhook implementation requirements
   - Define webhook delivery mechanism (HTTP POST to desktop app)
   - Define retry logic and error handling

**Deliverable**: 
- `YesiLdefter/WEBHOOK_ARCHITECTURE.md` - Design document
- `YesiLdefter/Tkn/tWebhookReceiver.cs` - Desktop webhook receiver (skeleton)

**Estimated Effort**: 2-3 hours

---

### Task 3.2: Webhook Implementation (Desktop Side)
**Goal**: Implement webhook receiver in desktop application.

**Dependencies**: Task 3.1 (Webhook Infrastructure Design)

**Technical Steps**:
1. **Implement Webhook Receiver**:
   - Complete `YesiLdefter/Tkn/tWebhookReceiver.cs`
   - Add HTTP listener on localhost (configurable port)
   - Add request validation (JWT or webhook secret)
   - Add payload parsing (JSON deserialization)
   - Add event routing to registered handlers

2. **Integrate with Application Events**:
   - Register webhook handlers for key events
   - Update UI when webhook received (if needed)
   - Sync data when webhook indicates changes
   - Handle session synchronization

3. **Add Configuration**:
   - Add webhook port to `tApiConfig.cs`
   - Add webhook enable/disable flag
   - Add webhook secret configuration

4. **Testing**:
   - Create test webhook sender
   - Test webhook delivery
   - Test event handling

**Deliverable**: 
- Complete `YesiLdefter/Tkn/tWebhookReceiver.cs`
- Integration with application events
- Configuration in `tApiConfig.cs`

**Estimated Effort**: 3-4 hours

**Note**: API-side webhook implementation is separate and not in scope for desktop app.

---

## Work Block 4: Offline Support (Depends on Block 3)

### Task 4.1: Offline Cache Architecture
**Goal**: Design caching and queue system for offline work.

**Dependencies**: Task 3.2 (Webhook Implementation) - to understand sync requirements

**Technical Steps**:
1. **Design Cache Structure**:
   - File: `YesiLdefter/OFFLINE_ARCHITECTURE.md`
   - Define cache storage (SQLite local DB or file-based)
   - Define cache invalidation strategy
   - Define cache synchronization rules

2. **Design Operation Queue**:
   - Define queue structure for pending operations
   - Define operation types (create, update, delete)
   - Define conflict resolution strategy
   - Define retry logic

3. **Create Cache Manager**:
   - File: `YesiLdefter/Tkn/tOfflineCache.cs`
   - Methods:
     - `CacheData<T>(string key, T data)` - Cache data
     - `GetCachedData<T>(string key)` - Retrieve cached data
     - `InvalidateCache(string key)` - Invalidate cache entry
     - `IsOnline()` - Check connection status

4. **Create Operation Queue**:
   - File: `YesiLdefter/Tkn/tOperationQueue.cs`
   - Methods:
     - `EnqueueOperation(Operation op)` - Add operation to queue
     - `ProcessQueue()` - Process queued operations when online
     - `GetPendingOperations()` - Get list of pending operations
     - `ClearProcessedOperations()` - Remove processed operations

**Deliverable**: 
- `YesiLdefter/OFFLINE_ARCHITECTURE.md` - Design document
- `YesiLdefter/Tkn/tOfflineCache.cs` - Cache manager (skeleton)
- `YesiLdefter/Tkn/tOperationQueue.cs` - Operation queue (skeleton)

**Estimated Effort**: 2-3 hours

---

### Task 4.2: Offline Support Implementation
**Goal**: Implement caching and queue for offline operations.

**Dependencies**: Task 4.1 (Offline Cache Architecture)

**Technical Steps**:
1. **Implement Cache Manager**:
   - Complete `YesiLdefter/Tkn/tOfflineCache.cs`
   - Choose storage: SQLite database or JSON files
   - Implement serialization/deserialization
   - Implement cache expiration
   - Implement cache size limits

2. **Implement Operation Queue**:
   - Complete `YesiLdefter/Tkn/tOperationQueue.cs`
   - Implement queue storage (SQLite or file-based)
   - Implement operation serialization
   - Implement conflict detection
   - Implement retry logic with exponential backoff

3. **Integrate with API Client**:
   - Modify `YesiLdefter/Tkn/UstadApiClient.cs`
   - Add offline mode detection
   - Route requests to queue when offline
   - Use cache when offline and data available
   - Process queue when connection restored

4. **Add UI Indicators**:
   - Add offline/online status indicator
   - Show pending operations count
   - Show sync progress
   - Show sync errors

**Deliverable**: 
- Complete `YesiLdefter/Tkn/tOfflineCache.cs`
- Complete `YesiLdefter/Tkn/tOperationQueue.cs`
- Updated `YesiLdefter/Tkn/UstadApiClient.cs` with offline support
- UI indicators for offline status

**Estimated Effort**: 4-5 hours

---

## Work Block 5: Core Feature Reinforcement (Priority: High - Pre-Release)

**Goal**: Ensure core features work consistently across desktop, web, and mobile platforms. Prevent "half-done" application state.

**Dependencies**: None (can start immediately, parallel with Block 1)

### Task 5.1: Cross-Platform Feature Audit

**Goal**: Identify which features exist in which platform and ensure consistency.

**Dependencies**: None

**Technical Steps**:
1. **Audit Desktop Features**:
   - Review `YesiLdefter/Forms/` for implemented features
   - Document: Feature name, form file, status (working/partial/broken)
   - Check database scripts → feature mapping from `FEATURE_EXTRACTION_GUIDE.md`

2. **Audit Web Features**:
   - Review `UstadWeb/ustad-web/apps/ustad-web-yesildefter/src/app/` for implemented features
   - Document: Feature name, route/page, status
   - Compare with desktop features

3. **Audit Mobile Features**:
   - Review `UstadWeb/ustad-web/apps/ustad-mobile-shell/app/` for implemented features
   - Document: Feature name, screen/component, status
   - Compare with desktop/web features

4. **Create Feature Parity Matrix**:
   - File: `YesiLdefter/CORE_FEATURES_PARITY.md`
   - Columns: Feature Name | Desktop | Web | Mobile | API Endpoint | Status
   - Mark: ✅ Implemented, ⚠️ Partial, ❌ Missing, 🔄 In Progress
   - Identify gaps and inconsistencies

5. **Prioritize Core Features**:
   - Must-have for release: Authentication, Firm Selection, Basic CRUD
   - Should-have: QR Code, Data Sync, Reports
   - Nice-to-have: Advanced features, integrations

**Deliverable**: 
- `YesiLdefter/CORE_FEATURES_PARITY.md` - Feature parity matrix
- List of features that need reinforcement

**Estimated Effort**: 2-3 hours

---

### Task 5.2: API Endpoint Verification

**Goal**: Ensure all platforms use the same API endpoints correctly.

**Dependencies**: Task 5.1 (Feature Audit) - to identify which endpoints are used

**Technical Steps**:
1. **Extract API Endpoints from Each Platform**:
   - Desktop: `YesiLdefter/Tkn/UstadApiClient.cs`
   - Web: `UstadWeb/ustad-web/apps/ustad-web-yesildefter/src/lib/config/api.ts`
   - Mobile: `UstadWeb/ustad-web/apps/ustad-mobile-shell/services/api.ts`

2. **Create API Endpoint Matrix**:
   - File: `YesiLdefter/API_ENDPOINTS_MATRIX.md`
   - Columns: Endpoint | Method | Desktop | Web | Mobile | Status
   - Document request/response formats
   - Document authentication requirements

3. **Verify Endpoint Consistency**:
   - Check if all platforms use same endpoint URLs
   - Check if request/response formats match
   - Check if authentication is consistent (JWT token usage)
   - Identify missing endpoints in any platform

4. **Test Critical Endpoints**:
   - `/auth/login` - Authentication
   - `/auth/qr-login` - QR authentication
   - `/UstadFirm/user/{userGUID}` - Firm list
   - `/auth/select-firm` - Firm selection
   - `/auth/db-connection-info` - Database connection

5. **Document Discrepancies**:
   - List endpoints that work in one platform but not others
   - List endpoints with different request/response formats
   - Create fix list

**Deliverable**: 
- `YesiLdefter/API_ENDPOINTS_MATRIX.md` - Endpoint consistency matrix
- List of endpoints that need fixing

**Estimated Effort**: 2-3 hours

---

### Task 5.3: Quick Feature Reinforcement

**Goal**: Fix critical gaps and ensure core features work across all platforms.

**Dependencies**: Task 5.1 (Feature Audit), Task 5.2 (API Verification)

**Technical Steps**:
1. **Fix Authentication Issues**:
   - Ensure desktop, web, mobile all use same login flow
   - Verify JWT token handling is consistent
   - Test QR code flow end-to-end
   - Fix any token refresh issues

2. **Fix Firm Selection**:
   - Ensure firm selection works in all platforms
   - Verify firm context is shared correctly
   - Test firm switching

3. **Fix Data Synchronization**:
   - Ensure data created in one platform appears in others
   - Verify API calls are made correctly
   - Test error handling

4. **Add Missing Critical Features**:
   - Identify top 3-5 missing features from parity matrix
   - Implement in priority order
   - Focus on features that can be done quickly (1-2 hours each)

5. **Test Cross-Platform Flow**:
   - Test: Desktop login → QR code → Mobile scan → Mobile access
   - Test: Web login → Firm selection → Data access
   - Test: Mobile login → Firm selection → Data access
   - Document any broken flows

**Deliverable**: 
- Fixed authentication flow
- Fixed firm selection
- Fixed data synchronization
- List of remaining gaps (if any)

**Estimated Effort**: 4-6 hours (depends on number of issues found)

---

## Work Block 6: Performance Monitoring (Can Start Anytime)

### Task 5.1: Logging Infrastructure
**Goal**: Create production-ready logging system.

**Dependencies**: None (can start anytime)

**Technical Steps**:
1. **Choose Logging Framework**:
   - Evaluate: NLog, Serilog, or built-in `System.Diagnostics`
   - Recommendation: NLog (lightweight, flexible)

2. **Create Logging Wrapper**:
   - File: `YesiLdefter/Tkn/tLogger.cs`
   - Methods:
     - `LogInfo(string message, Exception ex = null)`
     - `LogWarning(string message, Exception ex = null)`
     - `LogError(string message, Exception ex = null)`
     - `LogDebug(string message)` - Only in debug builds
   - Support structured logging (key-value pairs)

3. **Configure Logging Targets**:
   - File: `YesiLdefter/NLog.config`
   - Targets:
     - File target (rotating logs)
     - Console target (for debugging)
     - Event Log target (optional, for production)
   - Rules: Different log levels for different targets

4. **Replace Debug.WriteLine**:
   - Find all `System.Diagnostics.Debug.WriteLine` calls
   - Replace with `tLogger.LogInfo/LogError/etc.`
   - Priority: Splash screen, login, API client

**Deliverable**: 
- `YesiLdefter/Tkn/tLogger.cs` - Logging wrapper
- `YesiLdefter/NLog.config` - Logging configuration
- Updated code to use logger instead of Debug.WriteLine

**Estimated Effort**: 2-3 hours

---

### Task 5.2: Performance Monitoring
**Goal**: Add performance metrics and monitoring.

**Dependencies**: Task 5.1 (Logging Infrastructure)

**Technical Steps**:
1. **Create Performance Monitor**:
   - File: `YesiLdefter/Tkn/tPerformanceMonitor.cs`
   - Methods:
     - `StartTimer(string operationName)` - Start timing
     - `StopTimer(string operationName)` - Stop and log duration
     - `RecordMetric(string metricName, double value)` - Record metric
     - `GetMetrics()` - Get all recorded metrics

2. **Add Performance Instrumentation**:
   - Instrument API calls (measure response time)
   - Instrument database operations (if applicable)
   - Instrument UI operations (form load, render)
   - Instrument WebView2 operations (initialization, navigation)

3. **Create Performance Dashboard** (Optional):
   - File: `YesiLdefter/Forms/ms_PerformanceMonitor.cs`
   - Display real-time metrics
   - Show performance history
   - Alert on slow operations

4. **Add Health Checks**:
   - API connectivity check
   - Database connectivity check (if applicable)
   - WebView2 availability check
   - Disk space check

**Deliverable**: 
- `YesiLdefter/Tkn/tPerformanceMonitor.cs`
- Instrumentation in key operations
- Optional performance dashboard

**Estimated Effort**: 3-4 hours

---

## Task Dependency Graph

```
Block 1 (Parallel):
├── Task 1.1: Database Feature Analysis (2-3h)
└── Task 1.2: Design Token System (2-3h)

Block 2 (Depends on 1.2):
└── Task 2.1: DevExpress Skin Generator (3-4h)

Block 3 (Can start after Block 1):
├── Task 3.1: Webhook Design (2-3h)
└── Task 3.2: Webhook Implementation (3-4h) [Depends on 3.1]

Block 4 (Depends on Block 3):
├── Task 4.1: Offline Cache Design (2-3h) [Depends on 3.2]
└── Task 4.2: Offline Implementation (4-5h) [Depends on 4.1]

Block 5 (Parallel with Block 1 - Priority: High):
├── Task 5.1: Cross-Platform Feature Audit (2-3h)
├── Task 5.2: API Endpoint Verification (2-3h) [Depends on 5.1]
└── Task 5.3: Quick Feature Reinforcement (4-6h) [Depends on 5.1, 5.2]

Block 6 (Independent):
├── Task 6.1: Logging Infrastructure (2-3h)
└── Task 6.2: Performance Monitoring (3-4h) [Depends on 6.1]
```

## Recommended Order for Tomorrow's Presentation

### Must Complete Today (Priority: Critical):
1. **Task 5.1: Cross-Platform Feature Audit** (2-3h)
   - **CRITICAL**: Prevents "half-done" application state
   - Identifies what works and what doesn't
   - Foundation for all other work
   - Can be done in parallel with 1.1

2. **Task 1.1: Database Feature Analysis** (2-3h)
   - Critical for presentation - shows what features exist
   - Can be done quickly
   - High impact for demo
   - Can be done in parallel with 5.1

3. **Task 5.2: API Endpoint Verification** (2-3h)
   - **CRITICAL**: Ensures all platforms communicate correctly
   - Depends on 5.1
   - Prevents broken cross-platform flows
   - Can be done in parallel with 1.2

4. **Task 5.3: Quick Feature Reinforcement** (4-6h)
   - **CRITICAL**: Fixes gaps found in 5.1 and 5.2
   - Depends on 5.1 and 5.2
   - Ensures core features work across all platforms
   - Focus on authentication, firm selection, data sync

### Should Complete Today (If Time Permits):
5. **Task 1.2: Design Token System** (2-3h)
   - Foundation for UI improvements
   - Can show design system approach
   - Enables future skin work
   - Can be done in parallel with 5.3

6. **Task 2.1: DevExpress Skin Generator** (3-4h)
   - If time permits after core features are fixed
   - Shows professional UI customization
   - High visual impact
   - Depends on 1.2

### Nice to Have Today:
7. **Task 6.1: Logging Infrastructure** (2-3h)
   - Can be done in parallel with others
   - Production-ready improvement
   - Low risk

### Post-Presentation:
8. **Task 3.1-3.2: Webhook System** (5-7h total)
9. **Task 4.1-4.2: Offline Support** (6-8h total)
10. **Task 6.2: Performance Monitoring** (3-4h)

## Quick Reference: File Locations

### Existing Files to Modify:
- `YesiLdefter/Tkn/tDevExpressTheme.cs` - Add token loading
- `YesiLdefter/main.cs` (line 250) - Update skin application
- `YesiLdefter/Tkn/UstadApiClient.cs` - Add offline support
- `YesiLdefter/Tkn/tApiConfig.cs` - Add webhook config

### New Files to Create:
- `YesiLdefter/Forms/Templates/shared/design-tokens.json`
- `YesiLdefter/Tkn/tDesignTokenLoader.cs`
- `YesiLdefter/Tkn/tDevExpressSkinGenerator.cs`
- `YesiLdefter/Tkn/tWebhookReceiver.cs`
- `YesiLdefter/Tkn/tOfflineCache.cs`
- `YesiLdefter/Tkn/tOperationQueue.cs`
- `YesiLdefter/Tkn/tLogger.cs`
- `YesiLdefter/Tkn/tPerformanceMonitor.cs`
- `YesiLdefter/Scripts/FEATURE_MATRIX.md`
- `YesiLdefter/CORE_FEATURES_PARITY.md` ⭐ **NEW - Priority**
- `YesiLdefter/API_ENDPOINTS_MATRIX.md` ⭐ **NEW - Priority**
- `YesiLdefter/WEBHOOK_ARCHITECTURE.md`
- `YesiLdefter/OFFLINE_ARCHITECTURE.md`
- `YesiLdefter/NLog.config`

## Notes

- **No Time Estimates**: Tasks are linked by dependencies, not time. Work at your own pace.
- **Parallel Work**: Tasks in the same block can be done in parallel if you have multiple developers.
- **Incremental**: Each task produces a deliverable that can be used independently.
- **Presentation Focus**: Prioritize Block 1 tasks for tomorrow's presentation.
- **Production Ready**: Block 5 (logging/monitoring) improves production readiness but isn't required for initial release.

