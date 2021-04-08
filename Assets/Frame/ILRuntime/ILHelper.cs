using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Frame
{
    public static class ILHelper
    {
        public static void InitILRuntime(ILRuntime.Runtime.Enviorment.AppDomain appdomain)
        {
            // 注册不带返回值的委托
            RegisterMethodDelegate(appdomain);

            // 注册带返回值的委托
            RegisterFunctionDelegate(appdomain);

            // 注册复杂委托
            RegisterDelegateConvertor(appdomain);

            // 执行绑定
            //CLRBindings.Initialize(appdomain);

            // 跨域绑定适配器
            Assembly assembly = typeof(ILHelper).Assembly;
            foreach (Type type in assembly.GetTypes())
            {
                object[] attrs = type.GetCustomAttributes(typeof(ILAdapterAttribute), false);
                if (attrs.Length == 0)
                {
                    continue;
                }
                object obj = Activator.CreateInstance(type);
                CrossBindingAdaptor adaptor = obj as CrossBindingAdaptor;
                if (adaptor == null)
                {
                    continue;
                }
                appdomain.RegisterCrossBindingAdaptor(adaptor);
            }

            //demo
            //appdomain.RegisterCrossBindingAdaptor(new MonoBehaviourAdapter());
            //appdomain.RegisterCrossBindingAdaptor(new CoroutineAdapter());
            //appdomain.RegisterCrossBindingAdaptor(new InheritanceAdapter());
            //appdomain.RegisterValueTypeBinder(typeof(Vector3), new Vector3Binder());

            // 注册LitJson
            //LitJson.JsonMapper.RegisterILRuntimeCLRRedirection(appdomain);
        }

        private static void RegisterDelegateConvertor(ILRuntime.Runtime.Enviorment.AppDomain appdomain)
        {
            #region 委托转换器

            appdomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction<float>>((action) =>
            {
                return new UnityEngine.Events.UnityAction<float>((a) =>
                {
                    ((System.Action<float>)action)(a);
                });
            });
            appdomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction<string>>((action) =>
            {
                return new UnityEngine.Events.UnityAction<string>((str) =>
                {
                    ((System.Action<string>)action)(str);
                });
            });
            appdomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction>((action) =>
            {
                return new UnityEngine.Events.UnityAction(() =>
                {
                    ((System.Action)action)();
                });
            });

            appdomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction<int>>((action) =>
            {
                return new UnityEngine.Events.UnityAction<int>((a) =>
                {
                    ((System.Action<int>)action)(a);
                });
            });

            appdomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction<bool>>((action) =>
            {
                return new UnityEngine.Events.UnityAction<bool>((a) =>
                {
                    ((System.Action<bool>)action)(a);
                });
            });

            appdomain.DelegateManager.RegisterDelegateConvertor<System.Threading.WaitCallback>((act) =>
            {
                return new System.Threading.WaitCallback((state) =>
                {
                    ((Action<System.Object>)act)(state);
                });
            });
            #endregion
        }

        private static void RegisterFunctionDelegate(ILRuntime.Runtime.Enviorment.AppDomain appdomain)
        {
            appdomain.DelegateManager.RegisterFunctionDelegate<List<float>, List<float>>();
            //appdomain.DelegateManager.RegisterFunctionDelegate<ReadOnlyCollectionsExtensions.Wrappers.ReadOnlyDictionary<System.Int32, ILRuntime.Runtime.Intepreter.ILTypeInstance>>();
        }

        /// <summary>
        /// 注册委托
        /// </summary>
        private static void RegisterMethodDelegate(ILRuntime.Runtime.Enviorment.AppDomain appdomain)
        {
            //appdomain.DelegateManager.RegisterMethodDelegate<List<object>>();

            appdomain.DelegateManager.RegisterMethodDelegate<System.Object>();

            #region Json
            //appdomain.DelegateManager.RegisterMethodDelegate<LitJson.JsonData>();
            #endregion

            #region Http
            appdomain.DelegateManager.RegisterMethodDelegate<System.Boolean, System.Int64, System.String>();
            #endregion

            #region Network 网络部分
            appdomain.DelegateManager.RegisterMethodDelegate<bool>();
            appdomain.DelegateManager.RegisterMethodDelegate<byte[], int, int>();
            appdomain.DelegateManager.RegisterMethodDelegate<float>();
            appdomain.DelegateManager.RegisterMethodDelegate<ILTypeInstance>();
            appdomain.DelegateManager.RegisterMethodDelegate<int>();
            appdomain.DelegateManager.RegisterMethodDelegate<string>();
            #endregion

            #region Event 事件系统
            //appdomain.DelegateManager.RegisterMethodDelegate<Frame.IL.EventArgsClassInheritanceAdaptor.EventArgsAdaptor>();
            #endregion

            appdomain.DelegateManager.RegisterMethodDelegate<System.Boolean, System.Int32, System.Int32>();
            appdomain.DelegateManager.RegisterMethodDelegate<System.String, System.Single>();
            appdomain.DelegateManager.RegisterMethodDelegate<System.String, UnityEngine.GameObject>();
            appdomain.DelegateManager.RegisterMethodDelegate<System.String, UnityEngine.Object>();
            appdomain.DelegateManager.RegisterMethodDelegate<UnityEngine.GameObject, int>();
            appdomain.DelegateManager.RegisterMethodDelegate<UnityEngine.GameObject>();
            appdomain.DelegateManager.RegisterMethodDelegate<UnityEngine.Transform, System.String>();
            appdomain.DelegateManager.RegisterMethodDelegate<UnityEngine.U2D.SpriteAtlas>();
            appdomain.DelegateManager.RegisterMethodDelegate<byte[]>();

            #region ResMgr 资源系统
            appdomain.DelegateManager.RegisterMethodDelegate<UnityEngine.Object>();
            appdomain.DelegateManager.RegisterMethodDelegate<UnityEngine.Sprite>(); //Atlas Mgr
            appdomain.DelegateManager.RegisterMethodDelegate<System.UInt32>();
            //appdomain.DelegateManager.RegisterMethodDelegate<Frame.ThreadMgr.ThreadJob<ReadOnlyCollectionsExtensions.Wrappers.ReadOnlyDictionary<System.Int32, ILRuntime.Runtime.Intepreter.ILTypeInstance>>>();

            #endregion

            appdomain.DelegateManager.RegisterMethodDelegate<UnityEngine.Texture2D>();
            appdomain.DelegateManager.RegisterFunctionDelegate<System.Single>();
            appdomain.DelegateManager.RegisterMethodDelegate<System.String, ILRuntime.Runtime.Intepreter.ILTypeInstance>();
        }
    }
}