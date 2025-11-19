// Adapted from: https://github.com/llamacademy/ai-series-part-4/blob/main/Assets/Scripts/ObjectPool/PoolableObject.cs

using UnityEngine;

public class PoolableObject : MonoBehaviour
{
    [Tooltip("The Object Pool that this object belongs to.")]
    public ObjectPool Parent;

    public virtual void OnDisable()
    {
        Parent.ReturnObjectToPool(this);
    }
}