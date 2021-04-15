using Frame.Res;
using Frame.ThreadMgr;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Frame
{
    public class Main : MonoBehaviour
    {
        /// <summary>
        /// 设计分辨率
        /// </summary>
        private Vector2 designResolution = new Vector2(1920f, 1080f);
        void Start()
        {
            AdaptationScreen();
            ILLauncher.Instance.Init();
        }

        /// <summary>
        /// 屏幕适配
        /// </summary>
        private void AdaptationScreen()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            CanvasScaler[] scalers = UIRoot.Instance.gameObject.GetComponentsInChildren<CanvasScaler>();
            foreach (var scaler in scalers)
            {
                //Root节点下其设置的基准分辨率为准
                scaler.referenceResolution = designResolution;
                //通常按高高比和宽宽比中较小的那边比例为准，防止适配出屏
                if (Screen.width / scaler.referenceResolution.x > Screen.height / scaler.referenceResolution.y)
                {
                    scaler.matchWidthOrHeight = 1.0f;
                }
                else
                {
                    scaler.matchWidthOrHeight = 0f;
                }
            }
        }
    }
}


