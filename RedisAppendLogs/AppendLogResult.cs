namespace RedisAppendLogs
{
    public struct AppendLogResult
    {
        public readonly bool Success;
        public readonly AppendLogHandle Next;
        
        internal AppendLogResult(bool success, AppendLogHandle next)
        {
            Success = success;
            Next = next;
        }
        
        public static implicit operator AppendLogHandle(AppendLogResult result)
            => result.Next;

        internal static AppendLogResult Fail = new AppendLogResult(false, AppendLogHandle.None);

        internal static AppendLogResult Ok(AppendLogHandle next)
            => new AppendLogResult(true, next);

        internal static AppendLogResult<T> Ok<T>(T value, AppendLogHandle next)
            => new AppendLogResult<T>(true, value, next);

    }
    

    public struct AppendLogResult<T>
    {
        public readonly bool Success;
        public readonly T Value;
        public readonly AppendLogHandle Next;

        internal AppendLogResult(bool success, T value, AppendLogHandle next)
        {
            Success = true;
            Value = value;
            Next = next;
        }
        

        public static implicit operator AppendLogResult(AppendLogResult<T> result)
            => new AppendLogResult(result.Success, result.Next);
        
        public static implicit operator AppendLogHandle(AppendLogResult<T> result)
            => result.Next;

        public static implicit operator T(AppendLogResult<T> result)
            => result.Value;

    }
        
}
