using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureFunctionsAPI
{
    public class OpenApiConfiguration : OpenApiConfigurationOptions
    {
        public override OpenApiInfo Info { get; set; } = new OpenApiInfo
        {
            Title = "Student Feedback Platform API",
            Version = "v1.0.0",
            Description = "Unified Swagger documentation for all HTTP endpoints.",
            Contact = new OpenApiContact
            {
                Name = "Student Feedback Team",
                Email = "support@studentfeedback.app"
            },
            License = new OpenApiLicense
            {
                Name = "MIT",
                Url = new Uri("https://opensource.org/licenses/MIT")
            }
        };
    }
}
