# Sitecore Media Security Module

**Version:** 1.0.0  
**Date:** December 23, 2025

## Overview

The Sitecore Media Security Module provides claims-based authorization for media library items, enabling you to restrict access to specific media files based on user claims. This module solves the common challenge of securing media in Sitecore while maintaining proper functionality across all environments, including those with aggressive media caching (UAT, Production).

### Key Features

✅ **Claims-Based Authorization** - Validates user access using standard claims from identity providers  
✅ **Multi-Claim Support** - Users can have multiple state claims (OR logic)  
✅ **Three-Way Claim Checking** - Validates claims via full URL, short name, and user profile properties  
✅ **Cache Bypass** - Automatically bypasses media cache for secured items  
✅ **Proper HTTP Status Codes** - Returns 401 for unauthenticated users, 403 for unauthorized access  
✅ **Comprehensive Logging** - Detailed logging for troubleshooting across environments  
✅ **Selective Security** - Only folders with the Secure Media Folder template are protected  
✅ **Production Ready** - Works in DEV, UAT, and Production with media caching enabled

---

## Architecture

### Components

```
IPCoop.Foundation.MediaSecurity/
├── Configuration/
│   └── MediaSecurityServicesConfigurator.cs    # DI registration
├── Extensions/
│   ├── ClaimsPrincipalExtensions.cs            # Claims extraction helpers
│   └── UserProfileExtensions.cs                # User profile property accessors
├── Logging/
│   └── MediaSecurityLogger.cs                  # Structured logging
├── Models/
│   ├── AuthorizationResult.cs                  # Authorization result model
│   └── RuleNameType.cs                         # Rule name enumeration
├── Pipelines/
│   └── HttpRequestBegin/
│       └── SecureMediaRequestProcessor.cs      # Main pipeline processor
├── Security/
│   ├── Interfaces/
│   │   └── IMediaAuthorizationService.cs       # Service interface
│   └── Services/
│       └── MediaAuthorizationService.cs        # Authorization implementation
└── App_Config/Include/Foundation/
    └── IPCoop.Foundation.MediaSecurity.config         # Sitecore configuration
```

### Authorization Flow

```
┌─────────────────────────────────────────────────────────────┐
│ 1. User requests media file: /~/media/secure-folder/file.pdf │
└─────────────────────┬───────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────┐
│ 2. SecureMediaRequestProcessor (httpRequestBegin pipeline)   │
│    - Checks if request is for media                          │
│    - Resolves media item from Sitecore                       │
└─────────────────────┬───────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────┐
│ 3. Find Secure Media Folder in item ancestry                 │
│    - Traverses parent folders                                │
│    - Checks for Secure Media Folder template                 │
│    - Gets RuleName field value                               │
└─────────────────────┬───────────────────────────────────────┘
                      │
         ┌────────────┴────────────┐
         │                         │
┌────────▼──────────┐    ┌────────▼─────────────────────────┐
│ No secure folder  │    │ Secure folder with RuleName found │
│ or RuleName empty │    │ (e.g., "IsHawaiiUser")            │
└────────┬──────────┘    └────────┬─────────────────────────┘
         │                         │
┌────────▼──────────┐    ┌────────▼─────────────────────────┐
│ Allow normal      │    │ 4. Bypass media cache             │
│ media processing  │    │    - Set no-cache headers         │
└───────────────────┘    └────────┬─────────────────────────┘
                                   │
                         ┌─────────▼─────────────────────────┐
                         │ 5. MediaAuthorizationService       │
                         │    - Check user authentication     │
                         │    - Map RuleName to claim name    │
                         │    - Validate claim (3 methods)    │
                         └────────┬──────────────────────────┘
                                  │
                    ┌─────────────┴─────────────┐
                    │                           │
         ┌──────────▼─────────┐      ┌─────────▼──────────┐
         │ User authenticated │      │ User NOT           │
         │ with valid claim   │      │ authenticated      │
         └──────────┬─────────┘      └─────────┬──────────┘
                    │                           │
         ┌──────────▼─────────┐      ┌─────────▼──────────┐
         │ 6a. Allow access   │      │ 6b. Return 401     │
         │     (200 OK)       │      │     Unauthorized   │
         │     Log success    │      │     Prompt login   │
         └────────────────────┘      └────────────────────┘
                    
         ┌────────────────────────────────────────┐
         │ User authenticated but no valid claim  │
         └────────────────┬───────────────────────┘
                          │
                ┌─────────▼──────────┐
                │ 6c. Return 403     │
                │     Forbidden      │
                │     Access denied  │
                └────────────────────┘
```

