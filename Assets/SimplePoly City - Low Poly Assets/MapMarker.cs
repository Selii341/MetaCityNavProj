using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapMarker : MonoBehaviour
{
    [Header("Minimap UI")]
    public Camera minimapCam;
    public RectTransform minimapContainer;
    public Image minimapPrefab;     // your red-pin UI image
    private Image minimapInstance;

    [Header("World Marker")]
    public GameObject worldPinPrefab;     // a small 3D arrow or billboard
    private GameObject worldPinInstance;

    void Awake()
    {
        // instantiate but start disabled
        minimapInstance = Instantiate(minimapPrefab, minimapContainer);
        minimapInstance.gameObject.SetActive(false);

        worldPinInstance = Instantiate(worldPinPrefab, transform.position,
                                        Quaternion.identity, transform);
        worldPinInstance.SetActive(false);
    }

    void Update()
    {
        if (minimapInstance.gameObject.activeSelf)
        {
            // follow world → minimap
            Vector3 vp = minimapCam.WorldToViewportPoint(transform.position);
            float x = (vp.x - 0.5f) * minimapContainer.rect.width;
            float y = (vp.y - 0.5f) * minimapContainer.rect.height;
            minimapInstance.rectTransform.anchoredPosition = new Vector2(x, y);
        }
    }

    /// <summary>
    /// Called by TaskManager when this marker should appear/disappear.
    /// </summary>
    public void SetActive(bool on)
    {
        minimapInstance.gameObject.SetActive(on);
        worldPinInstance.SetActive(on);
    }
}
