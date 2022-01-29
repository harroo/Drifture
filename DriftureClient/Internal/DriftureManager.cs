
using System;

public static class DriftureManager {

    public static string thisName;

    public static Action <ulong> DespawnEntity;
    public static Action <ulong, object> InteractEntity;
    public static Action <ulong, int, object> AttackEntity;
}
