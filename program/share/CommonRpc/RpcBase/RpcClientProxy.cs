using CommonRpc.Net;
using CommonRpc.Rpc;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace CommonRpc.RpcBase
{
    public class RpcClientProxy
    {
        private static readonly string dllName;
        private static ModuleBuilder moduleBuilder = null;
        private static AssemblyBuilder assemblyBuilder = null;

        private static readonly Dictionary<Type, Type> interfaceType2ImplDict = new Dictionary<Type, Type>();
        static RpcClientProxy()
        {
            var assemblyName = new AssemblyName(nameof(RpcClientProxy));
            dllName = assemblyName.Name + ".dll";
#if NET462
            assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
#elif NET6_0_OR_GREATER
            assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
#endif
            moduleBuilder = assemblyBuilder.DefineDynamicModule(dllName);
        }

        public static T Resolve<T>(RpcInvoker rpcInvoker) where T : class
        {
            var interfaceType = typeof(T);
            if (!interfaceType.IsInterface)
                throw new ArgumentException("param needs to be an interface type");


            //不加lock由于网络会话层是多线程的，会出Bug
            lock(interfaceType2ImplDict)
            {
                interfaceType2ImplDict.TryGetValue(interfaceType, out Type implType);
                if (implType == null)
                {
                    implType = CreateType(interfaceType);
                    interfaceType2ImplDict.Add(interfaceType, implType);
                }
                return (T)Activator.CreateInstance(implType, rpcInvoker);
            }
        }

        private static Type CreateType(Type interfaceType)
        {
            var typeName = string.Format("{0}.{1}Proxy", typeof(RpcClientProxy).FullName, interfaceType.Name);
            var type = moduleBuilder.GetType(typeName);
            if (type != null)
                return type;

            var typeBuilder = moduleBuilder.DefineType(typeName);
            typeBuilder.AddInterfaceImplementation(interfaceType);

            var fieldBuilder = typeBuilder.DefineField("_peerBaseInvoker", typeof(RpcInvoker), FieldAttributes.Private);

            CreateConstructor(typeBuilder, fieldBuilder);
            CreateMethods(interfaceType, typeBuilder, fieldBuilder);

            return typeBuilder.CreateType();
        }

        private static void CreateConstructor(TypeBuilder typeBuilder, FieldBuilder fieldBuilder)
        {
            var ctor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[] { typeof(RpcInvoker) });
            var il = ctor.GetILGenerator(16);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, fieldBuilder);
            il.Emit(OpCodes.Ret);
        }

        private static void CreateMethods(Type interfaceType, TypeBuilder typeBuilder, FieldBuilder fieldBuilder)
        {
            var serviceAttribute = interfaceType.GetCustomAttribute<RpcServiceAttribute>();
            
            var methodList = RpcTool.GetSortedMethods(interfaceType);
            ushort i = serviceAttribute.MinMsgId;

            foreach (MethodInfo methodInfo in methodList)
            {
                CreateMethod(methodInfo, typeBuilder, fieldBuilder,i);
                i++;
            }
        }


        /// <summary>
        /// 暂时处理形式如以下格式的RPC调用
        /// Object PlayerXXX(Object arg0) √
        /// Object PlayerXXX()
        /// void PlayerXXX(Object arg0)
        /// void PlayerXXX() √
        /// 这里是客户端调用，对应服务端见
        /// <see cref="RpcTool.CreateMethodMeta(string, object)"/>
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <param name="typeBuilder"></param>
        /// <param name="fieldBuilder"></param>
        /// <param name="msgId"></param>
        /// <returns></returns>
        private static MethodBuilder CreateMethod(MethodInfo methodInfo, TypeBuilder typeBuilder, FieldBuilder fieldBuilder,ushort msgId)
        {
            var args = methodInfo.GetParameters();
            var methodBuilder = typeBuilder.DefineMethod(methodInfo.Name, MethodAttributes.Public | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.HideBySig,
                methodInfo.CallingConvention, methodInfo.ReturnType, args.Select(t => t.ParameterType).ToArray());
            var il = methodBuilder.GetILGenerator(256);

            bool hasParam = args.Count() > 0;
            bool hasRet = methodInfo.ReturnType != typeof(void);

            //Object PlayerXXX(Object arg0)
            if (hasRet && hasParam)
            {
                MethodInfo sendMethod = typeof(RpcInvoker).GetMethod(nameof(RpcInvoker.SendRequestParamObjRetObj), BindingFlags.Instance | BindingFlags.Public);
                MethodInfo genericMethod = sendMethod.MakeGenericMethod(args.Single().ParameterType, methodInfo.ReturnType);

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, fieldBuilder);
                il.Emit(OpCodes.Ldc_I4, msgId);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Callvirt, genericMethod);
                il.Emit(OpCodes.Ret);

                return methodBuilder;
            }
            //object PlayerXXX()
            else if(hasRet && !hasParam)
            {
                MethodInfo sendMethod = typeof(RpcInvoker).GetMethod(nameof(RpcInvoker.SendRequestParamVoidRetObj), BindingFlags.Instance | BindingFlags.Public);
                MethodInfo genericMethod = sendMethod.MakeGenericMethod(methodInfo.ReturnType);

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, fieldBuilder);
                il.Emit(OpCodes.Ldc_I4, msgId);
                il.Emit(OpCodes.Callvirt, genericMethod);
                il.Emit(OpCodes.Ret);

                return methodBuilder;
            }
            //void PlayerXXX(Object arg0)
            else if (!hasRet && hasParam)
            {
                MethodInfo sendMethod = typeof(RpcInvoker).GetMethod(nameof(RpcInvoker.SendRequestParamObjRetVoid), BindingFlags.Instance | BindingFlags.Public);
                MethodInfo genericMethod = sendMethod.MakeGenericMethod(args.Single().ParameterType);

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, fieldBuilder);
                il.Emit(OpCodes.Ldc_I4, msgId);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Callvirt, genericMethod);
                il.Emit(OpCodes.Ret);

                return methodBuilder;
            }

            //void PlayerXXX()
            else if (!hasRet && !hasParam)
            {
                MethodInfo sendMethod = typeof(RpcInvoker).GetMethod(nameof(RpcInvoker.SendRequestParamVoidRetVoid), BindingFlags.Instance | BindingFlags.Public);

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, fieldBuilder);
                il.Emit(OpCodes.Ldc_I4, msgId);
                il.Emit(OpCodes.Callvirt, sendMethod);
                il.Emit(OpCodes.Ret);

                return methodBuilder;
            }


            return null;

        }



        public static void Save()
        {
#if NET462
            assemblyBuilder.Save(dllName);
#endif
        }


    }
}
