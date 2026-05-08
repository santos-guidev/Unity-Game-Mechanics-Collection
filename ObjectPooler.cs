using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A generic object pooler that reuses game objects to improve performance.
/// Implemented as a Singleton for easy access.
/// </summary>
public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance { get; private set; }

    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    public List<Pool> pools;
    private Dictionary<string, Queue<GameObject>> _poolDictionary;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            _poolDictionary.Add(pool.tag, objectPool);
        }
    }

    /// <summary>
    /// Spawns an object from the pool.
    /// </summary>
    /// <param name="tag">The tag of the object to spawn.</param>
    /// <param name="position">The position to spawn the object at.</param>
    /// <param name="rotation">The rotation to spawn the object with.</param>
    /// <returns>The spawned GameObject.</returns>
    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!_poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool with tag {tag} doesn't exist.");
            return null;
        }

        GameObject objectToSpawn = _poolDictionary[tag].Dequeue();

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        IPoolable poolableObject = objectToSpawn.GetComponent<IPoolable>();
        poolableObject?.OnSpawnFromPool();

        _poolDictionary[tag].Enqueue(objectToSpawn); // Re-add to the end of the queue

        return objectToSpawn;
    }

    /// <summary>
    /// Returns an object to the pool.
    /// </summary>
    /// <param name="tag">The tag of the object to return.</param>
    /// <param name="obj">The GameObject to return.</param>
    public void ReturnToPool(string tag, GameObject obj)
    {
        if (!_poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool with tag {tag} doesn't exist.");
            Destroy(obj);
            return;
        }

        IPoolable poolableObject = obj.GetComponent<IPoolable>();
        poolableObject?.OnReturnToPool();

        obj.SetActive(false);
        // The object is already in the queue from SpawnFromPool, so no need to Enqueue again here
        // This method is more for calling OnReturnToPool and setting inactive
    }
}
