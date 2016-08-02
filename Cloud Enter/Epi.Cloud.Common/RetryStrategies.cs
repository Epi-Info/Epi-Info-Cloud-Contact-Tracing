using System;
using System.Threading;

namespace Epi.Cloud.Common
{
    public class RetryStrategies
    {
        private int _numberOfRetries = 3;
        private TimeSpan _interval = TimeSpan.FromMilliseconds(100);

        public RetryStrategies()
        {
        }
        public RetryStrategies(int numberOfRetries, TimeSpan interval)
        {
            _numberOfRetries = numberOfRetries;
            _interval = interval;
        }

        public virtual T ExecuteWithRetry<T>(Func<T> action, Func<Exception, T> exceptionHandeler = null)
        {
            return ExecuteWithRetry<T>(_numberOfRetries, _interval, action, exceptionHandeler);
        }

        public virtual T ExecuteWithRetry<T>(int numberOfRetries, TimeSpan interval, Func<T> action, Func<Exception, T> exceptionHandeler = null)
        {
            T result = default(T);
            while (true)
            {
                try
                {
                    result = action();
                    break;
                }
                catch (Exception ex)
                {
                    if (ex.GetType() == typeof(System.NullReferenceException)) throw;

                    numberOfRetries -= 1;
                    if (numberOfRetries > 0)
                        Thread.Sleep(interval);
                    else
                    {
                        if (exceptionHandeler == null) throw;
                        result = exceptionHandeler(ex);
                        if (result == null) throw;
                    }
                }
            }
            return result;
        }
        public virtual void ExecuteWithRetry(Action action, Func<Exception, bool> exceptionHandeler = null)
        {
            ExecuteWithRetry(_numberOfRetries, _interval, action, exceptionHandeler);
        }

        public virtual void ExecuteWithRetry(int numberOfRetries, TimeSpan interval, Action action, Func<Exception, bool> exceptionHandeler = null)
        {
            while (true)
            {
                try
                {
                    action();
                    break;
                }
                catch (Exception ex)
                {
                    numberOfRetries -= 1;
                    if (numberOfRetries > 0)
                        Thread.Sleep(interval);
                    else if (exceptionHandeler == null || exceptionHandeler(ex) == false)
                        throw;
                }
            }
        }
    }
}
