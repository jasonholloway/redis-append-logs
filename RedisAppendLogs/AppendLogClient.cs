using StackExchange.Redis;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RedisAppendLogs
{
    public class AppendLogClient
    {
        IConnectionMultiplexer _redis;

        public AppendLogClient(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task<AppendLogResult<string>> Read(AppendLogHandle handle)
        {
            var db = _redis.GetDatabase();

            var value = (string)await db.StringGetAsync(handle.Key);

            return AppendLogResult.Ok(value, next: handle.WithOffset(value.Length));
        }
        

        public async Task<AppendLogResult> Append(AppendLogHandle handle, string val)
        {
            if (handle.Offset == null) throw new AppendLogException("Can't append using a handle without an offset!");
            
            await _luaAppend.EnsureLoaded(_redis);
            
            var newOffset = (long)await _redis.GetDatabase().ScriptEvaluateAsync(
                                                                _luaAppend.LoadedLuaScript, 
                                                                new { key = (RedisKey)handle.Key, offset = handle.Offset, val }
                                                                );
            if (newOffset < 0) return AppendLogResult.Fail;

            return AppendLogResult.Ok(next: handle.WithOffset(newOffset));            
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
