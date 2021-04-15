using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Frame
{
    public class ResourceMgr 
    {
        private static ResourceMgr _instance;
        public static ResourceMgr Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ResourceMgr();
                }
                return _instance;
            }
        }

        public Task<T> LoadAssetAsync<T>(string name) where T : Object
        {
            var op = Addressables.LoadAssetAsync<T>(name);
            return op.Task;
        }

        public void Unload(Object name)
        {
            Addressables.Release(name);
        }

        public void UnloadGameObject(GameObject obj)
        {
            Addressables.ReleaseInstance(obj);
        }
    }

}
