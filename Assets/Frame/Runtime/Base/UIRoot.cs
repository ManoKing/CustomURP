using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frame
{
    public class UIRoot : MonoBehaviour
    {
        public static UIRoot Instance;
        void Awake()
        {
            Instance = this;
        }
    }
}

