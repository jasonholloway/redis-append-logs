namespace RedisAppendLogs
{
    public struct AppendLogResult
    {
        public readonly bool Success;
        public readonly AppendLogHandle? Next;

        public AppendLogResult(AppendLogHandle next)
        {
            Success = true;
            Next = next;
        }

        public AppendLogResult(object _ = null)
        {
            Success = false;
            Next = null;
        }

        public static explicit operator AppendLogHandle(AppendLogResult result)
            => result.Next.Value;
    }



}
