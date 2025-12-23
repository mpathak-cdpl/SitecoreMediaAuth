# Deployment Checklist - Sitecore Media Security Module

**Project:** Sitecore Media Request Handler  
**Module:** IPCoop.Foundation.MediaSecurity  
**Version:** 1.0.0  
**Date:** _______________  
**Deployed By:** _______________  
**Environment:** □ DEV  □ UAT  □ PRODUCTION

---

## Pre-Deployment Checklist

### Code Preparation
- [ ] All code changes committed to source control
- [ ] Branch merged to deployment branch (if applicable)
- [ ] Version number updated in assembly info
- [ ] Release notes prepared

### Build & Test
- [ ] Solution builds successfully in Release configuration
- [ ] No compiler warnings or errors
- [ ] Unit tests pass (if available)
- [ ] Integration tests pass in DEV environment
- [ ] Code review completed and approved

### Environment Preparation
- [ ] Target environment identified and accessible
- [ ] Deployment window scheduled (if required)
- [ ] Stakeholders notified of deployment
- [ ] Rollback plan documented
- [ ] Database backup completed
- [ ] File system backup completed

### Documentation Review
- [ ] README.md reviewed and up-to-date
- [ ] IMPLEMENTATION.md reviewed
- [ ] Configuration settings documented
- [ ] Known issues documented

---

## Deployment Steps

### Step 1: Build Solution
**Time Estimate:** 2 minutes

```powershell
cd c:\Projects\SitecoreMediaRequestHandler
dotnet build SitecoreMediaRequestHandler.sln --configuration Release
```

- [ ] Build completed successfully
- [ ] Output DLL exists: `src\Foundation\MediaSecurity\code\bin\Release\IPCoop.Foundation.MediaSecurity.dll`
- [ ] DLL version verified: Right-click → Properties → Details → File version

**Notes:** _________________________________

---

### Step 2: Stop Application (Optional)
**Time Estimate:** 1 minute

⚠️ **For Production:** Consider app pool stop for zero-downtime deployment

```powershell
# Option 1: Stop app pool
Stop-WebAppPool -Name "YourAppPoolName"

# Option 2: App_Offline.htm
Copy-Item app_offline.htm [Sitecore]\App_Offline.htm
```

- [ ] Application stopped (if required)
- [ ] Maintenance page displayed (if applicable)

**Notes:** _________________________________

---

### Step 3: Deploy Assembly
**Time Estimate:** 1 minute

**Source:** `src\Foundation\MediaSecurity\code\bin\Release\IPCoop.Foundation.MediaSecurity.dll`  
**Target:** `[Sitecore Web Root]\bin\IPCoop.Foundation.MediaSecurity.dll`

```powershell
Copy-Item "src\Foundation\MediaSecurity\code\bin\Release\IPCoop.Foundation.MediaSecurity.dll" `
          "[Sitecore Web Root]\bin\" -Force
```

- [ ] DLL copied successfully
- [ ] DLL size matches source: _______ KB
- [ ] DLL timestamp verified: _____________
- [ ] File permissions correct (IIS app pool identity can read)

**Notes:** _________________________________

---

### Step 4: Deploy Configuration
**Time Estimate:** 1 minute

**Source:** `src\Foundation\MediaSecurity\code\App_Config\Include\Foundation\IPCoop.Foundation.MediaSecurity.config`  
**Target:** `[Sitecore Web Root]\App_Config\Include\Foundation\IPCoop.Foundation.MediaSecurity.config`

```powershell
# Create Foundation folder if it doesn't exist
New-Item "[Sitecore Web Root]\App_Config\Include\Foundation" -ItemType Directory -Force

# Copy config file
Copy-Item "src\Foundation\MediaSecurity\code\App_Config\Include\Foundation\IPCoop.Foundation.MediaSecurity.config" `
          "[Sitecore Web Root]\App_Config\Include\Foundation\" -Force
```

- [ ] Config file copied successfully
- [ ] Config folder exists: `App_Config\Include\Foundation\`
- [ ] File permissions correct

**Environment-Specific Configuration:**

- [ ] Verify `MediaSecurity.Enabled` setting: true / false
- [ ] Verify `MediaSecurity.ClaimUrlBase` setting: _______________________

**Notes:** _________________________________

---

### Step 5: Install Sitecore Templates
**Time Estimate:** 3 minutes

**Option A: Using Sitecore CLI (Recommended)**

```powershell
cd src\Foundation\MediaSecurity\serialization

# Initialize Sitecore CLI (if not already done)
dotnet sitecore login --auth [URL] --cm https://yoursite.com

