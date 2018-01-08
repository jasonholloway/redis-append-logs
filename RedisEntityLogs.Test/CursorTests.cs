using System.Threading.Tasks;
using Xunit;
using Shouldly;

namespace RedisEntityLogs.Test
{
    public class CursorTests : RedisTestsBase
    {
        public CursorTests(RedisFixture fx) : base(fx)
        {
        }
        
        [Fact]
        public async Task Cursor_WritesAndWrites()
        {
            var cursor = Client.CreateCursor("flumpt");

            await cursor.CatchUp();

            await cursor.Write("abc");
            await cursor.Write("defghi");
            await cursor.Write("jklmno");
            await cursor.Write("pqrstuv");
            await cursor.Write("wxyz");

            var result = (string)await Redis.StringGetAsync("flumpt");
            result.ShouldBe("abcdefghijklmnopqrstuvwxyz");
        }

        [Fact]
        public async Task Cursor_WritesFromArbitraryPosition()
        {
            await Redis.StringSetAsync("Grarrrr!", "1234");

            var cursor = Client.CreateCursor(new LogRef("Grarrrr!", 4));

            await cursor.Write("56789");

            var result = (string)await Redis.StringGetAsync("Grarrrr!");
            result.ShouldBe("123456789");
        }
        

        [Fact]
        public async Task Cursor_CatchesUp()
        {
            await Redis.StringSetAsync("Shlarrpt", "1234");
            
            var cursor = Client.CreateCursor("Shlarrpt");

            var result = await cursor.CatchUp();
            result.ShouldBe("1234");
        }

    }

}
