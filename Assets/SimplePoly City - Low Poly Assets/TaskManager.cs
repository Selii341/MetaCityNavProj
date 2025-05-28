using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEngine.XR;
using System.Collections.Generic;
using System.Collections;

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance { get; private set; }

    [Header("UI Canvases")]
    public GameObject introCanvas;
    public GameObject endCanvas;
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
    public bool MiniMapVisible => miniMapImage.enabled;
    private float totalActiveTime = 0f;
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
        endCanvas.SetActive(false);
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
        // --- 1) If we're not currently running ---
        if (!running)
        {
            // Only allow starting when intro canvas is visible:
            if (introCanvas.activeSelf && CheckStartButton())
            {
                StartTasks();
            }
            // In any case, bail out so we don't tick timers or re-trigger StartTasks
            return;
        }

        // --- 2) While running: update UI + check arrival ---
        UpdateTimerUI();
        UpdateNavigationArrow();

        // If we've reached the current target, go to next
        if (Vector3.Distance(player.position, taskPoints[currentIndex].position) < arriveDistance)
            NextTask();

    }
   
    bool CheckStartButton()
    {
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

        currentIndex = 0;
        initialStartTime = Time.time;
        lastTaskStartTime = Time.time;

        UpdateTaskUI();
        UpdateAids();
        UpdateCubeMarkers();
        NavAgent.Instance.SetDestination(taskPoints[0].position);         // <— show cube 0 only

        //// 第一次为 Task1 自动寻路目标
        //NavAgent.Instance.SetDestination(taskPoints[currentIndex].position);

    }

    public void NextTask()
    {
        // Stop ticking and record time immediately
        running = false;
        float now = Time.time;
        taskTimes[currentIndex] = now - lastTaskStartTime;
        totalActiveTime += taskTimes[currentIndex];

        // Advance index BEFORE showing popup
        currentIndex++;

        // Show popup coroutine
        StartCoroutine(ShowFinishNotification(currentIndex));

        Debug.Log($"NextTask called! OldIndex={currentIndex}");



        StartCoroutine(ShowFinishNotification(currentIndex));

        //if (currentIndex >= taskPoints.Length)
        //{
        //    // 所有关卡完成，显示最终统计
        //    running = false;
        //    taskText.text = "Well Done！";
        //    UpdateAids();   // 关闭所有辅助

        //    // 立刻把最后一次的 elapsed 刷到 UI
        //    float overall = now - initialStartTime;
        //    string summary = "";
        //    for (int i = 0; i < taskTimes.Length; i++)
        //        summary += $"Task{i + 1}: {taskTimes[i]:F1}s\n";
        //    summary += $"Overall: {overall:F1}s";
        //    timerText.text = summary;
        //    return;
        //}

        //UpdateTaskUI();
        //UpdateAids();
        //UpdateCubeMarkers();
        ////切换寻路目标到下一个 TaskPoint
        //NavAgent.Instance.SetDestination(taskPoints[currentIndex].position);
        //// Recalculate path & redraw the line
        //Vector3 targetPos = TaskManager.Instance.GetCurrentTarget().position;
        //NavAgent.Instance.SetDestination(targetPos);
        ////NavAgent.Instance.SetDestination(GetCurrentTarget().position);


    }
    IEnumerator ShowFinishNotification(int finishedTaskNumber)
    {
        endCanvas.SetActive(true);
        var txt = endCanvas.GetComponentInChildren<Text>();
        txt.text = $"Task {finishedTaskNumber} Complete!";

        // wait for left‐hand primary button
        bool pressed = false;
        while (!pressed)
        {
            var devices = new List<InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(
                InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller,
                devices);

            foreach (var d in devices)
            {
                if (d.TryGetFeatureValue(CommonUsages.primaryButton, out pressed) && pressed)
                    break;
            }
            yield return null;
        }

        endCanvas.SetActive(false);

        // If there’s more tasks, resume; else show final summary
        if (currentIndex < taskPoints.Length)
        {
            running = true;
            hudCanvas.SetActive(true);

            lastTaskStartTime = Time.time;
            UpdateTaskUI();
            UpdateAids();
            UpdateCubeMarkers();
            NavAgent.Instance.SetDestination(taskPoints[currentIndex].position);
        }
        else
        {
            // all done: show final times
            taskText.text = "Well Done!\nAll Tasks Complete";
            float overall = Time.time - initialStartTime;
            string summary = "";
            for (int i = 0; i < taskTimes.Length; i++)
                summary += $"Task {i + 1}: {taskTimes[i]:F1}s\n";
            summary += $"Overall Active: {totalActiveTime:F1}s";

            timerText.text = summary;
        }
    }
    void UpdateTimerUI()
    {
        float now = Time.time;
        float currentElapsed = now - lastTaskStartTime;
        float overallElapsed = now - initialStartTime;

        string line = $"Current ({currentIndex + 1}): {currentElapsed:F1}s\n";
        for (int i = 0; i < currentIndex; i++)
            line += $"Task {i + 1}: {taskTimes[i]:F1}s\n";
        line += $"Overall: {overallElapsed:F1}s";

        timerText.text = line;
    }

    void UpdateNavigationArrow()
    {
        Vector3 dir = (taskPoints[currentIndex].position - player.position).normalized;
        float ang = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        // apply to your arrow RectTransform here if needed
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
        worldArrow.enabled = (taskNum >= 2);


        // 小地图：从 3 号任务开始可见
        miniMapImage.enabled = (taskNum == 3|| taskNum == 5);

        // 声音：从 5 号任务开始可用
        navBeep.enabled = (taskNum == 1 || taskNum == 5);
    }
    //1.arrow only/+minimap/+blueline/+audio/+combo
    //2.arrow only/+blueline/+minimap/+audio/+combo
    //4.arrow only/+minimap/+audio/+blueline/+combo
    //3.arrowonly/+blueline/+audio/minimap/+combo
    //5.arrow only/+audio/+blueline/+minimap/+combo
    //6.arrow only/+audio/+minimap/+blueline/+combo
    //7.audio only/+arrow/+minimap/+blueline/+combo
    //8.audio only/+arrow/+blueline/+minimap/+combo
    //9.blueline only/+arrow/+audio/+minimap/+combo
    //10.minimap only/+arrow/+audio/+blueline/+combo
    public Transform GetCurrentTarget()
    {
        return taskPoints[currentIndex];
    }
   
}
