using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(VModularOneBone))]
public class VModularOneBoneEditor : Editor
{
    private VModularOneBone targetScript;
    
    // SerializedProperties
    private SerializedProperty groupName;
    private SerializedProperty targetHumanBone;
    private SerializedProperty sourceAttachmentBone;
    private SerializedProperty useParentBasedScaling;
    
    // Foldout states
    private bool showAccessorySettings = true;
    private bool showDebugInfo = false;
    
    
    void OnEnable()
    {
        targetScript = (VModularOneBone)target;
        
        // SerializedProperties 초기화
        groupName = serializedObject.FindProperty("groupName");
        targetHumanBone = serializedObject.FindProperty("targetHumanBone");
        sourceAttachmentBone = serializedObject.FindProperty("sourceAttachmentBone");
        useParentBasedScaling = serializedObject.FindProperty("useParentBasedScaling");
    }
    
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        
        // 헤더 그리기
        DrawHeader();
        
        GUILayout.Space(3);
        
        // 언어 선택기 (충분한 공간 확보)
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("언어 / Language", EditorStyles.boldLabel, GUILayout.Width(120));
        SupportedLanguage newLang = (SupportedLanguage)EditorGUILayout.EnumPopup(VModularLocalization.CurrentLanguage, GUILayout.MinWidth(100));
        if (newLang != VModularLocalization.CurrentLanguage)
        {
            VModularLocalization.CurrentLanguage = newLang;
        }
        EditorGUILayout.EndHorizontal();
        
        GUILayout.Space(5);
        
        // 주의사항 및 그룹명 섹션
        DrawNoticeAndGroupSection();
        
        GUILayout.Space(5);
        
        // 스케일 설정 섹션
        DrawScaleSettingsSection();
        
        GUILayout.Space(5);
        
        // 본 부착 설정 섹션
        DrawBoneAttachmentSection();
        
        GUILayout.Space(5);
        
        // 액션 버튼과 디버그 도구 제거됨
        
