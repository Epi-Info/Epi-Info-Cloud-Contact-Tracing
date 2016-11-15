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

        public virtual T ExecuteWithRetry<T>(Func<T> action, Func<Exception, int, int, RetryResponse<T>> exceptionHandler = null)
        {
            return ExecuteWithRetry<T>(_maximumRetries, _interval, action, exceptionHandler);
        }

        /// <summary>
        /// ExecuteWithRetry<T>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="maximumRetries"></param>
        /// <param name="interval"></param>
        /// <param name="action"></param>
        /// <param name="exceptionHandler"></param>
        /// <returns></returns>
        /// <remarks>
        /// Funct: Exception exception, int consumedRetries, int remainingRetries, RetryResponse
        /// </remarks>
        public virtual T ExecuteWithRetry<T>(int maximumRetries, TimeSpan interval, Func<T> action, Func<Exception, int, int, RetryResponse<T>> exceptionHandler = null)
        {
            T result = default(T);
            var remainingRetries = maximumRetries;
            var consumedRetries = 0;
            while (true)
            {
                try
                {
                    result = action();
                    break;
                }
                catch (Exception ex)
                {

                    if (exceptionHandler != null)
                    {
                        var retryResponse = exceptionHandler(ex, consumedRetries, remainingRetries);
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

                    remainingRetries -= 1;
                    consumedRetries += 1;

                    if (remainingRetries >= 0)
                        if (interval > TimeSpan.Zero) Thread.Sleep(interval);
                    else
                        throw;
                }
            }
            return result;
        }

        public virtual void ExecuteWithRetry(Action action, Func<Exception, int, int, RetryAction> exceptionHandler = null)
        {
            ExecuteWithRetry(_maximumRetries, _interval, action, exceptionHandler);
        }

        /// <summary>
        /// ExecuteWithRetry
        /// </summary>
        /// <param name="maximumRetries"></param>
        /// <param name="interval"></param>
        /// <param name="action"></param>
        /// <param name="exceptionHandler"></param>
        /// <remarks>
        /// Funct: Exception exception, int consumedRetries, int remainingRetries, RetryAction
        /// </remarks>
        public virtual void ExecuteWithRetry(int maximumRetries, TimeSpan interval, Action action, Func<Exception, int, int, RetryAction> exceptionHandler = null)
        {
            var remainingRetries = maximumRetries;
            var consumedRetries = 0;
            while (true)
            {
                try
                {
                    action();
                    break;
                }
                catch (Exception ex)
                {
                    if (exceptionHandler != null)
                    {
                        var retryAction = exceptionHandler(ex, consumedRetries, remainingRetries);
                        if (retryAction == RetryAction.ThrowException) throw;
                    }

                    remainingRetries -= 1;
                    consumedRetries += 1;

                    if (ex.GetType() == typeof(System.NullReferenceException)) throw;

                    if (remainingRetries >= 0)
                        if (interval > TimeSpan.Zero) Thread.Sleep(interval);
                    else 
                        throw;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="exceptionHandler"></param>
        public virtual void ExecuteWithRetry(Action action, Func<Exception, int, int, RetryResponse> exceptionHandler)
        {
            ExecuteWithRetry(_maximumRetries, _interval, action, exceptionHandler);
        }

        /// <summary>
        /// ExecuteWithRetry
        /// </summary>
        /// <param name="numberOfRetries"></param>
        /// <param name="interval"></param>
        /// <param name="action"></param>
        /// <param name="exceptionHandler"></param>
        /// <remarks>
        /// Funct: Exception exception, int numberOfRetries, int remainingRetries, RetryAction
        /// </remarks>
        public virtual void ExecuteWithRetry(int maximumRetries, TimeSpan interval, Action action, Func<Exception, int, int, RetryResponse> exceptionHandler)
        {
            var remainingRetries = maximumRetries;
            var consumedRetries = 0;
            while (true)
            {
                try
                {
                    action();
                    break;
                }
                catch (Exception ex)
                {
                    if (exceptionHandler != null)
                    {
                        var retryResponse = exceptionHandler(ex, consumedRetries, remainingRetries);
                        if (retryResponse.Action == RetryAction.ThrowException)
                        {
                            if (retryResponse.OverrideException != null)
                                throw retryResponse.OverrideException;
                            else
                                throw;
                        }

                        if (retryResponse.OverrideInterval.HasValue) interval = retryResponse.OverrideInterval.Value;
                    }

                    remainingRetries -= 1;
                    consumedRetries += 1;

                    if (ex.GetType() == typeof(System.NullReferenceException)) throw;

                    if (remainingRetries >= 0)
                        if (interval > TimeSpan.Zero) Thread.Sleep(interval);
                    else
                        throw;
                }
            }
        }
    }
}
