using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GUDArrow : MonoBehaviour
{
    public RectTransform arrowRect;
    public Image arrowImage;
    public Transform player;
    public float maxDistance = 20f;

    void Update()
    {
        // 1. Grab the active target from TaskManager
        Transform target = TaskManager.Instance.GetCurrentTarget();

        // 2. Rotate arrow to face it
        Vector3 dir = (target.position - player.position).normalized;
        float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        arrowRect.rotation = Quaternion.Euler(0, 0, -angle);

        // 3. Fade arrow by distance
        float dist = Vector3.Distance(player.position, target.position);
        float alpha = Mathf.Clamp01(1f - dist / maxDistance);
        var c = arrowImage.color;
        c.a = alpha;
        arrowImage.color = c;
    }
}
