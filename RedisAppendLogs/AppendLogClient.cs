using StackExchange.Redis;
using System.Linq;
using System.Threading.Tasks;

namespace RedisAppendLogs
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
        

        public async Task<AppendLogResult> Append(AppendLogHandle handle, string val)
        {
            await _luaAppend.EnsureLoaded(_redis);
            
            var newOffset = (long)await _redis.GetDatabase().ScriptEvaluateAsync(
                                                                _luaAppend.LoadedLuaScript, 
                                                                new { key = (RedisKey)handle.Key, offset = handle.Offset, val }
                                                                );            
            if (newOffset < 0) return new AppendLogResult();

            return new AppendLogResult(handle.WithOffset(newOffset));            
        }


        Script _luaAppend = new Script(@"
            local currLength = redis.call(""STRLEN"", @key)

            if tonumber(@offset) == currLength then 
                local newOffset = redis.call(""APPEND"", @key, @val)
                return newOffset
            else
                return -1
            end
        ");
                
        
        class Script
        {
            public Script(string script)
            {
                LuaScript = LuaScript.Prepare(script);
            }

            public LuaScript LuaScript { get; private set; }
            public LoadedLuaScript LoadedLuaScript { get; private set; }

            public async Task EnsureLoaded(IConnectionMultiplexer redis)
            {
                if (LoadedLuaScript != null) return;

                var servers = redis.GetEndPoints()
                    .Select(ep => redis.GetServer(ep));

                foreach (var server in servers)
                {
                    LoadedLuaScript = await LuaScript.LoadAsync(server, CommandFlags.HighPriority);
                }
            }
            
        }

    }
    
}
