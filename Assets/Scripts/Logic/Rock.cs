using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Rock : ItemBase
{
    int count = 0;
    void Start()
    {
        transform.GetComponent<Button>().onClick.AddListener(() => {
            ++count;
            if (count > 1)
            {
                // 变成道路
                type = 0;
                transform.GetComponent<CanvasGroup>().alpha = 0;
                // 通知全局，刷新界面
                transform.parent.parent.GetComponent<Main>().UpdateData();
            }
            else
            {
                transform.GetComponent<Image>().color = Color.red;
            }
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
