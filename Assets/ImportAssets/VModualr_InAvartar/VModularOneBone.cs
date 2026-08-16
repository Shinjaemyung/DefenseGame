using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;

// 주요 본들만 선별한 커스텀 enum (보기 편하게 카테고리별 정리)
public enum MainHumanBones
{
    [Header("=== 기본 라인 ===")]
    Hips,
    Spine, 
    Chest,
    Neck,
    Head,
    
    [Header("=== 왼쪽 팔 ===")]
    LeftShoulder,
    LeftUpperArm,
    LeftLowerArm,
    LeftHand,
    
    [Header("=== 오른쪽 팔 ===")]
    RightShoulder,
    RightUpperArm,
    RightLowerArm,
    RightHand,
    
    [Header("=== 왼쪽 다리 ===")]
    LeftUpperLeg,
    LeftLowerLeg,
    LeftFoot,
    LeftToes,
    
    [Header("=== 오른쪽 다리 ===")]
    RightUpperLeg,
    RightLowerLeg,
    RightFoot,
    RightToes
}

// V-모듈러 단일본 시스템 (휴머노이드 본 기반)
public class VModularOneBone : MonoBehaviour
{
    [SerializeField] private string groupName = "Accessory"; // 그룹명
    public MainHumanBones targetHumanBone = MainHumanBones.Head; // 부착할 휴머노이드 본
    
    [Tooltip("부착할 악세사리의 본 (직접 지정)")]
    public Transform sourceAttachmentBone; // 부착할 소스 본 (인스펙터에서 설정)
    
    [Header("스케일 설정")]
    [Tooltip("ON: 부모 본 기준으로 스케일 자동 계산 | OFF: 원본 스케일 유지")]
    public bool useParentBasedScaling = false; // 부모 본 기준 스케일 계산 사용 여부
    
    // 내부 변수들
    private bool isWearing = false; // 착용 상태
    private Animator targetAnimator; // 자동으로 찾은 애니메이터
    private Transform targetBone; // 실제 타겟 본 (휴머노이드 본에서 찾은)
    private Transform originalParent; // 소스 본의 원본 부모
    private Vector3 originalPosition; // 소스 본의 원본 위치
    private Quaternion originalRotation; // 소스 본의 원본 회전
    private Vector3 originalScale; // 소스 본의 원본 스케일
    private string originalName; // 소스 본의 원본 이름
    
    void Start()
    {
        Initialize();
        
        // 시작 시 자동 착용
        if (targetAnimator != null && targetBone != null && sourceAttachmentBone != null)
        {
            WearAccessory();
        }
    }
    
    void Reset()
    {
        // 컴포넌트가 처음 추가될 때 게임오브젝트 이름을 그룹명으로 설정
        if (string.IsNullOrEmpty(groupName) || groupName == "Accessory")
        {
            groupName = this.gameObject.name;
            Debug.Log($"[VModularOneBone] 그룹명이 자동으로 설정됨: {groupName}");
        }
    }
    
    void OnValidate()
    {
        // 에디터에서만 실행 (빌드된 게임에서는 실행하지 않음)
        #if UNITY_EDITOR
        // 그룹명이 비어있거나 기본값일 때만 게임오브젝트 이름으로 설정
        if (string.IsNullOrEmpty(groupName) || groupName == "Accessory")
        {
            groupName = this.gameObject.name;
        }
        #endif
    }
    
