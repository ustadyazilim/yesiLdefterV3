# 🧪 AUTHENTICATION TESTING GUIDE

## Overview
This document provides step-by-step testing procedures for the refactored authentication system.

---

## 🎯 **TEST EXECUTION ORDER**

Execute tests in this order to ensure proper validation:

1. **Smoke Tests** (5 minutes) - Verify basic functionality
2. **Security Tests** (10 minutes) - Verify no pre-auth DB access
3. **Functional Tests** (15 minutes) - Test all user flows
4. **Error Handling Tests** (10 minutes) - Test failure scenarios
5. **Integration Tests** (10 minutes) - Test full workflow

**Total Time:** ~50 minutes

---

## 🚨 **PRE-TESTING CHECKLIST**

Before starting tests, verify:

- [ ] Ustad.API is running (http://localhost:5000)
- [ ] Database is accessible
- [ ] Test user credentials available
- [ ] Fresh registry (or backup current registry)
- [ ] Network connectivity working
- [ ] Windows Event Viewer open for error monitoring

---

## 1️⃣ **SMOKE TESTS** (Critical - Must Pass)

### Test 1.1: Application Launches
**Purpose:** Verify application starts without errors

```
STEPS:
1. Launch YesiLdefter.exe
2. Observe application startup

EXPECTED:
✅ Application launches
✅ No error messages
✅ Login form appears
✅ No database connection attempts yet

ACTUAL:
[ ] Pass
[ ] Fail - Reason: _________________
```

### Test 1.2: Login Form Renders
**Purpose:** Verify standalone form creates UI correctly

```
STEPS:
1. Launch application
2. Observe login form

EXPECTED:
✅ Login form visible
✅ Email field present
✅ Password field present
✅ Login button present
✅ Forgot password button present
✅ Remember me checkbox present

ACTUAL:
[ ] Pass
[ ] Fail - Reason: _________________
```

### Test 1.3: Basic Login Works
**Purpose:** Verify API authentication functions

```
STEPS:
1. Launch application
2. Enter valid email: test@example.com
3. Enter valid password: TestPassword123
4. Click "Giriş Yap"

EXPECTED:
✅ Status shows "Giriş yapılıyor..."
✅ Status shows "Firma bilgileri alınıyor..."
✅ Login successful
✅ Main application launches
✅ Database connections established AFTER login

ACTUAL:
[ ] Pass
[ ] Fail - Reason: _________________
```

---

## 2️⃣ **SECURITY TESTS** (Critical - Must Pass)

### Test 2.1: No Pre-Auth Database Connection
**Purpose:** Verify database is NOT accessed before authentication

```
SETUP:
1. Install SQL Server Profiler or use Process Monitor
2. Configure to monitor YesiLdefter.exe

STEPS:
1. Launch application
2. Wait for login form to appear
3. DO NOT login yet
4. Check monitoring tool for database connections

EXPECTED:
✅ NO SQL connections to ManagerDB before login
✅ NO SQL connections to UstadCrmDB before login
✅ Login form rendered without database
✅ No error messages about database

ACTUAL:
[ ] Pass
[ ] Fail - Reason: _________________

EVIDENCE:
[ ] Screenshot of monitoring tool attached
[ ] SQL Profiler trace attached
```

### Test 2.2: No Hardcoded Password Fallback
**Purpose:** Verify application fails securely without environment variable

```
SETUP:
1. Backup current registry: HKEY_CURRENT_USER\Software\Üstad\YesiLdefter
2. Set localDbUses = true (to test local mode)
3. REMOVE environment variable: USTAD_MANAGER_DB_PASS

STEPS:
1. Launch application
2. Observe behavior

EXPECTED:
✅ Application throws exception
✅ Error message: "USTAD_MANAGER_DB_PASS environment variable is required..."
✅ Application does NOT use hardcoded password "ustad84352Yazilim"
✅ Clear error message shown to user

ACTUAL:
[ ] Pass
[ ] Fail - Reason: _________________

CLEANUP:
1. Restore registry from backup
2. Set localDbUses = false
```

### Test 2.3: JWT Token Security
**Purpose:** Verify JWT tokens are properly handled

```
STEPS:
1. Launch application
2. Login with valid credentials
3. Check v.tUser.JwtToken value (use debugger)
4. Verify token is used for subsequent API calls

EXPECTED:
✅ JWT token received from API
✅ Token stored in v.tUser.JwtToken
✅ Token used in Authorization header for /auth/db-connection-info
✅ Token not visible in UI
✅ Token not logged in plain text

ACTUAL:
[ ] Pass
[ ] Fail - Reason: _________________
```

---

## 3️⃣ **FUNCTIONAL TESTS** (Important)

### Test 3.1: Valid Login - Single Firm
**Purpose:** Test happy path with single firm user

```
SETUP:
Test user with ONE firm assigned

STEPS:
1. Launch application
2. Enter email: singlefirm@test.com
3. Enter password: ValidPassword123
4. Click "Giriş Yap"

EXPECTED:
✅ Status: "Giriş yapılıyor..."
✅ Status: "Firma bilgileri alınıyor..."
✅ Status: "Firma seçiliyor..."
✅ Firm auto-selected (no firm selection dialog)
✅ Database connections established
✅ Main application launches
✅ Firm info populated correctly

ACTUAL:
[ ] Pass
[ ] Fail - Reason: _________________
```

### Test 3.2: Valid Login - Multiple Firms
**Purpose:** Test firm selection for multi-firm users

```
SETUP:
Test user with MULTIPLE firms assigned

STEPS:
1. Launch application
2. Enter email: multifirm@test.com
3. Enter password: ValidPassword123
4. Click "Giriş Yap"

EXPECTED:
✅ Status: "Giriş yapılıyor..."
✅ Status: "Firma bilgileri alınıyor..."
✅ Multiple firms returned
✅ [Current: Auto-selects first firm]
✅ [Future: Firm selection dialog]
✅ Database connections established
✅ Main application launches

ACTUAL:
[ ] Pass
[ ] Fail - Reason: _________________

NOTE: Multi-firm selection dialog not yet implemented in standalone form
```

### Test 3.3: Invalid Password
**Purpose:** Test error handling for wrong password

```
STEPS:
1. Launch application
2. Enter valid email: test@example.com
3. Enter WRONG password: WrongPassword
4. Click "Giriş Yap"

EXPECTED:
✅ Status: "Giriş yapılıyor..."
✅ Error shown: "E-posta veya şifre hatalı"
✅ Login form remains visible
✅ User can retry
✅ No database connections established

ACTUAL:
[ ] Pass
[ ] Fail - Reason: _________________
```

### Test 3.4: User Not Found
**Purpose:** Test error handling for non-existent user

```
STEPS:
1. Launch application
2. Enter non-existent email: nonexistent@test.com
3. Enter any password: SomePassword
4. Click "Giriş Yap"

EXPECTED:
✅ Status: "Giriş yapılıyor..."
✅ Error shown: "E-posta veya şifre hatalı" OR "Kullanıcı bulunamadı"
✅ Login form remains visible
✅ User can retry
✅ No database connections established

ACTUAL:
[ ] Pass
[ ] Fail - Reason: _________________
```

### Test 3.5: Remember Me - Enabled
**Purpose:** Test remember me functionality

```
STEPS:
1. Launch application
2. Enter email: test@example.com
3. Enter password: TestPassword123
4. CHECK "Beni Hatırla" checkbox
5. Click "Giriş Yap"
6. Complete login
7. Close application
8. Re-launch application

EXPECTED:
✅ Email field pre-populated
✅ Password field pre-populated
✅ Remember me checkbox checked
✅ Email added to history dropdown

ACTUAL:
[ ] Pass
[ ] Fail - Reason: _________________
```

### Test 3.6: Remember Me - Disabled
**Purpose:** Test remember me OFF functionality

```
STEPS:
1. Launch application
2. Enter email: test@example.com
3. Enter password: TestPassword123
4. UNCHECK "Beni Hatırla" checkbox
5. Click "Giriş Yap"
6. Complete login
7. Close application
8. Re-launch application

EXPECTED:
✅ Email field pre-populated (from history)
✅ Password field EMPTY
✅ Remember me checkbox unchecked

ACTUAL:
[ ] Pass
[ ] Fail - Reason: _________________
```

### Test 3.7: Forgot Password
**Purpose:** Test password reset flow

```
STEPS:
1. Launch application
2. Enter email: test@example.com
3. Click "Şifremi Unuttum"
4. Observe behavior

EXPECTED:
✅ Status: "Şifre sıfırlama talebi gönderiliyor..."
✅ API call to /auth/resetPasswordRequest
✅ Success message: "Şifre sıfırlama talebi gönderildi..."
✅ User instructed to check email
✅ Login form remains visible

ACTUAL:
[ ] Pass
[ ] Fail - Reason: _________________
```

---

## 4️⃣ **ERROR HANDLING TESTS** (Important)

### Test 4.1: API Not Running
**Purpose:** Test behavior when Ustad.API is down

```
SETUP:
1. Stop Ustad.API service
2. Verify http://localhost:5000 not responding

STEPS:
1. Launch application
2. Enter valid credentials
3. Click "Giriş Yap"

EXPECTED:
✅ Status: "Giriş yapılıyor..."
✅ Retry attempts (up to 3 times)
✅ Error shown: "API bağlantısı kurulamadı"
✅ Clear instructions to user
✅ Application remains stable
✅ No crash

ACTUAL:
[ ] Pass
[ ] Fail - Reason: _________________

CLEANUP:
Start Ustad.API service
```

### Test 4.2: Network Timeout
**Purpose:** Test timeout handling

```
SETUP:
1. Configure firewall to drop packets (or use network simulator)
2. Create 5-second delay for API responses

STEPS:
1. Launch application
2. Enter valid credentials
3. Click "Giriş Yap"

EXPECTED:
✅ Status: "Giriş yapılıyor..."
✅ Request times out after ~30 seconds
✅ Error shown: timeout message
✅ User can retry
✅ Application remains stable

ACTUAL:
[ ] Pass
[ ] Fail - Reason: _________________
```

### Test 4.3: Invalid API Response
**Purpose:** Test handling of malformed API responses

```
SETUP:
Mock API to return invalid JSON or HTTP 500

STEPS:
1. Launch application
2. Enter valid credentials
3. Click "Giriş Yap"

EXPECTED:
✅ Error caught gracefully
✅ Clear error message shown
✅ Application remains stable
✅ No crash or freeze

ACTUAL:
[ ] Pass
[ ] Fail - Reason: _________________
```

### Test 4.4: Database Connection Failure (Post-Auth)
**Purpose:** Test DB connection issues after successful auth

```
SETUP:
1. Stop SQL Server or block port 1433
2. Keep API running

STEPS:
1. Launch application
2. Login successfully (API works)
3. Observe database connection attempt

EXPECTED:
✅ Login succeeds (API works)
✅ Encrypted connection strings retrieved
✅ Database connection fails with clear error
✅ User informed about database issue
✅ Application handles gracefully

ACTUAL:
[ ] Pass
[ ] Fail - Reason: _________________
```

---

## 5️⃣ **INTEGRATION TESTS** (Full Workflow)

### Test 5.1: Complete Workflow - First Time User
**Purpose:** Test full first-time user experience

```
SETUP:
1. Clean install (no registry)
2. Fresh user account

STEPS:
1. Launch application
2. Enter email (new to this machine)
3. Enter password
4. Do NOT check "Remember Me"
5. Click "Giriş Yap"
6. Complete firm selection if prompted
7. Observe main application launch
8. Use application briefly
9. Close application
10. Re-launch application

EXPECTED:
✅ First launch: Empty email dropdown
✅ Login successful
✅ Database connections established post-auth
✅ Main application launches
✅ Application functions normally
✅ Second launch: Email in dropdown history
✅ Password field empty (remember me was off)

ACTUAL:
[ ] Pass
[ ] Fail - Reason: _________________
```

### Test 5.2: Complete Workflow - Returning User
**Purpose:** Test returning user experience

```
SETUP:
User who has logged in before (registry exists)

STEPS:
1. Launch application
2. Observe pre-populated fields
3. Click "Giriş Yap"
4. Observe login process

EXPECTED:
✅ Email pre-populated from history
✅ Password pre-populated if "Remember Me" was checked
✅ Login successful
✅ Last firm auto-selected
✅ Main application launches
✅ User preferences retained

ACTUAL:
[ ] Pass
[ ] Fail - Reason: _________________
```

### Test 5.3: Complete Workflow - Different Firms
**Purpose:** Test switching between firms

```
SETUP:
Multi-firm user

STEPS:
1. Launch application
2. Login
3. Select Firm A
4. Use application
5. Close application
6. Re-launch application
7. Login
8. Select Firm B
9. Verify correct firm data

EXPECTED:
✅ Firm A selected in first session
✅ Correct database for Firm A
✅ Second launch: Firm A remembered
✅ Can select Firm B instead
✅ Correct database for Firm B
✅ Data segregated by firm

ACTUAL:
[ ] Pass
[ ] Fail - Reason: _________________
```

---

## 📊 **TEST RESULTS SUMMARY**

### Critical Tests (Must Pass)
- [ ] 1.1 Application Launches
- [ ] 1.2 Login Form Renders
- [ ] 1.3 Basic Login Works
- [ ] 2.1 No Pre-Auth Database Connection ⭐
- [ ] 2.2 No Hardcoded Password Fallback ⭐
- [ ] 2.3 JWT Token Security

### Important Tests
- [ ] 3.1 Valid Login - Single Firm
- [ ] 3.2 Valid Login - Multiple Firms
- [ ] 3.3 Invalid Password
- [ ] 3.4 User Not Found
- [ ] 3.5 Remember Me - Enabled
- [ ] 3.6 Remember Me - Disabled
- [ ] 3.7 Forgot Password

### Error Handling Tests
- [ ] 4.1 API Not Running
- [ ] 4.2 Network Timeout
- [ ] 4.3 Invalid API Response
- [ ] 4.4 Database Connection Failure

### Integration Tests
- [ ] 5.1 Complete Workflow - First Time User
- [ ] 5.2 Complete Workflow - Returning User
- [ ] 5.3 Complete Workflow - Different Firms

---

## ✅ **SIGN-OFF**

### Test Execution
- **Tester Name:** _________________
- **Date:** _________________
- **Environment:** _________________

### Results
- **Critical Tests:** ____ / 6 Passed
- **Important Tests:** ____ / 7 Passed
- **Error Handling:** ____ / 4 Passed
- **Integration:** ____ / 3 Passed

**Total:** ____ / 20 Passed

### Recommendation
- [ ] ✅ **PASS** - All critical tests passed, ready for production
- [ ] ⚠️ **CONDITIONAL PASS** - Minor issues, document them
- [ ] ❌ **FAIL** - Critical issues found, do NOT deploy

### Issues Found
```
Issue #1: _________________
Severity: [Critical/High/Medium/Low]
Steps to Reproduce: _________________

Issue #2: _________________
Severity: [Critical/High/Medium/Low]
Steps to Reproduce: _________________
```

### Notes
```
_________________
_________________
```

---

**Next Step:** If all tests pass, proceed to deployment

