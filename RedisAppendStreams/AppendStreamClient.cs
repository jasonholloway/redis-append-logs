using StackExchange.Redis;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace RedisAppendStreams
{
    public class AppendStreamClient
    {
        IConnectionMultiplexer _redis;

        public AppendStreamClient(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task<string> Read(string key)
        {
            var db = _redis.GetDatabase();
            var value = await db.StringGetAsync(key);
            return value;
        }

        public async Task<AppendResult> Append(AppendStreamHandle handle, string val)
        {
            var db = _redis.GetDatabase();

            var newOffset = (long)await db.ScriptEvaluateAsync(luaAppend, new RedisKey[] { handle.Key }, new RedisValue[] { handle.Offset, val });
            
            if (newOffset < 0) return new AppendResult();

            return new AppendResult(handle.WithOffset(newOffset));            
        }

        const string luaAppend = @"
            local stringKey = KEYS[1]
            local appendage = ARGV[2]

            local currOffset = tonumber(ARGV[1])
            local currLength = redis.call(""STRLEN"", stringKey)

            if currOffset == currLength then 
                local newOffset = redis.call(""APPEND"", stringKey, appendage)
                return newOffset
            else
                return -1
            end
        ";
        
    }



}
