
using UnityEngine;

using System;
using System.Collections.Generic;
using System.Threading;

namespace Drifture {

    public static class EntityManager {

        private static Mutex mutex = new Mutex();

        private static Dictionary<ulong, string> controllers
            = new Dictionary<ulong, string>();

        private static Dictionary<ulong, Entity> entities
            = new Dictionary<ulong, Entity>();


        public static void UpdateControl (ulong entityId, string playerNameId) { //called internally

            if (!entities.ContainsKey(entityId)) return;

            controllers[entityId] = playerNameId;

            entities[entityId].controllingNameId = playerNameId;
        }


        public static void UpdateTransform (ulong entityId, Vector3 entityPosition, Quaternion entityRotation) {

            if (!entities.ContainsKey(entityId)) return;

            entities[entityId].position = entityPosition;
            entities[entityId].rotation = entityRotation;
        }

        public static void UpdateMetaData (ulong entityId, byte[] metaData) {

            if (!entities.ContainsKey(entityId)) return;

            entities[entityId].metaData = metaData;
        }
    }

    public class Entity {

        public ulong entityId;
        public string controllingNameId;
        public Vector3 position;
        public Quaternion rotation;
        public byte[] metaData;
    }
}
