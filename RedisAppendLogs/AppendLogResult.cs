namespace RedisAppendLogs
{
    public struct AppendLogResult
    {
        public readonly bool Success;
        public readonly LogRef Next;
        
        internal AppendLogResult(bool success, LogRef next)
        {
            Success = success;
            Next = next;
        }
        
        public static implicit operator LogRef(AppendLogResult result)
            => result.Next;

        internal static AppendLogResult Fail = new AppendLogResult(false, LogRef.None);

        internal static AppendLogResult Ok(LogRef next)
            => new AppendLogResult(true, next);

        internal static AppendLogResult<T> Ok<T>(T value, LogRef next)
            => new AppendLogResult<T>(true, value, next);

    }
    

    public struct AppendLogResult<T>
    {
        public readonly bool Success;
        public readonly T Value;
        public readonly LogRef Next;

        internal AppendLogResult(bool success, T value, LogRef next)
        {
            Success = true;
            Value = value;
            Next = next;
        }
        

        public static implicit operator AppendLogResult(AppendLogResult<T> result)
            => new AppendLogResult(result.Success, result.Next);
        
        public static implicit operator LogRef(AppendLogResult<T> result)
            => result.Next;

        public static implicit operator T(AppendLogResult<T> result)
            => result.Value;

    }
        
}
