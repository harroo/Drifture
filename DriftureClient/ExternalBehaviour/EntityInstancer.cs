
using UnityEngine;

using System;

public class EntityInstancer : MonoBehaviour {

    public EntityInstance[] instances;

    public EntityBehaviour CreateInstance (int type, Vector3 pos, Quaternion rot) {

        EntityInstance instance = Array.Find(instances, e => e.type == type);

        return Instantiate(instance.prefab, pos, rot)
            .GetComponent<EntityBehaviour>();
    }
}

[Serializable]
public class EntityInstance {

    public string name;

    public int type;

    public GameObject prefab;
}