# Push items to Sitecore
dotnet sitecore ser push
```

- [ ] Sitecore CLI authenticated
- [ ] Serialization push completed successfully
- [ ] Template appears in Content Editor: `/sitecore/templates/Foundation/Media Security/Secure Media Folder`

**Option B: Manual Import (Alternative)**

1. Open Sitecore Content Editor
2. Navigate to `/sitecore/templates/Foundation/`
3. Create folder "Media Security"
4. Import YAML files using Developer → Serialization → Import or TDS/Unicorn

- [ ] Templates imported successfully
- [ ] "Secure Media Folder" template visible
- [ ] RuleName field visible with dropdown source

**Template Verification:**

- [ ] Template ID noted for code update: _________________________________
- [ ] RuleName field source: `IsHawaiiUser|IsAlaskaUser|IsRestUSUser|IsCanadaUser`
- [ ] Template inherits from standard folder template

**Notes:** _________________________________

---

### Step 6: Update Template ID in Code (Optional but Recommended)
**Time Estimate:** 2 minutes

**File:** `SecureMediaRequestProcessor.cs` (Line 28)

1. Copy template ID from Sitecore (Step 5)
2. Update constant in code:
   ```csharp
   private const string SecureMediaFolderTemplateId = "{B6A6710E-1C3E-4F3A-9C8D-2E4F5A6B7C8D}";
   ```
3. Uncomment template ID check in `IsSecureMediaFolder()` method (Line 165)
4. Rebuild and redeploy DLL

- [ ] Template ID copied: _________________________________
- [ ] Code updated with actual template ID
- [ ] Solution rebuilt
- [ ] DLL redeployed

⚠️ **Note:** This step can be done later. The code falls back to template name check.

**Notes:** _________________________________

---

### Step 7: Configure User Profile Fields
**Time Estimate:** 5 minutes

**Location:** `/sitecore/templates/System/Security/User Profile`

1. Open template in Content Editor
2. Create new section: "State Access"
3. Add fields:

- [ ] **HasHawaiiState** (Checkbox)
  - Title: "Has Hawaii State"
  - Help: "User has access to Hawaii media"

- [ ] **HasAlaskaState** (Checkbox)
  - Title: "Has Alaska State"
  - Help: "User has access to Alaska media"

- [ ] **HasRestUSState** (Checkbox)
  - Title: "Has Rest of US State"
  - Help: "User has access to Rest of US media"

- [ ] **HasCanadaState** (Checkbox)
  - Title: "Has Canada State"
  - Help: "User has access to Canada media"

- [ ] Standard Values updated (if required)
- [ ] Field names match code exactly (case-sensitive)

**Notes:** _________________________________

---

### Step 8: Restart Application
**Time Estimate:** 2 minutes

```powershell
# Option 1: IIS Reset (full restart)
iisreset

# Option 2: Recycle App Pool (faster)
Restart-WebAppPool -Name "YourAppPoolName"

# Option 3: Remove App_Offline.htm
Remove-Item "[Sitecore Web Root]\App_Offline.htm"
```

- [ ] Application restarted
- [ ] Sitecore loads successfully
- [ ] No errors in Event Viewer
- [ ] Sitecore log checked for startup errors

**Startup Time:** _______ seconds

**Notes:** _________________________________

---

### Step 9: Verify Configuration
**Time Estimate:** 3 minutes

**Check showconfig.aspx:**

1. Browse to: `http://yoursite.com/sitecore/admin/showconfig.aspx`
2. Search for: "SecureMediaRequestProcessor"

- [ ] Processor found in configuration
- [ ] Processor appears BEFORE "ItemResolver"
- [ ] Processor has `resolve="true"` attribute
- [ ] Service reference configured: `mediaSecurity/authorizationService`

**Check Settings:**

- [ ] `MediaSecurity.Enabled` = true (or expected value)
- [ ] `MediaSecurity.ClaimUrlBase` = https://ipcoop.com/claims/ (or expected value)

**Check DI Registration:**

- [ ] Search for: "MediaSecurityServicesConfigurator"
- [ ] Configurator registered under `<services>`

**Notes:** _________________________________

---

## Post-Deployment Testing

### Test 1: Non-Secured Media (Baseline)
**Time Estimate:** 2 minutes

**Test:** Access regular media without Secure Media Folder template

1. Navigate to: `http://yoursite.com/~/media/public-folder/test-image.jpg`
2. Expected Result: **200 OK** for all users

- [ ] Test passed: Media accessible
- [ ] Response time acceptable: _______ ms
- [ ] Cache headers normal (not no-cache)

**Notes:** _________________________________

---

