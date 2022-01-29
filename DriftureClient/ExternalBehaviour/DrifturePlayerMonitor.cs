
using UnityEngine;

using System;

using Drifture;

public class DrifturePlayerMonitor : MonoBehaviour {

    private float timer = 1.0f;

    private void Update () {

        timer -= Time.deltaTime;
        if (timer < 0) timer = 1.64f; else return;

        Submanager.SyncPlayerPosToServer(transform.position);
    }
}
