
//uses blitzbit for networking
//https://github.com/harroo/blitzbit

using UnityEngine;

using System.Collections.Generic;

using Drifture;
using BlitzBit;

public class GameManager: MonoBehaviour {

    public GameObject playerPrefab;
    public Transform player, cam;
    private int playerId;

    private BlitClient client;
    private UBlitClient uclient;

    private Dictionary<int, GameObject> playerCache
        = new Dictionary<int, GameObject>();

    private void Start () {

        //declare some stuff
        playerId = Random.Range(111111, 999999);
        client = new BlitClient();

        client.onError = (string error) => { Debug.LogError(error); };
        client.onLog = (string msg) => { Debug.Log(msg); };

        //configure drifture manager
        DriftureManager.thisName = "player_" + playerId.ToString();

        DriftureManager.InteractEntity = (ulong entityId, object sender) => {

            BlitPacket packet = new BlitPacket();
            packet.Append(entityId);
            packet.AppendT(sender);

            client.Send(3, packet.ToArray());
        };
        DriftureManager.AttackEntity = (ulong entityId, int damage, object sender) => {

            BlitPacket packet = new BlitPacket();
            packet.Append(entityId);
            packet.Append(damage);
            packet.AppendT(sender);

            client.Send(4, packet.ToArray());
        };
        DriftureManager.CreateEntity = (int type, Vector3 position, byte[] metaData) => {

            BlitPacket packet = new BlitPacket();
            packet.Append(type);
            packet.Append(position.x); packet.Append(position.y); packet.Append(position.z);
            packet.Append(metaData);

            client.Send(5, packet.ToArray());
        };
        DriftureManager.DeleteEntity = (ulong entityId) => {

            BlitPacket packet = new BlitPacket();
            packet.Append(entityId);

            client.Send(6, packet.ToArray());
        };

        //configure the entitymanager
        EntityManager.UpdateEntityPosition = (ulong entityId, Vector3 pos, Quaternion rot) => {

            BlitPacket packet = new BlitPacket();
            packet.Append(entityId);
            packet.Append(pos.x); packet.Append(pos.y); packet.Append(pos.z);
            packet.Append(rot.x); packet.Append(rot.y); packet.Append(rot.z); packet.Append(rot.w);

            uclient.Send(7, packet.ToArray());
        };
        EntityManager.EnsureEntityPosition = (ulong entityId, Vector3 pos, Quaternion rot) => {

            BlitPacket packet = new BlitPacket();
            packet.Append(entityId);
            packet.Append(pos.x); packet.Append(pos.y); packet.Append(pos.z);
            packet.Append(rot.x); packet.Append(rot.y); packet.Append(rot.z); packet.Append(rot.w);

            client.Send(8, packet.ToArray());
        };
        EntityManager.SyncEntityMetaData = (ulong entityId, byte[] metaData) => {

            BlitPacket packet = new BlitPacket();
            packet.Append(entityId);
            packet.Append(metaData);

            client.Send(10, packet.ToArray());
        };
        //update metadata
        client.AddPacket(11, (byte[] rb)=>{BlitPacket packet = new BlitPacket(rb);

            ulong entityId = packet.GetUInt64();
            byte[] metaData = packet.GetByteArray();

            EntityManager.UpdateMetaData(entityId, metaData);
        });
        //spawn entity
        client.AddPacket(12, (byte[] rb)=>{BlitPacket packet = new BlitPacket(rb);

            Debug.Log("spawn entity");

            ulong entityId = packet.GetUInt64();
            int type = packet.GetInt32();
            Vector3 position = new Vector3(packet.GetSingle(), packet.GetSingle(), packet.GetSingle());
            Quaternion rotation = new Quaternion(packet.GetSingle(), packet.GetSingle(), packet.GetSingle(), packet.GetSingle());
            byte[] metaData = packet.GetByteArray();

            EntityManager.SpawnEntity(entityId, type, position, rotation, metaData);
        });
        //spawn entity to
        client.AddPacket(16, (byte[] rb)=>{BlitPacket packet = new BlitPacket(rb);

            Debug.Log("spawn entity to");

            ulong entityId = packet.GetUInt64();
            int type = packet.GetInt32();
            Vector3 position = new Vector3(packet.GetSingle(), packet.GetSingle(), packet.GetSingle());
            Quaternion rotation = new Quaternion(packet.GetSingle(), packet.GetSingle(), packet.GetSingle(), packet.GetSingle());
            byte[] metaData = packet.GetByteArray();
            string nameId = packet.GetString();

            if (nameId != DriftureManager.thisName) return;

            EntityManager.SpawnEntity(entityId, type, position, rotation, metaData);
        });
        //despawn entity
        client.AddPacket(13, (byte[] rb)=>{BlitPacket packet = new BlitPacket(rb);

            ulong entityId = packet.GetUInt64();

            EntityManager.DespawnEntity(entityId);
        });
        //despawn entity to
        client.AddPacket(17, (byte[] rb)=>{BlitPacket packet = new BlitPacket(rb);

            Debug.Log("spawn entity to");

            ulong entityId = packet.GetUInt64();
            string nameId = packet.GetString();

            if (nameId != DriftureManager.thisName) return;

            EntityManager.DespawnEntity(entityId);
        });
        //interact entity
        client.AddPacket(14, (byte[] rb)=>{BlitPacket packet = new BlitPacket(rb);

            ulong entityId = packet.GetUInt64();
            object sender = packet.GetObject();

            EntityManager.InteractEntity(entityId, sender);
        });
        //attack entity
        client.AddPacket(15, (byte[] rb)=>{BlitPacket packet = new BlitPacket(rb);

            ulong entityId = packet.GetUInt64();
            int damage = packet.GetInt32();
            object sender = packet.GetObject();

            EntityManager.AttackEntity(entityId, damage, sender);
        });


        //configure clients
        client.useCallBacks = true;

        //on control update
        client.AddPacket(0, (byte[] rb)=>{

            Submanager.UpdateControl(rb);
        });

        //on client join
        client.AddPacket(1, (byte[] rb)=>{BlitPacket packet = new BlitPacket(rb);

            Debug.Log("client joned: ");

            int id = packet.GetInt32(); if (playerId == id) return;
            if (playerCache.ContainsKey(id)) return;

            playerCache.Add(id,
                Instantiate(playerPrefab, Vector3.zero, Quaternion.identity)
            );

            Debug.Log("client joned: " + id.ToString());
        });

        //on client leave
        client.AddPacket(2, (byte[] rb)=>{BlitPacket packet = new BlitPacket(rb);

            int id = packet.GetInt32(); if (playerId == id) return;
            if (!playerCache.ContainsKey(id)) return;

            Destroy(playerCache[id]);
            playerCache.Remove(id);

            Debug.Log("client leaves: " + id.ToString());
        });

        //connect to the server
        client.Connect("localhost", 12369);

        //send join message
        BlitPacket joinPacket = new BlitPacket();
        joinPacket.Append(playerId);
        client.Send(1, joinPacket.ToArray());

        //test spawn entity
            DriftureManager.CreateEntity(0, new Vector3(Random.Range(-8, 8), Random.Range(3, 8), Random.Range(-8, 8)), new byte[0]{});

        uclient = new UBlitClient();
        uclient.useCallBacks = true;

        //player pos update
        uclient.AddPacket(0, (byte[] rb)=>{BlitPacket packet = new BlitPacket(rb);

            int id = packet.GetInt32();
            if (playerId == id) return;

            float px = packet.GetSingle();
            float py = packet.GetSingle();
            float pz = packet.GetSingle();

            float rx = packet.GetSingle();
            float ry = packet.GetSingle();
            float rz = packet.GetSingle();
            float rw = packet.GetSingle();

            if (!playerCache.ContainsKey(id)) return;

            playerCache[id].transform.position = new Vector3(px, py, pz);
            playerCache[id].transform.rotation = new Quaternion(rx, ry, rz, rw);
        });

        //entity pos update
        uclient.AddPacket(7, (byte[] rb)=>{BlitPacket packet = new BlitPacket(rb);

            ulong entityId = packet.GetUInt64();

            float px = packet.GetSingle(); float py = packet.GetSingle(); float pz = packet.GetSingle();

            float rx = packet.GetSingle(); float ry = packet.GetSingle();
            float rz = packet.GetSingle(); float rw = packet.GetSingle();

            EntityManager.UpdateTransform(entityId, new Vector3(px, py, pz), new Quaternion(rx, ry, rz, rw));
        });

        uclient.Setup("localhost", 12368);
    }

