using UnityEngine;

public class RestaurantTrigger : MonoBehaviour
{
    bool triggered = false;
    Collider myCollider;

    void Awake()
    {
        myCollider = GetComponent<Collider>();
        // only with a Trigger collider
        if (!myCollider.isTrigger)
            Debug.LogError("RestaurantTrigger needs isTrigger = true on its Collider!");
    }

    void OnTriggerEnter(Collider other)
    {
        // always log for debugging
        Debug.Log($"[RestaurantTrigger] OnTriggerEnter by {other.transform.root.name} (Tag={other.transform.root.tag})");

        if (triggered) return;

        // check root object tag
        var playerRoot = other.transform.root.gameObject;
        if (playerRoot.CompareTag("Player"))
        {
            Debug.Log("[RestaurantTrigger] Player detected! Calling NextTask()");
            triggered = true;
            TaskManager.Instance.NextTask();
            // disable so it never fires again
            myCollider.enabled = false;
        }
    }
}
