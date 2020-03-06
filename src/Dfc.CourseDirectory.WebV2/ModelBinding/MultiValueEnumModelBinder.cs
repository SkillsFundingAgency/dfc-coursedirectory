using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.ExceptionServices;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.WebV2.ModelBinding
{
    public class MultiValueEnumModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Metadata.IsEnum)
            {
                var loggerFactory = context.Services.GetRequiredService<ILoggerFactory>();
                return new MultiValueEnumModelBinder(
                    suppressBindingUndefinedValueToEnumType: true,
                    context.Metadata.UnderlyingOrModelType,
                    loggerFactory);
            }

            return null;
        }
    }

    public class MultiValueEnumModelBinder : EnumTypeModelBinder
    {
        private readonly Type _modelType;

        public MultiValueEnumModelBinder(
            bool suppressBindingUndefinedValueToEnumType,
            Type modelType,
            ILoggerFactory loggerFactory)
            : base(suppressBindingUndefinedValueToEnumType, modelType, loggerFactory)
        {
            _modelType = modelType;
        }

        protected override void CheckModel(
            ModelBindingContext bindingContext,
            ValueProviderResult valueProviderResult,
            object model)
        {
            if (bindingContext.ModelMetadata.IsFlagsEnum && valueProviderResult.Length > 1)
            {
                try
                {
                    // The base model binder knows how to convert an array of enum values...

                    var allValues = valueProviderResult.Values;
                    var converter = TypeDescriptor.GetConverter(_modelType);

                    var converted = allValues
                        .Select(v => converter.ConvertFrom(
                            context: null,
                            culture: valueProviderResult.Culture,
                            value: v))
                        .Cast<int>();

                    var allValuesAreDefined = converted.All(v => Enum.IsDefined(_modelType, v));
                    if (allValuesAreDefined)
                    {
                        model = Enum.ToObject(_modelType, converted.Sum());
                        bindingContext.Result = ModelBindingResult.Success(model);
                    }
                    else
                    {
                        bindingContext.ModelState.TryAddModelError(
                            bindingContext.ModelName,
                            bindingContext.ModelMetadata.ModelBindingMessageProvider.ValueIsInvalidAccessor(
                                valueProviderResult.ToString()));
                    }
                }
                catch (Exception exception)
                {
                    var isFormatException = exception is FormatException;
                    if (!isFormatException && exception.InnerException != null)
                    {
                        // TypeConverter throws System.Exception wrapping the FormatException,
                        // so we capture the inner exception.
                        exception = ExceptionDispatchInfo.Capture(exception.InnerException).SourceException;
                    }

                    bindingContext.ModelState.TryAddModelError(
                        bindingContext.ModelName,
                        exception,
                        bindingContext.ModelMetadata);
                }
            }
            else
            {
                base.CheckModel(bindingContext, valueProviderResult, model);
            }
        }
    }
}
