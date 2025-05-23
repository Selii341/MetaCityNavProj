using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlexTrigger : MonoBehaviour
{
    private bool taskDone = false;

    void OnTriggerEnter(Collider other)
    {
        if (taskDone) return;

        // 假设玩家手柄或玩家主体带有 tag “Player”
        if (other.CompareTag("Player"))
        {
            taskDone = true;

            // 停止巡逻
            var wander = GetComponent<Alex>();
            if (wander != null)
                wander.enabled = false;

            // 进入下一个任务
            TaskManager.Instance.NextTask();

            // 可选：隐藏 Alex 或播放一个提示动画
            // gameObject.SetActive(false);
        }
    }
}
