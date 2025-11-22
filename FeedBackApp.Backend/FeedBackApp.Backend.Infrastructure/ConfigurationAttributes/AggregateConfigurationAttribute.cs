using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.ConfigurationAttributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class AggregateConfigurationAttribute(string? ContainerName = default, string? Description = default) : Attribute
    {
        public string? ContainerName { get; } = ContainerName;
        public string? Description { get; } = Description;

    }
}
