using IPCoop.Foundation.MediaSecurity.Security.Interfaces;
using IPCoop.Foundation.MediaSecurity.Security.Services;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.DependencyInjection;

namespace IPCoop.Foundation.MediaSecurity.Configuration
{
    /// <summary>
    /// Configurator for dependency injection in Sitecore 9.3+
    /// Registers all services required by the Media Security module
    /// </summary>
    public class MediaSecurityServicesConfigurator : IServicesConfigurator
    {
        /// <summary>
        /// Configures services for dependency injection
        /// </summary>
        public void Configure(IServiceCollection serviceCollection)
        {
            // Register authorization service as singleton (stateless, thread-safe)
            serviceCollection.AddSingleton<IMediaAuthorizationService, MediaAuthorizationService>();
        }
    }
}
