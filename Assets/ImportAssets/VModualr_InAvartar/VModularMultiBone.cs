using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;

// 블렌드 쉐이프 설정을 위한 클래스
[System.Serializable]
public class OutfitBlendShapeSetting
{
    public string shapeKeyName; // 블렌드 쉐이프 키 이름
    [Range(0f, 100f)]
    public float weight; // 블렌드 쉐이프 가중치
}

// V-모듈러 멀티본 시스템 (바디 블렌드쉐이프 설정 기능 포함)
public class VModularMultiBone : MonoBehaviour
{
    [Header("그룹명 (중요: 고유하게 설정)")]
    [Tooltip("다른 의상과 겹치지 않는 고유한 그룹명을 설정하세요")]
    [SerializeField] private string outfitGroupName = "Outfit"; // 의상 그룹명
    
    [Header("바디 블렌드쉐이프 설정")]
    [Tooltip("바디 블렌드쉐이프 덮어씌우기 기능을 사용할지 여부")]
    public bool useBlendShapeOverride = false; // 블렌드쉐이프 덮어씌우기 사용 여부
    
    [Header("스케일 설정")]
    [Tooltip("ON: 부모 본 기준으로 스케일 자동 계산 | OFF: 원본 스케일 유지")]
    public bool useParentBasedScaling = false; // 부모 본 기준 스케일 계산 사용 여부
    
    [Tooltip("바디 스킨드 메쉬 렌더러 (블렌드쉐이프 설정용) - 위 토글이 켜져있을 때만 사용")]
    public SkinnedMeshRenderer targetBodyRenderer; // 바디 렌더러
    
    [Tooltip("이 의상이 착용될 때 바디에 적용될 블렌드쉐이프들 (위 토글이 켜져있을 때만 사용)")]
    public List<OutfitBlendShapeSetting> outfitBlendShapes = new List<OutfitBlendShapeSetting>(); // 의상 전용 블렌드쉐이프
    
    [Header("Objects to Toggle")]
    [Tooltip("이 의상이 착용될 때 비활성화할 오브젝트들 (직접 지정)")]
    public List<GameObject> objectsToHideWhenWearing = new List<GameObject>(); // 착용시 숨길 오브젝트들
    
    // 내부 변수들
    private bool isWearing = false; // 착용 상태
    private List<Transform> outfitParts = new List<Transform>(); // 의상 파트들
    private bool hasInitializedScale = false; // 스케일 초기 동기화 완료 여부
    private Animator targetAnimator; // 자동으로 찾은 애니메이터
    private Transform outfitArmature; // 자동으로 찾은 아마튜어
    
    // 바디 블렌드쉐이프 관련 캐시
    private Dictionary<string, int> bodyBlendShapeIndices = new Dictionary<string, int>(); // 바디 블렌드쉐이프 인덱스
    private Dictionary<string, float> originalBodyBlendShapeWeights = new Dictionary<string, float>(); // 원본 바디 블렌드쉐이프 값 (복원용)

    void Awake()
    {
        Initialize();
        
        // 시작 시 자동 착용
        if (outfitArmature != null && outfitParts.Count > 0)
        {
            StartCoroutine(DelayedWearCoroutine("시작"));
        }
    }
    
    void Reset()
    {
        // 컴포넌트가 처음 추가될 때 게임오브젝트 이름을 그룹명으로 설정
        if (string.IsNullOrEmpty(outfitGroupName) || outfitGroupName == "Outfit")
        {
            outfitGroupName = this.gameObject.name;
            Debug.Log($"[VModularMultiBone] 그룹명이 자동으로 설정됨: {outfitGroupName}");
        }
    }
    
    void OnValidate()
    {
        // 에디터에서만 실행 (빌드된 게임에서는 실행하지 않음)
        #if UNITY_EDITOR
        // 그룹명이 비어있거나 기본값일 때만 게임오브젝트 이름으로 설정
        if (string.IsNullOrEmpty(outfitGroupName) || outfitGroupName == "Outfit")
        {
            outfitGroupName = this.gameObject.name;
        }
        #endif
    }
    
    void OnEnable()
    
