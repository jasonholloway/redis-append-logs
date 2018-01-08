using System;
using System.Threading.Tasks;
using Xunit;
using LanguageExt;
using System.Linq;
using Shouldly;

namespace RedisAppendLogs.Test
{
    public class RedisAppendLogTests : RedisTestsBase
    {
        public RedisAppendLogTests(RedisFixture fx) : base(fx)
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
            
            var result = await Client.Read(new AppendLogHandle("hello"));

            result.Success.ShouldBeTrue();
            result.Value.ShouldBe("123456");
        }


        [Fact]
        public async Task Client_AppendsStrings()
        {
            var h = new AppendLogHandle("blah", 0);
            h = await Client.Append(h, "12345").ExpectOK();
            h = await Client.Append(h, "6789").ExpectOK();

            var val = (string)await Redis.StringGetAsync(h.Key);
            val.ShouldBe("123456789");
        }

        
        [Fact]
        public async Task Client_Appends_ToExistingString()
        {
            await Redis.StringSetAsync("flap", "12345");
            
            var h = new AppendLogHandle("flap", 5);            
            h = await Client.Append(h, "6789").ExpectOK();

            var val = (string)await Redis.StringGetAsync(h.Key);
            val.ShouldBe("123456789");
        }
        

        [Fact]
        public async Task Client_Appends_AndReadsStrings()
        {
            var h = new AppendLogHandle("oof", 0);
            h = await Client.Append(h, "12345").ExpectOK();
            h = await Client.Append(h, "6789").ExpectOK();

            var result = await Client.Read(h);

            result.Success.ShouldBeTrue();
            result.Value.ShouldBe("123456789");
        }
        

        [Fact]
        public async Task Client_FailsToAppend_WhenOffsetIsGazumped()
        {
            var h1 = new AppendLogHandle("squawk!", 0);

            var result1 = await Client.Append(h1, "12345");
            result1.Success.ShouldBeTrue();
            
            var result2 = await Client.Append(h1, "9876");
            result2.Success.ShouldBeFalse();
        }
        

        [Fact]
        public async Task OnlyOneAppenderCanWin_GivenCompetition()
        {
            var h = new AppendLogHandle("piffle", 0);
            h = await Client.Append(h, "abcdef").ExpectOK();
            
            var results = await Enumerable.Range(0, 20)
                                .Map(async _ => {
                                    await Task.Delay(10);
                                    return await Client.Append(h, "ghij");
                                })
                                .WhenAll();

            results.Count(r => r.Success).ShouldBe(1);
        }
        


        [Fact]
        public async Task HandleWithoutOffset_ReadsAll_AndGainsOffset()
        {
            var h = new AppendLogHandle("schnooo");

            await Redis.StringSetAsync(h.Key, "123456789");

            h = await Client.Read(h);

            h.Offset.ShouldBe(9);
        }

        

        [Fact]
        public async Task HandleWithoutOffset_CantWrite_ThrowsInstead()
        {
            var h = new AppendLogHandle("grargrgrgrgh");

            await Should.ThrowAsync<AppendLogException>(async () => {
                await Client.Append(h, "asdadadsad");
            });
        }



    }
        
}
