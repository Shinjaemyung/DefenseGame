using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

/// <summary>
/// 컴포넌트 관리, 전체 적용/되돌리기, 프리셋 기능 제공
/// </summary>
public class VModularManager : EditorWindow
{
    #region 변수 선언
    
    [Header("Target Settings")]
    public GameObject targetAvatar;
    
    // 스크롤 위치
    private Vector2 mainScrollPosition;
    private Vector2 componentScrollPosition;
    
    
    // 컴포넌트 캐시
    private List<VModularOneBone> singleBoneComponents = new List<VModularOneBone>();
    private List<VModularMultiBone> multiBoneComponents = new List<VModularMultiBone>();
    
    
    // 시스템 상태
    private bool isSystemInitialized = false;
    private bool showDebugInfo = false;
    private bool showAdvancedOptions = false;
    
    // 사용법 가이드
    private bool showHowToUse = false;
    
    // 빌드 준비 관련
    private bool nonScriptMode = false;
    
    // 백업 시스템 (아바타별로 독립 관리)
    private Dictionary<GameObject, Dictionary<Component, ComponentBackupData>> avatarBackups = new Dictionary<GameObject, Dictionary<Component, ComponentBackupData>>();
    private Dictionary<GameObject, bool> avatarBackupStatus = new Dictionary<GameObject, bool>();
    
    // 현재 아바타의 백업 상태 확인
    private bool HasCurrentAvatarBackup => targetAvatar != null && avatarBackupStatus.ContainsKey(targetAvatar) && avatarBackupStatus[targetAvatar];
    
    // 현재 아바타의 백업 데이터 가져오기
    private Dictionary<Component, ComponentBackupData> GetCurrentAvatarBackup()
    {
        if (targetAvatar == null) return new Dictionary<Component, ComponentBackupData>();
        
        if (!avatarBackups.ContainsKey(targetAvatar))
        {
            avatarBackups[targetAvatar] = new Dictionary<Component, ComponentBackupData>();
        }
        
        return avatarBackups[targetAvatar];
    }
    
    
    [System.Serializable]
    public class ComponentBackupData
    {
        public bool isWearing;
        public Dictionary<Transform, Transform> boneParents = new Dictionary<Transform, Transform>();
        public Dictionary<Transform, int> boneSiblingIndices = new Dictionary<Transform, int>();
        public Dictionary<Transform, string> originalNames = new Dictionary<Transform, string>(); // 원래 이름 백업
    }
    
    #endregion
    
    #region 메뉴 및 윈도우 초기화
    
    [MenuItem("VModular/VModular Manager")]
    public static void ShowWindow()
    {
        VModularManager window = EditorWindow.GetWindow<VModularManager>("V Modular Manager");
        window.minSize = new Vector2(450, 600);
        window.maxSize = new Vector2(800, 900);
    }
    
    void OnEnable()
    {
        RefreshSystemData();
    }
    
    void OnGUI()
    {
        mainScrollPosition = EditorGUILayout.BeginScrollView(mainScrollPosition);
        
        DrawHeader();
        DrawHowToUseGuide();
        DrawTargetSelection();
        DrawMainContent();
        
        EditorGUILayout.EndScrollView();
        
        if (GUI.changed)
        {
            Repaint();
        }
    }
    
    #endregion
    
    #region UI 그리기 메서드들
    
    private void DrawHeader()
    {
        EditorGUILayout.Space(10);
        
        // 언어 선택기
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        VModularLocalization.DrawLanguageSelector();
        EditorGUILayout.EndHorizontal();
        
        // 메인 헤더
        EditorGUILayout.BeginVertical("box");
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 16;
        titleStyle.alignment = TextAnchor.MiddleCenter;

        GUILayout.Label(VModularLocalization.Get("outfit_tool_desc"), EditorStyles.miniLabel);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(5);
    }
    
