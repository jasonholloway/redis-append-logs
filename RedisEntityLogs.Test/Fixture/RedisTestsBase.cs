using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using Xunit;

namespace RedisEntityLogs.Test
{

    public abstract class RedisTestsBase : IClassFixture<RedisFixture>, IAsyncLifetime
    {
        string _redisConfig;
        RedisFixture _fx;
        IConnectionMultiplexer _redisMultiplexer;
        
        protected IDatabase Redis { get; private set; }
        protected LogClient Client { get; private set; }
        
        public RedisTestsBase(RedisFixture fx)
        {
            _fx = fx;
            _redisConfig = "127.0.0.1:16379,127.0.0.1:26379";
        }

        async Task IAsyncLifetime.InitializeAsync()
        {
            _redisMultiplexer = await ConnectionMultiplexer.ConnectAsync(_redisConfig);
            Client = new LogClient(_redisMultiplexer);
            Redis = _redisMultiplexer.GetDatabase();
        }
        
        async Task IAsyncLifetime.DisposeAsync()
        {
            _redisMultiplexer.Dispose();
        }

    }

}
