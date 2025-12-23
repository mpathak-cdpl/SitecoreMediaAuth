using IPCoop.Foundation.MediaSecurity.Models;
using Sitecore.Security.Accounts;

namespace IPCoop.Foundation.MediaSecurity.Security.Interfaces
{
    /// <summary>
    /// Service interface for authorizing media access based on user claims and folder rules.
    /// Supports multi-claim validation with OR logic (user needs at least one matching claim).
    /// </summary>
    public interface IMediaAuthorizationService
    {
        /// <summary>
        /// Authorizes a user's access to a media item based on the folder's RuleName.
        /// Checks claims in three ways:
        /// 1. Full claim URL (e.g., https://ipcoop.com/claims/hasHawaiiState)
        /// 2. Short claim name (e.g., hasHawaiiState)
        /// 3. User profile property (e.g., userProfile.HasHawaiiState)
        /// </summary>
        /// <param name="user">The Sitecore user to authorize</param>
        /// <param name="ruleName">The RuleName from the secure media folder</param>
        /// <param name="mediaPath">The path of the media being accessed (for logging)</param>
        /// <returns>Authorization result with success/failure and detailed information</returns>
        AuthorizationResult AuthorizeMediaAccess(User user, string ruleName, string mediaPath);

        /// <summary>
        /// Gets the claim name(s) required for a given RuleName.
        /// Maps: IsHawaiiUser -> hasHawaiiState, IsAlaskaUser -> hasAlaskaState, etc.
        /// </summary>
        /// <param name="ruleName">The RuleName value from the folder</param>
        /// <returns>The corresponding claim name, or null if ruleName is invalid</returns>
        string GetRequiredClaimName(string ruleName);
    }
}
