namespace RedisAppendLogs
{
    public struct LogRef
    {
        public readonly string Key;
        public readonly long? Offset;

        public LogRef(string key, long? offset = null)
        {
            Key = key;
            Offset = offset;
        }

        public LogRef WithoutOffset()
            => new LogRef(Key);
        
        internal LogRef WithOffset(long newOffset)
            => new LogRef(Key, newOffset);
        
        internal static LogRef None = new LogRef();        
    }
    
}
