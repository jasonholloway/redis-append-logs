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
            
            var result = await Client.ReadFrom(new LogRef("hello"));

            result.Success.ShouldBeTrue();
            result.Value.ShouldBe("123456");
        }


        [Fact]
        public async Task Client_AppendsStrings()
        {
            var @ref = new LogRef("blah", 0);
            @ref = await Client.Append(@ref, "12345").ExpectOK();
            @ref = await Client.Append(@ref, "6789").ExpectOK();

            var val = (string)await Redis.StringGetAsync(@ref.Key);
            val.ShouldBe("123456789");
        }

        
        [Fact]
        public async Task Client_Appends_ToExistingString()
        {
            await Redis.StringSetAsync("flap", "12345");
            
            var @ref = new LogRef("flap", 5);            
            @ref = await Client.Append(@ref, "6789").ExpectOK();

            var val = (string)await Redis.StringGetAsync(@ref.Key);
            val.ShouldBe("123456789");
        }
        

        [Fact]
        public async Task Client_Appends_AndReadsStrings()
        {
            var @ref = new LogRef("oof", 0);
            @ref = await Client.Append(@ref, "12345").ExpectOK();
            @ref = await Client.Append(@ref, "6789").ExpectOK();

            var result = await Client.ReadFrom(@ref.WithoutOffset());

            result.Success.ShouldBeTrue();
            result.Value.ShouldBe("123456789");
        }
        

        [Fact]
        public async Task AppendingFails_WhenOffsetIsGazumped()
        {
            var @ref = new LogRef("squawk!", 0);

            var result1 = await Client.Append(@ref, "12345");
            result1.Success.ShouldBeTrue();
            
            var result2 = await Client.Append(@ref, "9876");
            result2.Success.ShouldBeFalse();
        }
        

        [Fact]
        public async Task OnlyOneAppenderCanWin_GivenCompetition()
        {
            var @ref = new LogRef("piffle", 0);
            @ref = await Client.Append(@ref, "abcdef").ExpectOK();
            
            var results = await Enumerable.Range(0, 20)
                                .Map(async _ => {
                                    await Task.Delay(10);
                                    return await Client.Append(@ref, "ghij");
                                })
                                .WhenAll();

            results.Count(r => r.Success).ShouldBe(1);
        }
        


        [Fact]
        public async Task Reading_WithoutOffset_ReturnsFullLog_AndOffsetToo()
        {
            var @ref = new LogRef("schnooo");

            await Redis.StringSetAsync(@ref.Key, "12345678");

            @ref = await Client.ReadFrom(@ref);
            @ref.Offset.ShouldBe(8);

            @ref = await Client.Append(@ref, "9");
            @ref.Offset.ShouldBe(9);

            var result = (string)await Redis.StringGetAsync(@ref.Key);
            result.ShouldBe("123456789");
        }

        

        [Fact]
        public async Task Reading_WithoutOffset_Throws()
        {
            var @ref = new LogRef("grargrgrgrgh");

            await Should.ThrowAsync<AppendLogException>(async () => {
                await Client.Append(@ref, "asdadadsad");
            });
        }



        [Fact]
        public async Task ReadingWithOffset_ReturnsFromOffsetOnwards()
        {
            await Redis.StringSetAsync("squeeak", "0123456789");
            
            var @ref = new LogRef("squeeak", 5);

            var result = await Client.ReadFrom(@ref);

            result.Value.ShouldBe("56789");
            result.Next.Offset.ShouldBe(10);
        }



    }
        
}
