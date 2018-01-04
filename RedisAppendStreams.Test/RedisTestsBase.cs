﻿using StackExchange.Redis;
using System.Threading.Tasks;
using Xunit;

namespace RedisAppendStreams.Test
{

    public abstract class RedisTestsBase : IClassFixture<RedisFixture>, IAsyncLifetime
    {
        string _dockerHost;
        int _redisPort;

        RedisFixture _fx;
        ConnectionMultiplexer _multiplexer;

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
            _multiplexer = await ConnectionMultiplexer.ConnectAsync($"{_dockerHost}:{_redisPort}");
            Redis = _multiplexer.GetDatabase();
            Client = new AppendStreamClient(Redis);
        }
        
        async Task IAsyncLifetime.DisposeAsync()
        {
            _multiplexer.Dispose();
        }

    }

}