
using UnityEngine;

using System;
using System.Text;
using System.Collections.Generic;
using System.Threading;

namespace Drifture {

    public static class Submanager {

        private static Mutex mutex = new Mutex();

        private static List<byte[]> outMessageQueue
            = new List<byte[]>();

        private static Dictionary<string, Vector3> playerPosCache
            = new Dictionary<string, Vector3>();

        private static Dictionary<string, Dictionary<ulong, bool>> playerViewCache
            = new Dictionary<string, Dictionary<ulong, bool>>();

        private static int drawRange = 256;

        public static void SetRange (int range) { mutex.WaitOne(); try {

            drawRange = range;

        } finally { mutex.ReleaseMutex(); } }


        public static void UpdatePlayerPos (byte[] playerData) {

            mutex.WaitOne(); try {

                float x = BitConverter.ToSingle(playerData, 0);
                float y = BitConverter.ToSingle(playerData, 4);
                float z = BitConverter.ToSingle(playerData, 8);

                byte[] nameData = new byte[playerData.Length - 12];
                Buffer.BlockCopy(playerData, 12, nameData, 0, nameData.Length);
                string playerNameId = Encoding.Unicode.GetString(nameData);

                playerPosCache[playerNameId] = new Vector3(x, y, z);

            } finally { mutex.ReleaseMutex(); }
        }

        public static void ClearPlayerCache (string playerNameId) {

            mutex.WaitOne(); try {

                playerPosCache.Remove(playerNameId);
                playerViewCache.Remove(playerNameId);

            } finally { mutex.ReleaseMutex(); }
        }

        public static void UpdateControl (ulong entityId, string playerNameId) {

            mutex.WaitOne(); try {

                EntityManager.UpdateControllingPlayer(entityId, playerNameId);

                byte[] nameData = Encoding.Unicode.GetBytes(playerNameId);
                byte[] outData = new byte[8 + nameData.Length];

                Buffer.BlockCopy(BitConverter.GetBytes(entityId), 0, outData, 0, 8);
                Buffer.BlockCopy(nameData, 0, outData, 8, nameData.Length);

                outMessageQueue.Add(outData);

            } finally { mutex.ReleaseMutex(); }
        }

        public static void RunChecks () {

            mutex.WaitOne(); try {

                //check control
                foreach (var entity in EntityManager.Entities) {

                    double closest = Mathf.Infinity;
                    string playerNameId = "";

                    foreach (var kvp in playerPosCache) {

                        double distance = (kvp.Value - entity.position).magnitude;
                        if (distance < closest) {

                            closest = distance;
                            playerNameId = kvp.Key;
                        }
                    }

                    if (playerNameId == "") continue;

                    if (playerNameId == entity.controllerNameId) continue;

                    UpdateControl(entity.entityId, playerNameId);
                }

                //check view
                foreach (var entity in EntityManager.Entities) {

                    foreach (var kvp in playerPosCache) {

                        if (!playerViewCache.ContainsKey(kvp.Key))
                            playerViewCache.Add(kvp.Key, new Dictionary<ulong, bool>());

                        if (!playerViewCache[kvp.Key].ContainsKey(entity.entityId))
                            playerViewCache[kvp.Key].Add(entity.entityId, false);

                        //if entity is in range of player
                        double distance = (kvp.Value - entity.position).magnitude;
                        if (distance < drawRange) {

                            //if the player cant see the entity
                            if (!playerViewCache[kvp.Key][entity.entityId]) {

                                Debug.Log("send creature to player");

                                playerViewCache[kvp.Key][entity.entityId] = true;
                                EntityManager.SpawnEntityTo(
                                    entity.entityId, entity.entityType,
                                    entity.position, entity.rotation,
                                    entity.metaData, kvp.Key
                                );
                            }

                        } else {

                            //if the playuer can see the entity
                            if (playerViewCache[kvp.Key][entity.entityId]) {

                                Debug.Log("send creature away from player");

                                playerViewCache[kvp.Key][entity.entityId] = false;
                                EntityManager.DespawnEntityTo(entity.entityId, kvp.Key);
                            }
                        }
                    }
                }

            } finally { mutex.ReleaseMutex(); }
        }

        public static int SendCount () {

            mutex.WaitOne(); try {

                return outMessageQueue.Count;

            } finally { mutex.ReleaseMutex(); }
        }

        public static byte[] PopSendQueue () {

            mutex.WaitOne(); try {

                byte[] outMessage = outMessageQueue[0];
                outMessageQueue.RemoveAt(0);

                return outMessage;

            } finally { mutex.ReleaseMutex(); }
        }
    }
}
