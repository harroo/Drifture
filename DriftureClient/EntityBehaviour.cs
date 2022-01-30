
using UnityEngine;

using Drifture;

public class EntityBehaviour : MonoBehaviour {

    public bool isLocal
        => controller == DriftureManager.thisName;

    [HideInInspector] public string controller;
    [HideInInspector] public ulong entityId;
    [HideInInspector] public int entityType;

    private bool hasRigidbody => __rigidBody != null;
    private Rigidbody __rigidBody;
    private RigidbodyConstraints rigidbodyConstraints;

    private void Start () {

        __rigidBody = GetComponent<Rigidbody>();
        if (__rigidBody != null)
            rigidbodyConstraints = __rigidBody.constraints;

        Spawn();
    }

    private void Update () {

        if (hasRigidbody)
            __rigidBody.constraints = isLocal
                ? rigidbodyConstraints
                : RigidbodyConstraints.FreezeAll;

        UnlocalTick();

        if (isLocal) {

            Tick();
            LocalStreamCheck();

            return;
        }
    }

    public Transform head;
    private float __timer = 1.0f;

    private void LateUpdate () {

        if (!isLocal) return;

        __timer -= Time.deltaTime;
        if (__timer < 0) __timer = 0.1f; else return;

        EntityManager.UpdateEntityPosition(
            entityId, transform.position,
            head == null ? Quaternion.identity : head.rotation
        );
    }

    private float __localTimer = 4.0f;
    private Vector3 _posCache;
    private Quaternion _rotCache;

    private void LocalStreamCheck () {

        __localTimer -= Time.deltaTime;
        if (__localTimer < 0) __localTimer = 1.0f; else return;

        if (_posCache == transform.position
            && _rotCache == head.rotation) return;

        _posCache = transform.position;
        _rotCache = head.rotation;

        EntityManager.EnsureEntityPosition(
            entityId, transform.position,
            head == null ? Quaternion.identity : head.rotation
        );
    }

    public void SetPos (Vector3 position, Quaternion rotation) {

        transform.position = position;
        if (head != null) head.rotation = rotation;
    }
    public void SetData (byte[] metaData) {

        OnMetaDataSet(metaData);
    }
    public byte[] GetData () {

        return MetaDataRequest();
    }
    public void OnDespawnEvent () {
        if (isLocal) OnDespawn();
        OnDespawnShow();
    }
    public void OnInteractEvent (object sender) {
        if (isLocal) OnInteract(sender);
        OnInteractShow(sender);
    }
    public void OnAttackEvent (int damage, object sender) {
        if (isLocal) OnAttack(damage, sender);
        OnAttackShow(damage, sender);
    }

    public void SyncMetaData () {

        if (!isLocal) return;

        EntityManager.SyncEntityMetaData(entityId, GetData());
    }

    public virtual void Spawn () {} //called from Start();

    public virtual void Tick () {} //called once per frame if locally controlled

    public virtual void UnlocalTick () {} //called once per frame even if not locally controlled

    public virtual void OnDespawn () {} //called on despawn locally only

    public virtual void OnDespawnShow () {} //called on despawn no matter what

    public virtual void OnInteract (object sender) {} //called when right clicked only if local

    public virtual void OnInteractShow (object sender) {} //called when right clicked no matter what

    public virtual void OnAttack (int damage, object sender) {} //called when left clicked only if local

    public virtual void OnAttackShow (int damage, object sender) {} //called when left clicked no matter what

    public virtual void OnMetaDataSet (byte[] metaData) {} //called when metadata is set local or not

    public virtual byte[] MetaDataRequest () { return new byte[0]; } //called as a req for the meta data
}
