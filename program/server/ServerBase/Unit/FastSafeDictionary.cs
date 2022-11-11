using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectCommon.Unit
{
    public class FastSafeDictionary<KEY, VALUE>
    {
        object m_lock = new object();
        Dictionary<KEY, VALUE> m_dic;
        VALUE m_null = default(VALUE);
        int m_Count = 0;

        public FastSafeDictionary()
        {
            m_dic = new Dictionary<KEY, VALUE>();
        }

        public FastSafeDictionary(IEqualityComparer<KEY> comparer)
        {
            m_dic = new Dictionary<KEY, VALUE>(comparer);
        }

        public void Add(KEY key, VALUE value)
        {
            lock (m_lock)
            {
                m_dic.Add(key, value);
            }
        }

        public bool Remove(KEY key)
        {
            lock (m_lock)
            {
                return m_dic.Remove(key);
            }
        }

        public bool TryAdd(KEY key, VALUE value)
        {
            if (m_dic.ContainsKey(key))
                return false;

            lock (m_lock)
            {
                if (m_dic.ContainsKey(key))
                    return false;

                m_dic[key] = value;
                Interlocked.Increment(ref m_Count);
                return true;
            }
        }

        public bool TryGetValue(KEY key, out VALUE value)
        {
            if (!m_dic.ContainsKey(key))
            {
                value = m_null;
                return false;
            }

            lock (m_lock)
            {
                if (!m_dic.ContainsKey(key))
                {
                    value = m_null;
                    return false;
                }

                value = m_dic[key];
                return true;
            }
        }

        public bool TryRemove(KEY key, out VALUE value)
        {
            if (!m_dic.ContainsKey(key))
            {
                value = m_null;
                return false;
            }

            lock (m_lock)
            {
                if (!m_dic.ContainsKey(key))
                {
                    value = m_null;
                    return false;
                }

                value = m_dic[key];
                bool ret = m_dic.Remove(key);
                if (ret)
                {
                    Interlocked.Decrement(ref m_Count);
                }
                return ret;
            }
        }

        public KeyValuePair<KEY, VALUE>[] ToArray()
        {
            lock (m_lock)
            {
                return m_dic.ToArray();
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
