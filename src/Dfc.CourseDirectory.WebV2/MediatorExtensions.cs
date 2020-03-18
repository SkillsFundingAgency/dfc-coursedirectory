using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;

namespace Dfc.CourseDirectory.WebV2
{
    public static class MediatorExtensions
    {
        public static Task<TResult> SendAndMapResponse<TResponse, TResult>(
            this IMediator mediator,
            IRequest<TResponse> request,
            Func<TResponse, TResult> mapResponse) => SendAndMapResponse(mediator, request, mapResponse, _ => { });

        public static async Task<TResult> SendAndMapResponse<TResponse, TResult>(
            this IMediator mediator,
            IRequest<TResponse> request,
            Func<TResponse, TResult> mapResponse,
            Action<IErrorMappingDescriptor<TResult>> configureErrorMapping)
        {
            var errorMappings = new ErrorMappingDescriptor<TResult>();
            configureErrorMapping(errorMappings);

            try
            {
                var response = await mediator.Send(request);
                return mapResponse(response);
            }
            catch (Exception ex)
            {
                if (errorMappings.ExceptionMappers.TryGetValue(ex.GetType(), out var exMap))
                {
                    return exMap(ex);
                }

                throw;
            }
        }

        public interface IErrorMappingDescriptor<TResult>
        {
            IReadOnlyDictionary<Type, Func<Exception, TResult>> ExceptionMappers { get; }

            IErrorMappingDescriptor<TResult> MapException<TException>(Func<TException, TResult> map)
                where TException : Exception;
        }

        private class ErrorMappingDescriptor<TResult> : IErrorMappingDescriptor<TResult>
        {
            private readonly Dictionary<Type, Func<Exception, TResult>> _mappers;

            public ErrorMappingDescriptor()
            {
                _mappers = new Dictionary<Type, Func<Exception, TResult>>();
            }

            public IReadOnlyDictionary<Type, Func<Exception, TResult>> ExceptionMappers => _mappers;

            public IErrorMappingDescriptor<TResult> MapException<TException>(Func<TException, TResult> map)
                where TException : Exception
            {
                var key = typeof(TException);
                _mappers.Add(key, ex => map((TException)ex));

                return this;
            }
        }
    }
}
