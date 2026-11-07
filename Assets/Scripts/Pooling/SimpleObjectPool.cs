using System.Collections.Generic;
using UnityEngine;

namespace FieldMission
{
    // Generic pool for reusable effects/projectiles. Add IPoolable to objects that need reset logic.
    public class SimpleObjectPool : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private int initialSize = 8;
        private readonly Queue<GameObject> available = new Queue<GameObject>();

        private void Awake()
        {
            for (int i = 0; i < initialSize; i++) CreateOne();
        }

        private GameObject CreateOne()
        {
            GameObject item = Instantiate(prefab, transform);
            item.SetActive(false);
            available.Enqueue(item);
            return item;
        }

        public GameObject Get(Vector3 position, Quaternion rotation)
        {
            if (prefab == null) return null;
            GameObject item = available.Count > 0 ? available.Dequeue() : CreateAndTake();
            item.transform.SetPositionAndRotation(position, rotation);
            item.SetActive(true);
            IPoolable[] poolables = item.GetComponents<IPoolable>();
            foreach (IPoolable poolable in poolables) poolable.OnSpawnedFromPool();
            return item;
        }

        private GameObject CreateAndTake()
        {
            GameObject item = Instantiate(prefab, transform);
            item.SetActive(false);
            return item;
        }

        public void Return(GameObject item)
        {
            if (item == null) return;
            IPoolable[] poolables = item.GetComponents<IPoolable>();
            foreach (IPoolable poolable in poolables) poolable.OnReturnedToPool();
            item.SetActive(false);
            available.Enqueue(item);
        }
    }
}
