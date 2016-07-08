#define RunSynchronous 

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Epi.Cloud.Common.Configuration;
using StackExchange.Redis;

namespace Epi.Cloud.CacheServices
{
    public abstract class RedisCache
    {
        private static string _cacheConnectionString;

        private static int _numberOfRetries = 3;
        private static TimeSpan _interval = TimeSpan.FromMilliseconds(100);

        private static Dictionary<string, CacheStats> _statistics = new Dictionary<string, CacheStats>();

        class CacheStats
        {
            public string Key { get; set; }
            public int Hits { get; set; }
            public int Misses { get; set; }
            public int GetExceptions { get; set; }

            public int SetSuccesses { get; set; }
            public int SetFailures { get; set; }
            public int SetExceptions { get; set; }

            public int ExistHits { get; set; }
            public int ExistMisses { get; set; }
            public int ExistExceptions { get; set; }

            public Exception LastGetException { get; set; }
            public Exception LastSetException { get; set; }
            public Exception LastExistException { get; set; }
        }

        private static string CacheConnectionString()
        {
            var cacheConnectionStringKey = ConfigurationHelper.GetEnvironmentResourceKey("CacheConnectionString");
            _cacheConnectionString = ConfigurationManager.ConnectionStrings[cacheConnectionStringKey].ConnectionString;
            return _cacheConnectionString;
        }

        private enum StatType
        {
            Hit,
            Miss,
            Set,
            SetFail,
            ExistHit,
            ExistMiss,
            GetException,
            SetException,
            ExistException,
        }

        public RedisCache()
        {
        }

        private void UpdateStats(string key, StatType statType, Exception exception = null)
        {
            lock (_statistics)
            {
                CacheStats stats;
                if (!_statistics.TryGetValue(key, out stats))
                {
                    switch (statType)
                    {
                        case StatType.Hit:
                            stats = new CacheStats { Key = key, Hits = 1 };
                            break;
                        case StatType.Miss:
                            stats = new CacheStats { Key = key, Misses = 1 };
                            break;
                        case StatType.GetException:
                            stats = new CacheStats { Key = key, GetExceptions = 1 };
                            if (exception != null) stats.LastGetException = exception;
                            break;
                        case StatType.Set:
                            stats = new CacheStats { Key = key, SetSuccesses = 1 };
                            break;
                        case StatType.SetFail:
                            stats = new CacheStats { Key = key, SetFailures = 1 };
                            break;
                        case StatType.SetException:
                            stats = new CacheStats { Key = key, SetExceptions = 1 };
                            if (exception != null) stats.LastSetException = exception;
                            break;
                        case StatType.ExistHit:
                            stats = new CacheStats { Key = key, ExistHits = 1 };
                            break;
                        case StatType.ExistMiss:
                            stats = new CacheStats { Key = key, ExistMisses = 1 };
                            break;
                        case StatType.ExistException:
                            stats = new CacheStats { Key = key, ExistExceptions = 1 };
                            if (exception != null) stats.LastExistException = exception;
                            break;

                    }

                    _statistics.Add(key, stats);
                }
                else
                {
                    switch (statType)
                    {
                        case StatType.ExistHit:
                            stats.ExistHits++;
                            break;
                        case StatType.ExistMiss:
                            stats.ExistMisses++;
                            break;
                        case StatType.Hit:
                            stats.Hits++;
                            break;
                        case StatType.Miss:
                            stats.Misses++;
                            break;
                        case StatType.GetException:
                            stats.GetExceptions++;
                            if (exception != null) stats.LastGetException = exception;
                            break;
                        case StatType.Set:
                            stats.SetSuccesses++;
                            break;
                        case StatType.SetFail:
                            stats.SetFailures++;
                            break;
                        case StatType.SetException:
                            stats.SetExceptions++;
                            if (exception != null) stats.LastSetException = exception;
                            break;
                    }
                }
            }
        }

        private static IDatabase Cache
        {
            get
            {
                try
                {
                    return Connection.GetDatabase();
                }
                catch (Exception ex)
                {
                    ConnectionMultiplexer.Connect(CacheConnectionString());
                    return Connection.GetDatabase();
                }
            }
        }

