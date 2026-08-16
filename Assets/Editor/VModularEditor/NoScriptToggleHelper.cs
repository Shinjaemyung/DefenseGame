using UnityEngine;

/// <summary>
/// 노 스크립트 모드에서 토글 기능을 제공하는 헬퍼 클래스
/// MultiBone 컴포넌트의 간단한 토글을 Unity Event로 실행할 수 있도록 함
/// </summary>
public class NoScriptToggleHelper : MonoBehaviour
{
    [Header("Target Settings")]
    public GameObject targetObject;
    
    /// <summary>
    /// 타겟 오브젝트를 토글합니다
    /// Unity Event에서 직접 호출 가능
    /// </summary>
    public void ToggleTarget()
    {
        if (targetObject != null)
        {
            try
            {
                bool currentState = targetObject.activeSelf;
                targetObject.SetActive(!currentState);
                Debug.Log($"[NoScriptToggleHelper] {targetObject.name} 토글됨: {!currentState}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[NoScriptToggleHelper] 토글 중 오류 발생: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning("[NoScriptToggleHelper] targetObject가 null입니다.");
        }
    }
    
    /// <summary>
    /// 타겟 오브젝트를 활성화합니다
    /// Unity Event에서 직접 호출 가능
    /// </summary>
    public void ActivateTarget()
    {
        if (targetObject != null)
        {
            targetObject.SetActive(true);
            Debug.Log($"[NoScriptToggleHelper] {targetObject.name} 활성화됨");
        }
    }
    
    /// <summary>
    /// 타겟 오브젝트를 비활성화합니다
    /// Unity Event에서 직접 호출 가능
    /// </summary>
    public void DeactivateTarget()
    {
        if (targetObject != null)
        {
            targetObject.SetActive(false);
            Debug.Log($"[NoScriptToggleHelper] {targetObject.name} 비활성화됨");
        }
    }
}
