using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XSession.Modules.Test
{
    internal static class DataQueue
    {
        private static readonly int MaxCount = 100;
        private static readonly object s_lock = new object();
        private static Queue<DebugInfo> s_queue = new Queue<DebugInfo>(200);

        public static void Add(DebugInfo debugInfo)
        {
            lock( s_lock ) {
                s_queue.Enqueue(debugInfo);

                if( s_queue.Count > MaxCount )
                    s_queue.Dequeue();
            }
        }

        public static List<DebugInfo> GetAll()
        {
            lock( s_lock ) {
                return (from x in s_queue.AsEnumerable()
                        select x
                        ).ToList();
            }
        }
    }
}
