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

        public T ExecuteWithRetry<T>(Func<T> action)
        {
            return ExecuteWithRetry<T>(_numberOfRetries, _interval, action);
        }

        public T ExecuteWithRetry<T>(int numberOfRetries, TimeSpan interval, Func<T> action)
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
                    numberOfRetries -= 1;
                    if (numberOfRetries > 0)
                        Thread.Sleep(interval);
                    else
                        throw;
                }
            }
            return result;
        }
        public void ExecuteWithRetry(Action action)
        {
            ExecuteWithRetry(_numberOfRetries, _interval, action);
        }

        public void ExecuteWithRetry(int numberOfRetries, TimeSpan interval, Action action)
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
                    else
                        throw;
                }
            }
        }
    }
}
