using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEngine.XR;
using System.Collections.Generic;

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance { get; private set; }

    [Header("UI Canvases")]
    public GameObject introCanvas;
    public GameObject hudCanvas;

    [Header("HUD Elements")]
    //public RectTransform arrowRect;   // 箭头的 RectTransform
    //public Image arrowImage;
    public RawImage miniMapImage;     // 小地图的 RawImage
    public Text taskText;             // “任务 X/5” 文本
    public Text timerText;            // “计时” 文本

    [Header("Audio Guidance (Task 5)")]
    public AudioSource navBeep;       // 循环导航声音

    [Header("Player & Tasks")]
    public Transform player;
    public Transform[] taskPoints;
    public string[] taskDescriptions;
    public float arriveDistance = 2f;

    private int currentIndex = 0;
    private bool running = false;
    private float startTime;

    private float initialStartTime;    // 记录总开始时间
    private float lastTaskStartTime;   // 记录当前关卡开始时刻
    private float[] taskTimes;         // 存放每关完成用时

    [Header("Task Locations")]
    public GameObject[] taskCubes;    // assign your 5 cubes in the Inspector

    [Header("3D Arrow (Task 4)")]
    public Image worldArrow;

    /// <summary>
    /// Show only the cube matching currentIndex, hide the rest
    /// </summary>
    void UpdateCubeMarkers()
    {
        for (int i = 0; i < taskCubes.Length; i++)
        {
            taskCubes[i].SetActive(i == currentIndex);
        }
    }


    // TaskManager.cs 中，在类体里添加：
    public int CurrentIndex => currentIndex;


    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 初始只显示 Intro
        introCanvas.SetActive(true);
        hudCanvas.SetActive(false);

        // HUD 元素先都隐藏
        //arrowImage.enabled = false;
        //arrowImage.enabled = false;
        miniMapImage.enabled = false;
        worldArrow.enabled = false;
        navBeep.enabled = false;
        timerText.text = "";

        taskTimes = new float[taskPoints.Length];
    }

    void Update()
    {
        // 任务未开始，等待按 E
        //if (!running)
        //{
        //    if (Input.GetKeyDown(KeyCode.E))
        //        StartTasks();
        //    return;
        //}
        if (!running && CheckStartButton())
        {
            StartTasks();

        }
        // 1）更新当前关卡的计时
        float now = Time.time;
        float currentElapsed = now - lastTaskStartTime;

        // 2）计算总用时
        float overallElapsed = now - initialStartTime;

        // 3）拼接已完成关卡的用时
        string line = $"Current ({currentIndex + 1}): {currentElapsed:F1}s\n";
        for (int i = 0; i < currentIndex; i++)
            line += $"Task{i + 1}: {taskTimes[i]:F1}s\n";
        line += $"Overall: {overallElapsed:F1}s";

        timerText.text = line;

        // 导航箭头
        Vector3 dir = (taskPoints[currentIndex].position - player.position).normalized;
        float ang = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        //arrowRect.rotation = Quaternion.Euler(0, 0, -ang);

        // 到达检测
        if (Vector3.Distance(player.position, taskPoints[currentIndex].position) < arriveDistance)
            NextTask();

    }
    bool CheckStartButton()
    {
        // look for the primary button on any right-hand controller
        var devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller,
            devices);

        foreach (var d in devices)
        {
            if (d.TryGetFeatureValue(CommonUsages.primaryButton, out bool pressed) && pressed)
                return true;
        }
        return false;
    }

    public void StartTasks()
    {
        // 切 UI、开时钟、进第一个任务
        introCanvas.SetActive(false);
        hudCanvas.SetActive(true);
        running = true;

        initialStartTime = Time.time;
        lastTaskStartTime = Time.time;
        currentIndex = 0;
        UpdateTaskUI();
        UpdateAids();
        UpdateCubeMarkers();            // <— show cube 0 only

        // 第一次为 Task1 自动寻路目标
        NavAgent.Instance.SetDestination(taskPoints[currentIndex].position);

    }

    public void NextTask()
    {
        Debug.Log($"NextTask called! OldIndex={currentIndex}");

        float now = Time.time;
        // 1）记录刚刚完成的关卡用时
        taskTimes[currentIndex] = now - lastTaskStartTime;

        // 2）切到下一关
        currentIndex++;
        lastTaskStartTime = now;

        if (currentIndex >= taskPoints.Length)
        {
            // 所有关卡完成，显示最终统计
            running = false;
            taskText.text = "Well Done！";
            UpdateAids();   // 关闭所有辅助

            // 立刻把最后一次的 elapsed 刷到 UI
            float overall = now - initialStartTime;
            string summary = "";
            for (int i = 0; i < taskTimes.Length; i++)
                summary += $"Task{i + 1}: {taskTimes[i]:F1}s\n";
            summary += $"Overall: {overall:F1}s";
            timerText.text = summary;
            return;
        }

        UpdateTaskUI();
        UpdateAids();
        UpdateCubeMarkers();
        //切换寻路目标到下一个 TaskPoint
        NavAgent.Instance.SetDestination(taskPoints[currentIndex].position);
        // Recalculate path & redraw the line
        Vector3 targetPos = TaskManager.Instance.GetCurrentTarget().position;
        NavAgent.Instance.SetDestination(targetPos);
        //NavAgent.Instance.SetDestination(GetCurrentTarget().position);




    }
    void UpdateTaskUI()
    {
        taskText.text = $"Current Task {currentIndex + 1}/{taskPoints.Length}: {taskDescriptions[currentIndex]}";
    }
    /// <summary>
    /// 根据 currentIndex 决定开启哪些辅助
    /// </summary>
    void UpdateAids()
    {
        // 任务序号从 0 开始算，所以 +1
        int taskNum = currentIndex + 1;

        // 箭头：从 2 号任务开始可见
        //arrowImage.enabled = (taskNum >= 2 && taskNum < 4);

        // 3D world arrow 
        // WORLD-SPACE ARROW: show from Task 3 onward
        worldArrow.enabled = (taskNum == 2 || taskNum == 5);


        // 小地图：从 3 号任务开始可见
        miniMapImage.enabled = (taskNum == 3 || taskNum == 5);

        // 声音：从 5 号任务开始可用
        navBeep.enabled = (taskNum >= 4);
    }

    public Transform GetCurrentTarget()
    {
        return taskPoints[currentIndex];
    }
   
}
