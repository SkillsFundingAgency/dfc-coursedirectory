using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Moq;

namespace Dfc.CourseDirectory.Testing
{
    public class Interceptor<T> : IInterceptor
        where T : class
    {
        private readonly Mock<T> _mock;

        public Interceptor()
        {
            _mock = new Mock<T>(MockBehavior.Loose);
        }

        public void Callback(Expression<Action<T>> expression, Action action) =>
            _mock.Setup(expression).Callback(action);

        public void Callback<R>(Expression<Action<T>> expression, Action<R> action) =>
            _mock.Setup(expression).Callback(action);

        public void Callback<R1, R2>(Expression<Action<T>> expression, Action<R1, R2> action) =>
            _mock.Setup(expression).Callback(action);

        public void Callback<R1, R2, R3>(Expression<Action<T>> expression, Action<R1, R2, R3> action) =>
            _mock.Setup(expression).Callback(action);

        public void Callback<R1, R2, R3, R4>(Expression<Action<T>> expression, Action<R1, R2, R3, R4> action) =>
            _mock.Setup(expression).Callback(action);

        public void Callback<TResult>(Expression<Func<T, TResult>> expression, Action action) =>
            _mock.Setup(expression).Callback(action);

        public void Callback<TResult, R>(Expression<Func<T, TResult>> expression, Action<R> action) =>
            _mock.Setup(expression).Callback(action);

        public void Callback<TResult, R1, R2>(Expression<Func<T, TResult>> expression, Action<R1, R2> action) =>
            _mock.Setup(expression).Callback(action);

        public void Callback<TResult, R1, R2, R3>(Expression<Func<T, TResult>> expression, Action<R1, R2, R3> action) =>
            _mock.Setup(expression).Callback(action);

        public void Callback<TResult, R1, R2, R3, R4>(Expression<Func<T, TResult>> expression, Action<R1, R2, R3, R4> action) =>
            _mock.Setup(expression).Callback(action);

        public T CreateProxy(T inner)
        {
            var proxyGenerator = new ProxyGenerator();
            return proxyGenerator.CreateInterfaceProxyWithTarget<T>(inner, this);
        }

        public void Reset() => _mock.Reset();

        public void Verify<TResult>(Expression<Func<T, TResult>> expression, string failMessage) =>
            _mock.Verify(expression, failMessage);

        public void Verify<TResult>(Expression<Func<T, TResult>> expression, Func<Times> times) =>
            _mock.Verify(expression, times);

        public void Verify(Expression<Action<T>> expression) => _mock.Verify(expression);

        public void Verify<TResult>(Expression<Func<T, TResult>> expression, Times times, string failMessage) =>
            _mock.Verify(expression, times, failMessage);

        public void Verify(Expression<Action<T>> expression, Times times) => _mock.Verify(expression, times);

        public void Verify(Expression<Action<T>> expression, Func<Times> times) => _mock.Verify(expression, times);

        public void Verify(Expression<Action<T>> expression, string failMessage) =>
            _mock.Verify(expression, failMessage);

        public void Verify(Expression<Action<T>> expression, Times times, string failMessage) =>
            _mock.Verify(expression, times, failMessage);

        public void Verify(Expression<Action<T>> expression, Func<Times> times, string failMessage) =>
            _mock.Verify(expression, times, failMessage);

        public void Verify<TResult>(Expression<Func<T, TResult>> expression) => _mock.Verify(expression);

        public void Verify<TResult>(Expression<Func<T, TResult>> expression, Times times) =>
            _mock.Verify(expression, times);

        public void VerifyGet<TProperty>(Expression<Func<T, TProperty>> expression) => _mock.VerifyGet(expression);

        public void VerifyGet<TProperty>(Expression<Func<T, TProperty>> expression, Times times) =>
            _mock.VerifyGet(expression, times);

        public void VerifyGet<TProperty>(Expression<Func<T, TProperty>> expression, Func<Times> times,
            string failMessage)
            => _mock.VerifyGet(expression, times, failMessage);

        public void VerifyGet<TProperty>(Expression<Func<T, TProperty>> expression, Times times, string failMessage)
            => _mock.VerifyGet(expression, times, failMessage);

        public void VerifyGet<TProperty>(Expression<Func<T, TProperty>> expression, Func<Times> times) =>
            _mock.VerifyGet(expression, times);

        public void VerifyGet<TProperty>(Expression<Func<T, TProperty>> expression, string failMessage) =>
            _mock.VerifyGet(expression, failMessage);

        public void VerifyNoOtherCalls() => _mock.VerifyNoOtherCalls();

        public void VerifySet(Action<T> setterExpression, Func<Times> times, string failMessage) =>
            _mock.VerifySet(setterExpression, times, failMessage);

        public void VerifySet(Action<T> setterExpression, Times times, string failMessage) =>
            _mock.VerifySet(setterExpression, times, failMessage);

        public void VerifySet(Action<T> setterExpression, string failMessage) =>
            _mock.VerifySet(setterExpression, failMessage);

        public void VerifySet(Action<T> setterExpression, Func<Times> times) =>
            _mock.VerifySet(setterExpression, times);

        public void VerifySet(Action<T> setterExpression, Times times) => _mock.VerifySet(setterExpression, times);

        public void VerifySet(Action<T> setterExpression) => _mock.VerifySet(setterExpression);

        void IInterceptor.Intercept(Castle.DynamicProxy.IInvocation invocation)
        {
            invocation.Proceed();

            if (invocation.ReturnValue is Task task)
            {
                // If method is async, wait until it's done before triggering callbacks
                task.ContinueWith(r => InvokeMethodOnMock(), TaskContinuationOptions.ExecuteSynchronously);
            }
            else
            {
                InvokeMethodOnMock();
            }

            void InvokeMethodOnMock() => invocation.Method.Invoke(_mock.Object, invocation.Arguments);
        }
    }
}