### Test 2: Unauthenticated Access to Secured Media
**Time Estimate:** 2 minutes

**Prerequisites:**
- [ ] Created test folder with Secure Media Folder template
- [ ] Set RuleName to "IsHawaiiUser"
- [ ] Uploaded test media file

**Test:** Access secured media as anonymous user

1. Open browser in incognito/private mode
2. Navigate to: `http://yoursite.com/~/media/secured-folder/test.pdf`
3. Expected Result: **401 Unauthorized**

- [ ] Test passed: 401 returned
- [ ] Response includes WWW-Authenticate header
- [ ] Error message displayed: "Authentication is required"
- [ ] Log entry: `[MediaSecurity] UNAUTHORIZED (401)`

**Notes:** _________________________________

---

### Test 3: Authenticated User Without Claims
**Time Estimate:** 3 minutes

**Prerequisites:**
- [ ] Test user created: _______________
- [ ] User has NO state claims/checkboxes set

**Test:** Access secured media as authenticated user without claims

1. Log in as test user
2. Navigate to: `http://yoursite.com/~/media/secured-folder/test.pdf`
3. Expected Result: **403 Forbidden**

- [ ] Test passed: 403 returned
- [ ] Error message: "You do not have permission"
- [ ] Log entry: `[MediaSecurity] FORBIDDEN (403)`
- [ ] Log shows user claims: `UserClaims: []`

**Notes:** _________________________________

---

### Test 4: Authenticated User With Correct Claim
**Time Estimate:** 3 minutes

**Prerequisites:**
- [ ] Test user profile updated
- [ ] HasHawaiiState checkbox = checked
- [ ] User logged out and back in

**Test:** Access secured media with matching claim

1. Log in as test user (with HasHawaiiState)
2. Navigate to: `http://yoursite.com/~/media/hawaii-folder/test.pdf`
3. Expected Result: **200 OK** with file download

- [ ] Test passed: 200 returned
- [ ] File downloads correctly
- [ ] Cache-Control header: `no-cache, no-store, must-revalidate`
- [ ] Log entry: `[MediaSecurity] AUTHORIZED`
- [ ] Log shows matched claim: `MatchedClaim: hasHawaiiState`

**Notes:** _________________________________

---

### Test 5: Multi-State Access
**Time Estimate:** 3 minutes

**Prerequisites:**
- [ ] Test user has multiple checkboxes: HasHawaiiState + HasAlaskaState
- [ ] Multiple folders exist: hawaii-folder, alaska-folder, canada-folder

**Test:** Verify user can access all granted states

1. Access Hawaii folder: _______________
   - [ ] Expected: 200 OK ✓
   
2. Access Alaska folder: _______________
   - [ ] Expected: 200 OK ✓
   
3. Access Canada folder: _______________
   - [ ] Expected: 403 Forbidden ✓

**Notes:** _________________________________

---

### Test 6: Cache Bypass Verification
**Time Estimate:** 2 minutes

**Test:** Verify secured media is not cached

```powershell
# Check response headers
curl -I http://yoursite.com/~/media/secured-folder/test.pdf
```

Expected Headers:
- [ ] `Cache-Control: no-cache, no-store, must-revalidate, max-age=0`
- [ ] `Pragma: no-cache`
- [ ] `Expires: [past date]`

- [ ] Test passed: All no-cache headers present
- [ ] Log entry: `[MediaSecurity] CACHE_BYPASS`

**Notes:** _________________________________

---

### Test 7: Log Analysis
**Time Estimate:** 3 minutes

**Location:** `[Sitecore]\Data\logs\log.[YYYYMMDD].txt`

Search for: `[MediaSecurity]`

Expected log entries:
- [ ] `CONFIGURATION` entry at startup
- [ ] `SECURE_FOLDER_DETECTED` for secured folders
- [ ] `CACHE_BYPASS` for secured media
- [ ] `CLAIM_CHECK` entries (Found/Not Found)
- [ ] `AUTHORIZED` or `FORBIDDEN` or `UNAUTHORIZED` results
- [ ] No ERROR entries (or errors are expected/documented)

Sample log entries found:
```
[Date Time] INFO  [MediaSecurity] CONFIGURATION | Enabled: True | ClaimUrlBase: https://ipcoop.com/claims/
[Date Time] INFO  [MediaSecurity] SECURE_FOLDER_DETECTED | MediaPath: ... | FolderPath: ... | RuleName: ...
[Date Time] INFO  [MediaSecurity] AUTHORIZED | User: ... | MediaPath: ... | RuleName: ... | MatchedClaim: ...
```

- [ ] Logs look correct
- [ ] No unexpected errors
- [ ] Authorization flow visible in logs

