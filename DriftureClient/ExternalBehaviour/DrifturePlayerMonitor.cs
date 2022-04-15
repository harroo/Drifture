
using UnityEngine;

using System;

using Drifture;

public class DrifturePlayerMonitor : MonoBehaviour {

    private float timer = 1.0f;

    private Vector3 posCache;

    private void Update () {

        timer -= Time.deltaTime;
        if (timer < 0) timer = 1.64f; else return;

        if (posCache == transform.position) return;
        posCache = transform.position;

        Submanager.SyncPlayerPosToServer(transform.position);
    }
}
