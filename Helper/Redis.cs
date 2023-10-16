using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Helper
{
    public static class Redis
    {
        private static string redisHost = "redis-17213.c282.east-us-mz.azure.cloud.redislabs.com";
        private static int redisPort = 17213;
        private static string redisPassword = "Rjkr8q6SwbwSpuqcUE6VXDpmv8Y8KSVu";
        private static ConnectionMultiplexer redis;
        
        static Redis()
        {
            var redisConfig = new ConfigurationOptions
            {
                EndPoints = { $"{redisHost}:{redisPort}" },
                Password = redisPassword,
                Ssl = false,
                AbortOnConnectFail = false // optional
            };
            redis = ConnectionMultiplexer.Connect(redisConfig);
        }

        public static string GetRedisVal(string key)
        {
            var redisDb = redis.GetDatabase();
            var cacheValue = redisDb.StringGet(key);
            if (cacheValue.HasValue)
            {
                return cacheValue;
            }
            return string.Empty;
        }

        public static void SetRedisVal(string key, string value, int duration)
        {
            var redisDb = redis.GetDatabase();
            var cacheDuration = TimeSpan.FromMinutes(duration);
            redisDb.StringSet(key, value, cacheDuration);
        }
    }
}
