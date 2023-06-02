using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    public int count = 6 * 7;
    public List<string> nameList = new List<string>() {"Way","Soil","Rock" };
    public int[,] data = new int[6, 7];
    List<ItemBase> items = new List<ItemBase>();
    void Start()
    {
        
        for (int i = 0; i < count; i++)
        {
            var name = RandomGeneration();
            var obj = Resources.Load<GameObject>(name);
            var item = Instantiate(obj, transform);
            var itemBase = item.transform.GetChild(0).GetComponent<ItemBase>();

            // 存储到一个记录的数据结构中
            var type = name == "Way" ? 0 : 1;
            data[i/7,i%7] = type;
            itemBase.type = type;
            items.Add(itemBase);
        }
        
        // 地图产生完成后，根据记录的完整数据
        // 生成相同地块链接
        // 生成路边显示，路后遮罩 
        FindOnes(data);

        
        //var isConnected = IsConnected(data);
        //Debug.LogError("==========" + isConnected);
    }


    //提供一个接口刷新界面数据结构
    public void UpdateData()
    {
        for (int i = 0; i < items.Count; i++)
        {
            data[i / 7, i % 7] = items[i].type;
            //取消之前的遮罩
            items[i].transform.parent.GetChild(1).gameObject.SetActive(false);
        }
        //重新生成遮罩
        FindOnes(data);
        var isConnected = IsConnected(data);
        Debug.LogError("==========" + isConnected);
        if (isConnected) //如果有道路能到最下方
        {
            DownMove();
        }
    }

    public void DownMove()
    {
        //生成新的地图
        for (int i = 6; i >= 0; i--)
        {
            DestroyImmediate(items[i].transform.parent.gameObject);
            //下移
            items.RemoveAt(i);
        }
        //将data数组的第一行移除，后面的移动到前面，最后一行重新赋值
        int[] newData = new int[7];
        for (int i = 0; i < 7; i++)
        {
            var name = RandomGeneration();
            var type = name == "Way" ? 0 : 1;
            newData[i] = type;

            var obj = Resources.Load<GameObject>(name);
            var item = Instantiate(obj, transform);
            var itemBase = item.transform.GetChild(0).GetComponent<ItemBase>();
            itemBase.type = type;
            items.Add(itemBase);
        }
        ShiftRows(data, newData);
        UpdateData();
    }

    /*
    int[,] data = {
    {1,0,0,1,1,1,1},
    {0,1,0,1,1,1,1},
    {1,0,1,1,1,0,1},
    {0,1,1,1,0,1,1},
    {0,1,0,1,1,1,1},
    {1,0,1,1,1,1,0},
};C#中有个二维数组，将第一行{1,0,0,1,1,1,1}移除，其他行上移，第二行变成第一行，第三行变成第二行，第四行变成第三行，以此递推；
    最后一行插入新的一行，提供一个方法
     
     
     */
    public static void ShiftRows(int[,] data, int[] newRow)
    {
        int numRows = data.GetLength(0);

        // 将各行向上移动一行
        for (int i = 0; i < numRows - 1; i++)
        {
            for (int j = 0; j < data.GetLength(1); j++)
            {
                data[i, j] = data[i + 1, j];
            }
        }

        // 最后一行变为新行
        for (int j = 0; j < data.GetLength(1); j++)
        {
            data[numRows - 1, j] = newRow[j];
        }
    }

    public bool IsConnected(int[,] data)
    {
        // 遍历第一行所有元素，找到第一个为0的元素
        for (int j = 0; j < data.GetLength(1); j++)
        {
            if (data[0, j] == 0)
            {
                // 从该元素开始进行深度优先搜索
                bool[,] visited = new bool[data.GetLength(0), data.GetLength(1)];
                if (DFS(data, visited, 0, j))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool DFS(int[,] data, bool[,] visited, int i, int j)
    {
        // 标记当前节点为已访问
        visited[i, j] = true;

        // 如果当前节点在最后一行，则表明能够连接成功
        if (i == data.GetLength(0) - 1)
        {
            return true;
        }

        // 搜索当前节点上下左右是否有0，并继续进行深度优先搜索
        if (i > 0 && data[i - 1, j] == 0 && visited[i - 1, j] == false && DFS(data, visited, i - 1, j))
        {
            return true;
        }
        if (i < data.GetLength(0) - 1 && data[i + 1, j] == 0 && visited[i + 1, j] == false && DFS(data, visited, i + 1, j))
        {
            return true;
        }
        if (j > 0 && data[i, j - 1] == 0 && visited[i, j - 1] == false && DFS(data, visited, i, j - 1))
        {
            return true;
        }
        if (j < data.GetLength(1) - 1 && data[i, j + 1] == 0 && visited[i, j + 1] == false && DFS(data, visited, i, j + 1))
        {
            return true;
        }

        return false;
    }


    public void FindOnes(int[,] data)
    {
        // 遍历二维数组
        for (int i = 0; i < data.GetLength(0); i++)
        {
            for (int j = 0; j < data.GetLength(1); j++)
            {
                // 如果数组中相应位置为1，检查其上下左右是否也为1
                if (data[i, j] == 1)
                {
                    bool up = (i == 0 || data[i - 1, j] == 1); // 检查上方
                    bool down = (i == data.GetLength(0) - 1 || data[i + 1, j] == 1); // 检查下方
                    bool left = (j == 0 || data[i, j - 1] == 1); // 检查左侧
                    bool right = (j == data.GetLength(1) - 1 || data[i, j + 1] == 1); // 检查右侧

                    // 如果上下左右都是1，则输出该位置
                    if (up && down && left && right)
                    {
                        Debug.LogError($"({i}, {j})");
                        transform.GetChild(i * 7 + j).transform.Find("Mask").gameObject.SetActive(true);
                    }
                }
            }
        }
    }

    string RandomGeneration()
    {
        int rock = 29;
        int soil = 34;
        int way = 37;
        // 计算总权重
        int totalWeight = rock + soil + way;

        int randomValue = Random.Range(0, totalWeight);

        if (randomValue < rock)
        {
            return nameList[2];
        }
        else if (randomValue < soil + way)
        {
            return nameList[1];
        }
        else
        {
            return nameList[0];
        }
    }
}
