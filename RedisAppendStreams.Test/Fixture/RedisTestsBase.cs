using StackExchange.Redis;
using System.Threading.Tasks;
using Xunit;

namespace RedisAppendStreams.Test
{

    public abstract class RedisTestsBase : IClassFixture<RedisFixture>, IAsyncLifetime
    {
        string _dockerHost;
        int _redisPort;

        RedisFixture _fx;
        IConnectionMultiplexer _redisMultiplexer;
        
        protected IDatabase Redis { get; private set; }
        protected AppendStreamClient Client { get; private set; }
        
        public RedisTestsBase(RedisFixture fx)
        {
            _fx = fx;
            _dockerHost = "localhost";
            _redisPort = 6379;
        }

        async Task IAsyncLifetime.InitializeAsync()
        {
            _redisMultiplexer = await ConnectionMultiplexer.ConnectAsync($"{_dockerHost}:{_redisPort}");
            Client = new AppendStreamClient(_redisMultiplexer);
            Redis = _redisMultiplexer.GetDatabase();
        }
        
        async Task IAsyncLifetime.DisposeAsync()
        {
            _redisMultiplexer.Dispose();
        }

    }

}
