using Frame.Res;
using Frame.ThreadMgr;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frame
{
    public class Main : MonoBehaviour
    {
        void Start()
        {
            ILLauncher.Instance.Init();
        }
    }
}