    {

        Initialize();
        
        // 시작 시 자동 착용
        if (outfitArmature != null && outfitParts.Count > 0)
        {
            StartCoroutine(DelayedWearCoroutine("시작"));
        }

        // 오브젝트가 활성화될 때 (Start 이후에만)
        if (outfitArmature != null && outfitParts.Count > 0 && Time.time > 0.1f)
        {
            if (isWearing)
            {
                // 이미 착용된 상태라면 블렌드쉐이프와 오브젝트 토글만 재적용
                Debug.Log($"[VModularMultiBone] 활성화로 인한 재적용 (이미 착용됨): {outfitGroupName}");
                SaveOriginalBlendShapeWeights(); // 재적용 직전 상태 저장 (다른 의상이 변경했을 수 있음)
                ApplyOutfitBlendShapes();
                ToggleObjects(false);
            }
            else
            {
                // 착용되지 않은 상태라면 전체 착용
                StartCoroutine(DelayedWearCoroutine("활성화"));
            }
        }
    }
    
    void OnDisable()
    {
        // 오브젝트가 비활성화될 때 블렌드쉐이프와 오브젝트만 원상복구 (벗기기 없음)
        if (isWearing)
        {
            Debug.Log($"[VModularMultiBone] 비활성화로 인한 원상복구 (벗기기 없음): {outfitGroupName}");
            RestoreOutfitEffects();
        }
    }
    
    /// <summary>
    /// 의상 효과만 원상복구 (이 의상이 건드린 블렌드쉐이프만 복원, 의상 파트는 본에 그대로 유지)
    /// </summary>
    void RestoreOutfitEffects()
    {
        // 이 의상이 건드린 블렌드쉐이프들만 원래 값으로 복원
        RestoreOriginalBlendShapeWeights();
        
        // 이 의상이 숨긴 오브젝트들 다시 보이기
        ToggleObjects(true);
    }
    
    IEnumerator DelayedWearCoroutine(string reason)
    {
        yield return null; // 1프레임 대기
        
        if (this.enabled && this.gameObject.activeInHierarchy)
        {
            Debug.Log($"[VModularMultiBone] {reason}으로 인한 의상 착용: {outfitGroupName}");
            WearOutfit();
        }
    }
    
    void Initialize()
    {
        Debug.Log($"[VModularMultiBone] {name} 초기화 시작...");
        
        // 애니메이터를 부모 중에서 자동으로 찾기
        targetAnimator = FindAnimatorInParents(this.transform);
        if (targetAnimator == null)
        {
            Debug.LogError($"[VModularMultiBone] 부모 중에서 Animator를 찾을 수 없습니다! 이 오브젝트가 아바타 하위에 있는지 확인해주세요.");
            return;
        }
        
        // 아마튜어를 애니메이터에서 자동으로 찾기
        outfitArmature = FindArmatureFromAnimator(targetAnimator);
        if (outfitArmature == null)
        {
            Debug.LogError($"[VModularMultiBone] 애니메이터에서 아마튜어를 찾을 수 없습니다!");
            return;
        }
        
        Debug.Log($"[VModularMultiBone] 타겟 애니메이터: {targetAnimator.name}");
        Debug.Log($"[VModularMultiBone] 타겟 아마튜어: {outfitArmature.name}");
        
        // 바디 렌더러 확인 및 블렌드쉐이프 캐시 (토글이 켜져있을 때만)
        if (useBlendShapeOverride)
        {
            if (targetBodyRenderer != null)
            {
                CacheBodyBlendShapeIndices();
                Debug.Log($"[VModularMultiBone] 바디 렌더러: {targetBodyRenderer.name} (블렌드쉐이프 {bodyBlendShapeIndices.Count}개)");
            }
            else
            {
                Debug.LogWarning($"[VModularMultiBone] 블렌드쉐이프 덮어씌우기가 활성화되어 있지만 Target Body Renderer가 설정되지 않았습니다.");
            }
        }
        else
        {
            Debug.Log($"[VModularMultiBone] 블렌드쉐이프 덮어씌우기 기능이 비활성화되어 있습니다.");
        }
        
        // 의상 파트들 수집
        CollectOutfitParts();
        Debug.Log($"[VModularMultiBone] 의상 파트 {outfitParts.Count}개 수집됨");
        
        Debug.Log($"[VModularMultiBone] 토글 오브젝트 {objectsToHideWhenWearing.Count}개 설정됨");
        Debug.Log($"[VModularMultiBone] 의상 전용 블렌드쉐이프 {outfitBlendShapes.Count}개 설정됨");
        
        // 모든 자식 렌더러들의 Update When Offscreen 옵션 활성화
        EnableUpdateWhenOffscreen();
        
        Debug.Log($"[VModularMultiBone] {name} 초기화 완료!");
    }
    
