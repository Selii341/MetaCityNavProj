using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class IntroUI : MonoBehaviour
{
    public Text introText;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            // 交给 TaskManager 处理
            TaskManager.Instance.StartTasks();
        }
    }
}
