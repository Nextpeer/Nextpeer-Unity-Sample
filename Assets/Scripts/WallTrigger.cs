using System;
using System.Collections.Generic;
using UnityEngine;

public class WallTrigger:MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {
        Nextpeer.PushDataToOtherPlayers(System.Text.Encoding.UTF8.GetBytes(NextpeerGameManager.TRIGG_WALLS));
        Destroy(this.gameObject);
    }
}