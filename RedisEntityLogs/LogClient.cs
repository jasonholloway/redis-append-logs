using StackExchange.Redis;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RedisEntityLogs
{
    public class LogClient
    {
        IConnectionMultiplexer _redisMultiplexer;

        public LogClient(IConnectionMultiplexer redisMultiplexer)
        {
            _redisMultiplexer = redisMultiplexer;
        }

        
        public async Task<ReadResult<string>> ReadFrom(LogRef @ref)
        {
            long offset = @ref.Offset.GetValueOrDefault(0);

            var value = (string)await Redis.StringGetRangeAsync(@ref.Key, offset, -1);

            return AppendResult.Ok(value, next: @ref.WithOffset(offset + value.Length));
        }
        

        public async Task<AppendResult> AppendTo(LogRef @ref, string val)
        {
            if (@ref.Offset == null) throw new LogException("Can't append using a handle without an offset!");
            
            await _luaAppend.EnsureLoaded(_redisMultiplexer);
            
            var newOffset = (long)await Redis.ScriptEvaluateAsync(
                                            script: _luaAppend.LoadedLuaScript, 
                                            parameters: new { key = (RedisKey)@ref.Key, offset = @ref.Offset, val }
                                        );

            if (newOffset < 0) return AppendResult.Fail;

            return AppendResult.Ok(next: @ref.WithOffset(newOffset));            
        }
        

        IDatabase Redis => _redisMultiplexer.GetDatabase();
        

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
