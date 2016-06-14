//#define UseAsync

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
            //return false;
            var keyExists = false;
            key = (prefix + key).ToLowerInvariant();
            try
            {
#if UseAsync
                var task = Cache.KeyExistsAsync(key);
                keyExists = await task;
#else
                keyExists = Cache.KeyExists(key);
#endif
                return keyExists;
            }
            catch (Exception ex)
            {
                return keyExists;
            }
        }

        protected async Task<string> Get(string prefix, string key)
        {
 //           return null;
            key = (prefix + key).ToLowerInvariant();
            try
            {
#if UseAsync
                var task = Cache.StringGetAsync(key);
                var redisValue = await task;
#else
                var redisValue = Cache.StringGet(key);
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
            //return false;
            var isSuccessful = false;
            key = (prefix + key).ToLowerInvariant();
            try
            {
#if UseAsync
                var task = Cache.StringSetAsync(key, value);
                isSuccessful = await task;
#else
                isSuccessful = Cache.StringSet(key, value);
#endif
                return isSuccessful;
            }
            catch (Exception ex)
            {
                return isSuccessful;
            }
        }

        protected async void Delete(string prefix, string key)
        {
            key = (prefix + key).ToLowerInvariant();
            try
            {
#if UseAsync
                await Cache.KeyDeleteAsync(key);
#else
                Cache.KeyDelete(key);
#endif
            }
            catch (Exception ex)
            {
            }
        }
    }
}
