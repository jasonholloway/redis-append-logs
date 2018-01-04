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
using Shouldly;

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
            
            var val = await Client.Read("hello");

            val.ShouldBe("123456");
        }


        [Fact]
        public async Task Client_AppendsStrings()
        {
            var h = new AppendStreamHandle("blah");
            h = await Client.Append(h, "12345");
            h = await Client.Append(h, "6789");

            var val = (string)await Redis.StringGetAsync(h.Key);
            val.ShouldBe("123456789");
        }
        

        [Fact]
        public async Task Client_FailsToAppend_WhenOffsetIsGazumped()
        {
            var h1 = new AppendStreamHandle("squawk!");

            var result1 = await Client.Append(h1, "12345");
            result1.Success.ShouldBeTrue();
            
            var result2 = await Client.Append(h1, "9876");
            result2.Success.ShouldBeFalse();
        }
        

        [Fact]
        public async Task OnlyOneAppenderCanWin_GivenCompetition()
        {
            var h = new AppendStreamHandle("piffle");
            h = await Client.Append(h, "abcdef");
            
            var results = await Enumerable.Range(0, 20)
                                .Map(async _ => {
                                    await Task.Delay(10);
                                    return await Client.Append(h, "ghij");
                                })
                                .WhenAll();

            results.Count(r => r.Success).ShouldBe(1);
        }
        
    }
        
}
