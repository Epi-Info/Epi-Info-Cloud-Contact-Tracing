#define RunSynchronous 

using System;
using System.Configuration;
using System.Threading.Tasks;
using Epi.Cloud.Common.Configuration;
using StackExchange.Redis;

namespace Epi.Cloud.CacheServices
{
    public abstract class RedisCache
    {
        private static string _cacheConnectionString;

        private static string CacheConnectionString()
        {
            var cacheConnectionStringKey = ConfigurationHelper.GetEnvironmentResourceKey("CacheConnectionString");
            _cacheConnectionString = ConfigurationManager.ConnectionStrings[cacheConnectionStringKey].ConnectionString;
            return _cacheConnectionString;
        }

        public RedisCache()
        {
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

        private static readonly Lazy<ConnectionMultiplexer> LazyConnection
          = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(CacheConnectionString()));

        public static ConnectionMultiplexer Connection
        {
            get
            {
                return LazyConnection.Value;
            }
        }

        protected async Task<bool> KeyExists(string prefix, string key)
        {
            key = (prefix + key).ToLowerInvariant();
            try
            {
#if RunSynchronous
                return Cache.KeyExists(key);
#else
                return await Cache.KeyExistsAsync(key);
#endif
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        protected async Task<string> Get(string prefix, string key)
        {
            key = (prefix + key).ToLowerInvariant();
            try
            {
#if RunSynchronous
                var redisValue = Cache.StringGet(key);
                if (redisValue.HasValue)
                {
                    Cache.KeyExpire(key, new TimeSpan(0, 5, 0));
                }
#else
                var redisValue = await Cache.StringGetAsync(key);
                if (redisValue.HasValue)
                {
                    Cache.KeyExpireAsync(key, new TimeSpan(0, 5, 0));
                }
#endif
                return redisValue;
            }
            catch (Exception ex)
            {
                return (string)null;
            }
        }

        protected async Task<bool> Set(string prefix, string key, string value)
        {
            key = (prefix + key).ToLowerInvariant();
            try
            {
#if RunSynchronous
                return Cache.StringSet(key, value, new TimeSpan(0, 5, 0));
#else
                return await Cache.StringSetAsync(key, value);
                //return await Cache.StringSetAsync(key, value, new TimeSpan(0, 5, 0));
#endif
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        protected async void Delete(string prefix, string key)
        {
            key = (prefix + key).ToLowerInvariant();
            try
            {
#if RunSynchronous
                Cache.KeyDelete(key);
#else
                await Cache.KeyDeleteAsync(key);
#endif
            }
            catch (Exception ex)
            {
            }
        }
    }
}
