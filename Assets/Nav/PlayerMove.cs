using UnityEngine;
using UnityEngine.AI; 

public class PlayerMove : MonoBehaviour
{
    public NavMeshAgent nav; //获取导航网格代理组件，通过此组件来告知AI目标
    public Transform target; //目标的位置

    private void Update()
    {
        nav.SetDestination(target.position); //每帧更新目标位置
    }
}
