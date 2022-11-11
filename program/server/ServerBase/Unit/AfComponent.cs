using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectCommon.Unit
{
    // keep this class clean which should only deal with Autofac 
    public abstract class AfComponent
    {

        public AfComponent(AfContainer mgr)
        {
            ParentMgr = mgr;
            //Init();
        }

        /// <summary>
        /// 父管理
        /// </summary>
        protected AfContainer ParentMgr { get; private set; }


        ///// <summary>
        ///// 初始化
        ///// </summary>
        //public abstract void Init();

        ///// <summary>
        ///// 释放
        ///// </summary>
        //public abstract void Release();

        /// <summary>
        /// tick逻辑 todo 移出来
        /// </summary>
        /// <param name="elapsed"></param>
        public abstract void Tick(double elapsed);

    }
}
