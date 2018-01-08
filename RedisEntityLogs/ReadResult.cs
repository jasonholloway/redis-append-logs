namespace RedisEntityLogs
{
    public struct ReadResult<T>
    {
        public readonly bool Success;
        public readonly T Value;
        public readonly LogRef Next;

        internal ReadResult(bool success, T value, LogRef next)
        {
            Success = true;
            Value = value;
            Next = next;
        }
        
                
        public static implicit operator LogRef(ReadResult<T> result)
            => result.Next;

        public static implicit operator T(ReadResult<T> result)
            => result.Value;

    }
        
}
