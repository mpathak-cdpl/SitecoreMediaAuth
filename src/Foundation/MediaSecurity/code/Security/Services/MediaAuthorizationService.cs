using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using IPCoop.Foundation.MediaSecurity.Extensions;
using IPCoop.Foundation.MediaSecurity.Logging;
using IPCoop.Foundation.MediaSecurity.Models;
using IPCoop.Foundation.MediaSecurity.Security.Interfaces;
using Sitecore.Configuration;
using Sitecore.Security.Accounts;

namespace IPCoop.Foundation.MediaSecurity.Security.Services
{
    /// <summary>
    /// Implementation of media authorization service.
    /// Validates user access to media items based on folder RuleName and user claims.
    /// Supports multiple claims per user with OR logic (user needs at least one matching claim).
    /// </summary>
    public class MediaAuthorizationService : IMediaAuthorizationService
    {
        private readonly string _claimUrlBase;

        // Mapping between RuleName values and required claim names
        private static readonly Dictionary<string, string> RuleNameToClaimMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "IsHawaiiUser", "hasHawaiiState" },
            { "IsAlaskaUser", "hasAlaskaState" },
            { "IsRestUSUser", "hasRestUSState" },
            { "IsCanadaUser", "hasCanadaState" }
        };

        public MediaAuthorizationService()
        {
            // Read claim URL base from Sitecore settings
            _claimUrlBase = Settings.GetSetting("MediaSecurity.ClaimUrlBase", "https://ipcoop.com/claims/");
        }

        /// <summary>
        /// Authorizes a user's access to a media item based on the folder's RuleName.
        /// Checks claims in three ways:
        /// 1. Full claim URL (e.g., https://ipcoop.com/claims/hasHawaiiState)
        /// 2. Short claim name (e.g., hasHawaiiState)
        /// 3. User profile property (e.g., userProfile.HasHawaiiState)
        /// </summary>
        public AuthorizationResult AuthorizeMediaAccess(User user, string ruleName, string mediaPath)
        {
            try
            {
                // Check if user is authenticated
                if (user == null || !user.IsAuthenticated)
                {
                    return AuthorizationResult.Unauthenticated(mediaPath, ruleName);
                }

                var username = user.Name;

                // Get required claim name from RuleName
                var requiredClaimName = GetRequiredClaimName(ruleName);
                if (string.IsNullOrEmpty(requiredClaimName))
                {
                    var errorMsg = $"Invalid or unknown RuleName: {ruleName}";
                    MediaSecurityLogger.LogError("AuthorizeMediaAccess", new ArgumentException(errorMsg), mediaPath);
                    return AuthorizationResult.Forbidden(username, ruleName, new List<string>(), new List<string>(), mediaPath);
                }

                // Collect all user claims for logging
                var userClaims = new List<string>();

                // Method 1: Check ClaimsPrincipal claims (full URL and short name)
                var httpContext = HttpContext.Current;
                var claimsPrincipal = httpContext?.User as ClaimsPrincipal;
                
                if (claimsPrincipal != null)
                {
                    userClaims.AddRange(claimsPrincipal.GetAllClaimTypes());
                    
                    if (claimsPrincipal.HasClaim(requiredClaimName, _claimUrlBase))
                    {
                        MediaSecurityLogger.LogClaimCheck(username, requiredClaimName, true, "ClaimsPrincipal");
                        return AuthorizationResult.Success(username, ruleName, requiredClaimName, userClaims, mediaPath);
                    }
                    
                    MediaSecurityLogger.LogClaimCheck(username, requiredClaimName, false, "ClaimsPrincipal");
                }

                // Method 2: Check Sitecore User Identity claims
                var userIdentity = user.RuntimeSettings.Identity as ClaimsIdentity;
                if (userIdentity != null)
                {
                    var userPrincipal = new ClaimsPrincipal(userIdentity);
                    var identityClaims = userPrincipal.GetAllClaimTypes();
                    
                    // Add claims we haven't seen yet
                    foreach (var claim in identityClaims.Where(c => !userClaims.Contains(c)))
                    {
                        userClaims.Add(claim);
                    }

                    if (userPrincipal.HasClaim(requiredClaimName, _claimUrlBase))
                    {
                        MediaSecurityLogger.LogClaimCheck(username, requiredClaimName, true, "Sitecore User Identity");
                        return AuthorizationResult.Success(username, ruleName, requiredClaimName, userClaims, mediaPath);
                    }
                    
                    MediaSecurityLogger.LogClaimCheck(username, requiredClaimName, false, "Sitecore User Identity");
                }

                // Method 3: Check user profile custom properties
                var hasProfileClaim = CheckUserProfileClaim(user, requiredClaimName);
                if (hasProfileClaim)
                {
                    userClaims.Add($"UserProfile.{requiredClaimName}");
                    MediaSecurityLogger.LogClaimCheck(username, requiredClaimName, true, "User Profile");
                    return AuthorizationResult.Success(username, ruleName, $"UserProfile.{requiredClaimName}", userClaims, mediaPath);
                }
                
                MediaSecurityLogger.LogClaimCheck(username, requiredClaimName, false, "User Profile");

                // User is authenticated but doesn't have required claim
                return AuthorizationResult.Forbidden(username, ruleName, new List<string> { requiredClaimName }, userClaims, mediaPath);
            }
            catch (Exception ex)
            {
                MediaSecurityLogger.LogError("AuthorizeMediaAccess", ex, mediaPath);
                
                // Fail closed - deny access on error
                return AuthorizationResult.Forbidden(
                    user?.Name ?? "Unknown", 
                    ruleName, 
                    new List<string> { GetRequiredClaimName(ruleName) }, 
                    new List<string>(), 
                    mediaPath);
            }
        }

        /// <summary>
        /// Gets the claim name(s) required for a given RuleName.
        /// Maps: IsHawaiiUser -> hasHawaiiState, IsAlaskaUser -> hasAlaskaState, etc.
        /// </summary>
        public string GetRequiredClaimName(string ruleName)
        {
            if (string.IsNullOrEmpty(ruleName))
            {
                return null;
            }

            return RuleNameToClaimMap.TryGetValue(ruleName, out var claimName) 
                ? claimName 
                : null;
        }

        /// <summary>
        /// Checks if the user has the required claim in their profile
        /// Uses the UserProfileExtensions methods
        /// </summary>
        private bool CheckUserProfileClaim(User user, string claimName)
        {
            if (user == null || string.IsNullOrEmpty(claimName))
            {
                return false;
            }

            try
            {
                switch (claimName.ToLowerInvariant())
                {
                    case "hashawaiistate":
                        return user.HasHawaiiState();
                    
                    case "hasalaskastate":
                        return user.HasAlaskaState();
                    
                    case "hasrestusstate":
                        return user.HasRestUSState();
                    
                    case "hascanadastate":
                        return user.HasCanadaState();
                    
                    default:
                        return false;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
