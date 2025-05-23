using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Renderer))]
public class WorldSpaceArrowController : MonoBehaviour
{
    public RectTransform arrowRect;  // your UI Image’s RectTransform
    public Image arrowImage; // the UI Image component
    public Camera playerCam;  // your main VR/AR camera
    public float maxDistance = 50f;
    public Vector3 localOffset = new Vector3(0, -0.2f, 2f);
    public Color farColor = Color.red;
    public Color nearColor = new Color(1f, 0.5f, 0f);

    void Start()
    {
        // Make arrow always in front of camera
        if (arrowRect != null && playerCam != null)
        {
            arrowRect.SetParent(playerCam.transform);
            arrowRect.localPosition = localOffset;
            arrowRect.localRotation = Quaternion.identity;
        }
    }
    void Update()
    {
        //int idx = TaskManager.Instance.CurrentIndex;

        //// Only show from Task 3 onwards (0-based idx 2)
        //bool show = (idx >= 2);
        //if (arrowImage.enabled != show)
        //{
        //    arrowImage.enabled = show;
        //    arrowRect.gameObject.SetActive(show);
        //}
        //if (!show) return;

        // 1) Float in front of camera
        //arrowRect.localPosition = localOffset;

        // 2) Compute flattened world direction to target
        Transform target = TaskManager.Instance.GetCurrentTarget();
        Vector3 toTarget = target.position - playerCam.transform.position;
        toTarget.y = 0;
        if (toTarget.sqrMagnitude < 0.001f) return;

        // 3) Convert to camera-local and get heading
        Vector3 localDir = playerCam.transform.InverseTransformDirection(toTarget.normalized);
        float angle = Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg;

        // 4) **Only change the Z angle** of the RectTransform
        Vector3 eul = arrowRect.localEulerAngles;
        eul.z = -angle;            // negative if your sprite points "up" originally
        arrowRect.localEulerAngles = eul;

        // 5) Fade & tint by distance
        float dist = toTarget.magnitude;
        float t = Mathf.Clamp01(1f - dist / maxDistance);
        Color c = Color.Lerp(farColor, nearColor, t);
        c.a = t;
        arrowImage.color = c;

    }
}

