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

    
    public class AppendStreamClient
    {
        IDatabase _db;

        public AppendStreamClient(IDatabase db)
        {
            _db = db;
        }

        public async Task<string> Read(string key)
        {
            var value = await _db.StringGetAsync(key);
            return value;
        }

        public async Task<AppendResult> Append(AppendStreamHandle handle, string val)
        {
            var currString = (string)await _db.StringGetAsync(handle.Key);
            if (currString != null && handle.Offset != currString.Length) return new AppendResult();

            await Task.Delay(10);

            var newLength = await _db.StringAppendAsync(handle.Key, val);

            return new AppendResult(handle.WithNewOffset(newLength));
        }
    }


    public struct AppendStreamHandle
    {
        public readonly string Key;
        public readonly long Offset;

        public AppendStreamHandle(string key, long offset = 0)
        {
            Key = key;
            Offset = offset;
        }

        internal AppendStreamHandle WithNewOffset(long newOffset)
            => new AppendStreamHandle(Key, newOffset);
    }


    public struct AppendResult
    {
        public readonly bool Success;
        public readonly AppendStreamHandle? Next;

        public AppendResult(AppendStreamHandle next)
        {
            Success = true;
            Next = next;
        }

        public AppendResult(object _ = null)
        {
            Success = false;
            Next = null;
        }

        public static implicit operator AppendStreamHandle(AppendResult result)
            => result.Next.Value;
    }



}