    /// <summary>
    /// 모든 자식 렌더러들의 Update When Offscreen 옵션 활성화
    /// </summary>
    void EnableUpdateWhenOffscreen()
    {
        // 이 오브젝트와 모든 자식에서 SkinnedMeshRenderer 찾기
        SkinnedMeshRenderer[] skinnedRenderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);
        
        int enabledCount = 0;
        foreach (var renderer in skinnedRenderers)
        {
            if (renderer != null && !renderer.updateWhenOffscreen)
            {
                renderer.updateWhenOffscreen = true;
                enabledCount++;
                Debug.Log($"[VModularMultiBone] Update When Offscreen 활성화: {renderer.name}");
            }
        }
        
        if (enabledCount > 0)
        {
            Debug.Log($"[VModularMultiBone] Update When Offscreen 옵션을 {enabledCount}개 렌더러에 적용함");
        }
        else
        {
            Debug.Log($"[VModularMultiBone] SkinnedMeshRenderer가 없거나 이미 모두 활성화되어 있음");
        }
    }
    
    /// <summary>
    /// 한 단계 부모에서 애니메이터 컴포넌트 찾기
    /// </summary>
    Animator FindAnimatorInParents(Transform current)
    {
        if (current.parent != null)
        {
            Animator animator = current.parent.GetComponent<Animator>();
            if (animator != null)
            {
                Debug.Log($"[VModularMultiBone] 애니메이터 발견: {current.parent.name}");
                return animator;
            }
        }
        return null;
    }
    
    /// <summary>
    /// 애니메이터에서 아마튜어 찾기 (일반적으로 "Armature" 또는 Hips 본의 부모)
    /// </summary>
    Transform FindArmatureFromAnimator(Animator animator)
    {
        // 1. Hips 본을 통해 아마튜어 찾기 (일반적인 방법)
        Transform hips = animator.GetBoneTransform(HumanBodyBones.Hips);
        if (hips != null && hips.parent != null)
        {
            Debug.Log($"[VModularMultiBone] Hips를 통해 아마튜어 발견: {hips.parent.name}");
            return hips.parent;
        }
        
        // 2. "Armature" 이름으로 검색
        Transform armature = FindChildByName(animator.transform, "Armature");
        if (armature != null)
        {
            Debug.Log($"[VModularMultiBone] 이름을 통해 아마튜어 발견: {armature.name}");
            return armature;
        }
        
        // 3. 애니메이터 자신이 아마튜어인 경우
        Debug.Log($"[VModularMultiBone] 애니메이터 자체를 아마튜어로 사용: {animator.name}");
        return animator.transform;
    }
    
    /// <summary>
    /// 이름으로 자식 찾기 (재귀)
    /// </summary>
    Transform FindChildByName(Transform parent, string name)
    {
        if (parent.name == name)
            return parent;
        
        foreach (Transform child in parent)
        {
            Transform found = FindChildByName(child, name);
            if (found != null)
                return found;
        }
        
        return null;
    }
    
    /// <summary>
    /// V-avatarTool의 CollectAllOutfitObjects와 동일한 기능
    /// 의상 오브젝트의 모든 하위 Transform을 재귀적으로 수집
    /// </summary>
    void CollectOutfitParts()
    {
        outfitParts.Clear();
        CollectAllOutfitObjects(this.transform);
    }
    
    /// <summary>
    /// V-avatarTool과 동일한 재귀적 수집 로직
    /// </summary>
    void CollectAllOutfitObjects(Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (child != this.transform) // 자기 자신은 제외
            {
                outfitParts.Add(child);
                CollectAllOutfitObjects(child);
            }
        }
    }
    
    /// <summary>
    /// 오브젝트들을 직접 토글 (이름 검색 없이)
    /// </summary>
    void ToggleObjects(bool show)
    {
        // 에디터에서는 오브젝트 토글 안함 (런타임에서만 작동)
        if (!Application.isPlaying)
        {
            Debug.Log($"[VModularMultiBone] 에디터 모드에서는 오브젝트 토글 건너뜀: {outfitGroupName}");
            return;
        }
        
        foreach (GameObject obj in objectsToHideWhenWearing)
        {
            if (obj != null)
            {
                if (obj.activeSelf != show)
                {
                    obj.SetActive(show);
                    Debug.Log($"[VModularMultiBone] 오브젝트 토글: {obj.name} -> {(show ? "보이기" : "숨기기")}");
                }
            }
        }
    }
    
    /// <summary>
    /// 바디 블렌드쉐이프 인덱스 캐시
    /// </summary>
    void CacheBodyBlendShapeIndices()
    {
        bodyBlendShapeIndices.Clear();
        if (targetBodyRenderer == null || targetBodyRenderer.sharedMesh == null) return;
        
        for (int i = 0; i < targetBodyRenderer.sharedMesh.blendShapeCount; i++)
        {
            string shapeName = targetBodyRenderer.sharedMesh.GetBlendShapeName(i);
            bodyBlendShapeIndices[shapeName] = i;
        }
        
        Debug.Log($"[VModularMultiBone] 바디 블렌드쉐이프 {bodyBlendShapeIndices.Count}개 캐시됨");
    }
    
    /// <summary>
    /// 이 의상이 건드릴 블렌드쉐이프들의 현재 값만 저장 (복원용)
    /// </summary>
    void SaveOriginalBlendShapeWeights()
    {
        // 블렌드쉐이프 덮어씌우기가 비활성화되어 있으면 실행하지 않음
        if (!useBlendShapeOverride)
        {
            return;
        }
        
        // 에디터에서는 블랜드셰이프 저장 안함 (런타임에서만 작동)
        if (!Application.isPlaying)
        {
            Debug.Log($"[VModularMultiBone] 에디터 모드에서는 블렌드쉐이프 저장 건너뜀: {outfitGroupName}");
            return;
        }
        
        originalBodyBlendShapeWeights.Clear();
        
        if (targetBodyRenderer != null && targetBodyRenderer.sharedMesh != null && outfitBlendShapes.Count > 0)
        {
            Debug.Log($"[VModularMultiBone] 의상이 사용할 블렌드쉐이프 {outfitBlendShapes.Count}개의 현재 상태 저장 중...");
            
            foreach (var setting in outfitBlendShapes)
            {
                if (bodyBlendShapeIndices.TryGetValue(setting.shapeKeyName, out int index))
                {
                    float currentWeight = targetBodyRenderer.GetBlendShapeWeight(index);
                    originalBodyBlendShapeWeights[setting.shapeKeyName] = currentWeight;
                    
                    Debug.Log($"[VModularMultiBone] 저장: {setting.shapeKeyName} = {currentWeight} (인덱스 {index})");
                }
                else
                {
                    Debug.LogWarning($"[VModularMultiBone] 블렌드쉐이프를 찾을 수 없어 저장 불가: {setting.shapeKeyName}");
                }
            }
            
            Debug.Log($"[VModularMultiBone] 의상 전용 블렌드쉐이프 {originalBodyBlendShapeWeights.Count}개 현재 상태 저장 완료");
        }
        else if (outfitBlendShapes.Count == 0)
        {
            Debug.Log($"[VModularMultiBone] 의상 전용 블렌드쉐이프가 없어 저장 생략");
        }
        else
        {
            Debug.LogWarning($"[VModularMultiBone] 바디 렌더러가 없어 블렌드쉐이프 저장 불가");
        }
    }
    
    /// <summary>
    /// 의상 전용 블렌드쉐이프를 바디에 적용
    /// </summary>
    void ApplyOutfitBlendShapes()
    {
        // 블렌드쉐이프 덮어씌우기가 비활성화되어 있으면 실행하지 않음
        if (!useBlendShapeOverride)
        {
            return;
        }
        
        // 에디터에서는 블랜드셰이프 변경 안함 (런타임에서만 작동)
        if (!Application.isPlaying)
        {
            Debug.Log($"[VModularMultiBone] 에디터 모드에서는 블렌드쉐이프 적용 건너뜀: {outfitGroupName}");
            return;
        }
        
        if (targetBodyRenderer == null || outfitBlendShapes.Count == 0) return;
        
        Debug.Log($"[VModularMultiBone] 의상 전용 블렌드쉐이프 {outfitBlendShapes.Count}개 적용 중...");
        
        foreach (var setting in outfitBlendShapes)
        {
            if (bodyBlendShapeIndices.TryGetValue(setting.shapeKeyName, out int index))
            {
                targetBodyRenderer.SetBlendShapeWeight(index, setting.weight);
                Debug.Log($"[VModularMultiBone] 바디 블렌드쉐이프 적용: {setting.shapeKeyName} = {setting.weight}");
            }
            else
            {
                Debug.LogWarning($"[VModularMultiBone] 바디에서 블렌드쉐이프를 찾을 수 없음: {setting.shapeKeyName}");
            }
        }
    }
    
    /// <summary>
    /// 이 의상이 건드린 블렌드쉐이프들만 원래 상태로 복원
    /// </summary>
    void RestoreOriginalBlendShapeWeights()
    {
        // 블렌드쉐이프 덮어씌우기가 비활성화되어 있으면 실행하지 않음
        if (!useBlendShapeOverride)
        {
            return;
        }
        
        // 에디터에서는 블랜드셰이프 복원 안함 (런타임에서만 작동)
        if (!Application.isPlaying)
        {
            Debug.Log($"[VModularMultiBone] 에디터 모드에서는 블렌드쉐이프 복원 건너뜀: {outfitGroupName}");
            return;
        }
        
        if (targetBodyRenderer == null || originalBodyBlendShapeWeights.Count == 0) 
        {
            if (originalBodyBlendShapeWeights.Count == 0)
            {
                Debug.Log($"[VModularMultiBone] 복원할 블렌드쉐이프가 없음 (이 의상이 블렌드쉐이프를 건드리지 않음)");
            }
            return;
        }
        
        Debug.Log($"[VModularMultiBone] 이 의상이 건드린 블렌드쉐이프 {originalBodyBlendShapeWeights.Count}개 복원 중...");
        
        int restoredCount = 0;
        foreach (var kvp in originalBodyBlendShapeWeights)
        {
            string shapeName = kvp.Key;
            float originalWeight = kvp.Value;
            
            if (bodyBlendShapeIndices.TryGetValue(shapeName, out int index))
            {
                float currentWeight = targetBodyRenderer.GetBlendShapeWeight(index);
                targetBodyRenderer.SetBlendShapeWeight(index, originalWeight);
                restoredCount++;
                
                Debug.Log($"[VModularMultiBone] 복원: {shapeName} {currentWeight} → {originalWeight} (인덱스 {index})");
            }
            else
            {
                Debug.LogWarning($"[VModularMultiBone] 복원 실패 - 블렌드쉐이프를 찾을 수 없음: {shapeName}");
            }
        }
        
        Debug.Log($"[VModularMultiBone] 의상 전용 블렌드쉐이프 {restoredCount}개 복원 완료");
    }
    
    // 의상 착용 (매번 호출 시 현재 블렌드쉐이프 상태 저장)
    [ContextMenu("Wear Outfit")]
    public void WearOutfit()
    {
        if (isWearing) 
        {
            Debug.Log($"[VModularMultiBone] 이미 착용 중인 의상: {outfitGroupName} - 착용 과정 생략");
            return;
        }
        
        Debug.Log($"[VModularMultiBone] 의상 착용 시작: {outfitGroupName}");
        
        if (outfitParts.Count == 0)
        {
            Debug.LogWarning($"[VModularMultiBone] 의상 파트가 없습니다! 자식 오브젝트가 있는지 확인해주세요.");
            return;
        }
        
        if (outfitArmature == null)
        {
            Debug.LogError($"[VModularMultiBone] Outfit Armature가 설정되지 않았습니다!");
            return;
        }
        
        // 의상 적용 직전의 블렌드쉐이프 상태 저장 (이 의상이 건드릴 블렌드쉐이프만)
        SaveOriginalBlendShapeWeights();
        
        int movedCount = 0;
        
        // 개선된 방식: 의상 파트들을 아바타 본에 직접 이동
        foreach (Transform outfitPart in outfitParts)
        {
            // 동일한 이름의 매칭 본 찾기
            Transform matchingBone = FindMatchingChildByName(outfitArmature, outfitPart.name);
            
            if (matchingBone != null)
            {
                Debug.Log($"[VModularMultiBone] 매칭 성공: '{outfitPart.name}' -> '{matchingBone.name}' (경로: {GetTransformPath(matchingBone)})");
                
                // 월드 좌표계에서의 위치, 회전, 스케일 보존
                Vector3 worldPos = outfitPart.position;
                Quaternion worldRot = outfitPart.rotation;
                Vector3 worldScale = outfitPart.lossyScale;
                
                Debug.Log($"[VModularMultiBone] 부착 전 상태 - 위치: {worldPos}, 회전: {worldRot.eulerAngles}, 스케일: {worldScale}");
                Debug.Log($"[VModularMultiBone] 이동 전 부모: {(outfitPart.parent != null ? outfitPart.parent.name : "없음")}");
                
                // 매칭 본 아래로 이동 (false로 설정하여 로컬 변환값 유지)
                outfitPart.SetParent(matchingBone, false);
                
                // 월드 좌표계 값으로 복원
                outfitPart.position = worldPos;
                outfitPart.rotation = worldRot;
                
                // 스케일 처리 (토글에 따라)
                if (useParentBasedScaling)
                {
                    // 부모의 스케일을 고려하여 자동 계산
                    if (outfitPart.parent != null)
                    {
                        Vector3 parentScale = outfitPart.parent.lossyScale;
                        Vector3 requiredLocalScale = new Vector3(
                            parentScale.x != 0 ? worldScale.x / parentScale.x : 1f,
                            parentScale.y != 0 ? worldScale.y / parentScale.y : 1f,
                            parentScale.z != 0 ? worldScale.z / parentScale.z : 1f
                        );
                        outfitPart.localScale = requiredLocalScale;
                        Debug.Log($"[VModularMultiBone] 부모 기준 스케일 자동 계산: {requiredLocalScale}");
                    }
                }
                else
                {
                    // 원본 월드 스케일을 그대로 유지하려고 시도 (localScale 조정 없음)
                    Debug.Log($"[VModularMultiBone] 원본 스케일 유지 모드: {outfitPart.localScale}");
                }
                
                // 접미사 추가
                AddOutfitSuffix(outfitPart);
                
                Debug.Log($"[VModularMultiBone] 이동 후 부모: {(outfitPart.parent != null ? outfitPart.parent.name : "없음")}");
                Debug.Log($"[VModularMultiBone] 부착 후 상태 - 위치: {outfitPart.position}, 회전: {outfitPart.rotation.eulerAngles}, 스케일: {outfitPart.lossyScale}");
                
                movedCount++;
                Debug.Log($"[VModularMultiBone] ✅ {outfitPart.name}을 {matchingBone.name}에 성공적으로 부착함");
            }
            else
            {
                Debug.LogWarning($"[VModularMultiBone] ✗ 매칭되는 본을 찾을 수 없음: {outfitPart.name}");
            }
        }
        
        Debug.Log($"[VModularMultiBone] {movedCount}/{outfitParts.Count}개 파트가 성공적으로 부착됨");
        
        // 스케일 초기화 완료 표시
        hasInitializedScale = true;
        
        // 의상 전용 블렌드쉐이프 적용
        ApplyOutfitBlendShapes();
        
        // 오브젝트들 숨기기
        ToggleObjects(false);
        
        isWearing = true;
    }
    
    // 의상 효과 제거 (이 의상이 건드린 블렌드쉐이프만 복원, 본에서 벗기기는 제외 - 작동 불안정)
    [ContextMenu("Remove Outfit")]
    public void RemoveOutfit()
    {
        if (!isWearing) return;
        
        Debug.Log($"[VModularMultiBone] 의상 효과 제거 (본에서 벗기기 제외): {outfitGroupName}");
        
        // 이 의상이 건드린 블렌드쉐이프와 오브젝트 토글만 원상복구
        RestoreOutfitEffects();
        
        // 스케일 초기화 플래그 리셋
        hasInitializedScale = false;
        
        isWearing = false;
        
        Debug.Log($"[VModularMultiBone] 의상 효과 제거 완료 (의상 파트는 본에 유지됨)");
        
        /*
        // 본에서 벗기기 기능 (작동 불안정으로 비활성화)
        // V-avatarTool 방식: 접미사가 붙은 오브젝트들을 원래 위치로 이동
        string suffixToFind = $"[{outfitGroupName}]";
        List<Transform> attachedParts = new List<Transform>();
        
        // 아마튜어에서 접미사가 붙은 의상 파트들 찾기
        CollectAttachedOutfitParts(outfitArmature, suffixToFind, attachedParts);
        
        Debug.Log($"[VModularMultiBone] 아마튜어에서 {attachedParts.Count}개의 부착된 의상 파트 발견");
        
        // V-avatarTool의 MoveMatchingChildByName과 같은 방식으로 원래 위치로 이동
        foreach (Transform attachedPart in attachedParts)
        {
            Debug.Log($"[VModularMultiBone] 의상 파트 분리 중: {attachedPart.name}");
            
            // 위치, 회전, 스케일 보존
            Vector3 originalPosition = attachedPart.position;
            Quaternion originalRotation = attachedPart.rotation;
            Vector3 originalScale = attachedPart.localScale;
            
            // 원래 의상 오브젝트로 이동
            attachedPart.SetParent(this.transform, true);
            attachedPart.position = originalPosition;
            attachedPart.rotation = originalRotation;
            attachedPart.localScale = originalScale;
            
            // 접미사 제거
            RemoveOutfitSuffix(attachedPart);
            
            Debug.Log($"[VModularMultiBone] ✅ {attachedPart.name} 분리 완료");
        }
        */
    }
    
    /// <summary>
    /// V-avatarTool의 CollectAttachedOutfitParts와 동일한 기능
    /// 아마튜어에서 특정 접미사가 붙은 의상 파트들을 재귀적으로 찾기
    /// </summary>
    void CollectAttachedOutfitParts(Transform parent, string suffix, List<Transform> result)
    {
        foreach (Transform child in parent)
        {
            if (child.name.Contains(suffix))
            {
                result.Add(child);
            }
            
            // 재귀적으로 자식들도 검색
            CollectAttachedOutfitParts(child, suffix, result);
        }
    }
    
    void SyncScale(Transform targetBone, Transform outfitBone)
    {
        Vector3 targetWorldScale = targetBone.lossyScale;
        Vector3 parentWorldScale = outfitBone.parent != null ? outfitBone.parent.lossyScale : Vector3.one;
        
        Vector3 requiredLocalScale = new Vector3(
            Mathf.Approximately(parentWorldScale.x, 0f) ? 1f : targetWorldScale.x / parentWorldScale.x,
            Mathf.Approximately(parentWorldScale.y, 0f) ? 1f : targetWorldScale.y / parentWorldScale.y,
            Mathf.Approximately(parentWorldScale.z, 0f) ? 1f : targetWorldScale.z / parentWorldScale.z
        );
        
        outfitBone.localScale = requiredLocalScale;
    }
    
    /// <summary>
    /// 의상 접미사 추가 (한글 없이)
    /// </summary>
    void AddOutfitSuffix(Transform target)
    {
        string formattedSuffix = $"[{outfitGroupName}]";
        if (!target.name.Contains(formattedSuffix))
        {
            target.name = target.name + formattedSuffix;
        }
    }
    
    void RemoveOutfitSuffix(Transform target)
    {
        target.name = Regex.Replace(target.name, @"\([^\)]*\)|\[[^\]]*\]", "");
        
        // 자식들에게도 적용
        foreach (Transform child in target)
        {
            RemoveOutfitSuffix(child);
        }
    }
    
    string GetCleanName(string name)
    {
        // 1차: 접미사 제거 (의상), [Outfit] 등
        string cleaned = Regex.Replace(name, @"\([^\)]*\)|\[[^\]]*\]", "");
        
        // 2차: _Mesh, _mesh 접미사 제거
        cleaned = Regex.Replace(cleaned, @"_[Mm]esh$", "");
        
        // 3차: _Copied, _copied 접미사 제거  
        cleaned = Regex.Replace(cleaned, @"_[Cc]opied$", "");
        
        // 4차: 앞뒤 공백 제거
        cleaned = cleaned.Trim();
        
        return cleaned;
    }
    
    /// <summary>
    /// 동일한 이름의 본 찾기 (단순화된 버전)
    /// </summary>
    Transform FindMatchingChildByName(Transform parent, string targetName)
    {
        // 정확한 이름 매칭만 시도 (재귀적으로)
        if (parent.name == targetName)
        {
            return parent;
        }
        
        // 재귀적으로 자식들에서 검색
        foreach (Transform child in parent)
        {
            Transform found = FindMatchingChildByName(child, targetName);
            if (found != null)
            {
                return found;
            }
        }
        
        return null;
    }
    
    string GetTransformPath(Transform target)
    {
        string path = target.name;
        Transform current = target.parent;
        
        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }
        
        return path;
    }
    
    // 런타임에서 호출할 수 있는 public 메소드들
    public void Toggle()
    {
        if (isWearing)
            RemoveOutfit();
        else
            WearOutfit();
    }
    
    public bool IsWearing => isWearing;
    
    [ContextMenu("Force Initialize")]
    public void ForceInitialize()
    {
        Initialize();
    }
    
    [ContextMenu("Debug Info")]
    public void DebugInfo()
    {
        Debug.Log($"=== VModularMultiBone Debug Info ===");
        Debug.Log($"착용 상태: {isWearing}");
        Debug.Log($"아웃핏 아마튜어: {(outfitArmature != null ? outfitArmature.name : "없음")}");
        Debug.Log($"바디 렌더러: {(targetBodyRenderer != null ? targetBodyRenderer.name : "없음")}");
        Debug.Log($"바디 블렌드쉐이프: {bodyBlendShapeIndices.Count}개");
        Debug.Log($"의상 파트 수: {outfitParts.Count}");
        Debug.Log($"토글 오브젝트 수: {objectsToHideWhenWearing.Count}개");
        Debug.Log($"의상 전용 블렌드쉐이프: {outfitBlendShapes.Count}개");
        Debug.Log($"저장된 원본 블렌드쉐이프: {originalBodyBlendShapeWeights.Count}개 (이 의상이 건드린 것만)");
        Debug.Log($"그룹명: {outfitGroupName}");
        Debug.Log($"스케일 초기화 완료: {hasInitializedScale}");
        
        if (outfitParts.Count > 0 && outfitArmature != null)
        {
            Debug.Log("=== 의상 파트 목록 ===");
            for (int i = 0; i < outfitParts.Count; i++)
            {
                Transform part = outfitParts[i];
                Transform matchingBone = FindMatchingChildByName(outfitArmature, part.name);
                Debug.Log($"{i + 1}. {part.name} -> {(matchingBone != null ? matchingBone.name : "매칭 없음")}");
            }
        }
        
        if (outfitBlendShapes.Count > 0)
        {
            Debug.Log("=== 의상 전용 블렌드쉐이프 목록 ===");
            for (int i = 0; i < outfitBlendShapes.Count; i++)
            {
                var setting = outfitBlendShapes[i];
                string originalValue = originalBodyBlendShapeWeights.ContainsKey(setting.shapeKeyName) 
                    ? $"(원본: {originalBodyBlendShapeWeights[setting.shapeKeyName]})" 
                    : "(원본 미저장)";
                Debug.Log($"{i + 1}. {setting.shapeKeyName} = {setting.weight} {originalValue}");
            }
        }
        
        if (originalBodyBlendShapeWeights.Count > 0)
        {
            Debug.Log("=== 저장된 원본 블렌드쉐이프 값들 ===");
            int index = 1;
            foreach (var kvp in originalBodyBlendShapeWeights)
            {
                Debug.Log($"{index}. {kvp.Key} = {kvp.Value}");
                index++;
            }
        }
    }
}