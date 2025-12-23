# Quick Reference Guide - Sitecore Media Security

## Quick Start

### 1. Deploy Files (5 minutes)
```
IPCoop.Foundation.MediaSecurity.dll → [Sitecore]\bin\
IPCoop.Foundation.MediaSecurity.config → [Sitecore]\App_Config\Include\Foundation\
```

### 2. Install Template (2 minutes)
```powershell
cd src\Foundation\MediaSecurity\serialization
dotnet sitecore ser push
```

### 3. Configure User Profile (3 minutes)
Add these fields to `/sitecore/templates/System/Security/User Profile`:
- HasHawaiiState (Checkbox)
- HasAlaskaState (Checkbox)
- HasRestUSState (Checkbox)
- HasCanadaState (Checkbox)

### 4. Secure a Folder (1 minute)
1. Change folder template to "Secure Media Folder"
2. Set RuleName field to one of: IsHawaiiUser, IsAlaskaUser, IsRestUSUser, IsCanadaUser

---

## Configuration Reference

### Settings (IPCoop.Foundation.MediaSecurity.config)

```xml
<!-- Enable/disable feature -->
<setting name="MediaSecurity.Enabled" value="true" />

<!-- Claim URL base for full-format claims -->
<setting name="MediaSecurity.ClaimUrlBase" value="https://ipcoop.com/claims/" />
```

---

## RuleName → Claim Mapping

| RuleName | Claim Name | User Profile Field |
|----------|------------|-------------------|
| IsHawaiiUser | hasHawaiiState | HasHawaiiState |
| IsAlaskaUser | hasAlaskaState | HasAlaskaState |
| IsRestUSUser | hasRestUSState | HasRestUSState |
| IsCanadaUser | hasCanadaState | HasCanadaState |

---

## Claim Formats Supported

The system checks claims in **three ways** (OR logic):

### 1. Full URL
```
Claim Type: https://ipcoop.com/claims/hasHawaiiState
Claim Value: true
```

### 2. Short Name
```
Claim Type: hasHawaiiState
Claim Value: true
```

### 3. User Profile Property
```
User.Profile.HasHawaiiState = "1" (or "true")
```

---

## HTTP Status Codes

| Status | Condition | User Action |
|--------|-----------|-------------|
| 200 OK | Authorized access | Normal download |
| 401 Unauthorized | Not authenticated | Redirected to login |
| 403 Forbidden | Authenticated but no claim | Access denied |
| 404 Not Found | Media doesn't exist | Normal Sitecore 404 |
| 500 Internal Error | Authorization error | Check logs, contact admin |

---

## Log Format

All log entries start with `[MediaSecurity]`:

```
[MediaSecurity] AUTHORIZED | User: domain\jsmith | MediaPath: /~/media/secure/doc.pdf | RuleName: IsHawaiiUser | MatchedClaim: hasHawaiiState

[MediaSecurity] FORBIDDEN (403) | User: domain\jdoe | MediaPath: /~/media/secure/doc.pdf | RuleName: IsHawaiiUser | UserClaims: [hasAlaskaState] | Reason: User does not have required claim

[MediaSecurity] UNAUTHORIZED (401) | User: Anonymous | MediaPath: /~/media/secure/doc.pdf | RuleName: IsHawaiiUser | UserClaims: [] | Reason: User not authenticated

[MediaSecurity] NO_SECURE_FOLDER | MediaPath: /~/media/public/image.jpg | Action: Normal media processing
```

---

## Common Tasks

### Grant User Access to State

**Option 1: User Profile (Recommended)**
1. Open Sitecore User Manager
2. Edit user → Profile tab
3. Check appropriate state checkbox (e.g., HasHawaiiState)
4. Save

**Option 2: External Identity Provider**
Configure IDP to issue claim:
- Type: `https://ipcoop.com/claims/hasHawaiiState` OR `hasHawaiiState`
- Value: `true`

### Grant Multi-State Access

Check multiple boxes in user profile, OR issue multiple claims from IDP:
- HasHawaiiState ✓
- HasAlaskaState ✓
- User can now access both Hawaii and Alaska folders

