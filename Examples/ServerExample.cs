
//uses blitzbit for networking
//https://github.com/harroo/blitzbit

using UnityEngine;

using System.Threading;
using System.Collections.Generic;

using Drifture;
using BlitzBit;

public static class Program {

    [RuntimeInitializeOnLoadMethod]
    public static void Main () {

        Debug.Log("start");

        //declare servers
        UBlitServer userver = new UBlitServer();
        BlitServer server = new BlitServer();

        server.onError = (string error) => { Debug.LogError(error); };
        server.onLog = (string msg) => { Debug.Log(msg); };

        //on recv player pos update from client
        server.AddPacket(0, (int sender, byte[] rb)=>{

            Submanager.UpdatePlayerPos(rb);
        });

        // Submanager.SetRange(12);

        List<int> clients = new List<int>();

        //on client join
        server.AddPacket(1, (int sender, byte[] rb)=>{BlitPacket packet = new BlitPacket(rb);

            int id = packet.GetInt32();
            clients.Add(id);

            foreach (var client in clients) {

                if (client == id) continue;

                BlitPacket joinPacket = new BlitPacket();
                joinPacket.Append(client);

                server.RelayTo(1, sender, joinPacket.ToArray());
            }

            server.RelayExclude(1, rb, sender);

            Debug.Log("client joned: " + id.ToString());
        });
        //on client leave
        server.AddPacket(2, (int sender, byte[] rb)=>{BlitPacket packet = new BlitPacket(rb);

            int id = packet.GetInt32();
            string nameId = packet.GetString();
            clients.Remove(id);
            Submanager.ClearPlayerCache(nameId);

            server.RelayExclude(2, rb, sender);

            Debug.Log("client leaves: " + id.ToString());
        });

        //interact entity
        server.AddPacket(3, (int sender, byte[] rb)=>{ server.RelayAll(14, rb); });
        //attack entity
        server.AddPacket(4, (int sender, byte[] rb)=>{ server.RelayAll(15, rb); Debug.Log("attack entity");});
        //create entity
        server.AddPacket(5, (int sender, byte[] rb)=>{BlitPacket packet = new BlitPacket(rb);

            int type = packet.GetInt32();
            Vector3 position = new Vector3(packet.GetSingle(), packet.GetSingle(), packet.GetSingle());
            byte[] metaData = packet.GetByteArray();

            EntityManager.CreateEntity(type, position, metaData);
        });
        //delete entity
        server.AddPacket(6, (int sender, byte[] rb)=>{BlitPacket packet = new BlitPacket(rb);

            ulong entityId = packet.GetUInt64();
            EntityManager.DeleteEntity(entityId);
        });
        //ensure entity position
        server.AddPacket(8, (int sender, byte[] rb)=>{BlitPacket packet = new BlitPacket(rb);

            ulong entityId = packet.GetUInt64();

            float px = packet.GetSingle(); float py = packet.GetSingle(); float pz = packet.GetSingle();
            float rx = packet.GetSingle(); float ry = packet.GetSingle();
            float rz = packet.GetSingle(); float rw = packet.GetSingle();

            EntityManager.UpdateTransform(entityId, new Vector3(px, py, pz), new Quaternion(rx, ry, rz, rw));
        });
        //update meta data
        server.AddPacket(10, (int sender, byte[] rb)=>{BlitPacket packet = new BlitPacket(rb);

            ulong entityId = packet.GetUInt64();
            byte[] metaData = packet.GetByteArray();

            EntityManager.UpdateMetaData(entityId, metaData);

            server.RelayAll(11, rb);
        });
        EntityManager.SpawnEntity = (ulong entityId, int type, Vector3 pos, Quaternion rot, byte[] metaData) => {

            BlitPacket packet = new BlitPacket();
            packet.Append(entityId);
            packet.Append(type);
            packet.Append(pos.x); packet.Append(pos.y); packet.Append(pos.z);
            packet.Append(rot.x); packet.Append(rot.y); packet.Append(rot.z); packet.Append(rot.w);
            packet.Append(metaData);

            server.RelayAll(12, packet.ToArray());
        };
        EntityManager.SpawnEntityTo = (ulong entityId, int type, Vector3 pos, Quaternion rot, byte[] metaData, string targetNameId) => {

            BlitPacket packet = new BlitPacket();
            packet.Append(entityId);
            packet.Append(type);
            packet.Append(pos.x); packet.Append(pos.y); packet.Append(pos.z);
            packet.Append(rot.x); packet.Append(rot.y); packet.Append(rot.z); packet.Append(rot.w);
            packet.Append(metaData);
            packet.Append(targetNameId);

            server.RelayAll(16, packet.ToArray());
        };
        EntityManager.DespawnEntity = (ulong entityId) => {

            BlitPacket packet = new BlitPacket();
            packet.Append(entityId);

            server.RelayAll(13, packet.ToArray());
        };
        EntityManager.DespawnEntityTo = (ulong entityId, string targetNameId) => {

            BlitPacket packet = new BlitPacket();
            packet.Append(entityId);
            packet.Append(targetNameId);

            server.RelayAll(17, packet.ToArray());
        };

        //start servers
        server.Start(12369);
        userver.Start(12368);

        //declare loop of submanaging thing
        Thread thread = new Thread(()=>{ for (;;) {

            if (Submanager.SendCount() != 0) {

                byte[] sendData = Submanager.PopSendQueue();

                server.RelayAll(0, sendData);
            }

            Submanager.RunChecks();

            Thread.Sleep(1024);
        }});
        thread.Start();


        //coinfigure shutdown thing
        Application.quitting += () => {

            userver.Stop();
            server.Stop();
            thread.Abort();

            Debug.Log("stopped");
        };
    }
}
