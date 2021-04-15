using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Frame
{
    public enum ModuleLevel
    {
        CommonUI,
        TopUI,
        PopupUI,
        TopPopupUI,
    }

    [System.Serializable]
    public class ParentContainer
    {
        public ModuleLevel mLevel;
        public GameObject mContainer;
    }

    public class UIRoot : MonoBehaviour
    {
        private Dictionary<int, GameObject> Containers;
        [SerializeField]
        private List<ParentContainer> parentContainers;

        public static UIRoot Instance;
        void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Containers = new Dictionary<int, GameObject>();
            if (parentContainers == null) return;
            for (int i = 0; i < parentContainers.Count; i++)
            {
                Containers.Add((int)parentContainers[i].mLevel, parentContainers[i].mContainer);
            }
        }

        public GameObject GetParent(int level)
        {
              return Containers[level];
        }
    }
}

