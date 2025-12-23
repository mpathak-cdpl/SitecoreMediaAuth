using System;

namespace IPCoop.Foundation.MediaSecurity.Models
{
    /// <summary>
    /// Defines the supported rule types for media folder authorization.
    /// Each rule type maps to specific user claims that must be validated.
    /// </summary>
    public enum RuleNameType
    {
        /// <summary>
        /// No rule specified - media is publicly accessible
        /// </summary>
        None = 0,

        /// <summary>
        /// Requires Hawaii state claim: hasHawaiiState
        /// </summary>
        IsHawaiiUser = 1,

        /// <summary>
        /// Requires Alaska state claim: hasAlaskaState
        /// </summary>
        IsAlaskaUser = 2,

        /// <summary>
        /// Requires Rest of US state claim: hasRestUSState
        /// </summary>
        IsRestUSUser = 3,

        /// <summary>
        /// Requires Canada state claim: hasCanadaState
        /// </summary>
        IsCanadaUser = 4
    }
}
