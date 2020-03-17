using System;
using MediatR.Pipeline;
using OneOf;

namespace Dfc.CourseDirectory.WebV2.Behaviors
{
    public class ErrorExceptionRequestExceptionHandler<TRequest, TResponse, TException>
        : RequestExceptionHandler<TRequest, TResponse, TException>
        where TException : Exception
        where TResponse : IOneOf
    {
        protected override void Handle(
            TRequest request,
            TException exception,
            RequestExceptionHandlerState<TResponse> state)
        {
            var exceptionType = exception.GetType();

            if (exceptionType.IsGenericType && exceptionType.GetGenericTypeDefinition() == typeof(ErrorException<>))
            {
                var error = exceptionType.GetProperty("Error").GetValue(exception);

                var convertMethod = typeof(TResponse).GetMethod(
                    "op_Implicit",
                    new[] { error.GetType() });

                var response = (TResponse)convertMethod.Invoke(null, new[] { error });
                state.SetHandled(response);
            }
        }
    }
}
