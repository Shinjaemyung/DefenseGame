using UnityEngine;

/// <summary>
/// 노 스크립트 모드에서 SingleBone의 exclusive toggle 기능을 제공하는 헬퍼 클래스
/// 같은 그룹 내에서 하나만 활성화되도록 관리
/// </summary>
public class NoScriptSingleBoneHelper : MonoBehaviour
{
    [Header("Target Settings")]
    public GameObject targetObject;
    
    [Header("Same Group Objects (to deactivate)")]
    public GameObject[] sameGroupObjects;
    
    /// <summary>
    /// 타겟 오브젝트를 활성화하고 같은 그룹의 다른 오브젝트들을 비활성화합니다
    /// Unity Event에서 직접 호출 가능
    /// </summary>
    public void ActivateExclusive()
    {
        if (targetObject != null)
        {
            try
            {
                // 같은 그룹의 다른 오브젝트들 비활성화
                if (sameGroupObjects != null)
                {
                    foreach (var obj in sameGroupObjects)
                    {
                        if (obj != null)
                        {
                            obj.SetActive(false);
                            Debug.Log($"[NoScriptSingleBoneHelper] {obj.name} 비활성화됨");
                        }
                    }
                }
                
                // 타겟 오브젝트 활성화
                targetObject.SetActive(true);
                Debug.Log($"[NoScriptSingleBoneHelper] {targetObject.name} 활성화됨 (exclusive)");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[NoScriptSingleBoneHelper] ActivateExclusive 중 오류 발생: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning("[NoScriptSingleBoneHelper] targetObject가 null입니다.");
        }
    }
    
    /// <summary>
    /// 타겟 오브젝트만 비활성화합니다
    /// </summary>
    public void DeactivateTarget()
    {
        if (targetObject != null)
        {
            targetObject.SetActive(false);
            Debug.Log($"[NoScriptSingleBoneHelper] {targetObject.name} 비활성화됨");
        }
    }
}
