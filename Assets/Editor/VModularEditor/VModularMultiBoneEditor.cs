using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(VModularMultiBone))]
public class VModularMultiBoneEditor : Editor
{
    private VModularMultiBone targetScript;
    
    // SerializedProperties
    private SerializedProperty outfitGroupName;
    private SerializedProperty useBlendShapeOverride;
    private SerializedProperty useParentBasedScaling;
    private SerializedProperty targetBodyRenderer;
    private SerializedProperty outfitBlendShapes;
    private SerializedProperty objectsToHideWhenWearing;
    
    // ReorderableList
    private ReorderableList blendShapesList;
    private ReorderableList hideObjectsList;
    
    // Foldout states
    private bool showBlendShapes = true;
    private bool showObjectsToToggle = true;
    private bool showDebugInfo = false;
    
    
    void OnEnable()
    {
        targetScript = (VModularMultiBone)target;
        
        // SerializedProperties 초기화
        outfitGroupName = serializedObject.FindProperty("outfitGroupName");
        useBlendShapeOverride = serializedObject.FindProperty("useBlendShapeOverride");
        useParentBasedScaling = serializedObject.FindProperty("useParentBasedScaling");
        targetBodyRenderer = serializedObject.FindProperty("targetBodyRenderer");
        outfitBlendShapes = serializedObject.FindProperty("outfitBlendShapes");
        objectsToHideWhenWearing = serializedObject.FindProperty("objectsToHideWhenWearing");
        
        // ReorderableList 초기화
        SetupBlendShapesList();
        SetupHideObjectsList();
        
    }
    
    
    void SetupBlendShapesList()
    {
        blendShapesList = new ReorderableList(serializedObject, outfitBlendShapes, true, true, true, true);
        
        blendShapesList.drawHeaderCallback = null; // 헤더 제거하여 "1/10" 표시 없애기
        
        blendShapesList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
            var element = blendShapesList.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;
            rect.height = EditorGUIUtility.singleLineHeight;
            
            var shapeKeyName = element.FindPropertyRelative("shapeKeyName");
            var weight = element.FindPropertyRelative("weight");
            
            // Shape Key Name (60% width)
            Rect nameRect = new Rect(rect.x, rect.y, rect.width * 0.6f - 5, rect.height);
            EditorGUI.PropertyField(nameRect, shapeKeyName, GUIContent.none);
            
            // Weight (40% width)
            Rect weightRect = new Rect(rect.x + rect.width * 0.6f, rect.y, rect.width * 0.4f, rect.height);
            EditorGUI.PropertyField(weightRect, weight, GUIContent.none);
        };
        
        blendShapesList.onAddCallback = (ReorderableList list) => {
            list.serializedProperty.arraySize++;
            var element = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
            element.FindPropertyRelative("shapeKeyName").stringValue = "";
            element.FindPropertyRelative("weight").floatValue = 0f;
        };
        
