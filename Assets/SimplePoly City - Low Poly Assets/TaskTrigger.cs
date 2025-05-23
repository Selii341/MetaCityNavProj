using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

[RequireComponent(typeof(Collider))]
public class TaskTrigger : MonoBehaviour
{
   [Tooltip("0-based task index — eg. Alex=0, Restaurant=1, Stadium=2, Supermarket=3, Home=4")]
    public int requiredTaskIndex;

    private bool isInside = false;
    private bool hasFired = false;
    //private bool triggered = false;
    private Collider col;

    void Awake()
    {
        col = GetComponent<Collider>();
        if (!col.isTrigger)
            Debug.LogWarning($"{name}: Collider.isTrigger should be checked!");
    }

    void OnTriggerEnter(Collider other)
    {
        // Only fire once
        if (hasFired) return;

        // Only at the right task
        if (TaskManager.Instance.CurrentIndex != requiredTaskIndex) return;

        // Only when the Player (root) enters
        if (!other.transform.root.CompareTag("Player")) return;

        // Fire!
        hasFired = true;
        Debug.Log($"{name}: Task {requiredTaskIndex + 1} triggered by Player collision.");

        TaskManager.Instance.NextTask();

        // Disable this script so it never fires again
        enabled = false;

    }
    //void OnTriggerExit(Collider other)
    //{
    //    // stop listening once controller leaves
    //    if (other.gameObject.CompareTag("Player"))
    //        isInside = false;
    //}

    //void Update()
    //{
    //    if (!isInside || hasFired) return;

    //    // get the right‐hand controller device
    //    InputDevice rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
    //    if (rightHand.TryGetFeatureValue(CommonUsages.primaryButton, out bool pressed) && pressed)
    //    {
    //        // fire once
    //        hasFired = true;
    //        Debug.Log($"{name}: Task {requiredTaskIndex + 1} triggered by A-button.");

    //        TaskManager.Instance.NextTask();

    //        // disable this script so it never fires again
    //        enabled = false;
    //    }
    //}

}
