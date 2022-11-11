using CommonRpc.Rpc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace CommonRpc.RpcBase
{
    public class MethodMeta
    {
        public bool HasRet { get; set; }
        public bool HasParam { get; set; }

        public object Invoke(object handler, object param)
        {
            if(HasParam && HasRet)
            {
                return FuncParamObjRetObj.Invoke(handler, param);
            }
            else if(!HasRet && !HasParam)
            {
                FuncParamVoidRetVoid.Invoke(handler);
            }
            else if(HasRet && !HasParam)
            {
                return FuncParamVoidRetObj.Invoke(handler);
            }
            else if(!HasRet && HasParam)
            {
                FuncParamObjRetVoid.Invoke(handler, param);
            }

            return null;
        }

        private Func<object, object, object> _FuncParamObjRetObj;
        public Func<object, object, object> FuncParamObjRetObj 
        { 
            get => _FuncParamObjRetObj;
            set 
            { 
                HasParam = true;
                HasRet=true; 
                _FuncParamObjRetObj = value; 
            }
        }


        private Func<object, object> _FuncParamVoidRetObj;
        public Func<object, object> FuncParamVoidRetObj
        {
            get => _FuncParamVoidRetObj;
            set
            {
                HasParam = false;
                HasRet = true;
                _FuncParamVoidRetObj = value;
            }
        }

        private Action<object, object> _FuncParamObjRetVoid;
        public Action<object, object> FuncParamObjRetVoid
        {
            get => _FuncParamObjRetVoid;
            set
            {
                HasParam = true;
                HasRet = false;
                _FuncParamObjRetVoid = value;
            }
        }


        private Action<object> _FuncParamVoidRetVoid;
        public Action<object> FuncParamVoidRetVoid
        {
            get => _FuncParamVoidRetVoid;
            set
            {
                HasParam = false;
                HasRet = false;
                _FuncParamVoidRetVoid = value;
            }
        }

        public Type RequestType { get; set; }
        public string MethodName { get; set; }

    }

    public static class RpcTool
    {
        public static Type GetInterfaceTypeWithRpcServiceAttr(Type serviceHandlerType)
        {
            Type[] interfaceTypeArray = serviceHandlerType.GetInterfaces();

            foreach (var interfaceType in interfaceTypeArray)
            {
                if (interfaceType.GetCustomAttribute<RpcServiceAttribute>() != null)
                    return interfaceType;
            }

            throw new InvalidOperationException("cannot find any interface has RpcService Attribute");
        }

        public static List<MethodInfo> GetSortedMethods(Type interfaceType)
        {
            var methodArray = interfaceType.GetMethods();
            List<MethodInfo> methodList = new List<MethodInfo>(methodArray);
            methodList.Sort((m1, m2) => { return m1.Name.CompareTo(m2.Name); });
            return methodList;
        }

        //需要保证调用方和处理方每个函数和id的对应关系一致
        public static (Dictionary<ushort, MethodMeta>, RpcServiceAttribute)
        GetRpcMethodDictAndServiceAttr(Type serviceHandlerType)
        {
            var serviceInterfaceType = GetInterfaceTypeWithRpcServiceAttr(serviceHandlerType);
            Debug.Assert(serviceInterfaceType.IsInterface);

            var id2FuncDict = new Dictionary<ushort, MethodMeta>();
            var serviceAttribute = serviceInterfaceType.GetCustomAttribute<RpcServiceAttribute>();

            var methodList = GetSortedMethods(serviceInterfaceType);
            ushort i = serviceAttribute.MinMsgId;
            foreach (var method in methodList)
            {
                id2FuncDict.Add(i, CreateMethodMeta(method.Name, serviceHandlerType));

                i++;
                Debug.Assert(i < serviceAttribute.MaxMsgId);
            }

            return (id2FuncDict, serviceAttribute);
        }


        /// <summary>
        /// 暂时处理形式如以下格式的RPC调用
        /// Object PlayerXXX(Object arg0) √
        /// Object PlayerXXX()
        /// void PlayerXXX(Object arg0) √
        /// void PlayerXXX() √
        /// 这里是服务端handler，客户端见
        /// <see cref="RpcClientProxy.CreateMethod"/>
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private static MethodMeta CreateMethodMeta(string methodName, Type serviceHandlerType)
        {
            var method = serviceHandlerType.GetMethod(methodName);

            bool hasRet = !(method.ReturnType == typeof(void));
            bool hasParam = method.GetParameters().Length > 0;

            // Object PlayerXXX(Object arg0) 
            if (hasRet && hasParam)
            {
                Type[] dynamicArgs = { typeof(object), typeof(object) };
                DynamicMethod dynamicMethod = new DynamicMethod("Nagi_Rpc_" + method.Name, typeof(object), dynamicArgs, typeof(RpcTool).Module);

                ILGenerator il = dynamicMethod.GetILGenerator(64);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.EmitCall(OpCodes.Call, method, null);
                il.Emit(OpCodes.Ret);

                Func<object, object, object> methodFunc =
                (Func<object, object, object>)dynamicMethod.CreateDelegate(typeof(Func<object, object, object>));

                var paramType = method.GetParameters().Single().ParameterType;

                return new MethodMeta()
                {
                    FuncParamObjRetObj = methodFunc,
                    RequestType = paramType,
                    MethodName = methodName,
                };
            }
            //object PlayerXXX()
            else if(hasRet && !hasParam)
            {
                Type[] dynamicArgs = {typeof(object)};
                DynamicMethod dynamicMethod = new DynamicMethod("Nagi_Rpc_" + method.Name, typeof(object), dynamicArgs, typeof(RpcTool).Module);

                ILGenerator il = dynamicMethod.GetILGenerator(64);
                il.Emit(OpCodes.Ldarg_0);
                il.EmitCall(OpCodes.Call, method, null);
                il.Emit(OpCodes.Ret);

                Func<object, object> methodFunc =
                (Func<object, object>)dynamicMethod.CreateDelegate(typeof(Func<object, object>));

                return new MethodMeta()
                {
                    FuncParamVoidRetObj = methodFunc,
                    RequestType = typeof(void),
                    MethodName = methodName,
                };
            }
            //void PlayerXXX(object)
            else if(!hasRet && hasParam)
            {
                Type[] dynamicArgs = { typeof(object), typeof(object) };
                DynamicMethod dynamicMethod = new DynamicMethod("Nagi_Rpc_" + method.Name, typeof(void), dynamicArgs, typeof(RpcTool).Module);

                ILGenerator il = dynamicMethod.GetILGenerator(64);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.EmitCall(OpCodes.Call, method, null);
                il.Emit(OpCodes.Ret);

                Action<object, object> methodFunc =
                (Action<object, object>)dynamicMethod.CreateDelegate(typeof(Action<object, object>));

                var paramType = method.GetParameters().Single().ParameterType;

                return new MethodMeta()
                {
                    FuncParamObjRetVoid = methodFunc,
                    RequestType = paramType,
                    MethodName = methodName,
                };
            }
            //void PlayerXXX()
            else if (!hasRet && !hasParam)
            {
                Type[] dynamicArgs = {typeof(object)};
                DynamicMethod dynamicMethod = new DynamicMethod("Nagi_Rpc_" + method.Name, typeof(void), dynamicArgs, typeof(RpcTool).Module);

                ILGenerator il = dynamicMethod.GetILGenerator(64);
                il.Emit(OpCodes.Ldarg_0);
                il.EmitCall(OpCodes.Call, method, null);
                il.Emit(OpCodes.Ret);

                Action<object> methodFunc =
                (Action<object>)dynamicMethod.CreateDelegate(typeof(Action<object>));


                return new MethodMeta()
                {
                    FuncParamVoidRetVoid = methodFunc,
                    RequestType = typeof(void),
                    MethodName = methodName,
                };
            }

            return null;

        }
    }

}
