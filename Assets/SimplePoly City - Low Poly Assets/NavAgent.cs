using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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

        agent.updatePosition = false;
        agent.updateRotation = false;

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

        // 如果玩家走动了，可以实时更新线路（可选）
        if (agent.hasPath)
            UpdatePathLine(path.corners[path.corners.Length - 1]);
        agent.nextPosition = transform.position;
    }
}