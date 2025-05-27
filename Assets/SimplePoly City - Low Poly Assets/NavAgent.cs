using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.Experimental.GraphView.GraphView;

public class NavAgent : MonoBehaviour
{
    public static NavAgent Instance { get; private set; }

    [Header("Components")]
    public NavMeshAgent agent;
    public LineRenderer line;

    private NavMeshPath path;

    void Awake()
    {
        //Remove Agent movement, keep it only for pathfinding

        //agent.updatePosition = false;
        //agent.updateRotation = false;

        if (Instance == null) Instance = this;
        else DestroyImmediate(gameObject);

        path = new NavMeshPath();
    }

    /// <summary>
    /// 给定一个新目标时调用
    /// </summary>
    public void SetDestination(Vector3 target)
    {
        if (agent == null) return;

        agent.SetDestination(target);

        // 同步计算路径并画线
        UpdatePathLine(target);
    }

    void UpdatePathLine(Vector3 target)
    {
        NavMesh.CalculatePath(agent.transform.position, target, NavMesh.AllAreas, path);

        // 绘制
        line.positionCount = path.corners.Length;
        line.SetPositions(path.corners);
    }

    void Update()
    {
        if (agent == null || path == null) return;
        int idx = TaskManager.Instance.CurrentIndex + 1;
        // decide which tasks get a path at all:
        //bool drawPath = (idx == 2 || idx == 3);  // example: only tasks 3 & 5
        //if (!drawPath)
        //{
        //    line.enabled = false;
        //    return;
        //}

        // 2) Decide per task+state which views should see it:
        //    e.g. Task 3 = minimap only; Task 4 = both; Task 5 = XR only
        //bool inMinimap = TaskManager.Instance.MiniMapVisible;
        //bool inBoth = (idx == 5);    // example: task 4 shows in both
        //bool inXR = !inMinimap || inBoth;  // XR sees it when minimap is off, or when BOTH
        //bool inMap = inMinimap || inBoth;  // Minimap sees it when on map, or when BOTH
        //bool inMap = inMinimap || inBoth;  // Minimap sees it when on map, or when BOTH

        // 3) Enable/disable and re‐layer the line so each camera culls correctly:
        if (idx == 5)
        {
            line.enabled = true;
            line.gameObject.layer = LayerMask.NameToLayer("UI");
        }
        else if (idx == 2)
        {
            line.enabled = true;
            line.gameObject.layer = LayerMask.NameToLayer("Player");
        }
        else if (idx == 1)
        {
            line.enabled = true;
            line.gameObject.layer = LayerMask.NameToLayer("Minimap");
        }
        else
        {
            // drawPath==true but neither view wants it? unlikely, but just in case:
            line.enabled = false;
            return;
        }

        // 如果玩家走动了，可以实时更新线路（可选）
        if (agent.hasPath)
            UpdatePathLine(path.corners[path.corners.Length - 1]);
        //agent.nextPosition = transform.position;
    }
}