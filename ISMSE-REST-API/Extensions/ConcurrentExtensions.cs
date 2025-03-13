using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace ISMSE_REST_API.Extensions
{
    public static class ConcurrentExtensions
    {
        public static void Clear<T>(this ConcurrentBag<T> concurrentBag)
        {
            var newBag = new ConcurrentBag<T>();
            Interlocked.Exchange(ref concurrentBag, newBag);
        }
    }
}