    private void Update () {

        client.RunCallBacks();
        uclient.RunCallBacks();

        if (Submanager.SendCount() != 0) {

            byte[] sendData = Submanager.PopSendQueue();

            client.Send(0, sendData);
        }

        if (Input.GetKeyDown(KeyCode.Mouse0)) {

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width/2, Screen.height/2));
            if (Physics.Raycast(ray, out hit)) {

                var eh = hit.collider.GetComponent<EntityBehaviour>();
                if (eh != null) {

                    Debug.Log("atack");

                    DriftureManager.AttackEntity(eh.entityId, 64, DriftureManager.thisName);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Mouse1)) {

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width/2, Screen.height/2));
            if (Physics.Raycast(ray, out hit)) {

                var eh = hit.collider.GetComponent<EntityBehaviour>();
                if (eh != null) {

                    DriftureManager.InteractEntity(eh.entityId, DriftureManager.thisName);
                }
            }
        }
    }

    private float timer = 1.0f;
    private void LateUpdate () {

        timer -= Time.deltaTime;
        if (timer < 0.0f) timer = 0.025f; else return;

        BlitPacket packet = new BlitPacket();
        packet.Append(playerId);
        packet.Append(player.position.x);
        packet.Append(player.position.y);
        packet.Append(player.position.z);

        packet.Append(cam.rotation.x);
        packet.Append(cam.rotation.y);
        packet.Append(cam.rotation.z);
        packet.Append(cam.rotation.w);

        uclient.Send(0, packet.ToArray());
    }

    private void OnDestroy () {

        //send leave message
        BlitPacket packet = new BlitPacket();
        packet.Append(playerId);
        packet.Append(DriftureManager.thisName);
        client.Send(2, packet.ToArray());
    }
}

/*packet id legend

    1 = client join
    2 = client leave

    3 = DriftureManager.InteractEntity
    4 = DriftureManager.AttackEntity
    5 = DriftureManager.CreateEntity
    6 = DriftureManager.DeleteEntity

    7 = EntityManager.UpdateEntityPosition
    8 = EntityManager.EnsureEntityPosition

    9 = EntityManager.UpdateTransform
    10 = EntityManager.SyncEntityMetaData
    11 = EntityManager.UpdateMetaData
    12 = EntityManager.SpawnEntity
    13 = EntityManager.DespawnEntity
    14 = EntityManager.InteractEntity
    15 = EntityManager.AttackEntity

    16 = EntityManager.SpawnEntityTo
    17 = EntityManager.DespawnEntityTo
*/
