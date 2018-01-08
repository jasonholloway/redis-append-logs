using System;

namespace RedisEntityLogs
{
    public class LogException : Exception
    {
        public LogException(string message) : base(message)
        { }
    }
    
}
