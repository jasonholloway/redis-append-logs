using System;
using System.Collections.Generic;
using System.Text;

namespace RedisEntityLogs
{
    public static class CursorExtensions
    {

        public static LogCursor CreateCursor(this LogClient client, string key)
            => client.CreateCursor(new LogRef(key));

        public static LogCursor CreateCursor(this LogClient client, LogRef @ref)
            => new LogCursor(client, @ref);

    }
}
