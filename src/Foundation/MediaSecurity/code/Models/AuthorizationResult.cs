using System;
using System.Collections.Generic;

namespace IPCoop.Foundation.MediaSecurity.Models
{
    /// <summary>
    /// Represents the result of a media authorization check.
    /// Contains success/failure status and detailed information for logging and troubleshooting.
    /// </summary>
    public class AuthorizationResult
    {
        /// <summary>
        /// Gets or sets whether the authorization was successful
        /// </summary>
        public bool IsAuthorized { get; set; }

        /// <summary>
        /// Gets or sets the reason for authorization success or failure
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// Gets or sets the rule name that was evaluated
        /// </summary>
        public string RuleName { get; set; }

        /// <summary>
        /// Gets or sets the claim that matched (if successful)
        /// </summary>
        public string MatchedClaim { get; set; }

        /// <summary>
        /// Gets or sets all claims found for the user (for logging purposes)
        /// </summary>
        public List<string> UserClaims { get; set; }

        /// <summary>
        /// Gets or sets the username being authorized
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets whether the user is authenticated
        /// </summary>
        public bool IsAuthenticated { get; set; }

        /// <summary>
        /// Gets or sets the media item path being accessed
        /// </summary>
        public string MediaPath { get; set; }

        /// <summary>
        /// Creates a successful authorization result
        /// </summary>
        public static AuthorizationResult Success(string username, string ruleName, string matchedClaim, List<string> userClaims, string mediaPath)
        {
            return new AuthorizationResult
            {
                IsAuthorized = true,
                Username = username,
                RuleName = ruleName,
                MatchedClaim = matchedClaim,
                UserClaims = userClaims ?? new List<string>(),
                IsAuthenticated = true,
                MediaPath = mediaPath,
                Reason = $"User has required claim '{matchedClaim}' for rule '{ruleName}'"
            };
        }

        /// <summary>
        /// Creates a failed authorization result for unauthenticated user
        /// </summary>
        public static AuthorizationResult Unauthenticated(string mediaPath, string ruleName)
        {
            return new AuthorizationResult
            {
                IsAuthorized = false,
                Username = "Anonymous",
                RuleName = ruleName,
                UserClaims = new List<string>(),
                IsAuthenticated = false,
                MediaPath = mediaPath,
                Reason = $"User is not authenticated. Rule '{ruleName}' requires authentication."
            };
        }

        /// <summary>
        /// Creates a failed authorization result for authenticated user without required claims
        /// </summary>
        public static AuthorizationResult Forbidden(string username, string ruleName, List<string> requiredClaims, List<string> userClaims, string mediaPath)
        {
            return new AuthorizationResult
            {
                IsAuthorized = false,
                Username = username,
                RuleName = ruleName,
                UserClaims = userClaims ?? new List<string>(),
                IsAuthenticated = true,
                MediaPath = mediaPath,
                Reason = $"User does not have any of the required claims [{string.Join(", ", requiredClaims ?? new List<string>())}] for rule '{ruleName}'. User claims: [{string.Join(", ", userClaims ?? new List<string>())}]"
            };
        }

        public AuthorizationResult()
        {
            UserClaims = new List<string>();
        }
    }
}
