using CommonRpc.RpcBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CommonRpc
{
    public static class ProtocolTool
    {
        private static Dictionary<RpcServiceAttribute, RpcForwardAttribute> _RpcForwardDict;
        public static Dictionary<RpcServiceAttribute, RpcForwardAttribute> RpcForwardDict
        {
            get
            {
                if (_RpcForwardDict == null)
                    RpcForwardDict = GetRpcForwardDict();
                return _RpcForwardDict;
            }
            private set { _RpcForwardDict = value; }
        }

        private static Dictionary<Type, RpcDataAttribute> _RpcDataDict;
        public static Dictionary<Type, RpcDataAttribute> RpcDataDict
        {
            get
            {
                if (_RpcDataDict == null)
                    RpcDataDict = GetRpcDataDict();
                return _RpcDataDict;
            }
            private set { _RpcDataDict = value; }
        }



        public static RpcForwardAttribute GetForwardAttrByMsgId(ushort msgId)
        {
            if (RpcForwardDict == null)
                throw new ArgumentNullException($"{nameof(RpcForwardDict)}","param is not initialized");

            foreach(var entry in RpcForwardDict)
            {
                if(entry.Key.Include(msgId))
                {
                    return entry.Value;
                }
            }

            return null;
        }

        public static Dictionary<RpcServiceAttribute, RpcForwardAttribute> GetRpcForwardDict()
        {
            var assemblyList = AppDomain.CurrentDomain.GetAssemblies();

            Dictionary<RpcServiceAttribute, RpcForwardAttribute> rpc2TargetDict = new Dictionary<RpcServiceAttribute, RpcForwardAttribute>();
            foreach (var assembly in assemblyList)
            {
                var interfaceType = assembly.GetTypes().Where(type => type.IsInterface);
                foreach (var type in interfaceType)
                {
                    var forwardAttr = type.GetCustomAttribute<RpcForwardAttribute>();
                    var rpcServiceAttr = type.GetCustomAttribute<RpcServiceAttribute>();
                    if (forwardAttr != null)
                    {
                        rpc2TargetDict.Add(rpcServiceAttr, forwardAttr);
                    }

                }
            }


            return rpc2TargetDict;
        }


        public static Dictionary<Type, RpcDataAttribute> GetRpcDataDict()
        {
            var assemblyList = AppDomain.CurrentDomain.GetAssemblies();
            Dictionary<Type, RpcDataAttribute> rpcDataDict = new Dictionary<Type, RpcDataAttribute>();

            foreach(var assembly in assemblyList)
            {
                var classType = assembly.GetTypes().Where(type => type.IsClass);
                foreach (var type in classType)
                {
                    var rpcDataAttr = type.GetCustomAttribute<RpcDataAttribute>();
                    if (rpcDataAttr != null)
                        rpcDataDict.Add(type, rpcDataAttr);
                }
            }


            return rpcDataDict;
        }

        public static bool IsAgressiveCompress(this Type type)
        {
            //TODO 这里好像MessagePack有bug
            //if (RpcDataDict == null)
                return false;

            var containType = RpcDataDict.TryGetValue(type, out var val);
            if (!containType)
                return false;

            return val.IsOptionOn(RpcDataOption.UseCompress);
        }

    }



}
