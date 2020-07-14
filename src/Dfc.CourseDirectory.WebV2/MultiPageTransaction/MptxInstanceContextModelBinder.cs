using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.WebV2.MultiPageTransaction
{
    public class MptxInstanceContextModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            var instanceContextType = typeof(MptxInstanceContext<>);
            var instanceContextWithParentType = typeof(MptxInstanceContext<,>);

            var modelType = context.Metadata.ModelType;

            if (modelType.IsGenericType &&
                (modelType.GetGenericTypeDefinition() == instanceContextType ||
                modelType.GetGenericTypeDefinition() == instanceContextWithParentType))
            {
                return new MptxInstanceContextModelBinder();
            }
            else
            {
                return null;
            }
        }
    }

    public class MptxInstanceContextModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var modelTypeGenericArguments = bindingContext.ModelType.GetGenericArguments();
            var stateType = modelTypeGenericArguments[0];
            var parentStateType = modelTypeGenericArguments.Length > 1 ? modelTypeGenericArguments[1] : null;

            var instanceProvider = bindingContext.HttpContext.RequestServices.GetRequiredService<MptxInstanceProvider>();
            var instanceContextFactory = bindingContext.HttpContext.RequestServices.GetRequiredService<MptxInstanceContextFactory>();

            var instance = instanceProvider.GetInstance();

            if (instance == null)
            {
                bindingContext.ModelState.AddModelError(bindingContext.FieldName, "No active MPTX instance.");
                bindingContext.Result = ModelBindingResult.Failed();
            }
            else if (!stateType.IsAssignableFrom(instance.StateType))
            {
                bindingContext.ModelState.AddModelError(
                    bindingContext.FieldName,
                    "Persisted state type does not match requested type.");
                bindingContext.Result = ModelBindingResult.Failed();
            }
            else
            {
                var context = instanceContextFactory.CreateContext(instance, stateType, parentStateType);
                bindingContext.Result = ModelBindingResult.Success(context);
            }

            return Task.CompletedTask;
        }
    }
}
