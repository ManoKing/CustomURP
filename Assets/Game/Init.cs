using Frame;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Game
{
    public static class Init
    {
        public async static Task Awake()
        {
            Debug.Log("Hot Fix Start");
            await Login();
        }
        // TODO
        public async static Task Login()
        {
            var obj = await ResourceMgr.Instance.LoadAssetAsync<GameObject>("UI/Login");
            GameObject.Instantiate(obj, UIRoot.Instance.GetParent(1).transform);
        }
    }
}

