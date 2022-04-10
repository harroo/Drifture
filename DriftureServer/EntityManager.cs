
using UnityEngine;

using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace Drifture {

    public static class EntityManager {

        private static Mutex mutex = new Mutex();

        private static Dictionary<ulong, string> controllers
            = new Dictionary<ulong, string>();

        private static Dictionary<ulong, Entity> entities
            = new Dictionary<ulong, Entity>();

        public static List<Entity> Entities { get { mutex.WaitOne(); try {

            return entities.Values.ToList();

        } finally { mutex.ReleaseMutex(); } } }

        public static Dictionary<ulong, Entity> GetEntityData () { mutex.WaitOne(); try {

            return entities;

        } finally { mutex.ReleaseMutex(); } }
        public static void SetEntityData (Dictionary<ulong, Entity> data) { mutex.WaitOne(); try {

            entities = data;

            ulong highestId = 0;
            foreach (var id in data.Keys) if (id > highestId) highestId = id;

            IdNext = highestId + 1;

        } finally { mutex.ReleaseMutex(); } }

        private static ulong IdNext = 0;


        public static void UpdateControllingPlayer (ulong entityId, string playerNameId) { //called internally

            mutex.WaitOne(); try {

                if (!entities.ContainsKey(entityId)) return;

                controllers[entityId] = playerNameId;

                entities[entityId].controllerNameId = playerNameId;

            } finally { mutex.ReleaseMutex(); }
        }


        public static void ClearPlayerCache (string playerNameId) {

            mutex.WaitOne(); try {

                foreach (ulong entityId in entities.Keys) {

                    if (controllers[entityId] == playerNameId)
                        UpdateControllingPlayer(entityId, "");
                }

            } finally { mutex.ReleaseMutex(); }
        }


        public static void CreateEntity (int type, Vector3 pos, byte[] mData) {

            mutex.WaitOne(); try {

                Entity entity = new Entity {

                    entityId = IdNext++,
                    entityType = type,
                    position = pos,
                    metaData = mData
                };

                entities.Add(entity.entityId, entity);

            } finally { mutex.ReleaseMutex(); }
        }

        public static void DeleteEntity (ulong entityId) {

            mutex.WaitOne(); try {

                if (!entities.ContainsKey(entityId)) return;

                entities.Remove(entityId);

                DespawnEntity(entityId);

            } finally { mutex.ReleaseMutex(); }
        }


        public static void UpdateTransform (ulong entityId, Vector3 entityPosition, Quaternion entityRotation) {

            mutex.WaitOne(); try {

                if (!entities.ContainsKey(entityId)) return;

                entities[entityId].position = entityPosition;
                entities[entityId].rotation = entityRotation;

            } finally { mutex.ReleaseMutex(); }
        }

        public static void UpdateMetaData (ulong entityId, byte[] metaData) {

            mutex.WaitOne(); try {

                if (!entities.ContainsKey(entityId)) return;

                entities[entityId].metaData = metaData;

            } finally { mutex.ReleaseMutex(); }
        }

        public static Action <ulong, int, Vector3, Quaternion, byte[]> SpawnEntity;
        public static Action <ulong, int, Vector3, Quaternion, byte[], string> SpawnEntityTo;
        public static Action <ulong> DespawnEntity;
        public static Action <ulong, string> DespawnEntityTo;
    }

    [Serializable]
    public class Entity {

        public ulong entityId;
        public int entityType;
        public string controllerNameId;
        private float px, py, pz; public Vector3 position {
            get { return new Vector3(px, py, pz); }
            set { px = value.x; py = value.y; pz = value.pz; }
        }
        private float rx, ry, rz, rw; public Quaternion rotation {
            get { return new Quaternion(rx, ry, rz, rw); }
            set { rx = value.x; ry = value.y; rz = value.z; rw = value.w; }
        }
        public byte[] metaData;
    }
}