        serializedObject.ApplyModifiedProperties();
    }
    
    void DrawHeader()
    {
        // Studio Raming 브랜딩 헤더
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();
        
        // 제품 타이틀
        EditorGUILayout.LabelField("💎 " + VModularLocalization.Get("onebone_title"), EditorStyles.largeLabel);
        
        // 우측에 브랜딩 정보
        GUILayout.FlexibleSpace();
        EditorGUILayout.BeginVertical();
        GUILayout.Space(2);
        EditorGUILayout.LabelField("Studio Raming", EditorStyles.miniLabel, GUILayout.Width(80));
        EditorGUILayout.LabelField("v1.0.0", EditorStyles.miniLabel, GUILayout.Width(80));
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        
        GUILayout.Space(5);
    }
    
    void DrawNoticeAndGroupSection()
    {
        
        // === 그룹명 ===
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("💍 " + VModularLocalization.Get("group_name"), EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(groupName, GUIContent.none);
        EditorGUILayout.EndVertical();
    }
    
    void DrawStatusSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("📊 " + VModularLocalization.Get("status"), EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        // 착용 상태
        bool isWearing = Application.isPlaying && targetScript != null && targetScript.IsWearing;
        string statusText = isWearing ? VModularLocalization.Get("wearing") : VModularLocalization.Get("not_wearing");
        EditorGUILayout.LabelField("💍 " + statusText, EditorStyles.label);
        
        // 초기화 상태
        bool isInitialized = targetScript != null && sourceAttachmentBone.objectReferenceValue != null;
        string initText = isInitialized ? VModularLocalization.Get("initialized") : VModularLocalization.Get("not_initialized");
        EditorGUILayout.LabelField("⚙️ " + initText, EditorStyles.label);
        
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }
    
    void DrawBoneAttachmentSection()
    {
        // 항상 펼쳐진 상태의 본 어태치먼트 섹션
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        // 섹션 헤더
        EditorGUILayout.LabelField("🔗 " + VModularLocalization.Get("bone_attachment"), EditorStyles.boldLabel);
        
        EditorGUILayout.Space(5);
        
        // Target Humanoid Bone
        EditorGUILayout.LabelField("🦴 " + VModularLocalization.Get("target_humanoid_bone"));
        EditorGUILayout.PropertyField(targetHumanBone, GUIContent.none);
        EditorGUILayout.HelpBox(VModularLocalization.Get("target_humanoid_bone_desc"), MessageType.Info);
        
        GUILayout.Space(10);
        
        // Source Attachment Bone
        EditorGUILayout.LabelField("📌 " + VModularLocalization.Get("source_attachment_bone"));
        EditorGUILayout.PropertyField(sourceAttachmentBone, GUIContent.none);
        EditorGUILayout.HelpBox(VModularLocalization.Get("source_attachment_bone_desc"), MessageType.Info);
        
        // 본 정보 미리보기
        if (Application.isPlaying && targetScript != null)
        {
            DrawBonePreview();
        }
        
        EditorGUILayout.EndVertical();
    }
    
    void DrawScaleSettingsSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        // 섹션 헤더와 토글 버튼
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("⚖️ " + VModularLocalization.Get("scale_settings"), EditorStyles.boldLabel, GUILayout.Width(200));
        
        GUILayout.FlexibleSpace();
        
        // ON/OFF 토글 버튼
        string toggleText = useParentBasedScaling.boolValue ? "ON" : "OFF";
        Color originalBg = GUI.backgroundColor;
        GUI.backgroundColor = useParentBasedScaling.boolValue ? new Color(0.4f, 0.8f, 0.4f, 1f) : new Color(0.8f, 0.4f, 0.4f, 1f);
        
        if (GUILayout.Button(toggleText, GUILayout.Width(50), GUILayout.Height(20)))
        {
            useParentBasedScaling.boolValue = !useParentBasedScaling.boolValue;
        }
        
        GUI.backgroundColor = originalBg;
        EditorGUILayout.EndHorizontal();
        
        // 설명 텍스트
        EditorGUILayout.Space(3);
        string descText = useParentBasedScaling.boolValue ?
            VModularLocalization.Get("scale_desc_on") :
            VModularLocalization.Get("scale_desc_off");
        
        EditorGUILayout.HelpBox(descText, MessageType.Info);
        
        EditorGUILayout.EndVertical();
    }
    
    void DrawBonePreview()
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("🔍 Bone Preview", EditorStyles.boldLabel);
        
        EditorGUI.BeginDisabledGroup(true);
        
        // 타겟 애니메이터 정보
        var targetAnimator = (System.Reflection.FieldInfo)typeof(VModularOneBone)
            .GetField("targetAnimator", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (targetAnimator != null)
        {
            var animator = targetAnimator.GetValue(targetScript) as Animator;
            EditorGUILayout.ObjectField("Target Animator", animator, typeof(Animator), true);
        }
        
        // 타겟 본 정보
        var targetBoneField = (System.Reflection.FieldInfo)typeof(VModularOneBone)
            .GetField("targetBone", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (targetBoneField != null)
        {
            var targetBone = targetBoneField.GetValue(targetScript) as Transform;
            EditorGUILayout.ObjectField("Target Bone", targetBone, typeof(Transform), true);
        }
        
        EditorGUI.EndDisabledGroup();
    }
    
    void DrawAccessorySettingsSection()
    {
        // 명확한 폴드아웃 텍스트 (배경색 제거)
        var foldoutStyle = new GUIStyle(EditorStyles.foldout);
        foldoutStyle.fontStyle = FontStyle.Bold;
        foldoutStyle.fontSize = 11;
        foldoutStyle.normal.textColor = Color.black;
        foldoutStyle.focused.textColor = Color.black;
        foldoutStyle.active.textColor = Color.black;
        
        showAccessorySettings = EditorGUILayout.Foldout(showAccessorySettings, "💍 " + VModularLocalization.Get("accessory_settings"), true, foldoutStyle);
        
        if (showAccessorySettings)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUI.indentLevel++;
            
            // Group Name
            EditorGUILayout.LabelField("🏷️ " + VModularLocalization.Get("group_name"));
            EditorGUILayout.PropertyField(groupName, GUIContent.none);
            
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }
    }
    
    void DrawActionButtons()
    {
        EditorGUILayout.BeginHorizontal();
        
        // Wear Accessory 버튼
        if (GUILayout.Button("💍 " + VModularLocalization.Get("wear_accessory"), GUI.skin.button))
        {
            if (Application.isPlaying && targetScript != null)
            {
                targetScript.WearAccessory();
            }
            else
            {
                Debug.LogWarning(VModularLocalization.Get("play_mode_required"));
            }
        }
        
        // Remove Accessory 버튼
        if (GUILayout.Button("🚫 " + VModularLocalization.Get("remove_accessory"), GUI.skin.button))
        {
            if (Application.isPlaying && targetScript != null)
            {
                targetScript.RemoveAccessory();
            }
            else
            {
                Debug.LogWarning(VModularLocalization.Get("play_mode_required"));
            }
        }
        
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
        
        // Initialize 버튼 (별도 줄)
        if (GUILayout.Button("⚙️ " + VModularLocalization.Get("initialize"), GUI.skin.button))
        {
            if (Application.isPlaying && targetScript != null)
            {
                targetScript.ForceInitialize();
            }
            else
            {
                Debug.LogWarning(VModularLocalization.Get("play_mode_required"));
            }
        }
    }
    
    void DrawDebugSection()
    {
        var foldoutRect = GUILayoutUtility.GetRect(0, 20, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(foldoutRect, new Color(0.7f, 0.7f, 0.7f, 0.5f));
        showDebugInfo = EditorGUI.Foldout(foldoutRect, showDebugInfo, "🐛 " + VModularLocalization.Get("debug"), true);
        
        if (showDebugInfo)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Debug Info 버튼
            if (GUILayout.Button("📊 Debug Info", GUILayout.Height(25)))
            {
                if (Application.isPlaying && targetScript != null)
                {
                    targetScript.DebugInfo();
                }
            }
            
            // Test 버튼들
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("🧪 " + VModularLocalization.Get("test") + " Wear"))
            {
                if (Application.isPlaying && targetScript != null)
                {
                    targetScript.WearAccessory();
                }
            }
            if (GUILayout.Button("🧪 " + VModularLocalization.Get("test") + " Remove"))
            {
                if (Application.isPlaying && targetScript != null)
                {
                    targetScript.RemoveAccessory();
                }
            }
            EditorGUILayout.EndHorizontal();
            
            // Toggle 버튼
            if (GUILayout.Button("🔄 Toggle", GUILayout.Height(25)))
            {
                if (Application.isPlaying && targetScript != null)
                {
                    targetScript.Toggle();
                }
            }
            
            EditorGUILayout.EndVertical();
        }
    }
    
    // Scene view에서 본 위치 표시
    void OnSceneGUI()
    {
        if (!Application.isPlaying || targetScript == null) return;
        
        // 타겟 본과 소스 본의 위치를 Scene view에 표시
        var targetBoneField = typeof(VModularOneBone)
            .GetField("targetBone", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (targetBoneField != null)
        {
            var targetBone = targetBoneField.GetValue(targetScript) as Transform;
            if (targetBone != null)
            {
                // 타겟 본 위치에 라벨 표시
                Handles.Label(targetBone.position + Vector3.up * 0.1f, "Target Bone", EditorStyles.boldLabel);
                Handles.color = Color.green;
                Handles.SphereHandleCap(0, targetBone.position, Quaternion.identity, 0.05f, EventType.Repaint);
            }
        }
        
        if (sourceAttachmentBone.objectReferenceValue != null)
        {
            var sourceBone = sourceAttachmentBone.objectReferenceValue as Transform;
            if (sourceBone != null)
            {
                // 소스 본 위치에 라벨 표시
                Handles.Label(sourceBone.position + Vector3.up * 0.1f, "Source Bone", EditorStyles.boldLabel);
                Handles.color = Color.blue;
                Handles.SphereHandleCap(0, sourceBone.position, Quaternion.identity, 0.05f, EventType.Repaint);
            }
        }
    }
}
