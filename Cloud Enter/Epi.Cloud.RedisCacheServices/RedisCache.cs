#define RunSynchronous 

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using Epi.Cloud.Common;
using Epi.Cloud.Common.Configuration;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Epi.Cloud.CacheServices
{
    public abstract class RedisCache
    {
        protected static readonly TimeSpan NoTimeout = TimeSpan.Zero;
        private static readonly TimeSpan InitialTimeout = new TimeSpan(1, 0, 0); // 1 hour
        private static readonly TimeSpan RenewTimeout = new TimeSpan(1, 0, 0); // 1 hour

        protected static JsonSerializerSettings DontSerializeNulls = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

        private static int _numberOfRetries = 3;
        private static TimeSpan _interval = TimeSpan.FromMilliseconds(100);

        private RetryStrategies _retryStrategy = new RetryStrategies(_numberOfRetries, _interval);

        private static Dictionary<string, object> _transientCache = new Dictionary<string, object>();


        public RedisCache()
        {
        }

        #region Statistics

        private static Dictionary<string, CacheStats> _statistics = new Dictionary<string, CacheStats>();

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

        private void UpdateStats(string cacheKey, StatType statType, Exception exception = null)
        {
            lock (_statistics)
            {
                CacheStats stats;
                if (!_statistics.TryGetValue(cacheKey, out stats))
                {
                    switch (statType)
                    {
                        case StatType.Hit:
                            stats = new CacheStats { Key = cacheKey, Hits = 1 };
                            break;
                        case StatType.Miss:
                            stats = new CacheStats { Key = cacheKey, Misses = 1 };
                            break;
                        case StatType.GetException:
                            stats = new CacheStats { Key = cacheKey, GetExceptions = 1 };
                            if (exception != null) stats.LastGetException = exception;
                            break;
                        case StatType.Set:
                            stats = new CacheStats { Key = cacheKey, SetSuccesses = 1 };
                            break;
                        case StatType.SetFail:
                            stats = new CacheStats { Key = cacheKey, SetFailures = 1 };
                            break;
                        case StatType.SetException:
                            stats = new CacheStats { Key = cacheKey, SetExceptions = 1 };
                            if (exception != null) stats.LastSetException = exception;
                            break;
                        case StatType.ExistHit:
                            stats = new CacheStats { Key = cacheKey, ExistHits = 1 };
                            break;
                        case StatType.ExistMiss:
                            stats = new CacheStats { Key = cacheKey, ExistMisses = 1 };
                            break;
                        case StatType.ExistException:
                            stats = new CacheStats { Key = cacheKey, ExistExceptions = 1 };
                            if (exception != null) stats.LastExistException = exception;
                            break;

                    }

                    _statistics.Add(cacheKey, stats);
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
        #endregion // Statistics

        #region Connection

        private static string _cacheConnectionString;

        private static string CacheConnectionString()
        {
            var cacheConnectionStringKey = ConfigurationHelper.GetEnvironmentResourceKey("CacheConnectionString");
            _cacheConnectionString = ConfigurationManager.ConnectionStrings[cacheConnectionStringKey].ConnectionString;
            return _cacheConnectionString;
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

        #endregion // Conection

        protected async Task<bool> KeyExists(string prefix, string key)
        {
            return KeyExists(prefix, key, RenewTimeout).Result;
        }
        protected async Task<bool> KeyExists(string prefix, string key, TimeSpan renewTimeout)
        {
            object transientTemp;
            var cacheKey = (prefix + key).ToLowerInvariant();
            bool exists = false;
            try
            {
#if RunSynchronous
                exists = _retryStrategy.ExecuteWithRetry(() => Cache.KeyExists(cacheKey));

                if (exists)
                {
                    if (renewTimeout != NoTimeout)
                    {
                        Cache.KeyExpire(cacheKey, renewTimeout);
                    }
                }
#else
                exists = _retryStrategy.ExecuteWithRetry(() => Cache.KeyExistsAsync(cacheKey)).Result;
                if (exists)
                {
                    if (renewTimeout != NoTimeout)
                    {
                        var isSuccesful = _retryStrategy.ExecuteWithRetry(() => Cache.KeyExpireAsync(cacheKey, renewTimeout)).Result ;
                    }
                }
#endif
                UpdateStats(cacheKey, exists ? StatType.ExistHit : StatType.ExistMiss);
                return exists;
            }
            catch (Exception ex)
            {
                UpdateStats(cacheKey, StatType.ExistException, ex);

                if (ex.GetType() == typeof(StackExchange.Redis.RedisConnectionException))
                    return _transientCache.TryGetValue(cacheKey, out transientTemp);
                return false;
            }
        }

        protected async Task<string> Get(string prefix, string key)
        {
            return Get(prefix, key, RenewTimeout).Result;
        }

        protected async Task<string> Get(string prefix, string key, TimeSpan renewTimeout)
        {
            object transientTemp;
            var cacheKey = (prefix + key).ToLowerInvariant();
            if (_transientCache.Count > 0)
            {
                var value = _transientCache.TryGetValue(cacheKey, out transientTemp) ? (string)transientTemp : (string)null;
                return value;
            }

            try
            {
#if RunSynchronous
                var redisValue = _retryStrategy.ExecuteWithRetry(() => Cache.StringGet(cacheKey));
                if (redisValue.HasValue)
                {
                    if (renewTimeout != NoTimeout)
                    {
                        Cache.KeyExpire(cacheKey, renewTimeout);
                    }
                    UpdateStats(cacheKey, StatType.Hit);
                }
#else
                var redisValue = _retryStrategy.ExecuteWithRetry(() => Cache.StringGetAsync(cacheKey)).Result;
                if (redisValue.HasValue)
                {
                    UpdateStats(cacheKey, StatType.Hit);
                    if (renewTimeout != NoTimeout)
                    {
                        var isSuccessful = _retryStrategy.ExecuteWithRetry(() => Cache.KeyExpireAsync(cacheKey, renewTimeout)).Result;
                    }
                }
#endif
                else
                {
                    UpdateStats(cacheKey, StatType.Miss);
                }
                return redisValue;
            }
            catch (Exception ex)
            {
                UpdateStats(cacheKey, StatType.GetException, ex);

                if (ex.GetType() == typeof(StackExchange.Redis.RedisConnectionException) || ex.GetType() == typeof(System.NullReferenceException))
                    return _transientCache.TryGetValue(cacheKey, out transientTemp) ? (string)transientTemp : (string)null;
                return (string)null;
            }
        }

        protected async Task<bool> Set(string prefix, string key, string value)
        {
            return Set(prefix, key, value, InitialTimeout).Result;
        }

        protected async Task<bool> Set(string prefix, string key, string value, TimeSpan timeout)
        {
            var cacheKey = ((prefix + key).ToLowerInvariant());
            if (_transientCache.Count > 0)
            {
                _transientCache[cacheKey] = value;
                return true;
            }
            try
            {
                bool isSuccesful = false;
#if RunSynchronous
                isSuccesful = _retryStrategy.ExecuteWithRetry(() => timeout == NoTimeout ? Cache.StringSet((RedisKey)cacheKey, value) : Cache.StringSet((RedisKey)cacheKey, value, timeout));
#else
                isSuccesful =  _retryStrategy.ExecuteWithRetry(() => (timeout == NoTimeout ?  Cache.StringSetAsync(cacheKey, value) : Cache.StringSetAsync(cacheKey, value, timeout))).Result;
#endif
                UpdateStats(cacheKey, isSuccesful ? StatType.Set : StatType.SetFail);
                return isSuccesful;
            }
            catch (Exception ex)
            {
                UpdateStats(cacheKey, StatType.SetException, ex);
                if (ex.GetType() == typeof(StackExchange.Redis.RedisConnectionException) || ex.GetType() == typeof(System.NullReferenceException))
                {
                    _transientCache[cacheKey] = value;
                    return true;
                }
                return false;
            }
        }

        protected async void Delete(string prefix, string key)
        {
            var cacheKey = (prefix + key).ToLowerInvariant();
            if (_transientCache.Count > 0)
            {
                _transientCache.Remove(cacheKey);
                return;
            }
            try
            {
#if RunSynchronous
                _retryStrategy.ExecuteWithRetry(() => Cache.KeyDelete(cacheKey));
#else
                var isSuccesful = _retryStrategy.ExecuteWithRetry(() => Cache.KeyDeleteAsync(cacheKey)).Result;
#endif
            }
            catch (Exception ex)
            {
            }
        }

        protected void DeleteAllKeys(string prefix, Action<RedisKey> onDelete = null)
        {
            if (_transientCache.Count > 0)
            {
                _transientCache.Clear();
                return;
            }
            var endpoints = Connection.GetEndPoints(true);
            foreach (var endpoint in endpoints)
            {
                var server = Connection.GetServer(endpoint);
                var cacheKeys = server.Keys(0, prefix.ToLowerInvariant() + "*");
                foreach (var cacheKey in cacheKeys)
                {
                    _retryStrategy.ExecuteWithRetry(() => Cache.KeyDelete(cacheKey));
                    onDelete?.Invoke(cacheKey);
                }
            }
        }

        public void ClearCache()
        {
            if (_transientCache.Count > 0)
            {
                _transientCache.Clear();
                return;
            }
            var endpoints = Connection.GetEndPoints(true);
            foreach (var endpoint in endpoints)
            {
                var server = Connection.GetServer(endpoint);
                server.FlushDatabase();
            }
        }

        public void ClearAllDatabases()
        {
            if (_transientCache.Count > 0)
            {
                _transientCache.Clear();
                return;
            }
            var endpoints = Connection.GetEndPoints(true);
            foreach (var endpoint in endpoints)
            {
                var server = Connection.GetServer(endpoint);
                server.FlushAllDatabases();
            }
        }
    }
}
