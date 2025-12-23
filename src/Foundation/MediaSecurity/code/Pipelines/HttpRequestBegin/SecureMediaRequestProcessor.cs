using System;
using System.Web;
using IPCoop.Foundation.MediaSecurity.Logging;
using IPCoop.Foundation.MediaSecurity.Models;
using IPCoop.Foundation.MediaSecurity.Security.Interfaces;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Pipelines.HttpRequest;
using Sitecore.SecurityModel;

namespace IPCoop.Foundation.MediaSecurity.Pipelines.HttpRequestBegin
{
    /// <summary>
    /// Pipeline processor that intercepts media requests and enforces authorization.
    /// Executes early in the httpRequestBegin pipeline, before media caching occurs.
    /// 
    /// Key Features:
    /// - Detects secured media folders by checking for RuleName field
    /// - Bypasses cache for all secured media items
    /// - Returns 401 for unauthenticated users (prompts login)
    /// - Returns 403 for authenticated users without required claims
    /// - Allows normal processing for non-secured media
    /// - Comprehensive logging for troubleshooting
    /// </summary>
    public class SecureMediaRequestProcessor : HttpRequestProcessor
    {
        private readonly IMediaAuthorizationService _authorizationService;
        private readonly bool _isEnabled;

        // Template ID for the Secure Media Folder template (update after template creation)
        // This should match the ID of your Secure Media Folder template
        private const string SecureMediaFolderTemplateId = "{B6A6710E-1C3E-4F3A-9C8D-2E4F5A6B7C8D}";
        
        // Field name in the Secure Media Folder template
        private const string RuleNameFieldName = "RuleName";

        public SecureMediaRequestProcessor(IMediaAuthorizationService authorizationService)
        {
            Assert.ArgumentNotNull(authorizationService, nameof(authorizationService));
            _authorizationService = authorizationService;
            _isEnabled = Settings.GetBoolSetting("MediaSecurity.Enabled", true);
        }

        /// <summary>
        /// Main process method called by Sitecore pipeline
        /// </summary>
        public override void Process(HttpRequestArgs args)
        {
            Assert.ArgumentNotNull(args, nameof(args));

            try
            {
                // Check if feature is enabled
                if (!_isEnabled)
                {
                    MediaSecurityLogger.LogFeatureDisabled(args.Url.FilePath);
                    return;
                }

                // Check if this is a media request
                if (!IsMediaRequest(args))
                {
                    return;
                }

                var mediaPath = args.Url.FilePath;

                // Get the media item
                var mediaItem = GetMediaItem(args);
                if (mediaItem == null)
                {
                    // Media item not found - let normal processing handle 404
                    return;
                }

                // Find the first secure folder in the item's ancestry
                var secureFolder = FindSecureMediaFolder(mediaItem);
                if (secureFolder == null)
                {
                    // No secure folder found - allow normal processing
                    MediaSecurityLogger.LogNoSecureFolder(mediaPath);
                    return;
                }

                // Get RuleName from the secure folder
                var ruleName = secureFolder[RuleNameFieldName];
                if (string.IsNullOrEmpty(ruleName))
                {
                    // RuleName is empty - allow normal processing
                    MediaSecurityLogger.LogNoSecureFolder(mediaPath);
                    return;
                }

                // Secure folder with RuleName detected
                MediaSecurityLogger.LogSecureFolderDetected(mediaPath, secureFolder.Paths.FullPath, ruleName);

                // Bypass cache for secured media
                BypassMediaCache(args);
                MediaSecurityLogger.LogCacheBypass(mediaPath);

                // Authorize the request
                var currentUser = Sitecore.Context.User;
                var authResult = _authorizationService.AuthorizeMediaAccess(currentUser, ruleName, mediaPath);

                // Log the authorization result
                MediaSecurityLogger.LogAuthorizationResult(authResult);

                // Handle authorization result
                if (!authResult.IsAuthorized)
                {
                    HandleUnauthorizedAccess(args, authResult);
                }
                // If authorized, continue normal processing
            }
            catch (Exception ex)
            {
                MediaSecurityLogger.LogError("SecureMediaRequestProcessor.Process", ex, args.Url.FilePath);
                
                // Fail closed - deny access on error
                args.Context.Response.StatusCode = 500;
                args.Context.Response.StatusDescription = "Internal Server Error";
                args.Context.Response.Write("An error occurred while processing your request.");
                args.AbortPipeline();
            }
        }

