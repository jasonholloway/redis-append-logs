using System;
using System.Threading.Tasks;

namespace RedisAppendLogs.Test
{
    public static class TestExtensions
    {
        public static AppendLogHandle ExpectOK(this AppendLogResult result)
            => result.Success ? (AppendLogHandle)result : throw new Exception("Append failed!");

        public static Task<AppendLogHandle> ExpectOK(this Task<AppendLogResult> resultTask)
            => resultTask.Map(r => r.ExpectOK());
    }

}
