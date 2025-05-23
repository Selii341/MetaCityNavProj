using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class Alex : MonoBehaviour
{
   private bool triggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        // 假设玩家主体或手柄的 GameObject 打了 Tag “Player”说
        if (other.CompareTag("Player"))
        {
            triggered = true;
            // 通知 TaskManager 完成当前任务，进入下一任务
            TaskManager.Instance.NextTask();
        }
    }

}
