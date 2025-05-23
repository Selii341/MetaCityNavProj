using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Limitcamera : MonoBehaviour
{
    public GameObject Player;
    private void LateUpdate()
    {
        transform.position = new Vector3(Player.transform.position.x, 30, Player.transform.position.z);
    }

}
