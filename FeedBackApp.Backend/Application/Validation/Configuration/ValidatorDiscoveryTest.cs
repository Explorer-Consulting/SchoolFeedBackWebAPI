using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Application.Validation.Configuration
{
    /// <summary>
    /// Helper class to verify that validators are being discovered and registered correctly.
    /// This can be used during development to ensure all validators are found.
    /// </summary>
    public static class ValidatorDiscoveryTest
    {
        /// <summary>
        /// Discovers all validators in the Application assembly and returns their types.
        /// Useful for debugging and verification.
        /// </summary>
        /// <returns>Collection of validator types found in the assembly</returns>
        public static IEnumerable<Type> DiscoverValidators()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var validatorTypes = assembly.GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract)
                .Where(type => type.GetInterfaces()
                    .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidator<>)))
                .ToList();

            return validatorTypes;
        }

        /// <summary>
        /// Verifies that all discovered validators can be resolved from the service provider.
        /// </summary>
        /// <param name="serviceProvider">The service provider to test</param>
        /// <returns>True if all validators can be resolved, false otherwise</returns>
        public static bool VerifyValidatorsRegistered(IServiceProvider serviceProvider)
        {
            var validators = DiscoverValidators();
            
            foreach (var validatorType in validators)
            {
                var validatorInterface = validatorType.GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidator<>));

                if (validatorInterface != null)
                {
                    try
                    {
                        var validator = serviceProvider.GetService(validatorInterface);
                        if (validator == null)
                        {
                            return false;
                        }
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}

