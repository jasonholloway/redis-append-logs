using StackExchange.Redis;
using System.Threading.Tasks;

namespace RedisAppendStreams
{
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
            var newOffset = (long)await _db.ScriptEvaluateAsync(luaAppend, new RedisKey[] { handle.Key }, new RedisValue[] { handle.Offset, val });

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
