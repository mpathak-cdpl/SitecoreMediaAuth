using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace IPCoop.Foundation.MediaSecurity.Extensions
{
    /// <summary>
    /// Extension methods for ClaimsPrincipal to extract and validate claims.
    /// Supports checking claims by full URL or short name.
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Checks if the principal has a specific claim by full URL or short name.
        /// Example: HasClaim("hasHawaiiState", "https://ipcoop.com/claims/") will check:
        /// 1. Claim with type "https://ipcoop.com/claims/hasHawaiiState"
        /// 2. Claim with type "hasHawaiiState"
        /// </summary>
        public static bool HasClaim(this ClaimsPrincipal principal, string claimName, string claimUrlBase)
        {
            if (principal == null || string.IsNullOrEmpty(claimName))
            {
                return false;
            }

            try
            {
                // Check for full URL format
                var fullClaimUrl = GetFullClaimUrl(claimName, claimUrlBase);
                if (principal.HasClaim(c => c.Type.Equals(fullClaimUrl, StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }

                // Check for short name format
                if (principal.HasClaim(c => c.Type.Equals(claimName, StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }

                // Also check claim value (some implementations store it in value instead of type)
                if (principal.HasClaim(c => c.Value.Equals(claimName, StringComparison.OrdinalIgnoreCase) ||
                                           c.Value.Equals(fullClaimUrl, StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets all claims for a user (both full URL and short names) for logging purposes
        /// </summary>
        public static List<string> GetAllClaimTypes(this ClaimsPrincipal principal)
        {
            if (principal?.Claims == null)
            {
                return new List<string>();
            }

            try
            {
                return principal.Claims
                    .Select(c => c.Type)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Checks if the principal has any of the specified claims (OR logic)
        /// </summary>
        public static bool HasAnyClaim(this ClaimsPrincipal principal, IEnumerable<string> claimNames, string claimUrlBase)
        {
            if (principal == null || claimNames == null || !claimNames.Any())
            {
                return false;
            }

            foreach (var claimName in claimNames)
            {
                if (principal.HasClaim(claimName, claimUrlBase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the first matching claim from a list of potential claims
        /// </summary>
        public static string GetFirstMatchingClaim(this ClaimsPrincipal principal, IEnumerable<string> claimNames, string claimUrlBase)
        {
            if (principal == null || claimNames == null || !claimNames.Any())
            {
                return null;
            }

            foreach (var claimName in claimNames)
            {
                if (principal.HasClaim(claimName, claimUrlBase))
                {
                    return claimName;
                }
            }

            return null;
        }

        /// <summary>
        /// Constructs the full claim URL from base URL and claim name
        /// </summary>
        private static string GetFullClaimUrl(string claimName, string claimUrlBase)
        {
            if (string.IsNullOrEmpty(claimUrlBase))
            {
                return claimName;
            }

            // Ensure base URL ends with /
            var baseUrl = claimUrlBase.TrimEnd('/') + "/";
            
            // Remove leading / from claim name if present
            var cleanClaimName = claimName.TrimStart('/');

            return baseUrl + cleanClaimName;
        }

        /// <summary>
        /// Converts IIdentity to ClaimsPrincipal if possible
        /// </summary>
        public static ClaimsPrincipal ToClaimsPrincipal(this IIdentity identity)
        {
            if (identity is ClaimsIdentity claimsIdentity)
            {
                return new ClaimsPrincipal(claimsIdentity);
            }

            return null;
        }
    }
}
