using System;
using System.Threading.Tasks;
using Xunit;
using LanguageExt;
using System.Linq;
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
            h = await Client.Append(h, "12345").ExpectOK();
            h = await Client.Append(h, "6789").ExpectOK();

            var val = (string)await Redis.StringGetAsync(h.Key);
            val.ShouldBe("123456789");
        }



        [Fact]
        public async Task Client_Appends_ToExistingString()
        {
            await Redis.StringSetAsync("flap", "12345");
            
            var h = new AppendStreamHandle("flap", 5);            
            h = await Client.Append(h, "6789").ExpectOK();

            var val = (string)await Redis.StringGetAsync(h.Key);
            val.ShouldBe("123456789");
        }




        [Fact]
        public async Task Client_Appends_AndReadsStrings()
        {
            var h = new AppendStreamHandle("oof");
            h = await Client.Append(h, "12345").ExpectOK();
            h = await Client.Append(h, "6789").ExpectOK();

            var result = await Client.Read(h.Key);
            result.ShouldBe("123456789");
        }

        //handle without offset can read
        //but cant write

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
            h = await Client.Append(h, "abcdef").ExpectOK();
            
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
