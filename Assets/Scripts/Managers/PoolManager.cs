using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Core.Utilities
{
    /// <summary>
    /// Managers a dictionary of pools, getting and returning 
    /// </summary>
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance { get; private set; }

        Dictionary<GameObject, ObjectPool<GameObject>> pools = new();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public GameObject GetObject(GameObject prefab)
        {
            if (!pools.ContainsKey(prefab))
            {
                pools[prefab] = new ObjectPool<GameObject>(
                    createFunc: () => Instantiate(prefab, transform),
                    actionOnGet: (obj) => obj.GetComponent<Poolable>()?.OnSpawn(),
                    actionOnRelease: (obj) => obj.GetComponent<Poolable>()?.OnDespawn(),
                    actionOnDestroy: (obj) => Destroy(obj),
                    defaultCapacity: 10,
                    maxSize: 50
                );
            }
            return pools[prefab].Get(); // 풀에서 가져오기
        }

        public void ReturnObject(GameObject prefab, GameObject obj)
        {
            if (pools.ContainsKey(prefab))
            {
                pools[prefab].Release(obj); // 풀로 반환
            }
            else
            {
                Destroy(obj); // 해당 오브젝트의 풀이 없으면 삭제
            }
        }
    }
}