using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Epi.Cloud.CacheServices
{
    public abstract class RedisCache
    {
        private readonly string _prefix;
        private static string CacheConnectionString = "";

        public RedisCache(string prefix)
        {
            _prefix = prefix;
        }

        private static IDatabase Cache
        {
            get
            {
                return Connection.GetDatabase();
            }
        }

        private static readonly Lazy<ConnectionMultiplexer> LazyConnection
          = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(CacheConnectionString));

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
            return await Cache.StringGetAsync(key);
        }

        protected async Task<bool> Set(string key, string value)
        {
            key = _prefix + key;
            if (key == null) throw new ArgumentNullException("key");
            return await Cache.StringSetAsync(key, value);
        }

        protected async void Delete(string key)
        {
            key = _prefix + key;
            if (key == null) throw new ArgumentNullException("key");
            await Cache.KeyDeleteAsync(key);
        }
    }
}
