using System;
using System.Threading;

namespace Epi.Cloud.Common
{
    public enum RetryAction
    {
        ContinueRetrying,
        ThrowException,
        ReturnResult
    }
    public class RetryResponse<T>
    {
        public RetryAction Action { get; set; }
        public T Result { get; set; }
        public TimeSpan? OverrideInterval { get; set; }
        public Exception OverrideException { get; set; }
    }

    public class RetryResponse
    {
        public RetryAction Action { get; set; }
        public TimeSpan? OverrideInterval { get; set; }
        public Exception OverrideException { get; set; }
    }

    public class RetryStrategies
    {
        private int _maximumRetries = 3;
        private TimeSpan _interval = TimeSpan.FromMilliseconds(100);

        public RetryStrategies()
        {
        }
        public RetryStrategies(int maximumRetries, TimeSpan interval)
        {
            _maximumRetries = maximumRetries;
            _interval = interval;
        }

        public virtual T ExecuteWithRetry<T>(Func<T> action, Func<Exception, int, int, RetryResponse<T>> exceptionHandeler = null)
        {
            return ExecuteWithRetry<T>(_maximumRetries, _interval, action, exceptionHandeler);
        }

        /// <summary>
        /// ExecuteWithRetry<T>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="maximumRetries"></param>
        /// <param name="interval"></param>
        /// <param name="action"></param>
        /// <param name="exceptionHandeler"></param>
        /// <returns></returns>
        /// <remarks>
        /// Funct: Exception exception, int numberOfRetries, int remainingRetries, RetryResponse
        /// </remarks>
        public virtual T ExecuteWithRetry<T>(int maximumRetries, TimeSpan interval, Func<T> action, Func<Exception, int, int, RetryResponse<T>> exceptionHandeler = null)
        {
            T result = default(T);
            var remainingRetries = maximumRetries;
            var numberOfRetries = 0;
            while (true)
            {
                try
                {
                    result = action();
                    break;
                }
                catch (Exception ex)
                {
                    remainingRetries -= 1;

                    if (exceptionHandeler != null)
                    {
                        var retryResponse = exceptionHandeler(ex, numberOfRetries, remainingRetries);
                        if (retryResponse.Action == RetryAction.ThrowException)
                        {
                            if (retryResponse.OverrideException != null)
                                throw retryResponse.OverrideException;
                            else
                                throw;
                        }
                        if (retryResponse.Action == RetryAction.ReturnResult) { return retryResponse.Result; }
                        if (retryResponse.OverrideInterval.HasValue) interval = retryResponse.OverrideInterval.Value;
                    }

                    if (ex.GetType() == typeof(System.NullReferenceException)) throw;

                    if (remainingRetries > 0)
                    {
                        Thread.Sleep(interval);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return result;
        }

        public virtual void ExecuteWithRetry(Action action, Func<Exception, int, int, RetryAction> exceptionHandeler = null)
        {
            ExecuteWithRetry(_maximumRetries, _interval, action, exceptionHandeler);
        }

        /// <summary>
        /// ExecuteWithRetry
        /// </summary>
        /// <param name="numberOfRetries"></param>
        /// <param name="interval"></param>
        /// <param name="action"></param>
        /// <param name="exceptionHandeler"></param>
        /// <remarks>
        /// Funct: Exception exception, int numberOfRetries, int remainingRetries, RetryAction
        /// </remarks>
        public virtual void ExecuteWithRetry(int numberOfRetries, TimeSpan interval, Action action, Func<Exception, int, int, RetryAction> exceptionHandeler = null)
        {
            var remainingRetries = numberOfRetries;
            while (true)
            {
                try
                {
                    action();
                    break;
                }
                catch (Exception ex)
                {
                    remainingRetries -= 1;
                    if (exceptionHandeler != null)
                    {
                        var retryAction = exceptionHandeler(ex, numberOfRetries, remainingRetries);
                        if (retryAction == RetryAction.ThrowException) throw;
                    }

                    if (ex.GetType() == typeof(System.NullReferenceException)) throw;

                    if (remainingRetries > 0)
                        Thread.Sleep(interval);
                    else 
                        throw;
                }
            }
        }

        public virtual void ExecuteWithRetry(Action action, Func<Exception, int, int, RetryResponse> exceptionHandeler)
        {
            ExecuteWithRetry(_maximumRetries, _interval, action, exceptionHandeler);
        }

        /// <summary>
        /// ExecuteWithRetry
        /// </summary>
        /// <param name="numberOfRetries"></param>
        /// <param name="interval"></param>
        /// <param name="action"></param>
        /// <param name="exceptionHandeler"></param>
        /// <remarks>
        /// Funct: Exception exception, int numberOfRetries, int remainingRetries, RetryAction
        /// </remarks>
        public virtual void ExecuteWithRetry(int numberOfRetries, TimeSpan interval, Action action, Func<Exception, int, int, RetryResponse> exceptionHandeler)
        {
            var remainingRetries = numberOfRetries;
            while (true)
            {
                try
                {
                    action();
                    break;
                }
                catch (Exception ex)
                {
                    remainingRetries -= 1;
                    if (exceptionHandeler != null)
                    {
                        var retryResponse = exceptionHandeler(ex, numberOfRetries, remainingRetries);
                        if (retryResponse.Action == RetryAction.ThrowException)
                        {
                            if (retryResponse.OverrideException != null)
                                throw retryResponse.OverrideException;
                            else
                                throw;
                        }

                        if (retryResponse.OverrideInterval.HasValue) interval = retryResponse.OverrideInterval.Value;
                    }

                    if (ex.GetType() == typeof(System.NullReferenceException)) throw;

                    if (remainingRetries > 0)
                        Thread.Sleep(interval);
                    else
                        throw;
                }
            }
        }
    }
}
