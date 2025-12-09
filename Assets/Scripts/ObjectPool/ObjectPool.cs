// Adapted from: https://github.com/llamacademy/ai-series-part-4/blob/main/Assets/Scripts/ObjectPool/ObjectPool.cs

using System.Collections.Generic;
using UnityEngine;

public class ObjectPool
{
    [Tooltip("The parent object for pooled instances.")]
    private GameObject Parent;
    [Tooltip("The prefab to use for the pool.")]
    private PoolableObject Prefab;
    [Tooltip("The size of the object pool.")]
    private int Size;
    [Tooltip("List of available objects in the pool.")]
    private List<PoolableObject> AvailableObjectsPool;

    private ObjectPool(PoolableObject Prefab, int Size)
    {
        this.Prefab = Prefab;
        this.Size = Size;
        AvailableObjectsPool = new List<PoolableObject>(Size);
    }

    public static ObjectPool CreateInstance(PoolableObject Prefab, int Size)
    {
        ObjectPool pool = new(Prefab, Size)
        {
            Parent = new GameObject(Prefab + " Pool")
        };
        pool.CreateObjects();

        return pool;
    }

    private void CreateObjects()
    {
        for (int i = 0; i < Size; i++)
        {
            CreateObject();
        }
    }

    private void CreateObject()
    {
        PoolableObject poolableObject = Object.Instantiate(Prefab, Vector3.zero, Quaternion.identity, Parent.transform);
        poolableObject.Parent = this;
        poolableObject.gameObject.SetActive(false); // PoolableObject handles re-adding the object to the AvailableObjects
    }

    public PoolableObject GetObject()
    {
        if (AvailableObjectsPool.Count == 0) // auto expand pool size if out of objects
        {
            CreateObject();
        }

        PoolableObject instance = AvailableObjectsPool[0];
        AvailableObjectsPool.RemoveAt(0);
        instance.gameObject.SetActive(true);
        return instance;
    }

    public void ReturnObjectToPool(PoolableObject Object)
    {
        AvailableObjectsPool.Add(Object);
    }
}
