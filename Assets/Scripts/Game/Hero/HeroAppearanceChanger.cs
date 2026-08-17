using System;
using System.Collections.Generic;
using UnityEngine;

public class HeroAppearanceChanger : MonoBehaviour
{
    [SerializeField] GameObject model;

    [Serializable]
    public class AppearanceSet
    {
        [Tooltip("이 세트를 적용할 때 켜질(활성화) 오브젝트들")]
        public List<GameObject> objectsToActivate = new List<GameObject>();
    }

    [Header("Appearance Sets")]
    [Tooltip("인스펙터에서 원하는 만큼 세트를 등록해두고, 코드/이벤트에서 이름이나 인덱스로 적용하면 됩니다.")]
    [SerializeField] private List<AppearanceSet> appearanceSets = new List<AppearanceSet>();

    private void Start()
    {
        ApplySet(0);
    }

    /// <summary>
    /// 인덱스로 등록된 세트를 찾아 활성화/비활성화를 일괄 적용합니다.
    /// </summary>
    public void ApplySet(int index)
    {
        if (appearanceSets == null || index < 0 || index >= appearanceSets.Count)
        {
            Debug.LogWarning($"[HeroAppearanceChanger] 잘못된 세트 인덱스: {index}");
            return;
        }
        ApplySet(appearanceSets[index]);
    }

    private void ApplySet(AppearanceSet set)
    {
        foreach (Transform child in model.transform)
        {
            child.gameObject.SetActive(false);
        }

        if (set.objectsToActivate != null)
        {
            foreach (GameObject go in set.objectsToActivate)
            {
                if (go != null) go.SetActive(true);
            }
        }
    }

}
