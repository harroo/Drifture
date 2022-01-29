
using UnityEngine;

public class ExampleEntity : EntityBehaviour {

    public Material mat1, mat2;

    public override void UnlocalTick () {

        GetComponent<MeshRenderer>().material = isLocal ? mat1 : mat2;
    }

    public override void Tick () {

        transform.Rotate(1 * Time.deltaTime, 2 * Time.deltaTime, 3 * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Space)) {

            float sl = Random.Range(2, 6) / 6.0f;
            transform.localScale = new Vector3(sl, sl, sl);

            SyncMetaData();
        }
    }

    public override byte[] MetaDataRequest () {

        return System.BitConverter.GetBytes(transform.localScale.x);
    }
    public override void OnMetaDataSet (byte[] data) {

        if (data.Length != 4) return;

        float sl = System.BitConverter.ToSingle(data, 0);
        transform.localScale = new Vector3(sl, sl, sl);
    }

    public override void OnAttack (int damage, object sender) {

        Debug.Log("attacked by " + (string)sender);

        DriftureManager.DeleteEntity(entityId);
    }
}
