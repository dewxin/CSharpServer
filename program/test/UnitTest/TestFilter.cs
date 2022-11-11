using CommonRpc;
using CommonRpc.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Protocol.Param;
using ServerCommon;
using SuperSocketSlim;
using SuperSocketSlim.Protocol;
using System;

namespace UnitTest
{
    [TestClass]
    public class TestFilter
    {
        [TestMethod]
        public void TestMethod1()
        {
            ProtobufReceiveFilter fixedHeaderReceiveFilter = new ProtobufReceiveFilter();

            var appSession = new AppSession<ProtobufRequestInfo>(fixedHeaderReceiveFilter);
            fixedHeaderReceiveFilter.Initialize(null, appSession);

            appSession.OnExecuteCommand += (requetInfo) =>
             {
                 var data = SerializerHelper.Deserialize(typeof(ServerConnectRet), requetInfo.Body) as ServerConnectRet;
                 Assert.AreEqual(data.PeerType, "good");
                 Assert.AreEqual(data.ServerId, 1u);
             };

            var originParam = new ServerConnectRet { PeerType = "good", ServerId = 1 };
            var msg = new MsgHeaderBody(msgId:1,invokeId:1);
            msg.SetMsgBody(originParam);

            var sentData = msg.GetData();

            foreach(var seg in sentData)
            {
                fixedHeaderReceiveFilter.ParseAndTryExecuteCommand(seg);
            }


        }



    }
}