        /// <summary>
        /// Checks if the current request is for media
        /// </summary>
        private bool IsMediaRequest(HttpRequestArgs args)
        {
            var filePath = args.Url.FilePath;
            
            // Check for standard Sitecore media paths
            return filePath.StartsWith("/~/media/", StringComparison.OrdinalIgnoreCase) ||
                   filePath.StartsWith("/-/media/", StringComparison.OrdinalIgnoreCase) ||
                   filePath.StartsWith("~/media/", StringComparison.OrdinalIgnoreCase) ||
                   filePath.StartsWith("-/media/", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the Sitecore media item from the request
        /// </summary>
        private Item GetMediaItem(HttpRequestArgs args)
        {
            try
            {
                // Use Sitecore's media manager to resolve the media item
                var mediaRequest = Sitecore.Resources.Media.MediaManager.GetMediaRequest(args.Url);
                if (mediaRequest == null)
                {
                    return null;
                }

                using (new SecurityDisabler())
                {
                    var mediaItem = Sitecore.Resources.Media.MediaManager.GetMedia(mediaRequest)?.MediaData?.MediaItem;
                    return mediaItem;
                }
            }
            catch (Exception ex)
            {
                MediaSecurityLogger.LogError("GetMediaItem", ex, args.Url.FilePath);
                return null;
            }
        }

        /// <summary>
        /// Traverses the item ancestry to find the first folder with Secure Media Folder template and non-empty RuleName
        /// </summary>
        private Item FindSecureMediaFolder(Item mediaItem)
        {
            if (mediaItem == null)
            {
                return null;
            }

            try
            {
                using (new SecurityDisabler())
                {
                    var current = mediaItem.Parent;
                    
                    while (current != null)
                    {
                        // Check if this folder is using the Secure Media Folder template
                        if (IsSecureMediaFolder(current))
                        {
                            return current;
                        }

                        current = current.Parent;
                    }
                }
            }
            catch (Exception ex)
            {
                MediaSecurityLogger.LogError("FindSecureMediaFolder", ex, mediaItem.Paths.FullPath);
            }

            return null;
        }

        /// <summary>
        /// Checks if an item is a Secure Media Folder (has the template and RuleName field)
        /// </summary>
        private bool IsSecureMediaFolder(Item item)
        {
            if (item == null)
            {
                return false;
            }

            try
            {
                // Option 1: Check by template ID (most reliable)
                // Uncomment this when you have created the template and updated the constant
                // if (item.TemplateID.ToString().Equals(SecureMediaFolderTemplateId, StringComparison.OrdinalIgnoreCase))
                // {
                //     return true;
                // }

                // Option 2: Check by template name (fallback)
                if (item.TemplateName.Equals("Secure Media Folder", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                // Option 3: Check if item has RuleName field (most flexible, works with template inheritance)
                if (item.Fields[RuleNameFieldName] != null)
                {
                    return true;
                }
            }
            catch
            {
                // Ignore errors and return false
            }

            return false;
        }

        /// <summary>
        /// Bypasses Sitecore media cache for secured media items
        /// </summary>
        private void BypassMediaCache(HttpRequestArgs args)
        {
            var response = args.Context.Response;
            
            // Set cache control headers to prevent caching
            response.Cache.SetCacheability(HttpCacheability.NoCache);
            response.Cache.SetNoStore();
            response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
            response.Cache.SetMaxAge(TimeSpan.Zero);
            response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            
            // Add additional no-cache headers
            response.Headers["Pragma"] = "no-cache";
            response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate, max-age=0";
        }

        /// <summary>
        /// Handles unauthorized access by returning appropriate HTTP status code
        /// </summary>
        private void HandleUnauthorizedAccess(HttpRequestArgs args, AuthorizationResult authResult)
        {
            var response = args.Context.Response;
            
            if (!authResult.IsAuthenticated)
            {
                // User is not authenticated - return 401 to trigger login
                response.StatusCode = 401;
                response.StatusDescription = "Unauthorized";
                response.Headers["WWW-Authenticate"] = "Bearer";
                response.Write("<html><body><h1>401 - Unauthorized</h1><p>Authentication is required to access this resource.</p></body></html>");
            }
            else
            {
                // User is authenticated but doesn't have required claims - return 403
                response.StatusCode = 403;
                response.StatusDescription = "Forbidden";
                response.Write("<html><body><h1>403 - Forbidden</h1><p>You do not have permission to access this resource.</p></body></html>");
            }

            args.AbortPipeline();
        }
    }
}
