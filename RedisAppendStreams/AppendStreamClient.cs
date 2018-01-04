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
            var currString = (string)await _db.StringGetAsync(handle.Key);
            if (currString != null && handle.Offset != currString.Length) return new AppendResult();
            
            var newLength = await _db.StringAppendAsync(handle.Key, val);

            return new AppendResult(handle.WithNewOffset(newLength));
        }
    }



}
