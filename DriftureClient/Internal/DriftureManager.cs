
using System;
using UnityEngine;

public static class DriftureManager {

    public static string thisName;

    public static Action <ulong, object> InteractEntity;
    public static Action <ulong, int, object> AttackEntity;

    public static Action <int, Vector3, byte[]> CreateEntity;
    public static Action <ulong> DeleteEntity;
}
