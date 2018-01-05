namespace RedisAppendLogs
{
    public struct AppendLogHandle
    {
        public readonly string Key;
        public readonly long Offset;

        public AppendLogHandle(string key, long offset = 0)
        {
            Key = key;
            Offset = offset;
        }

        internal AppendLogHandle WithOffset(long newOffset)
            => new AppendLogHandle(Key, newOffset);
    }
    
}
