using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectCommon.Unit
{
    public class FastSafeStack<T>
    {
        object m_lock = new object();
        Stack<T> m_stk;
        T m_null = default(T);
        int m_Count = 0;

        public FastSafeStack()
        {
            m_stk = new Stack<T>();
        }

        public FastSafeStack(IEnumerable<T> collection)
        {
            m_stk = new Stack<T>(collection);
            m_Count = m_stk.Count;
        }

        public void Push(T item)
        {
            lock (m_lock)
            {
                m_stk.Push(item);
                Interlocked.Increment(ref m_Count);
            }
        }

        public bool TryPop(out T result)
        {
            if (m_Count <= 0)
            {
                result = m_null;
                return false;
            }

            lock (m_lock)
            {
                if (m_Count <= 0)
                {
                    result = m_null;
                    return false;
                }

                result = m_stk.Pop();
                Interlocked.Decrement(ref m_Count);
                return true;
            }
        }

        public Stack<T>.Enumerator GetEnumerator()
        {
            lock (m_lock)
            {
                return m_stk.GetEnumerator();
            }
        }


        public int Count
        {
            get
            {
                return m_Count;
            }
        }

    }

}
