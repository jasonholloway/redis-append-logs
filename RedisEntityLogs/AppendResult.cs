namespace RedisEntityLogs
{
    public struct AppendResult
    {
        public readonly bool Success;
        public readonly LogRef Next;
        
        internal AppendResult(bool success, LogRef next)
        {
            Success = success;
            Next = next;
        }
        
        public static implicit operator LogRef(AppendResult result)
            => result.Next;

        internal static AppendResult Fail = new AppendResult(false, LogRef.None);

        internal static AppendResult Ok(LogRef next)
            => new AppendResult(true, next);

        internal static ReadResult<T> Ok<T>(T value, LogRef next)
            => new ReadResult<T>(true, value, next);

    }
        
}
