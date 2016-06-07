using System;
using System.Configuration;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Epi.Cloud.CacheServices
{
    public abstract class RedisCache
    {
        private readonly string _prefix;
        private static string _cacheConnectionString;

        private static string CacheConnectionString()
        {
            var environment = ConfigurationManager.AppSettings["Environment"];
            var environmentSuffix = environment != null ? "@" + environment : string.Empty;
            var cacheConnectionStringName = "CacheConnectionString" + environmentSuffix;
            _cacheConnectionString = ConfigurationManager.ConnectionStrings[cacheConnectionStringName].ConnectionString;
            return _cacheConnectionString;
        }

        public RedisCache()
        {
        }

        public RedisCache(string prefix)
        {
            _prefix = prefix;
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

        protected async Task<string> Get(string key)
        {
            key = _prefix + key;
            if (key == null) throw new ArgumentNullException("key");
            try
            {
                return await Cache.StringGetAsync(key);
            }
            catch (Exception ex)
            {
                return (string)null;
            }
        }

        protected async Task<bool> Set(string key, string value)
        {
            key = _prefix + key;
            if (key == null) throw new ArgumentNullException("key");
            try
            {
                return await Cache.StringSetAsync(key, value);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        protected async void Delete(string key)
        {
            key = _prefix + key;
            if (key == null) throw new ArgumentNullException("key");
            try
            {
                await Cache.KeyDeleteAsync(key);
            }
            catch (Exception ex)
            {
            }
        }
    }
}
