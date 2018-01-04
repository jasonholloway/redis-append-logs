namespace RedisAppendStreams
{
    public struct AppendStreamHandle
    {
        public readonly string Key;
        public readonly long Offset;

        public AppendStreamHandle(string key, long offset = 0)
        {
            Key = key;
            Offset = offset;
        }

        internal AppendStreamHandle WithOffset(long newOffset)
            => new AppendStreamHandle(Key, newOffset);
    }



}
