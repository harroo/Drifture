
public static class DriftureManager {

    public static string thisName;

    public static Action DespawnEntity (ulong entityId);
    public static Action InteractEntity (ulong entityId, object sender);
    public static Action AttackEntity (ulong entityId, int damage, object sender);
}
