namespace RedisAppendStreams
{
    public struct AppendResult
    {
        public readonly bool Success;
        public readonly AppendStreamHandle? Next;

        public AppendResult(AppendStreamHandle next)
        {
            Success = true;
            Next = next;
        }

        public AppendResult(object _ = null)
        {
            Success = false;
            Next = null;
        }

        public static implicit operator AppendStreamHandle(AppendResult result)
            => result.Next.Value;
    }



}
