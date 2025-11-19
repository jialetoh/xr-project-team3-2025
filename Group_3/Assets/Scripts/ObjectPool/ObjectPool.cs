// Adapted from: https://github.com/llamacademy/ai-series-part-4/blob/main/Assets/Scripts/ObjectPool/ObjectPool.cs

using System.Collections.Generic;
using UnityEngine;

public class ObjectPool
{
    [Tooltip("The prefab to use for the pool.")]
    private PoolableObject prefab;
    [Tooltip("The size of the object pool.")]
    private int size;
    [Tooltip("List of available objects in the pool.")]
    private List<PoolableObject> availableObjectsPool;

    private ObjectPool(PoolableObject prefab, int size)
    {
        this.prefab = prefab;
        this.size = size;
        availableObjectsPool = new List<PoolableObject>(size);
    }

    public static ObjectPool CreateInstance(PoolableObject prefab, int size)
    {
        ObjectPool pool = new ObjectPool(prefab, size);

        GameObject poolGameObject = new GameObject(prefab + " Pool");
        pool.CreateObjects(poolGameObject);

        return pool;
    }

    private void CreateObjects(GameObject parent)
    {
        for (int i = 0; i < size; i++)
        {
            PoolableObject poolableObject = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity, parent.transform);
            poolableObject.Parent = this;
            poolableObject.gameObject.SetActive(false); // PoolableObject handles re-adding the object to the AvailableObjects
        }
    }

    public PoolableObject GetObject()
    {
        PoolableObject instance = availableObjectsPool[0];
        availableObjectsPool.RemoveAt(0);
        instance.gameObject.SetActive(true);
        return instance;
    }

    public void ReturnObjectToPool(PoolableObject Object)
    {
        availableObjectsPool.Add(Object);
    }
}