---

## Installation

### Prerequisites

- Sitecore 9.3+ or Sitecore 10.x
- .NET Framework 4.8 or higher
- Visual Studio 2019 or later
- Access to Sitecore instance web root

### Step 1: Build the Project

```powershell
# Navigate to solution directory
cd c:\Projects\SitecoreMediaRequestHandler

# Restore NuGet packages and build
dotnet build SitecoreMediaRequestHandler.sln --configuration Release
```

### Step 2: Deploy to Sitecore

1. **Copy assemblies to Sitecore bin folder:**
   ```
   src\Foundation\MediaSecurity\code\bin\Release\IPCoop.Foundation.MediaSecurity.dll
   → [Sitecore Web Root]\bin\
   ```

2. **Copy configuration file:**
   ```
   src\Foundation\MediaSecurity\code\App_Config\Include\Foundation\IPCoop.Foundation.MediaSecurity.config
   → [Sitecore Web Root]\App_Config\Include\Foundation\
   ```

### Step 3: Install Sitecore Templates

1. **Using Sitecore CLI (Recommended):**
   ```powershell
   # Navigate to serialization folder
   cd src\Foundation\MediaSecurity\serialization
   
   # Push items to Sitecore
   dotnet sitecore ser push
   ```

2. **Manual Installation (Alternative):**
   - Open Sitecore Content Editor
   - Navigate to `/sitecore/templates/Foundation/`
   - Create folder "Media Security"
   - Import the template YML files using Sitecore serialization tools

### Step 4: Configure User Profile Fields

Add custom fields to your user profile template in Sitecore:

1. Navigate to: `/sitecore/templates/System/Security/User Profile`
2. Add the following fields to a new section called "State Access":
   - **HasHawaiiState** (Checkbox)
   - **HasAlaskaState** (Checkbox)
   - **HasRestUSState** (Checkbox)
   - **HasCanadaState** (Checkbox)

### Step 5: Update Template ID in Code (If Using Template ID Check)

In `SecureMediaRequestProcessor.cs`, update the constant if you want to use template ID checking:

```csharp
// Line 28 - Replace with your actual template ID
private const string SecureMediaFolderTemplateId = "{B6A6710E-1C3E-4F3A-9C8D-2E4F5A6B7C8D}";
```

To get your template ID:
1. Open Sitecore Content Editor
2. Navigate to: `/sitecore/templates/Foundation/Media Security/Secure Media Folder`
3. Copy the Item ID from the Quick Info section

### Step 6: Restart Sitecore

- Recycle the application pool or perform an IIS reset
- Monitor Sitecore logs for any startup errors

---

## Configuration

### Settings (IPCoop.Foundation.MediaSecurity.config)

| Setting | Default | Description |
|---------|---------|-------------|
| `MediaSecurity.Enabled` | `true` | Master toggle for the feature. Set to `false` to disable all authorization checks. |
| `MediaSecurity.ClaimUrlBase` | `https://ipcoop.com/claims/` | Base URL for full-form claims. Used to construct full claim URLs. |

### RuleName to Claim Mapping

| RuleName | Required Claim | User Profile Property |
|----------|----------------|----------------------|
| `IsHawaiiUser` | `hasHawaiiState` | `HasHawaiiState` |
| `IsAlaskaUser` | `hasAlaskaState` | `HasAlaskaState` |
| `IsRestUSUser` | `hasRestUSState` | `HasRestUSState` |
| `IsCanadaUser` | `hasCanadaState` | `HasCanadaState` |

---

## Usage

### Securing a Media Folder

1. **Create or locate the media folder** you want to secure in the Media Library

2. **Change the folder template:**
   - Right-click the folder → **Tasks** → **Change Template**
   - Select: `/sitecore/templates/Foundation/Media Security/Secure Media Folder`

