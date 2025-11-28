using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ValidateRequestAttribute(Type DataTransferObjectType) : Attribute
    {
        public Type DataTransferObjectType { get; init; } = DataTransferObjectType;

    }
}