### Test Authorization

```powershell
# Test as specific user
1. Log into Sitecore as test user
2. Open browser (not Content Editor)
3. Navigate to: http://yoursite.com/~/media/secured-folder/test.pdf
4. Check response:
   - 401 = Not logged in
   - 403 = Missing claim
   - 200 = Authorized

# Check logs
Look for: [MediaSecurity] entries in Sitecore log
```

### Disable Feature Temporarily

```xml
<!-- In IPCoop.Foundation.MediaSecurity.config -->
<setting name="MediaSecurity.Enabled" value="false" />
```

Restart IIS, all media becomes public.

---

## Troubleshooting Quick Checks

### ❌ All Media Returning 401/403
**Check:**
1. Is `MediaSecurity.Enabled` = true?
2. Did you restart IIS after deployment?
3. Check showconfig.aspx for SecureMediaRequestProcessor

**Fix:**
```powershell
# Verify config
http://yoursite.com/sitecore/admin/showconfig.aspx
# Search for: "SecureMediaRequestProcessor"
# Should appear BEFORE "ItemResolver"
```

### ❌ Authorized User Getting 403
**Check:**
1. Does user have correct claim?
2. Is claim format correct?
3. Check user profile fields

**Debug:**
```
Look in logs for:
[MediaSecurity] CLAIM_CHECK | User: ... | Claim: hasHawaiiState | Status: NOT_FOUND

Check all three sources:
- ClaimsPrincipal
- User Identity
- User Profile
```

### ❌ Media Not Being Cached (Performance Issue)
**Expected Behavior:**
Secured media is NEVER cached (by design). Only non-secured media should be cached.

**Verify:**
```powershell
# Check response headers
curl -I http://yoursite.com/~/media/secured-folder/doc.pdf
# Should see: Cache-Control: no-cache, no-store, must-revalidate
```

### ❌ Normal Media Returning 403
**Check:**
1. Is folder using Secure Media Folder template?
2. Is RuleName field empty?

**Fix:**
- Change template back to standard folder template, OR
- Clear RuleName field

---

## File Locations Reference

```
Project Structure:
c:\Projects\SitecoreMediaRequestHandler\
├── src\Foundation\MediaSecurity\code\
│   ├── IPCoop.Foundation.MediaSecurity.csproj
│   ├── Configuration\
│   │   └── MediaSecurityServicesConfigurator.cs
│   ├── Extensions\
│   │   ├── ClaimsPrincipalExtensions.cs
│   │   └── UserProfileExtensions.cs
│   ├── Logging\
│   │   └── MediaSecurityLogger.cs
│   ├── Models\
│   │   ├── AuthorizationResult.cs
│   │   └── RuleNameType.cs
│   ├── Pipelines\HttpRequestBegin\
│   │   └── SecureMediaRequestProcessor.cs
│   ├── Security\
│   │   ├── Interfaces\
│   │   │   └── IMediaAuthorizationService.cs
│   │   └── Services\
│   │       └── MediaAuthorizationService.cs
│   └── App_Config\Include\Foundation\
│       └── IPCoop.Foundation.MediaSecurity.config
├── serialization\
│   └── Templates\Foundation\Media Security\
│       ├── Secure Media Folder.yml
│       └── ... (other template files)
└── docs\
    ├── README.md (main documentation)
    ├── IMPLEMENTATION.md (technical guide)
    └── QUICK_REFERENCE.md (this file)

Deployment Targets:
[Sitecore Web Root]\bin\
    └── IPCoop.Foundation.MediaSecurity.dll

[Sitecore Web Root]\App_Config\Include\Foundation\
    └── IPCoop.Foundation.MediaSecurity.config

[Sitecore]\sitecore\templates\Foundation\Media Security\
    └── Secure Media Folder (+ fields)
```

---

## Testing Checklist

