using StackExchange.Redis;
using System.Linq;
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
            await EnsureLuaScriptsLoaded();

            var db = _redis.GetDatabase();

            var newOffset = (long)await db.ScriptEvaluateAsync(
                                            _luaAppend.LoadedLuaScript, 
                                            new { key = (RedisKey)handle.Key, offset = handle.Offset, val }
                                            );
            
            if (newOffset < 0) return new AppendResult();

            return new AppendResult(handle.WithOffset(newOffset));            
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



        bool _scriptsLoaded = false;

        async Task EnsureLuaScriptsLoaded()
        {
            if (_scriptsLoaded) return;
            
            var servers = _redis.GetEndPoints()
                .Select(ep => _redis.GetServer(ep));
            
            foreach(var server in servers) {
                await new[] { _luaAppend }
                    .Select(s => s.LoadTo(server))
                    .WhenAll();                
            }
            
            _scriptsLoaded = true;
        }

        
        class Script
        {
            public Script(string script)
            {
                LuaScript = LuaScript.Prepare(script);
            }

            public LuaScript LuaScript { get; private set; }
            public LoadedLuaScript LoadedLuaScript { get; private set; }

            public async Task LoadTo(IServer server)
            {
                LoadedLuaScript = await LuaScript.LoadAsync(server, CommandFlags.HighPriority);                
            }
        }

    }



}
