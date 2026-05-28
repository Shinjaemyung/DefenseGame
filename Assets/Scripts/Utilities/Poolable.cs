using Core.Utilities;
using UnityEngine;

public class Poolable : MonoBehaviour
{
    protected GameObject prefabReference;

    bool isPoolReturned = false;

    public void Init(GameObject prefab)
    {
        prefabReference = prefab;
    }

    public virtual void OnSpawn()
    {
        isPoolReturned = false;
        gameObject.SetActive(true);
    }

    public virtual void OnDespawn()
    {
        StopAllCoroutines();
        gameObject.SetActive(false);
    }

    public void ReturnToPool()
    {
        if (isPoolReturned) return;

        isPoolReturned = true;
        PoolManager.Instance.ReturnObject(prefabReference, gameObject);
    }
}
