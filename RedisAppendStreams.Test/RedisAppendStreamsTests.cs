using Docker.DotNet;
using Docker.DotNet.Models;
using System;
using System.Threading.Tasks;
using Xunit;
using LanguageExt;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using StackExchange.Redis;

namespace RedisAppendStreams.Test
{
    public class RedisAppendStreamsTests : RedisTestsBase
    {
        public RedisAppendStreamsTests(RedisFixture fx) : base(fx)
        {
        }

        [Fact]
        public async Task Redis_IsPingable()
        {
            await Redis.PingAsync();
        }


        [Fact]
        public async Task Client_ReadsString()
        {
            await Redis.StringSetAsync("hello", "123456");
            
            var client = new AppendStreamClient(Redis);

            throw new NotImplementedException();
        }

    }


    public class AppendStreamClient
    {
        IDatabase _db;

        public AppendStreamClient(IDatabase db)
        {
            _db = db;
        }
    }

}