3. **Set the RuleName field:**
   - Open the folder item in Content Editor
   - Navigate to the **Security** section
   - Select the appropriate RuleName from the dropdown:
     - `IsHawaiiUser`
     - `IsAlaskaUser`
     - `IsRestUSUser`
     - `IsCanadaUser`
   - Save the item

4. **Test access:**
   - Log out of Sitecore
   - Navigate to a media file URL in the secured folder
   - You should receive a 401 Unauthorized response

### Setting Up User Claims

**Option 1: Using User Profile (Simplest)**

1. Open Sitecore User Manager
2. Edit the user
3. In the Profile tab, set the appropriate state checkboxes:
   - Check `HasHawaiiState` for Hawaii access
   - Check `HasAlaskaState` for Alaska access
   - etc.

**Option 2: Using External Identity Provider**

Configure your identity provider (ADFS, Azure AD, Okta, etc.) to issue claims with one of these formats:

- **Full URL:** `https://ipcoop.com/claims/hasHawaiiState`
- **Short name:** `hasHawaiiState`

### Multi-State Access

Users can have access to multiple states simultaneously:

- Set multiple checkboxes in user profile, OR
- Issue multiple claims from your identity provider

**Example:** A user with both `hasHawaiiState` and `hasAlaskaState` claims can access media in folders with either `IsHawaiiUser` or `IsAlaskaUser` RuleName.

---

## Testing

### Test Scenarios

#### 1. Test Unauthenticated Access (Should Return 401)

```powershell
# Open browser in incognito mode
# Navigate to: http://yoursite.com/~/media/secured-folder/test.pdf
# Expected: 401 Unauthorized with message prompting login
```

#### 2. Test Authenticated User Without Claims (Should Return 403)

```powershell
# Log in as a user without any state claims
# Navigate to: http://yoursite.com/~/media/secured-folder/test.pdf
# Expected: 403 Forbidden with access denied message
```

#### 3. Test Authenticated User With Correct Claim (Should Return 200)

```powershell
# Log in as a user with hasHawaiiState claim
# Navigate to: http://yoursite.com/~/media/hawaii-folder/test.pdf
# (hawaii-folder has RuleName = "IsHawaiiUser")
# Expected: 200 OK with file content
```

#### 4. Test Multi-State Access

```powershell
# Log in as a user with both hasHawaiiState and hasAlaskaState claims
# Navigate to both folders:
#   - http://yoursite.com/~/media/hawaii-folder/test.pdf (Should work)
#   - http://yoursite.com/~/media/alaska-folder/test.pdf (Should work)
```

#### 5. Test Non-Secured Media (Should Return 200 for Everyone)

```powershell
# Navigate to media in a folder WITHOUT Secure Media Folder template
# Expected: 200 OK for all users (authenticated or not)
```

#### 6. Test Cache Bypass

```powershell
# Access secured media as authorized user
# Check response headers - should include:
#   Cache-Control: no-cache, no-store, must-revalidate, max-age=0
#   Pragma: no-cache
```

### Log Analysis

Check Sitecore logs for detailed authorization information:

```
[MediaSecurity] SECURE_FOLDER_DETECTED | MediaPath: /~/media/hawaii-docs/confidential.pdf | FolderPath: /sitecore/media library/hawaii-docs | RuleName: IsHawaiiUser

[MediaSecurity] CACHE_BYPASS | MediaPath: /~/media/hawaii-docs/confidential.pdf | Reason: Secured media always bypasses cache

[MediaSecurity] CLAIM_CHECK | User: domain\jsmith | Claim: hasHawaiiState | Status: FOUND | Source: ClaimsPrincipal

[MediaSecurity] AUTHORIZED | User: domain\jsmith | MediaPath: /~/media/hawaii-docs/confidential.pdf | RuleName: IsHawaiiUser | MatchedClaim: hasHawaiiState
```

---

## Troubleshooting

### Issue: Authorization not working in UAT/Production (but works in DEV)

**Symptoms:** Media files are accessible to all users despite having RuleName set

**Possible Causes:**
1. Media is being served from CDN or reverse proxy cache
2. Pipeline processor not registered correctly
3. Configuration file not deployed

