using System;
using System.Collections.Generic;
using System.Linq;
using IPCoop.Foundation.MediaSecurity.Models;
using Sitecore.Diagnostics;

namespace IPCoop.Foundation.MediaSecurity.Logging
{
    /// <summary>
    /// Provides comprehensive logging for media security authorization events.
    /// All log entries are structured for easy parsing and troubleshooting across environments.
    /// </summary>
    public static class MediaSecurityLogger
    {
        private const string LogPrefix = "[MediaSecurity]";

        /// <summary>
        /// Logs a successful authorization event
        /// </summary>
        public static void LogAuthorizationSuccess(string username, string mediaPath, string ruleName, string matchedClaim)
        {
            var message = $"{LogPrefix} AUTHORIZED | User: {username} | MediaPath: {mediaPath} | RuleName: {ruleName} | MatchedClaim: {matchedClaim}";
            Log.Info(message, typeof(MediaSecurityLogger));
        }

        /// <summary>
        /// Logs a failed authorization event with detailed information
        /// </summary>
        public static void LogAuthorizationFailure(string username, string mediaPath, string ruleName, List<string> userClaims, string reason, bool isAuthenticated)
        {
            var claimsString = userClaims != null && userClaims.Any() 
                ? string.Join(", ", userClaims) 
                : "None";

            var authStatus = isAuthenticated ? "FORBIDDEN (403)" : "UNAUTHORIZED (401)";
            
            var message = $"{LogPrefix} {authStatus} | User: {username} | MediaPath: {mediaPath} | RuleName: {ruleName} | UserClaims: [{claimsString}] | Reason: {reason}";
            Log.Warn(message, typeof(MediaSecurityLogger));
        }

        /// <summary>
        /// Logs when a secure folder with RuleName is detected
        /// </summary>
        public static void LogSecureFolderDetected(string mediaPath, string folderPath, string ruleName)
        {
            var message = $"{LogPrefix} SECURE_FOLDER_DETECTED | MediaPath: {mediaPath} | FolderPath: {folderPath} | RuleName: {ruleName}";
            Log.Info(message, typeof(MediaSecurityLogger));
        }

        /// <summary>
        /// Logs when media cache is bypassed for a secured item
        /// </summary>
        public static void LogCacheBypass(string mediaPath)
        {
            var message = $"{LogPrefix} CACHE_BYPASS | MediaPath: {mediaPath} | Reason: Secured media always bypasses cache";
            Log.Debug(message, typeof(MediaSecurityLogger));
        }

        /// <summary>
        /// Logs when no secure folder is found (normal media flow)
        /// </summary>
        public static void LogNoSecureFolder(string mediaPath)
        {
            var message = $"{LogPrefix} NO_SECURE_FOLDER | MediaPath: {mediaPath} | Action: Normal media processing (no authorization required)";
            Log.Debug(message, typeof(MediaSecurityLogger));
        }

        /// <summary>
        /// Logs errors during authorization processing
        /// </summary>
        public static void LogError(string context, Exception exception, string mediaPath = null)
        {
            var pathInfo = !string.IsNullOrEmpty(mediaPath) ? $" | MediaPath: {mediaPath}" : string.Empty;
            var message = $"{LogPrefix} ERROR | Context: {context}{pathInfo} | Exception: {exception.Message}";
            Log.Error(message, exception, typeof(MediaSecurityLogger));
        }

        /// <summary>
        /// Logs claim resolution attempts (for troubleshooting)
        /// </summary>
        public static void LogClaimCheck(string username, string claimName, bool found, string source)
        {
            var status = found ? "FOUND" : "NOT_FOUND";
            var message = $"{LogPrefix} CLAIM_CHECK | User: {username} | Claim: {claimName} | Status: {status} | Source: {source}";
            Log.Debug(message, typeof(MediaSecurityLogger));
        }

        /// <summary>
        /// Logs the complete authorization result (comprehensive entry)
        /// </summary>
        public static void LogAuthorizationResult(AuthorizationResult result)
        {
            if (result.IsAuthorized)
            {
                LogAuthorizationSuccess(result.Username, result.MediaPath, result.RuleName, result.MatchedClaim);
            }
            else
            {
                LogAuthorizationFailure(result.Username, result.MediaPath, result.RuleName, 
                    result.UserClaims, result.Reason, result.IsAuthenticated);
            }
        }

        /// <summary>
        /// Logs configuration settings at startup
        /// </summary>
        public static void LogConfiguration(bool isEnabled, string claimUrlBase)
        {
            var message = $"{LogPrefix} CONFIGURATION | Enabled: {isEnabled} | ClaimUrlBase: {claimUrlBase}";
            Log.Info(message, typeof(MediaSecurityLogger));
        }

        /// <summary>
        /// Logs when the feature is disabled
        /// </summary>
        public static void LogFeatureDisabled(string mediaPath)
        {
            var message = $"{LogPrefix} FEATURE_DISABLED | MediaPath: {mediaPath} | Action: Skipping authorization (feature disabled in config)";
            Log.Debug(message, typeof(MediaSecurityLogger));
        }
    }
}
