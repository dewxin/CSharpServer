using CommonRpc;
using CommonRpc.Net;
using CommonRpc.RpcBase;
using GameServerBase.ServerWorld;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Protocol.Service.WorldService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest
{
    [TestClass]
    public class TestMsgHeader
    {
        [TestMethod]
        public void TestMethod1()
        {
            var methodId = GetMethodId(typeof(Client2WorldHandler), nameof(IClient2World.Login));
            Assert.IsTrue(methodId > 0);

            var isReply = true;
            ushort peerId = 2;
            ushort invokeId = 2;
            MsgHeaderBody msgHeaderBody = 
                new MsgHeaderBody(methodId, invokeId:invokeId, isReply:isReply, clientPeerId:peerId);

            Assert.AreEqual(msgHeaderBody.ClientPeerId, peerId);
            Assert.AreEqual(msgHeaderBody.IsReply, isReply);
            Assert.AreEqual(msgHeaderBody.IsForward, true);
            Assert.AreEqual(msgHeaderBody.MsgId, methodId);
            Assert.AreEqual(msgHeaderBody.FromTarget, ForwardTarget.World);
            Assert.AreEqual(msgHeaderBody.ToTarget, ForwardTarget.Client);
        }

        private ushort GetMethodId(Type typeWhereMethodIn, string methodName)
        {
            var (id2MethodMetaDict, _) = RpcTool.GetRpcMethodDictAndServiceAttr(typeWhereMethodIn);

            foreach (var kv in id2MethodMetaDict)
            {
                if (kv.Value.MethodName == methodName)
                    return kv.Key;
            }

            return 0;
        }

    }
}
