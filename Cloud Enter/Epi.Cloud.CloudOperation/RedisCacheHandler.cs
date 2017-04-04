using System;
using StackExchange.Redis;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epi.Cloud.CloudOperation
{
    public class RedisCacheHandler
    {
        //static void Main(string[] args)
        //{
        //    RedisCacheHandler x = new RedisCacheHandler();
        //    var Test = x.ClearCache();
        //}
        private static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            string redisCacheName = ConfigurationManager.AppSettings["redisCacheName"];
            string redisCachePassword = ConfigurationManager.AppSettings["redisCachePassword"];
            return ConnectionMultiplexer.Connect(redisCacheName + ",abortConnect=false,ssl=true,password=" + redisCachePassword);
        });

        public static ConnectionMultiplexer Connection
        {
            get
            {
                return lazyConnection.Value;
            }
        }
        public bool ClearCache()
        {
            IDatabase cache = Connection.GetDatabase();
            try
            {
                var endpoints = Connection.GetEndPoints(true);
                foreach (var endpoint in endpoints)
                {
                    var server = Connection.GetServer(endpoint);

                    var keys = server.Keys();
                    foreach (var key in keys)
                    {
                        //server.FlushAllDatabases();
                        Console.WriteLine("Removing Key {0} from cache", key.ToString());
                        cache.KeyDelete(key);
                    } 
                }
                return true;

            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