**Notes:** _________________________________

---

### Test 8: Performance Check
**Time Estimate:** 3 minutes

**Test:** Measure response time overhead

1. **Non-secured media (baseline):**
   - URL: _______________
   - Response time: _______ ms

2. **Secured media (authorized user):**
   - URL: _______________
   - Response time: _______ ms
   - Overhead: _______ ms

- [ ] Overhead < 100ms (acceptable)
- [ ] No significant performance degradation
- [ ] Server CPU/memory normal

**Notes:** _________________________________

---

### Test 9: Feature Toggle
**Time Estimate:** 2 minutes

**Test:** Verify feature can be disabled

1. Set `MediaSecurity.Enabled` to `false` in config
2. Restart IIS
3. Access secured media as anonymous user
4. Expected: **200 OK** (feature disabled)

- [ ] Test passed: All media accessible when disabled
- [ ] Log entry: `[MediaSecurity] FEATURE_DISABLED`

5. Re-enable feature: Set to `true`
6. Restart IIS
7. Verify authorization working again

- [ ] Feature re-enabled successfully
- [ ] Authorization enforcement restored

**Notes:** _________________________________

---

## Environment-Specific Verification

### DEV Environment
- [ ] All tests passed
- [ ] Logging level: Debug or Info
- [ ] Test users configured with various claims
- [ ] Sample secured folders created

### UAT Environment
- [ ] All tests passed
- [ ] Logging level: Info
- [ ] Production-like user accounts tested
- [ ] CDN configuration verified (if applicable)
- [ ] Media caching behavior verified
- [ ] Performance acceptable under load

### PRODUCTION Environment
- [ ] All tests passed
- [ ] Logging level: Info or Warning
- [ ] Real user accounts tested
- [ ] CDN configured to respect no-cache headers
- [ ] Monitoring alerts configured
- [ ] Rollback plan ready
- [ ] Support team notified

---

## Post-Deployment Actions

### Documentation
- [ ] Deployment date/time recorded
- [ ] Deployment notes updated in release log
- [ ] Known issues documented (if any)
- [ ] User documentation provided to support team

### Monitoring
- [ ] Set up alerts for excessive 401/403 responses
- [ ] Monitor Sitecore logs for MediaSecurity errors
- [ ] Monitor application performance metrics
- [ ] CDN cache hit ratio (if applicable)

### User Communication
- [ ] Stakeholders notified of successful deployment
- [ ] Support team trained on new feature
- [ ] User documentation published (if required)
- [ ] FAQ updated (if applicable)

### Cleanup
- [ ] Remove test folders/media (if temporary)
- [ ] Remove test user accounts (if temporary)
- [ ] Archive deployment artifacts
- [ ] Update version tracking system

---

## Rollback Procedure (If Required)

### Immediate Rollback (Emergency)
**Time Estimate:** 2 minutes

1. Disable feature:
   ```xml
   <setting name="MediaSecurity.Enabled" value="false" />
   ```
2. Restart IIS
   ```powershell
   iisreset
   ```

- [ ] Feature disabled
- [ ] All media accessible
- [ ] Incident reported

### Full Rollback
**Time Estimate:** 5 minutes

1. Stop application
2. Remove DLL: `[Sitecore]\bin\IPCoop.Foundation.MediaSecurity.dll`
3. Remove config: `[Sitecore]\App_Config\Include\Foundation\IPCoop.Foundation.MediaSecurity.config`
4. Restart application
5. Verify media access restored

- [ ] Files removed
- [ ] Application restarted
- [ ] Functionality restored
- [ ] Root cause identified: _______________

### Database Rollback (If Templates Installed)

1. Delete templates: `/sitecore/templates/Foundation/Media Security/`
2. Restore user profile template (if modified)
3. Remove user profile field values

- [ ] Templates removed
- [ ] Database restored to pre-deployment state

---

## Sign-Off

### Deployment Team
- **Deployed By:** _______________________ Date: _______
- **Verified By:** _______________________ Date: _______

### Acceptance
- **Business Owner:** _______________________ Date: _______
- **Technical Lead:** _______________________ Date: _______

### Notes / Issues Encountered

_____________________________________________________________________________

_____________________________________________________________________________

_____________________________________________________________________________

_____________________________________________________________________________

---

## Appendix: Contact Information

**Development Team:**
- Lead Developer: _______________
- Email: _______________
- Phone: _______________

**Support Team:**
- Support Lead: _______________
- Email: _______________
- Phone: _______________

**Emergency Contacts:**
- On-Call: _______________
- Escalation: _______________

---

**End of Deployment Checklist**
