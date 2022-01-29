
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

        //configure drifture client name to a random name
        playerId = Random.Range(111111, 999999);
        DriftureManager.thisName = "player_" + playerId.ToString();

        //configure clients
        client = new BlitClient();
        client.useCallBacks = true;

        //on control update
        client.AddPacket(0, (byte[] rb)=>{

            Submanager.UpdateControl(rb);
        });

        //on client join
        client.AddPacket(1, (byte[] rb)=>{BlitPacket packet = new BlitPacket(rb);

            int id = packet.GetInt32(); if (playerId == id) return;

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

        uclient.Setup("localhost", 12368);
    }

    private void Update () {

        client.RunCallBacks();
        uclient.RunCallBacks();

        if (Submanager.SendCount() != 0) {

            byte[] sendData = Submanager.PopSendQueue();

            client.Send(0, sendData);
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
        client.Send(2, packet.ToArray());
    }
}