        // 컴팩트한 요소 높이
        blendShapesList.elementHeight = EditorGUIUtility.singleLineHeight + 2;
    }
    
    void SetupHideObjectsList()
    {
        hideObjectsList = new ReorderableList(serializedObject, objectsToHideWhenWearing, true, true, true, true);
        
        hideObjectsList.drawHeaderCallback = (Rect rect) => {
            EditorGUI.LabelField(rect, VModularLocalization.Get("objects_to_hide"));
        };
        
        hideObjectsList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
            var element = hideObjectsList.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;
            rect.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(rect, element, GUIContent.none);
        };
        
        hideObjectsList.onAddCallback = (ReorderableList list) => {
            list.serializedProperty.arraySize++;
        };
        
        // 컴팩트한 요소 높이
        hideObjectsList.elementHeight = EditorGUIUtility.singleLineHeight + 2;
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
        
        // 블렌드쉐이프 설정 (토글이 켜져있을 때만)
        if (useBlendShapeOverride.boolValue)
        {
            EditorGUILayout.Space(5);
            DrawBlendShapesSection();
        }
        
        EditorGUILayout.Space(5);
        
        // 토글 오브젝트 섹션
        DrawToggleObjectsSection();
        
        // 액션 버튼과 디버그 도구 제거됨
        
        serializedObject.ApplyModifiedProperties();
    }
    
    void DrawHeader()
    {
        // Studio Raming 브랜딩 헤더
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();
        
        // 제품 타이틀
        EditorGUILayout.LabelField("🎭 " + VModularLocalization.Get("multibone_title"), EditorStyles.largeLabel);
        
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
        EditorGUILayout.LabelField("🏷️ " + VModularLocalization.Get("group_name"), EditorStyles.boldLabel);
        outfitGroupName.stringValue = EditorGUILayout.TextField(outfitGroupName.stringValue);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(5);
        
        // === 스케일 설정 ===
        DrawScaleSettingsSection();
        
        EditorGUILayout.Space(5);
        
        // === 블렌드쉐이프 설정 ===
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        // 토글 버튼으로 블렌드쉐이프 설정
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("🎨 " + VModularLocalization.Get("blend_shape_override"), EditorStyles.boldLabel, GUILayout.Width(200));
        
        GUILayout.FlexibleSpace();
        
        // ON/OFF 토글 버튼
        string toggleText = useBlendShapeOverride.boolValue ? "ON" : "OFF";
        Color originalBg = GUI.backgroundColor;
        GUI.backgroundColor = useBlendShapeOverride.boolValue ? new Color(0.4f, 0.8f, 0.4f, 1f) : new Color(0.8f, 0.4f, 0.4f, 1f);
        
        if (GUILayout.Button(toggleText, GUILayout.Width(50), GUILayout.Height(20)))
        {
            useBlendShapeOverride.boolValue = !useBlendShapeOverride.boolValue;
        }
        
        GUI.backgroundColor = originalBg;
        EditorGUILayout.EndHorizontal();
        
        // 바디 렌더러 (토글이 켜져있을 때만)
        if (useBlendShapeOverride.boolValue)
        {
            EditorGUILayout.Space(5);
            EditorGUI.DrawRect(GUILayoutUtility.GetRect(0, 1, GUILayout.ExpandWidth(true)), new Color(0.5f, 0.5f, 0.5f, 0.3f));
            EditorGUILayout.Space(5);
            
            EditorGUILayout.LabelField("👥 " + VModularLocalization.Get("body_renderer"), EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(targetBodyRenderer, GUIContent.none);
            
            if (targetBodyRenderer.objectReferenceValue == null)
            {
                string warningText = GetBodyRendererWarning();
                EditorGUILayout.HelpBox(warningText, MessageType.Info);
            }
        }
        EditorGUILayout.EndVertical();
    }
    
    void DrawScaleSettingsSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        // 토글 버튼으로 스케일 설정
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
    
    
    
    string GetBodyRendererWarning()
    {
        return VModularLocalization.CurrentLanguage == SupportedLanguage.Korean ? 
            "바디 렌더러를 설정하지 않으면 블렌드쉐이프 기능이 작동하지 않습니다." :
            VModularLocalization.CurrentLanguage == SupportedLanguage.Japanese ?
            "ボディレンダラーを設定しないとブレンドシェイプ機能が動作しません。" :
            "Blend shape features will not work without setting a body renderer.";
    }
    
    void DrawStatusSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("📊 " + VModularLocalization.Get("status"), EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        // 착용 상태
        bool isWearing = Application.isPlaying && targetScript != null && (bool)typeof(VModularMultiBone)
            .GetField("isWearing", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.GetValue(targetScript);
        
        string statusText = isWearing ? VModularLocalization.Get("wearing") : VModularLocalization.Get("not_wearing");
        EditorGUILayout.LabelField("👔 " + statusText, EditorStyles.label);
        
        // 초기화 상태
        bool isInitialized = targetScript != null && targetBodyRenderer.objectReferenceValue != null;
        string initText = isInitialized ? VModularLocalization.Get("initialized") : VModularLocalization.Get("not_initialized");
        EditorGUILayout.LabelField("⚙️ " + initText, EditorStyles.label);
        
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }
    
    
    
    void DrawBlendShapesSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        showBlendShapes = EditorGUILayout.Foldout(showBlendShapes, $"🎨 " + VModularLocalization.Get("blend_shapes") + $" ({outfitBlendShapes.arraySize})", true);
        
        if (showBlendShapes)
        {
            EditorGUILayout.Space(3);
            
            string descText = VModularLocalization.CurrentLanguage == SupportedLanguage.Korean ? 
                "바디에 적용할 블렌드쉐이프를 설정하세요." :
                VModularLocalization.CurrentLanguage == SupportedLanguage.Japanese ?
                "ボディに適用するブレンドシェイプを設定してください。" :
                "Set blend shapes to apply to the body.";
                
            EditorGUILayout.HelpBox(descText, MessageType.Info);
            blendShapesList.DoLayoutList();
        }
        EditorGUILayout.EndVertical();
    }
    
    void DrawToggleObjectsSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        showObjectsToToggle = EditorGUILayout.Foldout(showObjectsToToggle, $"🔄 " + VModularLocalization.Get("objects_to_toggle") + $" ({objectsToHideWhenWearing.arraySize})", true);
        
        if (showObjectsToToggle)
        {
            EditorGUILayout.Space(3);
            
            string descText = VModularLocalization.CurrentLanguage == SupportedLanguage.Korean ? 
                "의상 착용시 숨길 오브젝트들을 설정하세요." :
                VModularLocalization.CurrentLanguage == SupportedLanguage.Japanese ?
                "衣装着用時に隠すオブジェクトを設定してください。" :
                "Set objects to hide when wearing the outfit.";
                
            EditorGUILayout.HelpBox(descText, MessageType.Info);
            hideObjectsList.DoLayoutList();
            
            EditorGUILayout.Space(3);
            
            // 드래그 앤 드롭 영역 (간단하게)
            var dropRect = GUILayoutUtility.GetRect(0, 30, GUILayout.ExpandWidth(true));
            string dropText = VModularLocalization.CurrentLanguage == SupportedLanguage.Korean ? 
                "📦 게임오브젝트를 여기에 드래그 앤 드롭" :
                VModularLocalization.CurrentLanguage == SupportedLanguage.Japanese ?
                "📦 ゲームオブジェクトをここにドラッグ&ドロップ" :
                "📦 Drag & Drop GameObjects Here";
                
            GUI.Box(dropRect, dropText, EditorStyles.centeredGreyMiniLabel);
            HandleDragAndDrop(dropRect, hideObjectsList);
        }
        EditorGUILayout.EndVertical();
    }
    
    
    void HandleDragAndDrop(Rect dropRect, ReorderableList list)
    {
        Event currentEvent = Event.current;
        
        if (currentEvent.type == EventType.DragUpdated || currentEvent.type == EventType.DragPerform)
        {
            if (dropRect.Contains(currentEvent.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                
                if (currentEvent.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    
                    foreach (Object draggedObject in DragAndDrop.objectReferences)
                    {
                        if (draggedObject is GameObject gameObject)
                        {
                            list.serializedProperty.arraySize++;
                            var newElement = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
                            newElement.objectReferenceValue = gameObject;
                        }
                    }
                    
                    serializedObject.ApplyModifiedProperties();
                }
                
                currentEvent.Use();
            }
        }
    }
}
