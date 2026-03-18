using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Dfc.CourseDirectory.Core.Extensions
{
    public static class ValidationResultExtension
    {
        /// <summary>
        /// Stores the errors in a ValidationResult object to the specified modelstate dictionary.
        /// </summary>
        /// <param name="result">The validation result to store</param>
        /// <param name="modelState">The ModelStateDictionary to store the errors in.</param>
        public static void AddToModelState(this ValidationResult result, ModelStateDictionary modelState)
        {
            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    modelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
            }
        }
        /// <summary>
        /// Stores the errors in a ValidationResult object to the specified modelstate dictionary.
        /// </summary>
        /// <param name="result">The validation result to store</param>
        /// <param name="modelState">The ModelStateDictionary to store the errors in.</param>
        /// <param name="prefix">An optional prefix. If omitted, the property names will be the keys. If specified, the prefix will be concatenated to the property name with a period. Eg "user.Name"</param>
        public static void AddToModelState(this ValidationResult result, ModelStateDictionary modelState, string prefix)
        {
            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    string key = string.IsNullOrEmpty(prefix)
                        ? error.PropertyName
                        : string.IsNullOrEmpty(error.PropertyName)
                            ? prefix
                            : prefix + "." + error.PropertyName;
                    modelState.AddModelError(key, error.ErrorMessage);
                }
            }
        }
    }
}
