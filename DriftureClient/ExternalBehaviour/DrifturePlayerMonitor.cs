
using UnityEngine;

using System;

public class DrifturePlayerMonitor : MonoBehaviour {

    private float timer = 1.0f;

    private void Update () {

        __timer -= Time.deltaTime;
        if (__timer < 0) __timer = 1.64f; else return;

        Submanager.SyncPlayerPosToServer(transform.position);
    }
}
