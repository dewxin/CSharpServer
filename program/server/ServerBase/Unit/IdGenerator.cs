using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectCommon.Unit
{
    public class IdGenerator
    {
        //private static IdGenerator instance = new IdGenerator();
        //public static IdGenerator Default { get { return instance; } }

        //Id重复利用池
        ConcurrentQueue<int> idQueue = new ConcurrentQueue<int>();
        // 当前ID索引
        private int idIndex = 0;

        public ushort GetIdUShort()
        {
            checked
            {
                return (ushort)GetIdInt();
            }
        }

        public int GetIdInt()
        {
            int retId = 0;

            if (!idQueue.IsEmpty)
                idQueue.TryDequeue(out retId);

            if (retId == 0)
            {
                retId = Interlocked.Increment(ref idIndex);
            }

            return retId;
        }

        public void ReleaseId(int id)
        {
            idQueue.Enqueue(id);
        }

    }
}
