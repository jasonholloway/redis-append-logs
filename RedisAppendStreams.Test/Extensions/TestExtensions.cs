using System;
using System.Threading.Tasks;

namespace RedisAppendStreams.Test
{
    public static class TestExtensions
    {
        public static AppendStreamHandle ExpectOK(this AppendResult result)
            => result.Success ? (AppendStreamHandle)result : throw new Exception("Append failed!");

        public static Task<AppendStreamHandle> ExpectOK(this Task<AppendResult> resultTask)
            => resultTask.Map(r => r.ExpectOK());
    }

}
