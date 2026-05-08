using UnityEngine;

/// <summary>
/// Interface for objects that can be pooled and reused.
/// </summary>
public interface IPoolable
{
    /// <summary>
    /// Called when the object is retrieved from the pool.
    /// </summary>
    void OnSpawnFromPool();

    /// <summary>
    /// Called when the object is returned to the pool.
    /// </summary>
    void OnReturnToPool();
}
