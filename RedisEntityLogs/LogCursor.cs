using System;
using System.Threading.Tasks;

namespace RedisEntityLogs
{
    public class LogCursor
    {
        public LogClient Client { get; set; }
        public LogRef Ref { get; private set; }
        
        internal LogCursor(LogClient client, LogRef @ref)
        {
            Client = client;
            Ref = @ref;
        }
        
        public async Task Write(string value)
        {
            var result = await Client.AppendTo(Ref, value);
            Ref = result.Next;
        }

        public async Task<string> CatchUp()
        {
            var result = await Client.ReadFrom(Ref);
            Ref = result.Next;
            return result.Value;
        }
    }
}
