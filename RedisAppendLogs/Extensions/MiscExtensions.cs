using System;
using System.Collections.Generic;
using System.Text;

namespace RedisAppendStreams.Extensions
{
    public static class MiscExtensions
    {
        public static T[] WrapAsArray<T>(this T obj)
            => new T[] { obj };


        public static IList<T> WrapAsList<T>(this T obj)
            => new T[] { obj };

    }
}