    void Initialize()
    {
        Debug.Log($"[VModularOneBone] {name} 초기화 시작...");
        
        // 애니메이터를 부모 중에서 자동으로 찾기
        targetAnimator = FindAnimatorInParents(this.transform);
        if (targetAnimator == null)
        {
            Debug.LogError($"[VModularOneBone] 부모 중에서 Animator를 찾을 수 없습니다! 이 오브젝트가 아바타 하위에 있는지 확인해주세요.");
            return;
        }
        
        // 소스 본 확인 및 자동 설정 (인스펙터에서 설정하지 않았을 때만)
        if (sourceAttachmentBone == null)
        {
            Debug.LogWarning($"[VModularOneBone] Source Attachment Bone이 설정되지 않았습니다! 자동으로 첫 번째 자식을 찾습니다.");
            
            if (transform.childCount > 0)
            {
                sourceAttachmentBone = transform.GetChild(0);
                Debug.Log($"[VModularOneBone] 자동 설정됨: {sourceAttachmentBone.name}");
            }
            else
            {
                // 자식이 없으면 자기 자신을 소스로 사용
                sourceAttachmentBone = this.transform;
                Debug.Log($"[VModularOneBone] 자기 자신을 소스 본으로 사용: {sourceAttachmentBone.name}");
            }
        }
        else
        {
            Debug.Log($"[VModularOneBone] 인스펙터에서 설정된 소스 본 사용: {sourceAttachmentBone.name}");
        }
        
        // 휴머노이드 본 찾기
        HumanBodyBones actualHumanBone = ConvertToHumanBodyBones(targetHumanBone);
        targetBone = targetAnimator.GetBoneTransform(actualHumanBone);
        if (targetBone == null)
        {
            Debug.LogError($"[VModularOneBone] 휴머노이드 본을 찾을 수 없습니다: {targetHumanBone} ({actualHumanBone})");
            return;
        }
        
        Debug.Log($"[VModularOneBone] 타겟 애니메이터: {targetAnimator.name}");
        Debug.Log($"[VModularOneBone] 타겟 본: {targetBone.name} ({targetHumanBone})");
        Debug.Log($"[VModularOneBone] 소스 본: {sourceAttachmentBone.name}");
        // 모든 자식 렌더러들의 Update When Offscreen 옵션 활성화
        EnableUpdateWhenOffscreen();
        
        Debug.Log($"[VModularOneBone] {name} 초기화 완료!");
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
                Debug.Log($"[VModularOneBone] Update When Offscreen 활성화: {renderer.name}");
            }
        }
        
        if (enabledCount > 0)
        {
            Debug.Log($"[VModularOneBone] Update When Offscreen 옵션을 {enabledCount}개 렌더러에 적용함");
        }
        else
        {
            Debug.Log($"[VModularOneBone] SkinnedMeshRenderer가 없거나 이미 모두 활성화되어 있음");
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
                Debug.Log($"[VModularOneBone] 애니메이터 발견: {current.parent.name}");
                return animator;
            }
        }
        return null;
    }
    
    /// <summary>
    /// 커스텀 MainHumanBones를 실제 HumanBodyBones로 변환
    /// </summary>
    HumanBodyBones ConvertToHumanBodyBones(MainHumanBones mainBone)
    {
        switch (mainBone)
        {
            // 기본 라인
            case MainHumanBones.Hips: return HumanBodyBones.Hips;
            case MainHumanBones.Spine: return HumanBodyBones.Spine;
            case MainHumanBones.Chest: return HumanBodyBones.Chest;
            case MainHumanBones.Neck: return HumanBodyBones.Neck;
            case MainHumanBones.Head: return HumanBodyBones.Head;
            
            // 왼쪽 팔
            case MainHumanBones.LeftShoulder: return HumanBodyBones.LeftShoulder;
            case MainHumanBones.LeftUpperArm: return HumanBodyBones.LeftUpperArm;
            case MainHumanBones.LeftLowerArm: return HumanBodyBones.LeftLowerArm;
            case MainHumanBones.LeftHand: return HumanBodyBones.LeftHand;
            
            // 오른쪽 팔
            case MainHumanBones.RightShoulder: return HumanBodyBones.RightShoulder;
            case MainHumanBones.RightUpperArm: return HumanBodyBones.RightUpperArm;
            case MainHumanBones.RightLowerArm: return HumanBodyBones.RightLowerArm;
            case MainHumanBones.RightHand: return HumanBodyBones.RightHand;
            
            // 왼쪽 다리
            case MainHumanBones.LeftUpperLeg: return HumanBodyBones.LeftUpperLeg;
            case MainHumanBones.LeftLowerLeg: return HumanBodyBones.LeftLowerLeg;
            case MainHumanBones.LeftFoot: return HumanBodyBones.LeftFoot;
            case MainHumanBones.LeftToes: return HumanBodyBones.LeftToes;
            
            // 오른쪽 다리
            case MainHumanBones.RightUpperLeg: return HumanBodyBones.RightUpperLeg;
            case MainHumanBones.RightLowerLeg: return HumanBodyBones.RightLowerLeg;
            case MainHumanBones.RightFoot: return HumanBodyBones.RightFoot;
            case MainHumanBones.RightToes: return HumanBodyBones.RightToes;
            
            default: return HumanBodyBones.Head; // 기본값
        }
    }
    
    /// <summary>
    /// 이 오브젝트의 모든 자식 본들을 재귀적으로 수집
    /// </summary>
    void CollectAllBones(Transform parent, List<Transform> boneList)
    {
        foreach (Transform child in parent)
        {
            boneList.Add(child);
            CollectAllBones(child, boneList);
        }
    }
    
    // 악세사리 착용
    [ContextMenu("Wear Accessory")]
    public void WearAccessory()
    {
        if (isWearing) return;
        
        Debug.Log($"[VModularOneBone] Wearing accessory: {groupName}");
        
        if (targetBone == null || sourceAttachmentBone == null)
        {
            Debug.LogError($"[VModularOneBone] 타겟 본 또는 소스 본이 없습니다!");
            return;
        }
        
        // 원본 상태 저장
        SaveOriginalState();
        
        // 소스 본과 그 모든 자식들을 타겟 본에 부착
        AttachToTargetBone();
        
        isWearing = true;
        
        Debug.Log($"[VModularOneBone] 악세사리 착용 완료: {sourceAttachmentBone.name} -> {targetBone.name} ({targetHumanBone})");
    }
    
    /// <summary>
    /// 원본 상태 저장
    /// </summary>
    void SaveOriginalState()
    {
        originalParent = sourceAttachmentBone.parent;
        originalPosition = sourceAttachmentBone.position;
        originalRotation = sourceAttachmentBone.rotation;
        originalScale = sourceAttachmentBone.localScale;
        originalName = sourceAttachmentBone.name;
        
        Debug.Log($"[VModularOneBone] 원본 상태 저장됨: {originalName}");
    }
    
    /// <summary>
    /// 소스 본을 타겟 본에 부착 (개선된 버전)
    /// </summary>
    void AttachToTargetBone()
    {
        // 월드 좌표계에서의 위치, 회전, 스케일 보존
        Vector3 worldPos = sourceAttachmentBone.position;
        Quaternion worldRot = sourceAttachmentBone.rotation;
        Vector3 worldScale = sourceAttachmentBone.lossyScale;
        
        Debug.Log($"[VModularOneBone] 부착 전 상태 - 위치: {worldPos}, 회전: {worldRot.eulerAngles}, 스케일: {worldScale}");
        
        // 타겟 본 아래로 이동
        sourceAttachmentBone.SetParent(targetBone, false); // false로 설정하여 로컬 변환값 유지
        
        // 월드 좌표계 값으로 복원
        sourceAttachmentBone.position = worldPos;
        sourceAttachmentBone.rotation = worldRot;
        
        // 스케일 처리 (토글에 따라)
        if (useParentBasedScaling)
        {
            // 부모의 스케일을 고려하여 자동 계산
            if (sourceAttachmentBone.parent != null)
            {
                Vector3 parentScale = sourceAttachmentBone.parent.lossyScale;
                Vector3 requiredLocalScale = new Vector3(
                    parentScale.x != 0 ? worldScale.x / parentScale.x : 1f,
                    parentScale.y != 0 ? worldScale.y / parentScale.y : 1f,
                    parentScale.z != 0 ? worldScale.z / parentScale.z : 1f
                );
                sourceAttachmentBone.localScale = requiredLocalScale;
                Debug.Log($"[VModularOneBone] 부모 기준 스케일 자동 계산: {requiredLocalScale}");
            }
        }
        else
        {
            // 원본 월드 스케일을 그대로 유지하려고 시도 (localScale 조정 없음)
            Debug.Log($"[VModularOneBone] 원본 스케일 유지 모드: {sourceAttachmentBone.localScale}");
        }
        
        // 접미사 추가 (본과 모든 자식들에게)
        AddAccessorySuffixRecursive(sourceAttachmentBone);
        
        Debug.Log($"[VModularOneBone] 부착 완료 - 새 부모: {targetBone.name}");
        Debug.Log($"[VModularOneBone] 부착 후 상태 - 위치: {sourceAttachmentBone.position}, 회전: {sourceAttachmentBone.rotation.eulerAngles}, 스케일: {sourceAttachmentBone.lossyScale}");
    }
    
    // 악세사리 벗기
    [ContextMenu("Remove Accessory")]
    public void RemoveAccessory()
    {
        if (!isWearing) return;
        
        Debug.Log($"[VModularOneBone] Removing accessory: {groupName}");
        
        // 소스 본을 원래 위치로 복원
        RestoreOriginalState();
        
        isWearing = false;
        
        Debug.Log($"[VModularOneBone] 악세사리 제거 완료: {sourceAttachmentBone.name}");
    }
    
    /// <summary>
    /// 원본 상태 복원
    /// </summary>
    void RestoreOriginalState()
    {
        Debug.Log($"[VModularOneBone] 원본 상태 복원 시작 - 원본 부모: {(originalParent != null ? originalParent.name : "없음")}");
        
        // 원본 부모로 이동
        sourceAttachmentBone.SetParent(originalParent, false);
        
        // 원본 변환값 복원
        sourceAttachmentBone.position = originalPosition;
        sourceAttachmentBone.rotation = originalRotation;
        sourceAttachmentBone.localScale = originalScale;
        
        // 접미사 제거 (본과 모든 자식들에서)
        RemoveAccessorySuffixRecursive(sourceAttachmentBone);
        
        Debug.Log($"[VModularOneBone] 원본 상태 복원됨: {sourceAttachmentBone.name}");
    }
    
    /// <summary>
    /// 악세사리 접미사 재귀적으로 추가
    /// </summary>
    void AddAccessorySuffixRecursive(Transform target)
    {
        string formattedSuffix = $"[{groupName}]";
        if (!target.name.Contains(formattedSuffix))
        {
            target.name = target.name + formattedSuffix;
        }
        
        // 모든 자식들에게도 적용
        foreach (Transform child in target)
        {
            AddAccessorySuffixRecursive(child);
        }
    }
    
    /// <summary>
    /// 악세사리 접미사 재귀적으로 제거
    /// </summary>
    void RemoveAccessorySuffixRecursive(Transform target)
    {
        target.name = Regex.Replace(target.name, @"\([^\)]*\)|\[[^\]]*\]", "");
        
        // 모든 자식들에게도 적용
        foreach (Transform child in target)
        {
            RemoveAccessorySuffixRecursive(child);
        }
    }
    
    // 런타임에서 호출할 수 있는 public 메소드들
    public void Toggle()
    {
        if (isWearing)
            RemoveAccessory();
        else
            WearAccessory();
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
        Debug.Log($"=== VModularOneBone Debug Info ===");
        Debug.Log($"착용 상태: {isWearing}");
        Debug.Log($"타겟 애니메이터: {(targetAnimator != null ? targetAnimator.name : "없음")}");
        Debug.Log($"타겟 본: {(targetBone != null ? targetBone.name : "없음")} ({targetHumanBone})");
        Debug.Log($"소스 본: {(sourceAttachmentBone != null ? sourceAttachmentBone.name : "없음")}");
        Debug.Log($"그룹명: {groupName}");
        
        if (sourceAttachmentBone != null)
        {
            Debug.Log($"현재 부모: {(sourceAttachmentBone.parent != null ? sourceAttachmentBone.parent.name : "없음")}");
            Debug.Log($"현재 위치: {sourceAttachmentBone.position}");
            Debug.Log($"현재 회전: {sourceAttachmentBone.rotation.eulerAngles}");
            Debug.Log($"현재 스케일: {sourceAttachmentBone.localScale}");
        }
    }
}