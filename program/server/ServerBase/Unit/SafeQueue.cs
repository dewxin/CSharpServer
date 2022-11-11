using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectCommon.Unit
{
    public class SafeQueue<T>
    {
        private Queue<T> queue;

        private int capacity;

        public SafeQueue(int capacity)
        {
            this.capacity = capacity;
            queue = new Queue<T>(capacity);
        }

        public bool Enqueue(T item)
        {
            if (queue.Count + 1 >= capacity)
                return false;

            //TODO
            //其实可以通过自己实现一个queue，将其看成一个管道
            //如果_tail和_head不会相互干扰，则不用加锁。
            //Supersocket 的queue好像可以抄一下
            lock(queue)
            {
                queue.Enqueue(item);
            }
            return true;
        }

        public T Dequeue()
        {
            lock(queue)
            {
                return queue.Dequeue();
            }
        }

        public bool IsEmpty => queue.Count == 0;

    }
}
