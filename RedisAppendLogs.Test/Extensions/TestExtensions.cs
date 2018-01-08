using System;
using System.Threading.Tasks;

namespace RedisAppendLogs.Test
{
    public static class TestExtensions
    {
        public static LogRef ExpectOK(this AppendLogResult result)
            => result.Success ? (LogRef)result : throw new Exception("Append failed!");

        public static Task<LogRef> ExpectOK(this Task<AppendLogResult> resultTask)
            => resultTask.Map(r => r.ExpectOK());
        
    }

}
