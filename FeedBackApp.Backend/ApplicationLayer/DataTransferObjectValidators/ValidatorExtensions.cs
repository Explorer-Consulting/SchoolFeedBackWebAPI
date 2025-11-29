using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ULID = NUlid.Ulid;

namespace ApplicationLayer.DataTransferObjectValidators
{
    public static class ValidatorExtensions
    {
        /// <summary>
        /// chech whether the string is a valid ULID
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IRuleBuilderOptions<T, string> MustBeULID<T>(this IRuleBuilder<T, string> builder)
        {
            return builder
                .NotEmpty()
                .Must(value => ULID.TryParse(value, out _))
                .WithMessage("{PropertyName} must be a valid ULID.");
        }
    }
}
