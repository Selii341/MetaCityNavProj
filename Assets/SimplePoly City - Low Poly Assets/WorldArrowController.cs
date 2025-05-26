using UnityEngine;
using UnityEngine.UI;

public class WorldSpaceArrowController : MonoBehaviour
{
    public RectTransform arrowRect;       // UI Image inside World Space Canvas
    public Image arrowImage;
    public Camera playerCam;
    public float maxDistance = 50f;
    public Vector3 localOffset = new Vector3(0f, -0.2f, 2f);
    public Color farColor = Color.red;
    public Color nearColor = new Color(1f, 0.5f, 0f);

    private Quaternion initialLocalRotation;

    void Start()
    {
        if (arrowRect == null || playerCam == null)
        {
            Debug.LogWarning("Assign arrowRect and playerCam in inspector");
            enabled = false;
            return;
        }

        // Save the original local rotation (with Y = -75 tilt)
        initialLocalRotation = arrowRect.localRotation;

        // Make it child of camera so it follows head
        //arrowRect.SetParent(playerCam.transform, false);
    }

    void Update()
    {
        if (!arrowImage.enabled) return;

        Transform target = TaskManager.Instance.GetCurrentTarget();
        if (target == null) return;



        // Always follow head position (HMD) — in front of eyes
        arrowRect.localPosition = localOffset;

        // Get flattened direction
        Vector3 camPos = playerCam.transform.position;
        Vector3 toTarget = target.position - camPos;
        toTarget.y = 0;

        if (toTarget.sqrMagnitude < 0.001f) return;

        Vector3 forward = playerCam.transform.forward;
        forward.y = 0;

        float angle = Vector3.SignedAngle(forward.normalized, toTarget.normalized, Vector3.up);

        // Only rotate Z, keep original X and Y from prefab
        arrowRect.localRotation = initialLocalRotation * Quaternion.Euler(0, 0, -angle);

        // Fade by distance
        float dist = toTarget.magnitude;
        float t = Mathf.Clamp01(1f - dist / maxDistance);
        Color c = Color.Lerp(farColor, nearColor, t);
        c.a = t;
        arrowImage.color = c;
    }
}
