using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RedisAppendStreams.Test
{
    public static class TaskExtensions
    {
        public static Task IgnoreException<T>(this Task task) where T : Exception
            => task.ContinueWith(t => {
                if (t.Exception == null || t.Exception.InnerException is T) return;
                else throw t.Exception;
            });

        public static Task<T[]> WhenAll<T>(this IEnumerable<Task<T>> tasks)
            => Task.WhenAll(tasks);

    }

}