**Solutions:**
1. Verify the processor is registered:
   ```
   Navigate to: http://yoursite.com/sitecore/admin/showconfig.aspx
   Search for: "SecureMediaRequestProcessor"
   Confirm it appears BEFORE ItemResolver
   ```

2. Check if cache headers are being sent:
   ```powershell
   curl -I http://yoursite.com/~/media/secured-folder/test.pdf
   # Look for: Cache-Control: no-cache
   ```

3. Clear all caches:
   - Sitecore Admin: Clear all caches
   - CDN: Purge CDN cache for media paths
   - IIS: Restart application pool

### Issue: Users with claims still getting 403 Forbidden

**Symptoms:** User has correct claim but cannot access media

**Possible Causes:**
1. Claim format mismatch
2. User profile properties not set
3. Claim URL base mismatch

**Solutions:**
1. Enable debug logging and check claim detection:
   ```
   Look for log entries: [MediaSecurity] CLAIM_CHECK
   Verify which sources were checked (ClaimsPrincipal, User Identity, User Profile)
   ```

2. Verify claim format matches one of these:
   - Full URL: `https://ipcoop.com/claims/hasHawaiiState`
   - Short name: `hasHawaiiState`
   - User profile: CheckboxField with value "1" or "true"

3. Check ClaimUrlBase setting matches your identity provider:
   ```xml
   <setting name="MediaSecurity.ClaimUrlBase" value="https://ipcoop.com/claims/" />
   ```

### Issue: All users getting 401 Unauthorized (including authenticated users)

**Symptoms:** Even logged-in users receive 401 response

**Possible Causes:**
1. Sitecore.Context.User not properly set
2. Authentication cookie not being sent
3. Pipeline running too early

**Solutions:**
1. Verify the processor runs after UserResolver:
   ```xml
   <!-- Should be BEFORE ItemResolver but AFTER UserResolver -->
   <processor type="IPCoop.Foundation.MediaSecurity.Pipelines.HttpRequestBegin.SecureMediaRequestProcessor, IPCoop.Foundation.MediaSecurity"
              patch:before="processor[@type='Sitecore.Pipelines.HttpRequest.ItemResolver, Sitecore.Kernel']" />
   ```

2. Check Sitecore.Context.User in logs:
   ```
   Look for: User: extranet\username or User: sitecore\username
   If showing: User: Anonymous → Authentication issue
   ```

### Issue: Normal media (non-secured folders) returning 403

**Symptoms:** Media in regular folders are being blocked

**Possible Causes:**
1. Template inheritance issue
2. RuleName field exists on standard folder template
3. Code logic error

**Solutions:**
1. Verify template check logic in `IsSecureMediaFolder()` method
2. Ensure only folders with explicit Secure Media Folder template are checked
3. Review logs for `NO_SECURE_FOLDER` entries on normal media

### Issue: Performance degradation after installation

**Symptoms:** Slow media delivery, high CPU usage

**Possible Causes:**
1. Processor running for all requests (not just media)
2. Excessive Sitecore item queries
3. Missing caching

**Solutions:**
1. Verify early exit for non-media requests:
   ```csharp
   // This check should happen first
   if (!IsMediaRequest(args)) { return; }
   ```

2. Consider adding in-memory caching for folder security metadata (future enhancement)

3. Monitor logs for excessive processing:
   ```
   Too many: [MediaSecurity] SECURE_FOLDER_DETECTED entries
   Review folder traversal logic
   ```

### Common Log Messages and Meanings

| Log Message | Meaning | Action Required |
|-------------|---------|-----------------|
| `FEATURE_DISABLED` | MediaSecurity.Enabled = false | Enable in config if needed |
| `NO_SECURE_FOLDER` | No secure folder found, normal flow | Normal - no action needed |
| `SECURE_FOLDER_DETECTED` | Secure folder found, authorization will run | Normal - indicates working correctly |
| `CACHE_BYPASS` | Cache headers set for secured media | Normal - indicates cache bypass working |
| `AUTHORIZED` | User granted access | Normal - successful authorization |
| `UNAUTHORIZED (401)` | User not authenticated | Expected for anonymous users |
| `FORBIDDEN (403)` | User authenticated but lacks claims | Check user claims configuration |
| `CLAIM_CHECK ... FOUND` | Claim found in specified source | Normal - claim validation working |
| `CLAIM_CHECK ... NOT_FOUND` | Claim not found in source | May be normal if found in other sources |
| `ERROR` | Exception during processing | Review exception details, check config |

