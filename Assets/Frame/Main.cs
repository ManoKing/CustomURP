using DH.Frame.Res;
using DH.Frame.ThreadMgr;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ResMgr.Init();
        ThreadJob.Init();
        ILLauncher.Instance.Init();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
