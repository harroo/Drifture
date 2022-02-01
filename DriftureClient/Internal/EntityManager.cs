
using UnityEngine;

using System;
using System.Collections.Generic;
using System.Threading;

namespace Drifture {

    public static class EntityManager {

        private static Mutex mutex = new Mutex();

        private static Dictionary<ulong, string> controllers
            = new Dictionary<ulong, string>();

        private static Dictionary<ulong, EntityBehaviour> entities
            = new Dictionary<ulong, EntityBehaviour>();


        public static void UpdateControl (ulong entityId, string playerNameId) { //called internally

            controllers[entityId] = playerNameId;

            if (!entities.ContainsKey(entityId)) return;

            entities[entityId].controller = playerNameId;
        }


        public static void UpdateTransform (ulong entityId, Vector3 entityPosition, Quaternion entityRotation) {

            if (!entities.ContainsKey(entityId)) return;

            entities[entityId].SetPos(entityPosition, entityRotation);
        }

        public static void UpdateMetaData (ulong entityId, byte[] metaData) {

            if (!entities.ContainsKey(entityId)) return;

            entities[entityId].OnMetaDataSet(metaData);
        }

        public static void SpawnEntity (ulong entityId, int type, Vector3 position, Quaternion rotation, byte[] metaData) {

            if (entities.ContainsKey(entityId)) return;

            EntityBehaviour behaviour = GameObject.FindObjectOfType<EntityInstancer>().CreateInstance(type, position, rotation);
            behaviour.OnMetaDataSet(metaData);
            behaviour.entityId = entityId;
            behaviour.controller = controllers.ContainsKey(entityId) ? controllers[entityId] : "";

            entities.Add(entityId, behaviour);
        }

        public static void DespawnEntity (ulong entityId) {

            if (!entities.ContainsKey(entityId)) return;

            entities[entityId].OnDespawnEvent();
            GameObject.Destroy(entities[entityId].gameObject);
            entities.Remove(entityId);
        }

        public static void InteractEntity (ulong entityId, object sender) {

            if (!entities.ContainsKey(entityId)) return;

            entities[entityId].OnInteractEvent(sender);
        }

        public static void AttackEntity (ulong entityId, int damage, object sender) {

            if (!entities.ContainsKey(entityId)) return;

            entities[entityId].OnAttackEvent(damage, sender);
        }


        public static Action <ulong, Vector3, Quaternion> UpdateEntityPosition;
        public static Action <ulong, Vector3, Quaternion> EnsureEntityPosition;
        public static Action <ulong, byte[]> SyncEntityMetaData;
    }
}