---

## Deployment Checklist

### Pre-Deployment

- [ ] Build solution in Release configuration
- [ ] Run unit tests (if available)
- [ ] Test in local/DEV environment
- [ ] Document custom claim URLs if using external identity provider
- [ ] Backup Sitecore database

### Deployment Steps

- [ ] Deploy assembly: `IPCoop.Foundation.MediaSecurity.dll` to `bin` folder
- [ ] Deploy config: `IPCoop.Foundation.MediaSecurity.config` to `App_Config\Include\Foundation\`
- [ ] Install templates via Sitecore CLI or manual import
- [ ] Update user profile template with state fields
- [ ] Recycle application pool / IIS reset
- [ ] Verify processor registration in `showconfig.aspx`

### Post-Deployment Verification

- [ ] Check Sitecore logs for startup errors
- [ ] Test unauthenticated access (expect 401)
- [ ] Test authenticated user without claims (expect 403)
- [ ] Test authenticated user with correct claim (expect 200)
- [ ] Verify cache bypass headers on secured media
- [ ] Test normal (non-secured) media still works
- [ ] Review logs for proper authorization flow
- [ ] Verify multi-state access works as expected

### Environment-Specific Configuration

**DEV Environment:**
- Set `MediaSecurity.Enabled` to `true`
- Use detailed logging level for troubleshooting

**UAT Environment:**
- Set `MediaSecurity.Enabled` to `true`
- Monitor logs closely for authorization patterns
- Test with production-like user accounts

**Production Environment:**
- Set `MediaSecurity.Enabled` to `true`
- Ensure CDN/reverse proxy not caching secured media
- Monitor logs for authorization failures
- Set up alerts for excessive 403/401 responses

---

## Extending the Solution

### Adding New Rules

To add a new rule (e.g., `IsMexicoUser`):

1. **Update the mapping dictionary** in `MediaAuthorizationService.cs`:
   ```csharp
   { "IsMexicoUser", "hasMexicoState" }
   ```

2. **Add user profile extension** in `UserProfileExtensions.cs`:
   ```csharp
   public static bool HasMexicoState(this User user)
   {
       return GetProfileBooleanValue(user, "HasMexicoState");
   }
   ```

3. **Update claim check** in `MediaAuthorizationService.cs`:
   ```csharp
   case "hasmexicostate":
       return user.HasMexicoState();
   ```

4. **Update RuleName field source** in Sitecore:
   - Edit the RuleName field template
   - Add `|IsMexicoUser` to the source

5. **Add user profile field** in Sitecore:
   - Add `HasMexicoState` checkbox to User Profile template

### Supporting Claim Values (Future Enhancement)

Currently, the module checks for claim presence (boolean). To support claim values:

1. Modify `ClaimsPrincipalExtensions.cs` to check claim values
2. Add claim value comparison in `MediaAuthorizationService.cs`
3. Update RuleName field to support value matching syntax

---

## Support and Maintenance

### Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | Dec 23, 2025 | Initial release with claims-based authorization |

### Known Limitations

1. Template ID must be updated manually in code if using template ID check
2. No built-in UI for claim management (use User Manager or external IDP)
3. Cache bypass may impact CDN performance (by design for security)
4. Requires Sitecore 9.3+ for DI support

### Future Enhancements

- [ ] Admin override capability (configurable)
- [ ] Support for claim values (not just presence)
- [ ] Caching of folder security metadata
- [ ] Performance monitoring metrics
- [ ] Sitecore Admin UI for claim configuration
- [ ] Support for role-based authorization
- [ ] Integration with Sitecore security roles

---

## License

This code is provided as-is for use in your Sitecore projects. Modify as needed for your requirements.

---

## Contact

For questions or issues, please contact your development team or refer to the IMPLEMENTATION.md document for detailed technical information.
