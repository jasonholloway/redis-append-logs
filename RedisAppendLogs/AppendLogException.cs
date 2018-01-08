using System;

namespace RedisAppendLogs
{
    public class AppendLogException : Exception
    {
        public AppendLogException(string message) : base(message)
        { }
    }
    
}
