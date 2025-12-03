using Application.Validation.Configuration;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Application.Validation.Configuration
{
    /// <summary>
    /// Extension methods for configuring FluentValidation in the dependency injection container.
    /// </summary>
    public static class ValidationServiceCollectionExtensions
    {
        /// <summary>
        /// Adds FluentValidation validators to the service collection.
        /// Automatically discovers and registers all validators in the Application assembly.
        /// </summary>
        /// <param name="services">The service collection to add validators to</param>
        /// <param name="configure">Optional action to configure validation options</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddFluentValidation(
            this IServiceCollection services,
            Action<ValidationOptions>? configure = null)
        {
            // Register validation options
            if (configure != null)
            {
                services.Configure(configure);
            }

            // Get the assembly containing validators (Application assembly)
            var assembly = Assembly.GetExecutingAssembly();

            // Register all validators from the Application assembly
            // This automatically discovers all classes that implement IValidator<T>
            // Uses FluentValidation.DependencyInjectionExtensions
            services.AddValidatorsFromAssembly(assembly);

            return services;
        }

        /// <summary>
        /// Adds FluentValidation validators from a specific assembly.
        /// </summary>
        /// <param name="services">The service collection to add validators to</param>
        /// <param name="assembly">The assembly to scan for validators</param>
        /// <param name="lifetime">The service lifetime for validators (default: Scoped)</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddFluentValidationFromAssembly(
            this IServiceCollection services,
            Assembly assembly,
            ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            services.AddValidatorsFromAssembly(assembly, lifetime);
            return services;
        }

        /// <summary>
        /// Configures FluentValidation global options such as property name resolver and error message formatting.
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configure">Action to configure FluentValidation global options</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection ConfigureFluentValidation(
            this IServiceCollection services,
            Action? configure = null)
        {
            if (configure != null)
            {
                configure();
            }
            else
            {
                // Default configuration: use camelCase for property names
                FluentValidation.ValidatorOptions.Global.PropertyNameResolver = (type, member, expression) =>
                {
                    if (member != null)
                    {
                        // Convert PascalCase to camelCase
                        return char.ToLowerInvariant(member.Name[0]) + member.Name.Substring(1);
                    }
                    return member?.Name ?? string.Empty;
                };
            }

            return services;
        }
    }
}

