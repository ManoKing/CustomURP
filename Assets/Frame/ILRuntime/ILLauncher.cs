using DH.Frame.IL;
using DH.Frame.Res;
using System;
using System.Collections.Generic;
#if ILRuntime
using System.IO;
#else
using System.Linq;
#endif
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Frame
{
    public sealed class ILLauncher : SingletonMono<ILLauncher>
    {

#if ILRuntime
		public ILRuntime.Runtime.Enviorment.AppDomain appDomain;
		private MemoryStream dllStream;
		private MemoryStream pdbStream;
#else
        public Assembly assembly;
#endif
        private List<Type> hotfixTypes;

        public Action UpdateAction;
        public Action LateUpdateAction;
        public Action OnApplicationQuitAction;

        public void Init()
        {
            LoadHotFix();
        }

        private void Update()
        {
            UpdateAction?.Invoke();
        }

        private void LateUpdate()
        {
            LateUpdateAction?.Invoke();
        }

        private void OnApplicationQuit()
        {
            OnApplicationQuitAction?.Invoke();
        }

        private void LoadHotFix()
        {
            byte[] assBytes;
            byte[] pdbBytes;
            Addressables.LoadAssetAsync<TextAsset>("Dll/hotfix.dll").Completed += dll =>
            {
                assBytes = dll.Result.bytes;
                Addressables.LoadAssetAsync<TextAsset>("Dll/hotfix.pdb").Completed += pdb =>
                {
                    pdbBytes = pdb.Result.bytes;
#if ILRuntime
                Debug.Log($"当前使用的是ILRuntime模式");
                this.appDomain = new ILRuntime.Runtime.Enviorment.AppDomain();
                this.dllStream = new MemoryStream(assBytes);
                this.pdbStream = new MemoryStream(pdbBytes);
                //this.pdbStream = null;//TODO 调试文件 出错加载会报错out of Memory
                this.appDomain.LoadAssembly(this.dllStream, this.pdbStream, new ILRuntime.Mono.Cecil.Pdb.PdbReaderProvider());
                
                ILHelper.InitILRuntime(this.appDomain);
                var iMethod = appDomain.GetType(GamePath.IL_Entry).GetMethod("Awake", 0);
                this.appDomain.Invoke(iMethod, null, null);
#else
                    Debug.Log($"当前使用的是Mono模式");
                    this.assembly = Assembly.Load(assBytes, pdbBytes);
                    this.hotfixTypes = this.assembly.GetTypes().ToList();
                    var methodInfo = assembly.GetType(GamePath.IL_Entry).GetMethod("Awake");
                    methodInfo?.Invoke(null, new object[methodInfo.GetParameters().Length]);
#endif
                };
            };


        }
    }

}