        private static Lazy<ConnectionMultiplexer> LazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            var redisConfig = ConfigurationOptions.Parse(CacheConnectionString());
            redisConfig.SyncTimeout = 3000;
            return ConnectionMultiplexer.Connect(redisConfig);
        });

        //private static readonly Lazy<ConnectionMultiplexer> LazyConnection
        //  = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(CacheConnectionString()));

        public static ConnectionMultiplexer Connection
        {
            get
            {
                return LazyConnection.Value;
            }
        }

        private T ExecuteWithRetry<T>(Func<T> action)
        {
            return ExecuteWithRetry<T>(_numberOfRetries, _interval, action);
        }

        private T ExecuteWithRetry<T>(int numberOfRetries, TimeSpan interval, Func<T> action)
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

        protected async Task<bool> KeyExists(string prefix, string key)
        {
            key = (prefix + key).ToLowerInvariant();
            bool exists = false;
            try
            {
#if RunSynchronous
                exists = ExecuteWithRetry(() => Cache.KeyExists(key));
                if (exists)
                {
                    Cache.KeyExpire(key, new TimeSpan(1, 0, 0));
                }
#else
                exists = await Cache.KeyExistsAsync(key);
                if (exists)
                {
                    Cache.KeyExpireAsync(key, new TimeSpan(1, 0, 0));
                }
#endif
                UpdateStats(key, exists ? StatType.ExistHit : StatType.ExistMiss);
                return exists;
            }
            catch (Exception ex)
            {
                UpdateStats(key, exists ? StatType.ExistHit : StatType.ExistMiss);
                return false;
            }
        }

        protected async Task<string> Get(string prefix, string key)
        {
            key = (prefix + key).ToLowerInvariant();
            try
            {
#if RunSynchronous
                var redisValue = ExecuteWithRetry(() => Cache.StringGet(key));
                if (redisValue.HasValue)
                {
                    Cache.KeyExpire(key, new TimeSpan(1, 0, 0));
                    UpdateStats(key, StatType.Hit);
                }
#else
                var redisValue = await Cache.StringGetAsync(key);
                if (redisValue.HasValue)
                {
                    UpdateStats(key, StatType.Hit);
                    Cache.KeyExpireAsync(key, new TimeSpan(1, 0, 0));
                }
#endif
                else
                {
                    UpdateStats(key, StatType.Miss);
                }
                return redisValue;
            }
            catch (Exception ex)
            {
                UpdateStats(key, StatType.GetException, ex);
                return (string)null;
            }
        }

        protected async Task<bool> Set(string prefix, string key, string value)
        {
            key = (prefix + key).ToLowerInvariant();
            try
            {
                bool isSuccesful = false;
#if RunSynchronous
                isSuccesful = ExecuteWithRetry(() => Cache.StringSet(key, value, new TimeSpan(1, 0, 0)));
#else
                //isSuccesful = await Cache.StringSetAsync(key, value);
                isSuccesful = await Cache.StringSetAsync(key, value, new TimeSpan(1, 0, 0));
#endif
                UpdateStats(key, isSuccesful ? StatType.Set : StatType.SetFail);
                return isSuccesful;
            }
            catch (Exception ex)
            {
                UpdateStats(key, StatType.SetException, ex);
                return false;
            }
        }

        protected async void Delete(string prefix, string key)
        {
            key = (prefix + key).ToLowerInvariant();
            try
            {
#if RunSynchronous
                ExecuteWithRetry(() => Cache.KeyDelete(key));
#else
                await Cache.KeyDeleteAsync(key);
#endif
            }
            catch (Exception ex)
            {
            }
        }

        protected void DeleteAllKeys(string prefix, Action<RedisKey> onDelete = null)
        {
            var endpoints = Connection.GetEndPoints(true);
            foreach (var endpoint in endpoints)
            {
                var server = Connection.GetServer(endpoint);
                var keys = server.Keys(0, prefix.ToLowerInvariant() + "*");
                foreach (var key in keys)
                {
                    Cache.KeyDelete(key);
                    if (onDelete != null) onDelete(key);
                }
            }
        }

        public void ClearCache()
        {
            var endpoints = Connection.GetEndPoints(true);
            foreach (var endpoint in endpoints)
            {
                var server = Connection.GetServer(endpoint);
                server.FlushDatabase();
            }
        }

        public void ClearAllDatabases()
        {
            var endpoints = Connection.GetEndPoints(true);
            foreach (var endpoint in endpoints)
            {
                var server = Connection.GetServer(endpoint);
                server.FlushAllDatabases();
            }
        }
    }
}
