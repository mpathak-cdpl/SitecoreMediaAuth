using System;
using Sitecore.Security.Accounts;

namespace IPCoop.Foundation.MediaSecurity.Extensions
{
    /// <summary>
    /// Extension methods for Sitecore User Profile to access state-based custom properties.
    /// These properties should be configured as custom user profile fields in Sitecore.
    /// </summary>
    public static class UserProfileExtensions
    {
        /// <summary>
        /// Gets whether the user has Hawaii state access from their profile
        /// </summary>
        public static bool HasHawaiiState(this User user)
        {
            return GetProfileBooleanValue(user, "HasHawaiiState");
        }

        /// <summary>
        /// Gets whether the user has Alaska state access from their profile
        /// </summary>
        public static bool HasAlaskaState(this User user)
        {
            return GetProfileBooleanValue(user, "HasAlaskaState");
        }

        /// <summary>
        /// Gets whether the user has Rest of US state access from their profile
        /// </summary>
        public static bool HasRestUSState(this User user)
        {
            return GetProfileBooleanValue(user, "HasRestUSState");
        }

        /// <summary>
        /// Gets whether the user has Canada state access from their profile
        /// </summary>
        public static bool HasCanadaState(this User user)
        {
            return GetProfileBooleanValue(user, "HasCanadaState");
        }

        /// <summary>
        /// Helper method to safely get boolean values from user profile
        /// </summary>
        private static bool GetProfileBooleanValue(User user, string propertyName)
        {
            if (user == null || user.Profile == null)
            {
                return false;
            }

            try
            {
                var value = user.Profile.GetCustomProperty(propertyName);
                
                if (string.IsNullOrEmpty(value))
                {
                    return false;
                }

                // Handle various boolean representations
                if (bool.TryParse(value, out bool result))
                {
                    return result;
                }

                // Handle "1" or "0" representations
                if (value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase) || 
                    value.Equals("yes", StringComparison.OrdinalIgnoreCase))
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
    }
}
