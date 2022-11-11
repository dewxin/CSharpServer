using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommonRpc
{
    //然后 删掉其中一个副本
    public class RpcIdGenerator
    {
        //private static IdGenerator instance = new IdGenerator();
        //public static IdGenerator Default { get { return instance; } }

        //Id重复利用池
        ConcurrentQueue<int> idQueue = new ConcurrentQueue<int>();
        // 当前ID索引
        private int idIndex = 0;

        public int GenerateId()
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