- [ ] Anonymous user accessing secured media → 401
- [ ] Authenticated user without claim → 403
- [ ] Authenticated user with correct claim → 200
- [ ] User with multiple claims → Access to all matching folders
- [ ] Normal (non-secured) media → 200 for everyone
- [ ] Cache-Control header on secured media → no-cache
- [ ] Log entries appear in Sitecore log
- [ ] Disabling feature → All media accessible

---

## Emergency Procedures

### Disable Module Immediately

**Option 1: Config Setting**
```xml
<setting name="MediaSecurity.Enabled" value="false" />
```

**Option 2: Remove Config File**
```powershell
# Rename or delete
[Sitecore]\App_Config\Include\Foundation\IPCoop.Foundation.MediaSecurity.config
```

**Option 3: Remove DLL**
```powershell
# Rename or delete (requires app pool restart)
[Sitecore]\bin\IPCoop.Foundation.MediaSecurity.dll
```

After any change, restart IIS:
```powershell
iisreset
```

### Rollback Deployment

1. Remove files deployed
2. Restore previous web.config (if modified)
3. Delete Sitecore templates (if problematic)
4. Restart IIS
5. Clear all caches in Sitecore

---

## Performance Metrics

**Expected Overhead per Secured Media Request:**
- IsMediaRequest check: < 1ms
- GetMediaItem: 5-20ms
- FindSecureMediaFolder: 10-50ms
- Authorization check: < 5ms
- **Total: ~20-75ms**

**No Performance Impact:**
- Non-media requests (early exit)
- Non-secured media (early exit after folder check)

---

## Support Contacts

**For questions about:**
- Configuration: See README.md
- Technical details: See IMPLEMENTATION.md
- Code modifications: See IMPLEMENTATION.md → Extensibility Points
- Deployment issues: See README.md → Troubleshooting

---

## Version Information

**Module Version:** 1.0.0  
**Release Date:** December 23, 2025  
**Sitecore Compatibility:** 9.3+, 10.x  
**Framework:** .NET Framework 4.8

---

## Quick Commands

```powershell
# Build solution
cd c:\Projects\SitecoreMediaRequestHandler
dotnet build --configuration Release

# Install templates
cd src\Foundation\MediaSecurity\serialization
dotnet sitecore ser push

# Check Sitecore config
# Browse to: http://yoursite.com/sitecore/admin/showconfig.aspx
# Search for: "SecureMediaRequestProcessor"

# View logs
# Location: [Sitecore]\Data\logs\log.[date].txt
# Search for: "[MediaSecurity]"

# Test media URL
# Format: http://yoursite.com/~/media/[folder]/[filename]
# Or: http://yoursite.com/-/media/[item-id].ashx

# Restart IIS
iisreset

# Recycle app pool only
# IIS Manager → Application Pools → [Your Pool] → Recycle
```

---

## Adding New Rule (Quick Steps)

1. Update `MediaAuthorizationService.cs`:
   ```csharp
   { "IsMexicoUser", "hasMexicoState" }
   ```

2. Update `UserProfileExtensions.cs`:
   ```csharp
   public static bool HasMexicoState(this User user)
   ```

3. Update `MediaAuthorizationService.CheckUserProfileClaim()`:
   ```csharp
   case "hasmexicostate": return user.HasMexicoState();
   ```

4. Update RuleName field source in Sitecore:
   Add `|IsMexicoUser` to dropdown

5. Add user profile field: `HasMexicoState` (Checkbox)

6. Rebuild, redeploy, restart IIS

---

## Important Notes

⚠️ **Cache Bypass:** Secured media is NEVER cached. This is intentional for security.

⚠️ **Template ID:** Update `SecureMediaFolderTemplateId` constant in `SecureMediaRequestProcessor.cs` after template creation for optimal performance.

⚠️ **Claims Format:** System checks 3 formats (full URL, short name, profile). At least one must match.

⚠️ **Multi-State:** Users can have multiple states. OR logic applies (any matching claim grants access).

⚠️ **Empty RuleName:** If RuleName is empty, media is public (no authorization).

⚠️ **Error Handling:** Module fails closed (denies access on error for security).

✅ **Logging:** All authorization attempts are logged with full details for troubleshooting.

---

End of Quick Reference Guide
