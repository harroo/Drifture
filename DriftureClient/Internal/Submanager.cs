
using UnityEngine;

using System;
using System.Text;
using System.Collections.Generic;
using System.Threading;

namespace Drifture {

    public static class Submanager {

        private static Mutex mutex = new Mutex();

        private static List<byte[]> inMessageQueue
            = new List<byte[]>();

        private static List<byte[]> outMessageQueue
            = new List<byte[]>();

        public static void SyncPlayerPosToServer (Vector3 position) {

            mutex.WaitOne(); try {

                byte[] nameData = Encoding.Unicode.GetBytes(DriftureManager.thisName);

                byte[] outData = new byte[12 + nameData.Length];

                Buffer.BlockCopy(BitConverter.GetBytes(position.x), 0, outData, 0, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(position.y), 0, outData, 4, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(position.z), 0, outData, 8, 4);

                Buffer.BlockCopy(nameData, 0, outData, 12, nameData.Length);

                outMessageQueue.Add(outData);

            } finally { mutex.ReleaseMutex(); }
        }

        public static void UpdateControl (byte[] updateData) {

            mutex.WaitOne(); try {

                ulong entityId = BitConverter.ToUInt64(updateData, 0);

                byte[] nameData = new byte[updateData.Length - 8];
                Buffer.BlockCopy(updateData, 8, nameData, 0, nameData.Length);
                string playerNameId = Encoding.Unicode.GetString(nameData);

                EntityManager.UpdateControl(entityId, playerNameId);

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
