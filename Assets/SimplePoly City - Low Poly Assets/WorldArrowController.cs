using UnityEngine;
using UnityEngine.UI;

public class WorldSpaceArrowController : MonoBehaviour
{
    public RectTransform arrowRect;  // Your UI Image’s RectTransform (child of a World Space Canvas)
    public Image arrowImage;          // The UI Image component
    public Camera playerCam;          // Your main VR/AR camera
    public float maxDistance = 50f;
    public Vector3 localOffset = new Vector3(0, -0.2f, 2f);  // Offset relative to camera
    public Color farColor = Color.red;
    public Color nearColor = new Color(1f, 0.5f, 0f);

    void Start()
    {
        // Make sure the arrow stays in the right parent (do NOT reparent it to camera)
        if (arrowRect == null || playerCam == null)
        {
            Debug.LogWarning("Please assign arrowRect and playerCam in inspector");
            enabled = false;
            return;
        }

        // Optional: Reset rotation and position of arrowRect initially
        arrowRect.localRotation = Quaternion.identity;
    }

    void Update()
    {
        Transform target = TaskManager.Instance.GetCurrentTarget();
        if (target == null)
        {
            arrowImage.enabled = false;  // Hide arrow if no target
            return;
        }
        else
        {
            arrowImage.enabled = true;
        }

        // 1) Position the arrow floating in front of the camera with offset
        Vector3 worldPos = playerCam.transform.position + playerCam.transform.TransformVector(localOffset);
        arrowRect.position = worldPos;

        // 2) Compute flattened direction from camera to target (ignore Y for horizontal direction)
        Vector3 toTarget = target.position - playerCam.transform.position;
        toTarget.y = 0;
        if (toTarget.sqrMagnitude < 0.001f) return;

        // 3) Convert to camera-local space direction and get angle to rotate arrow
        Vector3 localDir = playerCam.transform.InverseTransformDirection(toTarget.normalized);
        float angle = Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg;

        // 4) Only rotate around Z axis (arrow sprite should point "up" by default)
        Vector3 euler = arrowRect.localEulerAngles;
        euler.z = -angle;  // Negative if arrow points up originally
        arrowRect.localEulerAngles = euler;

        // 5) Fade and tint by distance
        float dist = toTarget.magnitude;
        float t = Mathf.Clamp01(1f - dist / maxDistance);
        Color c = Color.Lerp(farColor, nearColor, t);
        c.a = t;  // alpha fades with distance
        arrowImage.color = c;
    }
}