    /// <summary>
    /// 사용법 가이드 UI 그리기 (접혀있고 펴서 볼 수 있게)
    /// </summary>
    private void DrawHowToUseGuide()
    {
        EditorGUILayout.BeginVertical("box");
        
        // 접히는 헤더
        showHowToUse = EditorGUILayout.Foldout(showHowToUse, VModularLocalization.Get("how_to_use"), true, EditorStyles.foldoutHeader);
        
        if (showHowToUse)
        {
            EditorGUILayout.Space(5);
            
            // 기본 주의사항
            EditorGUILayout.LabelField(VModularLocalization.Get("basic_precautions"), EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(VModularLocalization.Get("multibone_precaution"), MessageType.Warning);
            EditorGUILayout.HelpBox(VModularLocalization.Get("hierarchy_requirement"), MessageType.Info);
            
            EditorGUILayout.Space(10);
            
            // 계층구조 예시
            EditorGUILayout.LabelField(VModularLocalization.Get("hierarchy_example"), EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            
            // 더 큰 폰트와 충분한 공간으로 계층구조 표시
            GUIStyle hierarchyStyle = new GUIStyle(EditorStyles.label);
            hierarchyStyle.fontSize = 10;  // 11 -> 13으로 증가
            hierarchyStyle.wordWrap = false;
            hierarchyStyle.richText = false;
            hierarchyStyle.alignment = TextAnchor.UpperLeft;
            hierarchyStyle.padding = new RectOffset(10, 10, 5, 5); // 여백 추가
            
            // Unity 버전별 안전한 폰트 로딩
            Font safeMonoFont = GetSafeMonospaceFont();
            if (safeMonoFont != null)
            {
                hierarchyStyle.font = safeMonoFont;
            }
            
            // 스크롤 가능한 텍스트 영역으로 표시
            string hierarchyText = VModularLocalization.Get("hierarchy_structure");
            
            // 충분한 높이 계산 (라인 수 * 라인 높이 + 여백)
            int lineCount = hierarchyText.Split('\n').Length;
            float textHeight = lineCount * (hierarchyStyle.fontSize + 4) + 20; // 라인 간격 + 여백
            
            // 최소 높이 보장
            textHeight = Mathf.Max(textHeight, 120f);
            
            // 텍스트 영역으로 표시 (읽기 전용) - 배경색 추가
            Color originalColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.95f, 0.95f, 0.95f, 1f); // 연한 회색 배경
            
            EditorGUILayout.BeginVertical();
            EditorGUILayout.TextArea(hierarchyText, hierarchyStyle, GUILayout.Height(textHeight), GUILayout.ExpandWidth(true));
        EditorGUILayout.EndVertical();
        
            GUI.backgroundColor = originalColor; // 원래 색상 복원
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(10);
            
            // 컴포넌트 차이점
            EditorGUILayout.LabelField(VModularLocalization.Get("component_difference"), EditorStyles.boldLabel);
            EditorGUILayout.LabelField(VModularLocalization.Get("multibone_description"), EditorStyles.miniLabel);
            EditorGUILayout.LabelField(VModularLocalization.Get("singlebone_description"), EditorStyles.miniLabel);
            
            EditorGUILayout.Space(5);
            
            // Head 본 규칙 - 경고 박스로 표시
            EditorGUILayout.HelpBox(VModularLocalization.Get("head_bone_rule"), MessageType.Warning);
            
            EditorGUILayout.Space(10);
            
            // 설정 가이드
            EditorGUILayout.LabelField(VModularLocalization.Get("setup_guide"), EditorStyles.boldLabel);
            
            // 1단계
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField(VModularLocalization.Get("step_1"), EditorStyles.boldLabel);
            EditorGUILayout.LabelField(VModularLocalization.Get("multibone_setup"), EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.Space(3);
            EditorGUILayout.LabelField(VModularLocalization.Get("singlebone_setup"), EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(5);
            
            // 2단계
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField(VModularLocalization.Get("step_2"), EditorStyles.boldLabel);
            EditorGUILayout.LabelField(VModularLocalization.Get("avatar_selection_guide"), EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(5);
            
            // 3단계
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField(VModularLocalization.Get("step_3"), EditorStyles.boldLabel);
            EditorGUILayout.LabelField(VModularLocalization.Get("apply_restore_guide"), EditorStyles.wordWrappedMiniLabel);
        EditorGUILayout.EndVertical();
        
            EditorGUILayout.Space(5);
            
            // 빌드 준비 가이드
            EditorGUILayout.LabelField(VModularLocalization.Get("build_prep_guide"), EditorStyles.boldLabel);
            EditorGUILayout.LabelField(VModularLocalization.Get("build_prep_overview"), EditorStyles.wordWrappedMiniLabel);
            
            EditorGUILayout.Space(5);
            
            // 빌드 준비 프로세스
            EditorGUILayout.LabelField(VModularLocalization.Get("build_prep_process"), EditorStyles.boldLabel);
            
            // 1단계
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField(VModularLocalization.Get("build_prep_step1"), EditorStyles.boldLabel);
            EditorGUILayout.LabelField(VModularLocalization.Get("build_prep_step1_desc"), EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(3);
            
            // 2단계
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField(VModularLocalization.Get("build_prep_step2"), EditorStyles.boldLabel);
            EditorGUILayout.LabelField(VModularLocalization.Get("build_prep_step2_desc"), EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(3);
            
            // 3단계
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField(VModularLocalization.Get("build_prep_step3"), EditorStyles.boldLabel);
            EditorGUILayout.LabelField(VModularLocalization.Get("build_prep_step3_desc"), EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(3);
            
            // 4단계
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField(VModularLocalization.Get("build_prep_step4"), EditorStyles.boldLabel);
            EditorGUILayout.LabelField(VModularLocalization.Get("build_prep_step4_desc"), EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(5);
            
            // 빌드 준비 모드
            EditorGUILayout.LabelField(VModularLocalization.Get("build_prep_modes"), EditorStyles.boldLabel);
            
            // 기본 모드
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField(VModularLocalization.Get("default_build_mode"), EditorStyles.boldLabel);
            EditorGUILayout.LabelField(VModularLocalization.Get("default_build_mode_desc"), EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(3);
            
            // Non스크립트 모드
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField(VModularLocalization.Get("non_script_build_mode"), EditorStyles.boldLabel);
            EditorGUILayout.LabelField(VModularLocalization.Get("non_script_build_mode_desc"), EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(5);
            
            // UI 제너레이터 사용법
            EditorGUILayout.LabelField(VModularLocalization.Get("build_prep_ui_generator_note"), EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField(VModularLocalization.Get("build_prep_ui_generator_desc"), EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(5);
            
            // 권장 워크플로우
            EditorGUILayout.LabelField(VModularLocalization.Get("build_prep_workflow"), EditorStyles.boldLabel);
            
            // 워크플로우 단계들
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField(VModularLocalization.Get("build_prep_workflow_step1"), EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.Space(2);
            EditorGUILayout.LabelField(VModularLocalization.Get("build_prep_workflow_step2"), EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.Space(2);
            EditorGUILayout.LabelField(VModularLocalization.Get("build_prep_workflow_step3"), EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.Space(2);
            EditorGUILayout.LabelField(VModularLocalization.Get("build_prep_workflow_step4"), EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(5);
            
            // WARUDO 전용 가이드
            EditorGUILayout.LabelField(VModularLocalization.Get("warudo_build_guide"), EditorStyles.boldLabel);
            
            // 호환성 정보
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(VModularLocalization.Get("warudo_compatibility"), EditorStyles.miniLabel);
            EditorGUILayout.LabelField(VModularLocalization.Get("vrc_incompatibility"), EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            // WARUDO 빌드 요구사항
            EditorGUILayout.LabelField(VModularLocalization.Get("warudo_folder_requirement"), EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField(VModularLocalization.Get("warudo_folder_instruction"), EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(5);
            
            // 첫 스크립트 빌드 설정
            EditorGUILayout.LabelField(VModularLocalization.Get("warudo_script_setup"), EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField(VModularLocalization.Get("warudo_script_instruction"), EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(5);
        }
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }
    
    private void DrawTargetSelection()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField($"🎯 {VModularLocalization.Get("avatar")}", EditorStyles.boldLabel);
        
        GameObject newTarget = EditorGUILayout.ObjectField(VModularLocalization.Get("avatar"), targetAvatar, typeof(GameObject), true) as GameObject;
        
        if (newTarget != targetAvatar)
        {
            targetAvatar = newTarget;
            RefreshSystemData();
        }
        
        if (targetAvatar != null)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField(string.Format(VModularLocalization.Get("selected"), targetAvatar.name));
        }
        else
        {
            EditorGUILayout.HelpBox(VModularLocalization.Get("select_avatar_with_vmodular"), MessageType.Info);
        }
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }
    
    private void DrawMainContent()
    {
        if (targetAvatar == null)
        {
            EditorGUILayout.HelpBox(VModularLocalization.Get("select_avatar_first"), MessageType.Warning);
            return;
        }
        
        // 메인 기능 - 전체 적용/되돌리기
        DrawOutfitControls();
        
        EditorGUILayout.Space(10);
        
        // 편의성 기능
        DrawConvenienceFeatures();
        
        EditorGUILayout.Space(10);
        
        // 빌드 준비 기능
        DrawBuildPreparation();
        
        EditorGUILayout.Space(10);
        
        // 컴포넌트 상태 보기
        DrawComponentStatus();
    }
    
    #endregion
    
    #region 메인 UI 그리기
    
    private void DrawOutfitControls()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField(VModularLocalization.Get("outfit_tool_title").Substring(2), EditorStyles.boldLabel); // 이모지 제거
        
        // 시스템 상태 간단 표시
        EditorGUILayout.LabelField(string.Format(VModularLocalization.Get("component_count"), singleBoneComponents.Count, multiBoneComponents.Count), EditorStyles.miniLabel);
        if (HasCurrentAvatarBackup)
        {
            EditorGUILayout.LabelField(VModularLocalization.Get("backup_exists"), EditorStyles.miniLabel);
        }
        
        EditorGUILayout.Space(5);
        
        // 메인 버튼들
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(VModularLocalization.Get("apply_all"), GUILayout.Height(40)))
        {
            ApplyAllComponents();
        }
        
        GUI.enabled = HasCurrentAvatarBackup;
        if (GUILayout.Button(VModularLocalization.Get("restore"), GUILayout.Height(40)))
        {
            RestoreFromBackup();
        }
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawConvenienceFeatures()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField(VModularLocalization.Get("convenience_features"), EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(VModularLocalization.Get("remove_missing_scripts"), GUILayout.Height(30)))
        {
            RemoveMissingScripts();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawBuildPreparation()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField(VModularLocalization.Get("build_preparation"), EditorStyles.boldLabel);
        
        // Non스크립트 모드 토글
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(VModularLocalization.Get("non_script_mode"), GUILayout.Width(120));
        nonScriptMode = EditorGUILayout.Toggle(nonScriptMode);
        if (nonScriptMode)
        {
            EditorGUILayout.LabelField(VModularLocalization.Get("non_script_mode_desc"), EditorStyles.miniLabel);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        // 빌드 준비 버튼
        Color originalColor = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.8f, 1f, 0.8f, 1f); // 연한 초록색
        
        if (GUILayout.Button(VModularLocalization.Get("start_build_preparation"), GUILayout.Height(40)))
        {
            StartBuildPreparation();
        }
        
        GUI.backgroundColor = originalColor;
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawComponentStatus()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField(VModularLocalization.Get("component_status"), EditorStyles.boldLabel);
        
        if (singleBoneComponents.Count == 0 && multiBoneComponents.Count == 0)
        {
            EditorGUILayout.LabelField(VModularLocalization.Get("no_components"), EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
            return;
        }
        
        // 전체 활성화/비활성화 버튼 (색상 적용)
        EditorGUILayout.BeginHorizontal();
        
        Color originalColor = GUI.backgroundColor;
        
        // 전체 활성화 버튼 (초록색)
        GUI.backgroundColor = new Color(0.5f, 1f, 0.5f, 1f);
        if (GUILayout.Button(VModularLocalization.Get("enable_all_components"), GUILayout.Height(25)))
        {
            SetAllComponentsActive(true);
        }
        
        // 전체 비활성화 버튼 (빨간색)
        GUI.backgroundColor = new Color(1f, 0.5f, 0.5f, 1f);
        if (GUILayout.Button(VModularLocalization.Get("disable_all_components"), GUILayout.Height(25)))
        {
            SetAllComponentsActive(false);
        }
        
        // 원래 색상 복원
        GUI.backgroundColor = originalColor;
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        componentScrollPosition = EditorGUILayout.BeginScrollView(componentScrollPosition, GUILayout.MaxHeight(250));
        
        // SingleBone 컴포넌트들
        if (singleBoneComponents.Count > 0)
        {
            EditorGUILayout.LabelField(VModularLocalization.Get("single_bone"), EditorStyles.boldLabel);
            foreach (var comp in singleBoneComponents)
            {
                if (comp != null)
                {
                    DrawSimpleComponentItem(comp);
                }
            }
        }
        
        // MultiBone 컴포넌트들
        if (multiBoneComponents.Count > 0)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField(VModularLocalization.Get("multi_bone"), EditorStyles.boldLabel);
            foreach (var comp in multiBoneComponents)
            {
                if (comp != null)
                {
                    DrawSimpleComponentItem(comp);
                }
            }
        }
        
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }
    
    
    private void DrawSimpleComponentItem(Component component)
    {
        EditorGUILayout.BeginHorizontal("box");
        
        // 컴포넌트 이름
        EditorGUILayout.LabelField(component.name, GUILayout.MinWidth(150));
        
        // 착용 상태 표시
        string statusText = "";
        if (component is VModularOneBone oneBone)
        {
            statusText = oneBone.IsWearing ? $"🟢 {VModularLocalization.Get("wearing")}" : $"⚪ {VModularLocalization.Get("not_wearing")}";
        }
        else if (component is VModularMultiBone multiBone)
        {
            statusText = multiBone.IsWearing ? $"🟢 {VModularLocalization.Get("wearing")}" : $"⚪ {VModularLocalization.Get("not_wearing")}";
        }
        
        EditorGUILayout.LabelField(statusText, GUILayout.Width(70));
        
        // GameObject 활성화 상태 표시 (색상 적용)
        Color originalTextColor = GUI.color;
        bool isActive = component.gameObject.activeSelf;

        
        // GameObject 활성화 토글 버튼 (색상으로 상태 표시)
        Color originalColor = GUI.backgroundColor;
        
        // 활성화 상태에 따른 버튼 색상 설정
        GUI.backgroundColor = isActive ? new Color(0.5f, 1f, 0.5f, 1f) : new Color(1f, 0.5f, 0.5f, 1f); // 초록색 : 빨간색
        
        // 버튼 텍스트도 상태에 맞게 변경
        string buttonText = isActive ? "🟢" : "⚫";
        
        if (GUILayout.Button(buttonText, GUILayout.Width(25)))
        {
            Undo.RecordObject(component.gameObject, "Toggle GameObject Active");
            component.gameObject.SetActive(!component.gameObject.activeSelf);
            EditorUtility.SetDirty(component.gameObject);
            Debug.Log($"[VModularManager] GameObject 토글: {component.name} -> {(component.gameObject.activeSelf ? "활성화" : "비활성화")}");
        }
        
        // 원래 색상 복원
        GUI.backgroundColor = originalColor;
        
        // 게임오브젝트 선택 버튼
        if (GUILayout.Button("🎯", GUILayout.Width(25)))
        {
            Selection.activeGameObject = component.gameObject;
        }
        
        EditorGUILayout.EndHorizontal();
    }
    
    
    
    #endregion
    
    #region 빌드 준비 기능
    
    /// <summary>
    /// 빌드 준비 프로세스 시작
    /// </summary>
    private void StartBuildPreparation()
    {
        if (targetAvatar == null)
        {
            EditorUtility.DisplayDialog(VModularLocalization.Get("error"), VModularLocalization.Get("select_avatar_first"), VModularLocalization.Get("ok"));
            return;
        }
        
        // 확인 다이얼로그
        string message = nonScriptMode ? 
            VModularLocalization.Get("build_prep_message") :
            VModularLocalization.Get("build_prep_message_no_script");
            
        if (!EditorUtility.DisplayDialog(VModularLocalization.Get("build_prep_confirm"), message, VModularLocalization.Get("start"), VModularLocalization.Get("cancel")))
        {
            return;
        }
        
        Debug.Log("[VModularManager] === 빌드 준비 시작 ===");
        
        try
        {
            // 1단계: 원본 복제
            GameObject clonedAvatar = CloneOriginalAvatar();
            if (clonedAvatar == null)
            {
                EditorUtility.DisplayDialog(VModularLocalization.Get("error"), "아바타 복제에 실패했습니다!", VModularLocalization.Get("ok"));
                return;
            }
            
            // 2단계: 원본 비활성화
            targetAvatar.SetActive(false);
            Debug.Log($"[VModularManager] 원본 비활성화: {targetAvatar.name}");
            
            // 3단계: 복제본에 모두 입히기 적용
            ApplyAllToClone(clonedAvatar);
            
            // 4단계: 복제본 이름 변경
            clonedAvatar.name = "Character";
            Debug.Log($"[VModularManager] 복제본 이름 변경: Character");
            
            // 5단계: Non스크립트 모드인 경우 스크립트 제거
            if (nonScriptMode)
            {
                RemoveVModularScripts(clonedAvatar);
            }
            
            // 완료 메시지
            string scriptRemovedText = nonScriptMode ? VModularLocalization.Get("scripts_removed") + "\n" : "";
            string resultMessage = string.Format(VModularLocalization.Get("build_prep_result"), targetAvatar.name, scriptRemovedText);
            
            EditorUtility.DisplayDialog(VModularLocalization.Get("build_prep_complete"), resultMessage, VModularLocalization.Get("ok"));
            Debug.Log("[VModularManager] === 빌드 준비 완료 ===");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[VModularManager] 빌드 준비 중 오류: {ex.Message}");
            EditorUtility.DisplayDialog(VModularLocalization.Get("error"), $"빌드 준비 중 오류가 발생했습니다:\n{ex.Message}", VModularLocalization.Get("ok"));
        }
    }
    
    /// <summary>
    /// 원본 아바타를 복제합니다
    /// </summary>
    private GameObject CloneOriginalAvatar()
    {
        try
        {
            // Instantiate로 복제
            GameObject clone = UnityEngine.Object.Instantiate(targetAvatar);
            clone.name = targetAvatar.name + "_Clone";
            
            // 씬에 등록
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            
            Debug.Log($"[VModularManager] 아바타 복제 완료: {clone.name}");
            return clone;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[VModularManager] 아바타 복제 실패: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// 복제본에 모든 V모듈러 컴포넌트를 적용합니다
    /// </summary>
    private void ApplyAllToClone(GameObject clone)
    {
        try
        {
            // 복제본에서 V모듈러 컴포넌트들 찾기
            var cloneSingleBones = clone.GetComponentsInChildren<VModularOneBone>(true);
            var cloneMultiBones = clone.GetComponentsInChildren<VModularMultiBone>(true);
            
            Debug.Log($"[VModularManager] 복제본에서 발견된 컴포넌트: SingleBone {cloneSingleBones.Length}개, MultiBone {cloneMultiBones.Length}개");
            
            int appliedCount = 0;
            
            // SingleBone 컴포넌트들 적용
            foreach (var singleBone in cloneSingleBones)
            {
                if (singleBone != null && singleBone.gameObject.activeSelf)
                {
                    try
                    {
                        singleBone.ForceInitialize();
                        if (!singleBone.IsWearing)
                        {
                            singleBone.WearAccessory();
                            appliedCount++;
                            Debug.Log($"[VModularManager] SingleBone 적용: {singleBone.gameObject.name}");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"[VModularManager] SingleBone 적용 실패: {singleBone.gameObject.name} - {ex.Message}");
                    }
                }
            }
            
            // MultiBone 컴포넌트들 적용
            foreach (var multiBone in cloneMultiBones)
            {
                if (multiBone != null && multiBone.gameObject.activeSelf)
                {
                    try
                    {
                        multiBone.ForceInitialize();
                        if (!multiBone.IsWearing)
                        {
                            multiBone.WearOutfit();
                            appliedCount++;
                            Debug.Log($"[VModularManager] MultiBone 적용: {multiBone.gameObject.name}");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"[VModularManager] MultiBone 적용 실패: {multiBone.gameObject.name} - {ex.Message}");
                    }
                }
            }
            
            Debug.Log($"[VModularManager] 복제본에 {appliedCount}개 컴포넌트 적용 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[VModularManager] 복제본 적용 중 오류: {ex.Message}");
        }
    }
    
    /// <summary>
    /// V모듈러 및 숏컷 관련 스크립트를 제거합니다
    /// </summary>
    private void RemoveVModularScripts(GameObject target)
    {
        try
        {
            int removedCount = 0;
            
            // 모든 자식 오브젝트 포함해서 검색
            Transform[] allTransforms = target.GetComponentsInChildren<Transform>(true);
            
            foreach (Transform transform in allTransforms)
            {
                GameObject obj = transform.gameObject;
                
                // V모듈러 관련 컴포넌트들 제거
                var vModularComponents = obj.GetComponents<Component>();
                foreach (var component in vModularComponents)
                {
                    if (component != null && IsVModularScript(component))
                    {
                        Undo.DestroyObjectImmediate(component);
                        removedCount++;
                        Debug.Log($"[VModularManager] V모듈러 스크립트 제거: {component.GetType().Name} from {obj.name}");
                    }
                }
            }
            
            Debug.Log($"[VModularManager] V모듈러/숏컷 스크립트 제거 완료: {removedCount}개 제거됨");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[VModularManager] 스크립트 제거 중 오류: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 컴포넌트가 V모듈러 관련 스크립트인지 확인합니다
    /// </summary>
    private bool IsVModularScript(Component component)
    {
        if (component == null) return false;
        
        string typeName = component.GetType().Name;
        
        // V모듈러 관련 스크립트들
        if (typeName.Contains("VModular") || 
            typeName.Contains("ShortcutObjectManager") ||
            typeName.Contains("VModularOneBone") ||
            typeName.Contains("VModularMultiBone") ||
            typeName.Contains("VModularUIGenerator") ||
            typeName.Contains("VModularManager"))
        {
            return true;
        }
        
        return false;
    }
    
    #endregion
    
    #region Unity 버전 호환성 메서드
    
    /// <summary>
    /// Unity 2018-2023 버전에서 안전하게 동작하는 고정폭 폰트를 가져옵니다
    /// </summary>
    private Font GetSafeMonospaceFont()
    {
        // 에디터 기본 폰트들만 사용 - Resources.GetBuiltinResource 사용 안함
        try
        {
            // 1순위: EditorStyles.miniLabel 폰트
            if (EditorStyles.miniLabel?.font != null)
            {
                return EditorStyles.miniLabel.font;
            }
        }
        catch
        {
            // 무시
        }
        
        try
        {
            // 2순위: GUI.skin 기본 폰트  
            if (GUI.skin?.font != null)
            {
                return GUI.skin.font;
            }
        }
        catch
        {
            // 무시
        }
        
        try
        {
            // 3순위: EditorStyles.label 폰트
            if (EditorStyles.label?.font != null)
            {
                return EditorStyles.label.font;
            }
        }
        catch
        {
            // 무시
        }
        
        // 최후의 수단: null 반환 (Unity가 기본 폰트 사용)
        return null;
    }
    
    #endregion
    
    #region GameObject 활성화 관리
    
    /// <summary>
    /// 모든 VModular 컴포넌트 GameObject의 활성화 상태를 설정합니다
    /// </summary>
    private void SetAllComponentsActive(bool active)
    {
        int count = 0;
        
        foreach (var comp in singleBoneComponents)
        {
            if (comp != null)
            {
                Undo.RecordObject(comp.gameObject, $"Set {comp.name} Active");
                comp.gameObject.SetActive(active);
                EditorUtility.SetDirty(comp.gameObject);
                count++;
            }
        }
        
        foreach (var comp in multiBoneComponents)
        {
            if (comp != null)
            {
                Undo.RecordObject(comp.gameObject, $"Set {comp.name} Active");
                comp.gameObject.SetActive(active);
                EditorUtility.SetDirty(comp.gameObject);
                count++;
            }
        }
        
        Debug.Log($"[VModularManager] {count}개 컴포넌트 GameObject를 {(active ? "활성화" : "비활성화")}했습니다");
    }
    
    #endregion
    
    #region 편의성 기능들
    
    /// <summary>
    /// CharacterToolbox에서 복제한 미싱 스크립트 제거 기능
    /// </summary>
    private void RemoveMissingScripts()
    {
        if (targetAvatar == null)
        {
            EditorUtility.DisplayDialog(VModularLocalization.Get("error"), VModularLocalization.Get("select_avatar_first"), VModularLocalization.Get("ok"));
            return;
        }
        
        int removedCount = 0;
        
        // 모든 자식 오브젝트 포함해서 검색
        Transform[] allTransforms = targetAvatar.GetComponentsInChildren<Transform>(true);
        
        foreach (Transform transform in allTransforms)
        {
            GameObject obj = transform.gameObject;
            
            // Unity의 내장 함수를 사용하여 미싱 스크립트 제거
            int missingCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(obj);
            if (missingCount > 0)
            {
                Undo.RecordObject(obj, "Remove Missing Scripts");
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj);
                removedCount += missingCount;
                Debug.Log($"[VModularManager] Removed {missingCount} missing scripts from {obj.name}");
                EditorUtility.SetDirty(obj);
            }
        }
        
        Debug.Log($"[VModularManager] Missing script removal completed. Removed {removedCount} missing scripts total.");
        
        // 씬을 더티로 마크
        if (removedCount > 0)
        {
            EditorUtility.SetDirty(targetAvatar);
            EditorUtility.DisplayDialog(VModularLocalization.Get("completed"), 
                $"{VModularLocalization.Get("missing_scripts_removed_success")}\n\n{string.Format(VModularLocalization.Get("scripts_removed_count"), removedCount)}", VModularLocalization.Get("ok"));
        }
        else
        {
            EditorUtility.DisplayDialog(VModularLocalization.Get("completed"), 
                VModularLocalization.Get("no_missing_scripts_found"), VModularLocalization.Get("ok"));
        }
    }
    
    #endregion
    
    #region 시스템 관리 메서드들
    
    /// <summary>
    /// 시스템 데이터를 새로고침합니다
    /// </summary>
    private void RefreshSystemData()
    {
        singleBoneComponents.Clear();
        multiBoneComponents.Clear();
        
        if (targetAvatar == null) return;
        
        // V모듈러 컴포넌트들 스캔
        singleBoneComponents.AddRange(targetAvatar.GetComponentsInChildren<VModularOneBone>(true));
        multiBoneComponents.AddRange(targetAvatar.GetComponentsInChildren<VModularMultiBone>(true));
        
        Debug.Log($"[VModularManager] 시스템 데이터 새로고침 완료: SingleBone {singleBoneComponents.Count}개, MultiBone {multiBoneComponents.Count}개");
        
        isSystemInitialized = true;
    }
    
    
    
    
    
    /// <summary>
    /// 모든 컴포넌트 적용하기 (VModularUIGenerator의 ApplyModularComponents 기반)
    /// </summary>
    private void ApplyAllComponents()
    {
        if (targetAvatar == null)
        {
            EditorUtility.DisplayDialog("오류", "타겟 아바타가 선택되지 않았습니다!", "확인");
            return;
        }
        
        // 백업 생성
        CreateBackup();
        
        Debug.Log("[VModularManager] === 전체 적용 시작 ===");
        
        int appliedCount = 0;
        int skippedCount = 0;
        
        // 먼저 모든 컴포넌트들을 강제로 초기화
        Debug.Log("[VModularManager] === 초기화 단계 ===");
        
        foreach (var singleBone in singleBoneComponents)
        {
            if (singleBone != null && singleBone.gameObject.activeSelf)
            {
                try
                {
                    Debug.Log($"[VModularManager] Initializing SingleBone: {singleBone.gameObject.name}");
                    singleBone.ForceInitialize();
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"❌ Failed to initialize SingleBone {singleBone.gameObject.name}: {ex.Message}");
                }
            }
        }
        
        foreach (var multiBone in multiBoneComponents)
        {
            if (multiBone != null && multiBone.gameObject.activeSelf)
            {
                try
                {
                    Debug.Log($"[VModularManager] Initializing MultiBone: {multiBone.gameObject.name}");
                    multiBone.ForceInitialize();
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"❌ Failed to initialize MultiBone {multiBone.gameObject.name}: {ex.Message}");
                }
            }
        }
        
        // 초기화 후 적용 단계
        Debug.Log("[VModularManager] === 적용 단계 ===");
        
        // Single Bone 컴포넌트들 적용
        foreach (var singleBone in singleBoneComponents)
        {
                        if (singleBone != null)
            {
                if (!singleBone.gameObject.activeSelf)
                {
                    Debug.Log($"⚠️ Skipping inactive SingleBone: {singleBone.gameObject.name}");
                    skippedCount++;
                    continue;
                }
                
                Debug.Log($"[VModularManager] Processing SingleBone: {singleBone.gameObject.name}");
                
                try
                {
                    // 현재 상태 확인
                    bool wasWearing = singleBone.IsWearing;
                    Debug.Log($"  Current wearing state: {wasWearing}");
                    
                    // 착용하지 않은 상태라면 착용 적용
                    if (!wasWearing)
                    {
                        singleBone.WearAccessory();
                        bool nowWearing = singleBone.IsWearing;
                        
                        if (nowWearing)
                        {
                            Debug.Log($"✅ Applied SingleBone: {singleBone.gameObject.name}");
                            appliedCount++;
                        }
                        else
                        {
                            Debug.LogWarning($"⚠️ SingleBone application may have failed: {singleBone.gameObject.name}");
                        }
                    }
                    else
                    {
                        Debug.Log($"ℹ️ SingleBone already wearing: {singleBone.gameObject.name}");
                        appliedCount++; // 이미 착용된 것도 적용된 것으로 카운트
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"❌ Failed to apply SingleBone {singleBone.gameObject.name}: {ex.Message}");
                }
            }
        }
        
        // Multi Bone 컴포넌트들 적용
        foreach (var multiBone in multiBoneComponents)
        {
            if (multiBone != null)
            {
                if (!multiBone.gameObject.activeSelf)
                {
                    Debug.Log($"⚠️ Skipping inactive MultiBone: {multiBone.gameObject.name}");
                    skippedCount++;
                    continue;
                }
                
                Debug.Log($"[VModularManager] Processing MultiBone: {multiBone.gameObject.name}");
                
                try
                {
                    // 현재 상태 확인
                    bool wasWearing = multiBone.IsWearing;
                    Debug.Log($"  Current wearing state: {wasWearing}");
                    
                    // 착용하지 않은 상태라면 착용 적용
                    if (!wasWearing)
                    {
                        multiBone.WearOutfit();
                        bool nowWearing = multiBone.IsWearing;
                        
                        if (nowWearing)
                        {
                            Debug.Log($"✅ Applied MultiBone: {multiBone.gameObject.name}");
                            appliedCount++;
                        }
                        else
                        {
                            Debug.LogWarning($"⚠️ MultiBone application may have failed: {multiBone.gameObject.name}");
                        }
                    }
                    else
                    {
                        Debug.Log($"ℹ️ MultiBone already wearing: {multiBone.gameObject.name}");
                        appliedCount++; // 이미 착용된 것도 적용된 것으로 카운트
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"❌ Failed to apply MultiBone {multiBone.gameObject.name}: {ex.Message}");
                }
            }
        }
        
        string resultMessage = $"전체 적용 완료!\n\n" +
                              $"✅ 적용됨: {appliedCount} 컴포넌트\n" +
                              $"⏭️ 건너뛴: {skippedCount} 비활성화 컴포넌트\n" +
                              $"📊 총 발견: {singleBoneComponents.Count + multiBoneComponents.Count} 컴포넌트\n\n" +
                              $"자세한 로그는 콘솔을 확인하세요.";
        
        EditorUtility.DisplayDialog("전체 적용 완료", resultMessage, "확인");
        
        Debug.Log($"[VModularManager] === SUMMARY ===");
        Debug.Log($"전체 적용 완료 for: {targetAvatar.name}");
        Debug.Log($"적용됨: {appliedCount}, 건너뛴: {skippedCount}, 전체: {singleBoneComponents.Count + multiBoneComponents.Count}");
    }
    
    /// <summary>
    /// 백업 생성 (컴포넌트 상태 + 본 계층구조)
    /// </summary>
    private void CreateBackup()
    {
        if (targetAvatar == null)
        {
            Debug.LogError("[VModularManager] 아바타가 선택되지 않아 백업을 생성할 수 없습니다!");
            return;
        }
        
        var currentBackup = GetCurrentAvatarBackup();
        currentBackup.Clear();
        
        Debug.Log($"[VModularManager] === {targetAvatar.name} 백업 생성 시작 ===");
        
        // 먼저 모든 컴포넌트들을 초기화 (본 구조가 제대로 세팅되도록)
        Debug.Log("[VModularManager] 백업 전 컴포넌트 안전 초기화");
        foreach (var comp in singleBoneComponents)
        {
            if (comp != null)
            {
                try
                {
                    // Editor 안전한 Reflection 호출
                    var initMethod = typeof(VModularOneBone).GetMethod("Initialize", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (initMethod != null)
                    {
                        initMethod.Invoke(comp, null);
                        Debug.Log($"[VModularManager] SingleBone 초기화 성공: {comp.gameObject.name}");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[VModularManager] SingleBone 초기화 실패: {comp.gameObject.name} - {ex.Message}");
                }
            }
        }
        
        foreach (var comp in multiBoneComponents)
        {
            if (comp != null)
            {
                try
                {
                    // Editor 안전한 Reflection 호출 - Initialize
                    var initMethod = typeof(VModularMultiBone).GetMethod("Initialize", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (initMethod != null)
                    {
                        initMethod.Invoke(comp, null);
                        Debug.Log($"[VModularManager] MultiBone Initialize 성공: {comp.gameObject.name}");
                    }
                    
                    // Editor 안전한 Reflection 호출 - CollectOutfitParts
                    var collectMethod = typeof(VModularMultiBone).GetMethod("CollectOutfitParts", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (collectMethod != null)
                    {
                        collectMethod.Invoke(comp, null);
                        Debug.Log($"[VModularManager] MultiBone CollectOutfitParts 성공: {comp.gameObject.name}");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[VModularManager] MultiBone 초기화 실패: {comp.gameObject.name} - {ex.Message}");
                }
            }
        }
        
        // SingleBone 컴포넌트 상태 및 본 구조 백업
        foreach (var comp in singleBoneComponents)
        {
            if (comp != null)
            {
                var backupData = new ComponentBackupData();
                backupData.isWearing = comp.IsWearing;
                
                // 본 구조 백업
                BackupBoneStructure(comp, backupData);
                
                currentBackup[comp] = backupData;
            }
        }
        
        // MultiBone 컴포넌트 상태 및 본 구조 백업
        foreach (var comp in multiBoneComponents)
        {
            if (comp != null)
            {
                var backupData = new ComponentBackupData();
                backupData.isWearing = comp.IsWearing;
                
                // 본 구조 백업
                BackupBoneStructure(comp, backupData);
                
                currentBackup[comp] = backupData;
            }
        }
        
        avatarBackupStatus[targetAvatar] = true;
        Debug.Log($"[VModularManager] {targetAvatar.name} 백업 생성 완료: {currentBackup.Count}개 컴포넌트 상태 및 본 구조 저장");
    }
    
    /// <summary>
    /// 컴포넌트의 본 구조를 백업합니다
    /// </summary>
    private void BackupBoneStructure(Component component, ComponentBackupData backupData)
    {
        try
        {
            List<Transform> bonesToBackup = new List<Transform>();
            
            // VModularOneBone에서 본 정보 가져오기
            if (component is VModularOneBone singleBone)
            {
                // sourceAttachmentBone 필드 가져오기
                var sourceAttachmentBoneField = typeof(VModularOneBone).GetField("sourceAttachmentBone", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (sourceAttachmentBoneField != null)
                {
                    Transform sourceAttachmentBone = sourceAttachmentBoneField.GetValue(singleBone) as Transform;
                    if (sourceAttachmentBone != null)
                    {
                        bonesToBackup.Add(sourceAttachmentBone);
                        Debug.Log($"[VModularManager] SingleBone 본 발견: {sourceAttachmentBone.name}");
                    }
                }
            }
            // VModularMultiBone에서 본 정보 가져오기
            else if (component is VModularMultiBone multiBone)
            {
                // outfitParts 필드 가져오기 (착용 전 상태)
                var outfitPartsField = typeof(VModularMultiBone).GetField("outfitParts", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (outfitPartsField != null)
                {
                    List<Transform> outfitParts = outfitPartsField.GetValue(multiBone) as List<Transform>;
                    if (outfitParts != null && outfitParts.Count > 0)
                    {
                        bonesToBackup.AddRange(outfitParts.Where(part => part != null));
                        Debug.Log($"[VModularManager] MultiBone outfitParts에서 발견: {outfitParts.Count}개");
                    }
                    else
                    {
                        Debug.LogWarning($"[VModularManager] MultiBone {multiBone.gameObject.name}의 outfitParts가 비어있음");
                    }
                }
                
                // 만약 이미 착용된 상태라면 아바타에서 접미사가 붙은 파트들을 찾기
                if (bonesToBackup.Count == 0 && multiBone.IsWearing)
                {
                    Debug.Log($"[VModularManager] MultiBone {multiBone.gameObject.name}이 이미 착용된 상태 - 아바타에서 파트들 찾는 중...");
                    
                    // outfitGroupName 필드 가져오기
                    var outfitGroupNameField = typeof(VModularMultiBone).GetField("outfitGroupName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (outfitGroupNameField != null)
                    {
                        string outfitGroupName = outfitGroupNameField.GetValue(multiBone) as string;
                        if (!string.IsNullOrEmpty(outfitGroupName))
                        {
                            // 아마튜어에서 접미사가 붙은 파트들 찾기
                            var outfitArmatureField = typeof(VModularMultiBone).GetField("outfitArmature", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            if (outfitArmatureField != null)
                            {
                                Transform outfitArmature = outfitArmatureField.GetValue(multiBone) as Transform;
                                if (outfitArmature != null)
                                {
                                    string suffix = $"[{outfitGroupName}]";
                                    FindAttachedOutfitParts(outfitArmature, suffix, bonesToBackup);
                                    Debug.Log($"[VModularManager] 아바타에서 접미사 '{suffix}'가 붙은 파트 {bonesToBackup.Count}개 발견");
                                    
                                    // 아바타의 모든 본들도 이름 백업을 위해 추가 (접미사가 붙어있을 수 있음)
                                    List<Transform> avatarBones = new List<Transform>();
                                    GetAllChildBones(outfitArmature, avatarBones);
                                    // 중복 제거
                                    foreach (var avatarBone in avatarBones)
                                    {
                                        if (!bonesToBackup.Contains(avatarBone))
                                        {
                                            bonesToBackup.Add(avatarBone);
                                        }
                                    }
                                    Debug.Log($"[VModularManager] 아바타 본 {avatarBones.Count}개 이름 백업 대상에 추가");
                                }
                            }
                        }
                    }
                }
            }
            
            // 본 계층구조 정보 저장
            if (bonesToBackup.Count > 0)
            {
                foreach (var bone in bonesToBackup)
                {
                    if (bone != null)
                    {
                        backupData.boneParents[bone] = bone.parent;
                        backupData.boneSiblingIndices[bone] = bone.GetSiblingIndex();
                        backupData.originalNames[bone] = bone.name; // 원래 이름 백업 추가!
                        Debug.Log($"[VModularManager] 본 백업: {bone.name} -> 부모: {(bone.parent ? bone.parent.name : "null")}, 인덱스: {bone.GetSiblingIndex()}");
                    }
                }
                Debug.Log($"[VModularManager] {component.gameObject.name}: 총 {bonesToBackup.Count}개 본 백업 완료");
            }
            else
            {
                Debug.LogWarning($"[VModularManager] {component.gameObject.name}에서 본을 찾을 수 없습니다 - MultiBone 의상이 착용된 상태에서도 찾지 못함");
                
                // 디버그를 위해 컴포넌트 상태 출력
                if (component is VModularMultiBone multiBone)
                {
                    Debug.LogWarning($"[VModularManager] MultiBone 디버그 - IsWearing: {multiBone.IsWearing}");
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[VModularManager] 본 구조 백업 중 오류 ({component.gameObject.name}): {ex.Message}");
        }
    }
    
    /// <summary>
    /// 아바타 아마튜어에서 특정 접미사가 붙은 의상 파트들을 재귀적으로 찾기
    /// (VModularMultiBone.CollectAttachedOutfitParts와 동일한 기능)
    /// </summary>
    private void FindAttachedOutfitParts(Transform parent, string suffix, List<Transform> foundParts)
    {
        if (parent == null) return;
        
        foreach (Transform child in parent)
        {
            if (child.name.Contains(suffix))
            {
                foundParts.Add(child);
                Debug.Log($"[VModularManager] 접미사 '{suffix}' 파트 발견: {child.name}");
            }
            
            // 재귀적으로 자식들도 확인
            FindAttachedOutfitParts(child, suffix, foundParts);
        }
    }
    
    /// <summary>
    /// MultiBone 의상 파트들을 원래 위치로 복원
    /// (VModularMultiBone의 주석처리된 본 벗기기 기능을 직접 구현)
    /// </summary>
    /// <returns>복원된 파트 개수</returns>
    private int RestoreMultiBoneParts(VModularMultiBone multiBone)
    {
        try
        {
            // outfitGroupName과 outfitArmature 필드 가져오기
            var outfitGroupNameField = typeof(VModularMultiBone).GetField("outfitGroupName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var outfitArmatureField = typeof(VModularMultiBone).GetField("outfitArmature", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (outfitGroupNameField == null || outfitArmatureField == null)
            {
                Debug.LogError($"[VModularManager] MultiBone 필드를 찾을 수 없습니다: {multiBone.gameObject.name}");
                return 0;
            }
            
            string outfitGroupName = outfitGroupNameField.GetValue(multiBone) as string;
            Transform outfitArmature = outfitArmatureField.GetValue(multiBone) as Transform;
            
            if (string.IsNullOrEmpty(outfitGroupName) || outfitArmature == null)
            {
                Debug.LogError($"[VModularManager] MultiBone 필수 데이터가 없습니다: {multiBone.gameObject.name} (그룹명: {outfitGroupName}, 아마튜어: {outfitArmature?.name})");
                return 0;
            }
            
            Debug.Log($"[VModularManager] MultiBone 본 복원 시작: {multiBone.gameObject.name} (그룹: {outfitGroupName})");
            
            // VModularMultiBone의 주석처리된 로직과 동일하게 구현
            string suffixToFind = $"[{outfitGroupName}]";
            List<Transform> attachedParts = new List<Transform>();
            
            // 아마튜어에서 접미사가 붙은 의상 파트들 찾기
            FindAttachedOutfitParts(outfitArmature, suffixToFind, attachedParts);
            
            Debug.Log($"[VModularManager] 아마튜어에서 {attachedParts.Count}개의 부착된 의상 파트 발견");
            
            int restoredParts = 0;
            
            // 스마트 계층구조 복원: 이름 패턴을 분석하여 원래 구조 재구성
            var restoredHierarchy = ReconstructOriginalHierarchy(attachedParts, multiBone.transform);
            
            foreach (Transform attachedPart in attachedParts)
            {
                if (attachedPart == null) continue;
                
                try
                {
                    // 접미사 제거 (이름 정리)
                    RemoveOutfitSuffix(attachedPart);
                    
                    // 재구성된 계층구조에서 이 파트의 적절한 부모 찾기
                    Transform targetParent = FindAppropriateParent(attachedPart, restoredHierarchy);
                    
                    if (targetParent != null)
                    {
                        attachedPart.SetParent(targetParent, false);
                        Debug.Log($"[VModularManager] ✅ {attachedPart.name} 스마트 복원: {targetParent.name} 하위로 배치");
            }
            else
            {
                        // 적절한 부모를 찾을 수 없으면 multiBone 바로 아래에 배치
                        attachedPart.SetParent(multiBone.transform, false);
                        Debug.Log($"[VModularManager] ⚠️ {attachedPart.name} 기본 위치로 복원");
                    }
                    
                    restoredParts++;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[VModularManager] ❌ {attachedPart.name} 복원 실패: {ex.Message}");
                }
            }
            
            // 아바타 아마튜어의 본들에서도 접미사 제거 (Hips[Shinano] → Hips)
            Debug.Log($"[VModularManager] 아바타 본 접미사 정리 시작: {outfitArmature.name}");
            RemoveOutfitSuffix(outfitArmature);
            
            Debug.Log($"[VModularManager] MultiBone 본 복원 완료: {multiBone.gameObject.name} ({restoredParts}/{attachedParts.Count}개 파트 복원, 아바타 본 정리 완료)");
            return restoredParts;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[VModularManager] MultiBone 본 복원 중 오류 ({multiBone.gameObject.name}): {ex.Message}");
            return 0;
        }
    }
    
    /// <summary>
    /// 파트 이름 분석을 통해 원래 계층구조를 재구성
    /// </summary>
    private Dictionary<string, Transform> ReconstructOriginalHierarchy(List<Transform> parts, Transform multiBoneRoot)
    {
        var hierarchy = new Dictionary<string, Transform>();
        var createdFolders = new Dictionary<string, Transform>();
        
        try
        {
            // 파트 이름들을 분석하여 그룹화
            var groups = new Dictionary<string, List<Transform>>();
            var rootParts = new List<Transform>();
            
            foreach (var part in parts)
            {
                if (part == null) continue;
                
                string cleanName = GetCleanPartName(part.name);
                
                // 루트 파트들 식별 (Root 접미사가 있는 것들)
                if (cleanName.EndsWith("_Root") || cleanName.EndsWith("Root"))
                {
                    rootParts.Add(part);
                }
                
                // 그룹 접두사 추출 (예: Hair_, Front_, Long_Wave 등)
                string groupPrefix = ExtractGroupPrefix(cleanName);
                
                if (!groups.ContainsKey(groupPrefix))
                {
                    groups[groupPrefix] = new List<Transform>();
                }
                groups[groupPrefix].Add(part);
            }
            
            Debug.Log($"[VModularManager] 파트 그룹 분석 결과: {groups.Count}개 그룹, {rootParts.Count}개 루트 파트");
            
            // 필요한 폴더 구조 생성
            foreach (var group in groups)
            {
                string groupName = group.Key;
                var groupParts = group.Value;
                
                if (groupParts.Count <= 1) continue; // 단일 파트는 폴더 불필요
                
                // 그룹 폴더 생성 또는 기존 것 찾기
                Transform groupFolder = FindOrCreateGroupFolder(groupName, multiBoneRoot, createdFolders);
                if (groupFolder != null)
                {
                    foreach (var part in groupParts)
                    {
                        hierarchy[GetCleanPartName(part.name)] = groupFolder;
                    }
                }
            }
            
            Debug.Log($"[VModularManager] 생성된 그룹 폴더: {createdFolders.Count}개");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[VModularManager] 계층구조 재구성 중 오류: {ex.Message}");
        }
        
        return hierarchy;
    }
    
    /// <summary>
    /// 파트 이름에서 그룹 접두사 추출
    /// </summary>
    private string ExtractGroupPrefix(string partName)
    {
        // 예: Long_Wave.001 -> Long_Wave, Ahoge.002 -> Ahoge, Front -> Hair
        
        // 번호 제거 (.001, .002 등)
        string baseName = System.Text.RegularExpressions.Regex.Replace(partName, @"\.\d{3}$", "");
        
        // 공통 패턴 분석
        if (baseName.Contains("Hair") || baseName.Contains("Long_Wave") || baseName.Contains("Ahoge"))
            return "Hair";
        if (baseName.Contains("Front"))
            return "Hair";
        if (baseName.Contains("Back"))
            return "Hair";
        if (baseName.Contains("Side"))
            return "Hair";
        if (baseName.Contains("Bun"))
            return "Hair";
        if (baseName.Contains("ACC"))
            return "Accessories";
        
        // 기본적으로 첫 번째 언더스코어까지를 접두사로 사용
        int underscoreIndex = baseName.IndexOf('_');
        if (underscoreIndex > 0)
        {
            return baseName.Substring(0, underscoreIndex);
        }
        
        return "Parts"; // 기본 그룹
    }
    
    /// <summary>
    /// 그룹 폴더를 찾거나 생성
    /// </summary>
    private Transform FindOrCreateGroupFolder(string groupName, Transform parent, Dictionary<string, Transform> createdFolders)
    {
        if (createdFolders.ContainsKey(groupName))
        {
            return createdFolders[groupName];
        }
        
        // 적절한 폴더 이름 결정
        string folderName = groupName + "_Parts";
        if (groupName == "Hair") folderName = "Hair_Parts";
        else if (groupName == "Accessories") folderName = "ACC_Parts";
        
        // 기존 폴더가 있는지 확인
        Transform existingFolder = parent.Find(folderName);
        if (existingFolder != null)
        {
            createdFolders[groupName] = existingFolder;
            return existingFolder;
        }
        
        // 새 폴더 생성
        GameObject newFolder = new GameObject(folderName);
        newFolder.transform.SetParent(parent, false);
        
        createdFolders[groupName] = newFolder.transform;
        Debug.Log($"[VModularManager] 그룹 폴더 생성: {folderName}");
        
        return newFolder.transform;
    }
    
    /// <summary>
    /// 파트에 대한 적절한 부모 찾기
    /// </summary>
    private Transform FindAppropriateParent(Transform part, Dictionary<string, Transform> hierarchy)
    {
        if (part == null) return null;
        
        string cleanName = GetCleanPartName(part.name);
        
        if (hierarchy.ContainsKey(cleanName))
        {
            return hierarchy[cleanName];
        }
        
        // 부분 매칭 시도 (번호가 제거된 이름으로)
        string baseName = System.Text.RegularExpressions.Regex.Replace(cleanName, @"\.\d{3}$", "");
        if (hierarchy.ContainsKey(baseName))
        {
            return hierarchy[baseName];
        }
        
        return null;
    }
    
    /// <summary>
    /// Transform의 모든 자식 본들을 재귀적으로 수집
    /// </summary>
    private void GetAllChildBones(Transform parent, List<Transform> bonesList)
    {
        if (parent == null) return;
        
        // 자기 자신도 추가
        if (!bonesList.Contains(parent))
        {
            bonesList.Add(parent);
        }
        
        // 모든 자식들을 재귀적으로 추가
        foreach (Transform child in parent)
        {
            GetAllChildBones(child, bonesList);
        }
    }
    
    /// <summary>
    /// 파트 이름에서 접미사를 완전히 제거하고 깨끗한 이름 추출 (문자열 직접 조작)
    /// </summary>
    private string GetCleanPartName(string partName)
    {
        if (string.IsNullOrEmpty(partName)) return "";
        
        string cleaned = partName;
        
        // 강제 접미사 제거 - 문자열 직접 조작
        // 대괄호 제거 [Shinano] → 빈 문자열
        while (cleaned.Contains("[") && cleaned.Contains("]"))
        {
            int startBracket = cleaned.IndexOf('[');
            int endBracket = cleaned.IndexOf(']');
            if (startBracket >= 0 && endBracket > startBracket)
            {
                cleaned = cleaned.Remove(startBracket, endBracket - startBracket + 1);
            }
            else
            {
                break;
            }
        }
        
        // 소괄호 제거 (GroupName) → 빈 문자열
        while (cleaned.Contains("(") && cleaned.Contains(")"))
        {
            int startParen = cleaned.IndexOf('(');
            int endParen = cleaned.IndexOf(')');
            if (startParen >= 0 && endParen > startParen)
            {
                cleaned = cleaned.Remove(startParen, endParen - startParen + 1);
            }
            else
            {
                break;
            }
        }
        
        // _Mesh 접미사 제거
        if (cleaned.EndsWith("_Mesh"))
        {
            cleaned = cleaned.Substring(0, cleaned.Length - 5);
        }
        else if (cleaned.EndsWith("_mesh"))
        {
            cleaned = cleaned.Substring(0, cleaned.Length - 5);
        }
        
        // 공백 제거
        cleaned = cleaned.Trim();
        
        // 빈 이름 방지
        if (string.IsNullOrEmpty(cleaned))
        {
            cleaned = "Part_" + partName.GetHashCode().ToString();
        }
        
        return cleaned;
    }
    
    /// <summary>
    /// 주어진 Transform이 MultiBone의 계층구조 안에 있는지 확인
    /// (아바타의 본이 아닌 의상 자체의 구조인지 판별)
    /// </summary>
    private bool IsWithinMultiBoneHierarchy(Transform target, Transform multiBoneRoot)
    {
        if (target == null || multiBoneRoot == null) return false;
        
        Transform current = target;
        while (current != null)
        {
            if (current == multiBoneRoot) return true;
            current = current.parent;
        }
        return false;
    }
    
    /// <summary>
    /// 의상 파트의 접미사를 강제로 완전히 제거 (문자열 직접 조작 방식)
    /// 재귀적으로 모든 자식에서 접미사를 제거하여 원래 상태로 복원
    /// </summary>
    private void RemoveOutfitSuffix(Transform target)
    {
        if (target == null) return;
        
        string originalName = target.name;
        string cleanedName = originalName;
        
        // 방법 1: 대괄호 접미사 강제 제거 [Shinano] → 빈 문자열
        while (cleanedName.Contains("[") && cleanedName.Contains("]"))
        {
            int startBracket = cleanedName.IndexOf('[');
            int endBracket = cleanedName.IndexOf(']');
            if (startBracket >= 0 && endBracket > startBracket)
            {
                // [Shinano] 부분을 완전히 제거
                cleanedName = cleanedName.Remove(startBracket, endBracket - startBracket + 1);
                Debug.Log($"[VModularManager] 🔍 대괄호 제거 중간: '{originalName}' → '{cleanedName}'");
            }
            else
            {
                break; // 무한루프 방지
            }
        }
        
        // 방법 2: 소괄호 접미사 강제 제거 (GroupName) → 빈 문자열
        while (cleanedName.Contains("(") && cleanedName.Contains(")"))
        {
            int startParen = cleanedName.IndexOf('(');
            int endParen = cleanedName.IndexOf(')');
            if (startParen >= 0 && endParen > startParen)
            {
                cleanedName = cleanedName.Remove(startParen, endParen - startParen + 1);
            }
            else
            {
                break; // 무한루프 방지
            }
        }
        
        // 방법 3: _Mesh, _mesh 접미사 제거
        if (cleanedName.EndsWith("_Mesh"))
        {
            cleanedName = cleanedName.Substring(0, cleanedName.Length - 5);
        }
        else if (cleanedName.EndsWith("_mesh"))
        {
            cleanedName = cleanedName.Substring(0, cleanedName.Length - 5);
        }
        
        // 방법 4: 공백과 특수문자 정리
        cleanedName = cleanedName.Trim();
        
        // 방법 5: 빈 이름 방지
        if (string.IsNullOrEmpty(cleanedName))
        {
            cleanedName = "Restored_" + originalName.GetHashCode().ToString();
        }
        
        // 강제로 이름 적용
        target.name = cleanedName;
        
        // 변경사항 무조건 로그 출력
        if (originalName != cleanedName)
        {
            Debug.Log($"[VModularManager] ✂️ 접미사 강제 제거: '{originalName}' → '{cleanedName}'");
        }
        else if (originalName.Contains("[") || originalName.Contains("("))
        {
            Debug.LogError($"[VModularManager] ❌ 접미사 제거 완전 실패: '{originalName}' (변화 없음!)");
        }
        
        // 모든 자식들에게 재귀적으로 적용
        foreach (Transform child in target)
        {
            RemoveOutfitSuffix(child);
        }
    }
    
    
    
    /// <summary>
    /// 백업에서 복원 (본 계층구조 먼저 복원 후 컴포넌트 상태 복원)
    /// </summary>
    private void RestoreFromBackup()
    {
        if (!HasCurrentAvatarBackup)
        {
            EditorUtility.DisplayDialog("오류", "현재 아바타에 대한 백업된 상태가 없습니다!", "확인");
            return;
        }
        
        var currentBackup = GetCurrentAvatarBackup();
        if (currentBackup.Count == 0)
        {
            EditorUtility.DisplayDialog("오류", "백업된 데이터가 없습니다!", "확인");
            return;
        }
        
        Debug.Log($"[VModularManager] === {targetAvatar.name} 백업에서 복원 시작 ===");
        
        int restoredCount = 0;
        int failedCount = 0;
        int bonesRestored = 0;
        
        // 1단계: 모든 본들을 원래 계층구조로 복원
        Debug.Log("[VModularManager] === 1단계: 본 계층구조 복원 ===");
        foreach (var kvp in currentBackup)
        {
            var component = kvp.Key;
            var backupData = kvp.Value;
            
            if (component == null) continue;
            
            try
            {
                // 본 계층구조 복원
                foreach (var boneKvp in backupData.boneParents)
                {
                    var bone = boneKvp.Key;
                    var originalParent = boneKvp.Value;
                    
                    if (bone != null)
                    {
                        // 원래 이름 복원 (항상 실행)
                        if (backupData.originalNames.ContainsKey(bone))
                        {
                            string originalName = backupData.originalNames[bone];
                            if (bone.name != originalName)
                            {
                                string currentName = bone.name;
                                bone.name = originalName;
                                Debug.Log($"🏷️ 이름 복원: '{currentName}' -> '{originalName}'");
                            }
                        }
                        
                        // 현재 부모와 원래 부모가 다른 경우에만 계층구조 복원
                        if (bone.parent != originalParent)
                        {
                            bone.SetParent(originalParent);
                            
                            // sibling index도 복원
                            if (backupData.boneSiblingIndices.ContainsKey(bone))
                            {
                                bone.SetSiblingIndex(backupData.boneSiblingIndices[bone]);
                            }
                            
                            bonesRestored++;
                            Debug.Log($"✅ 본 복원: {bone.name} -> 부모: {(originalParent ? originalParent.name : "null")}");
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ Failed to restore bone structure for {component.gameObject.name}: {ex.Message}");
            }
        }
        
        Debug.Log($"[VModularManager] 본 계층구조 복원 완료: {bonesRestored}개 본 복원됨");
        
        // 2단계: 컴포넌트 상태 복원 (착용/해제 상태)
        Debug.Log("[VModularManager] === 2단계: 컴포넌트 상태 복원 ===");
        foreach (var kvp in currentBackup)
        {
            var component = kvp.Key;
            var backupData = kvp.Value;
            bool originalState = backupData.isWearing;
            
            if (component == null) continue;
            
            try
            {
                if (component is VModularOneBone singleBone)
                {
                    if (originalState && !singleBone.IsWearing)
                    {
                        singleBone.WearAccessory();
                        Debug.Log($"✅ SingleBone 착용 복원: {singleBone.gameObject.name}");
                    }
                    else if (!originalState && singleBone.IsWearing)
                    {
                        singleBone.RemoveAccessory();
                        Debug.Log($"✅ SingleBone 해제 복원: {singleBone.gameObject.name}");
                    }
                    else
                    {
                        Debug.Log($"ℹ️ SingleBone 상태 변경 불필요: {singleBone.gameObject.name} (원래: {originalState}, 현재: {singleBone.IsWearing})");
                    }
                    restoredCount++;
                }
                else if (component is VModularMultiBone multiBone)
                {
                    // MultiBone의 경우 RemoveOutfit()이 본을 벗기지 않으므로 특별 처리
                    if (originalState && !multiBone.IsWearing)
                    {
                        multiBone.WearOutfit();
                        Debug.Log($"✅ MultiBone 착용 복원: {multiBone.gameObject.name}");
                    }
                    else if (!originalState && multiBone.IsWearing)
                    {
                        // 1. RemoveOutfit()으로 블렌드쉐이프 효과 제거
                        multiBone.RemoveOutfit();
                        Debug.Log($"🔄 MultiBone 블렌드쉐이프 해제: {multiBone.gameObject.name}");
                        
                        // 2. 본 구조 수동 복원 (VModularMultiBone의 주석처리된 로직 구현)
                        int partsRestored = RestoreMultiBoneParts(multiBone);
                        bonesRestored += partsRestored;
                    }
                    else
                    {
                        Debug.Log($"ℹ️ MultiBone 상태 변경 불필요: {multiBone.gameObject.name} (원래: {originalState}, 현재: {multiBone.IsWearing})");
                    }
                    restoredCount++;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ Failed to restore component state for {component.gameObject.name}: {ex.Message}");
                failedCount++;
            }
        }
        
        // 추가 안전장치: 모든 MultiBone의 아바타 본 접미사 최종 정리
        Debug.Log($"[VModularManager] === 아바타 본 최종 정리 시작 ===");
        foreach (var multiBone in multiBoneComponents)
        {
            if (multiBone == null) continue;
            
            try 
            {
                var outfitArmatureField = typeof(VModularMultiBone).GetField("outfitArmature", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (outfitArmatureField != null)
                {
                    Transform outfitArmature = outfitArmatureField.GetValue(multiBone) as Transform;
                    if (outfitArmature != null)
                    {
                        Debug.Log($"[VModularManager] 🧹 아바타 본 최종 정리: {outfitArmature.name}");
                        RemoveOutfitSuffix(outfitArmature);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[VModularManager] 아바타 본 정리 중 경고 ({multiBone.gameObject.name}): {ex.Message}");
            }
        }
        Debug.Log($"[VModularManager] === 아바타 본 최종 정리 완료 ===");
        
        string resultMessage = $"전체 복원 완료!\n\n" +
                              $"🦴 본 복원: {bonesRestored}개 (백업: {currentBackup.Sum(kvp => kvp.Value.boneParents.Count)}개)\n" +
                              $"✅ 컴포넌트 복원: {restoredCount}개\n" +
                              $"❌ 실패: {failedCount}개\n\n" +
                              $"🧹 아바타 본 정리: 완료\n\n" +
                              $"자세한 로그는 콘솔을 확인하세요.";
        
        EditorUtility.DisplayDialog("복원 완료", resultMessage, "확인");
        
        Debug.Log($"[VModularManager] === 복원 SUMMARY ===");
        Debug.Log($"본 복원: {bonesRestored}개, 백업된 본: {currentBackup.Sum(kvp => kvp.Value.boneParents.Count)}개");
        Debug.Log($"컴포넌트 복원: {restoredCount}, 실패: {failedCount}, 총 백업: {currentBackup.Count}");
}

#endregion
}