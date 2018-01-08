using System;
using System.Threading.Tasks;

namespace RedisEntityLogs.Test
{
    public static class TestExtensions
    {
        public static LogRef ExpectOK(this AppendResult result)
            => result.Success ? (LogRef)result : throw new Exception("Append failed!");

        public static Task<LogRef> ExpectOK(this Task<AppendResult> resultTask)
            => resultTask.Map(r => r.ExpectOK());
        
    }

}
