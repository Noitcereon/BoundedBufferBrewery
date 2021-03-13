using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoundedBufferLib
{
    public class BoundedBuffer<T>
    {
        private readonly Queue<T> _queue = new Queue<T>();
        private readonly object _locker = new object();

        public int Count { get; set; }

        public bool AddToQueue(T item)
        {
            int beforeAddCount = Count;
            lock (_locker)
            {
                _queue.Enqueue(item);
                Count++;
            }

            bool success = Count + 1 == beforeAddCount;
            return success;
        }

        public bool RemoveFromQueue()
        {
            lock (_locker)
            {
                bool success = _queue.TryDequeue(out _);
                if (success)
                {
                    Count--;
                }
                return success;
            }
        }
        public T TakeFromQueue()
        {
            lock (_locker)
            {
                bool success = _queue.TryDequeue(out T result);
                if (success)
                {
                    Count--;
                }
                return result;
            }
        }
    }
}
