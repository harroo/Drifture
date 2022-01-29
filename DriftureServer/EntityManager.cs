
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

        public static List<Entity> Entities => entities.Values.ToList();


        public static void UpdateControllingPlayer (ulong entityId, string playerNameId) { //called internally

            mutex.WaitOne(); try {

                if (!entities.ContainsKey(entityId)) return;

                controllers[entityId] = playerNameId;

                entities[entityId].controllerNameId = playerNameId;

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
    }

    public class Entity {

        public ulong entityId;
        public string controllerNameId;
        public Vector3 position;
        public Quaternion rotation;
        public byte[] metaData;
    }
}
