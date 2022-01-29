
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

        public static void UpdatePlayerPos (byte[] playerData) {

            mutex.WaitOne(); try {

                float x = BitConverter.ToInt32(playerData, 0);
                float y = BitConverter.ToInt32(playerData, 4);
                float z = BitConverter.ToInt32(playerData, 8);

                byte[] nameData = new byte[playerData.Length - 12];
                Buffer.BlockCopy(playerData, 12, nameData, 0, nameData.Length);
                string playerNameId = Encoding.Unicode.GetString(nameData);

                playerPosCache[playerNameId] = new Vector3(x, y, z);

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
