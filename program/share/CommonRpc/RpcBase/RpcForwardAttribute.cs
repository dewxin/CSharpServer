using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonRpc.RpcBase
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class RpcForwardAttribute :Attribute
    {
        public ForwardTarget FromTarget { get; private set; }
        public ForwardTarget ToTarget { get; private set; }

        public RpcForwardAttribute(ForwardTarget fromTarget,ForwardTarget forwardTarget)
        {
            FromTarget = fromTarget;
            ToTarget = forwardTarget;
        }
    }

    public enum ForwardTarget
    {
        None = 0,
        Client,
        Logic,
        World
    }
}
