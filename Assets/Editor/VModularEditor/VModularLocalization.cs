using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

// 지원 언어 enum
public enum SupportedLanguage
{
    English,
    Korean, 
    Japanese
}

// 다국어 지원 시스템
public static class VModularLocalization
{
    private static SupportedLanguage _currentLanguage = SupportedLanguage.Korean;
    
    // EditorPrefs 키
    private const string LANGUAGE_PREF_KEY = "VModular_Language";
    
    // 현재 언어 프로퍼티
    public static SupportedLanguage CurrentLanguage
    {
        get 
        { 
            if (!_languageLoaded)
            {
                LoadLanguagePreference();
            }
            return _currentLanguage; 
        }
        set 
        { 
            _currentLanguage = value;
            EditorPrefs.SetInt(LANGUAGE_PREF_KEY, (int)value);
        }
    }
    
    private static bool _languageLoaded = false;
    
    static void LoadLanguagePreference()
    {
        _currentLanguage = (SupportedLanguage)EditorPrefs.GetInt(LANGUAGE_PREF_KEY, (int)SupportedLanguage.Korean);
        _languageLoaded = true;
    }
    
    // 번역 딕셔너리
    private static Dictionary<string, Dictionary<SupportedLanguage, string>> translations = 
        new Dictionary<string, Dictionary<SupportedLanguage, string>>()
        {
            // === 공통 ===
            ["language"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Language",
                [SupportedLanguage.Korean] = "언어",
                [SupportedLanguage.Japanese] = "言語"
            },
            ["settings"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Settings",
                [SupportedLanguage.Korean] = "설정",
                [SupportedLanguage.Japanese] = "設定"
            },
            ["debug"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Debug",
                [SupportedLanguage.Korean] = "디버그",
                [SupportedLanguage.Japanese] = "デバッグ"
            },
            ["initialize"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Initialize",
                [SupportedLanguage.Korean] = "초기화",
                [SupportedLanguage.Japanese] = "初期化"
            },
            ["initialized"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Initialized",
                [SupportedLanguage.Korean] = "초기화됨",
                [SupportedLanguage.Japanese] = "初期化済み"
            },
            ["not_initialized"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Not Initialized",
                [SupportedLanguage.Korean] = "초기화되지 않음",
                [SupportedLanguage.Japanese] = "未初期化"
            },
            ["test"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Test",
                [SupportedLanguage.Korean] = "테스트",
                [SupportedLanguage.Japanese] = "テスト"
            },
            ["group_name"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Group Name (Must be Unique)",
                [SupportedLanguage.Korean] = "그룹명 (중요: 고유하게 설정)",
                [SupportedLanguage.Japanese] = "グループ名 (重要：ユニークに設定)"
            },
            ["blend_shape_override"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Blend Shape Override",
                [SupportedLanguage.Korean] = "바디 블렌드쉐이프 설정",
                [SupportedLanguage.Japanese] = "ボディブレンドシェイプ設定"
            },
            ["use_blend_shape"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Use Blend Shape Override",
                [SupportedLanguage.Korean] = "블렌드쉐이프 덮어씌우기 사용",
                [SupportedLanguage.Japanese] = "ブレンドシェイプオーバーライドを使用"
            },
            ["scale_settings"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Scale Settings",
                [SupportedLanguage.Korean] = "스케일 설정",
                [SupportedLanguage.Japanese] = "スケール設定"
            },
            ["use_parent_based_scaling"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Use Parent Based Scaling",
                [SupportedLanguage.Korean] = "부모 기준 스케일 자동 계산",
                [SupportedLanguage.Japanese] = "親基準スケール自動計算"
            },
            ["scale_tooltip_multibone"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "When ON: Automatically calculates scale based on parent bone | When OFF: Maintains original scale",
                [SupportedLanguage.Korean] = "ON: 현재 스케일 유지 | OFF: 부모 본 스케일 반영",
                [SupportedLanguage.Japanese] = "ON: 親ボーン基準でスケール自動計算 | OFF: オリジナルスケール維持"
            },
            ["scale_tooltip_onebone"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "When ON: Maintains current scale | When OFF: Applies parent bone scale",
                [SupportedLanguage.Korean] = "ON: 현재 스케일 유지 | OFF: 부모 본 스케일 반영",
                [SupportedLanguage.Japanese] = "ON: 現在のスケール維持 | OFF: 親ボーンスケール反映"
            },
            ["scale_desc_on"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "ON: Current scale will be maintained.",
                [SupportedLanguage.Korean] = "ON: 현재 스케일을 그대로 유지합니다.",
                [SupportedLanguage.Japanese] = "ON: 現在のスケールを維持します。"
            },
            ["scale_desc_off"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "OFF: Parent bone scale will be applied.",
                [SupportedLanguage.Korean] = "OFF: 부모 본의 스케일을 반영합니다.",
                [SupportedLanguage.Japanese] = "OFF: 親ボーンのスケールを反映します。"
            },
            // === VModularMultiBone ===
            ["multibone_title"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "V-Modular Multi-Bone System",
                [SupportedLanguage.Korean] = "V-모듈러 멀티본 시스템",
                [SupportedLanguage.Japanese] = "V-モジュラー マルチボーンシステム"
            },
            ["target_settings"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Target Settings",
                [SupportedLanguage.Korean] = "대상 설정",
                [SupportedLanguage.Japanese] = "ターゲット設定"
            },
            ["body_renderer"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Body Renderer",
                [SupportedLanguage.Korean] = "바디 렌더러",
                [SupportedLanguage.Japanese] = "ボディレンダラー"
            },
            ["body_renderer_desc"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "SkinnedMeshRenderer for body blend shapes",
                [SupportedLanguage.Korean] = "바디 블렌드쉐이프용 스킨드메쉬 렌더러",
                [SupportedLanguage.Japanese] = "ボディブレンドシェイプ用スキンドメッシュレンダラー"
            },
            ["blend_shapes"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Blend Shapes",
                [SupportedLanguage.Korean] = "블렌드쉐이프",
                [SupportedLanguage.Japanese] = "ブレンドシェイプ"
            },
            ["outfit_blend_shapes"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Outfit Blend Shapes",
                [SupportedLanguage.Korean] = "의상 전용 블렌드쉐이프",
                [SupportedLanguage.Japanese] = "衣装専用ブレンドシェイプ"
            },
            ["outfit_blend_shapes_desc"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Blend shapes applied when this outfit is worn",
                [SupportedLanguage.Korean] = "이 의상 착용 시 적용될 블렌드쉐이프",
                [SupportedLanguage.Japanese] = "この衣装着用時に適用されるブレンドシェイプ"
            },
            ["objects_to_toggle"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Objects to Toggle",
                [SupportedLanguage.Korean] = "토글할 오브젝트",
                [SupportedLanguage.Japanese] = "トグルオブジェクト"
            },
            ["objects_to_hide"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Objects to Hide When Wearing",
                [SupportedLanguage.Korean] = "착용 시 숨길 오브젝트",
                [SupportedLanguage.Japanese] = "着用時に隠すオブジェクト"
            },
            ["wear_outfit"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Wear Outfit",
                [SupportedLanguage.Korean] = "의상 착용",
                [SupportedLanguage.Japanese] = "衣装着用"
            },
            ["remove_outfit"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Remove Outfit",
                [SupportedLanguage.Korean] = "의상 제거",
                [SupportedLanguage.Japanese] = "衣装除去"
            },
            
            // === VModularOneBone ===
            ["onebone_title"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "V-Modular One-Bone System",
                [SupportedLanguage.Korean] = "V-모듈러 단일본 시스템",
                [SupportedLanguage.Japanese] = "V-モジュラー シングルボーンシステム"
            },
            ["bone_attachment"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Bone Attachment Settings",
                [SupportedLanguage.Korean] = "본 부착 설정",
                [SupportedLanguage.Japanese] = "ボーン取り付け設定"
            },
            ["target_humanoid_bone"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Target Humanoid Bone",
                [SupportedLanguage.Korean] = "대상 휴머노이드 본",
                [SupportedLanguage.Japanese] = "対象ヒューマノイドボーン"
            },
            ["target_humanoid_bone_desc"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Select humanoid bone to attach to",
                [SupportedLanguage.Korean] = "부착할 휴머노이드 본 선택",
                [SupportedLanguage.Japanese] = "取り付け先のヒューマノイドボーンを選択"
            },
            ["source_attachment_bone"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Source Attachment Bone",
                [SupportedLanguage.Korean] = "소스 부착 본",
                [SupportedLanguage.Japanese] = "ソース取り付けボーン"
            },
            ["source_attachment_bone_desc"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Accessory bone to attach (direct assignment)",
                [SupportedLanguage.Korean] = "부착할 악세사리의 본 (직접 지정)",
                [SupportedLanguage.Japanese] = "取り付けるアクセサリーのボーン（直接指定）"
            },
            ["wear_accessory"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Wear Accessory",
                [SupportedLanguage.Korean] = "악세사리 착용",
                [SupportedLanguage.Japanese] = "アクセサリー着用"
            },
            ["remove_accessory"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Remove Accessory",
                [SupportedLanguage.Korean] = "악세사리 제거",
                [SupportedLanguage.Japanese] = "アクセサリー除去"
            },
            
            // === ShortcutObjectManager ===
            ["shortcut_title"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Shortcut Object Manager",
                [SupportedLanguage.Korean] = "단축키 오브젝트 매니저",
                [SupportedLanguage.Japanese] = "ショートカットオブジェクトマネージャー"
            },
            ["categories"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Categories",
                [SupportedLanguage.Korean] = "카테고리",
                [SupportedLanguage.Japanese] = "カテゴリー"
            },
            ["category_name"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Category Name",
                [SupportedLanguage.Korean] = "카테고리명",
                [SupportedLanguage.Japanese] = "カテゴリー名"
            },
            ["mode"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Mode",
                [SupportedLanguage.Korean] = "모드",
                [SupportedLanguage.Japanese] = "モード"
            },
            ["single_mode"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Single",
                [SupportedLanguage.Korean] = "단일",
                [SupportedLanguage.Japanese] = "シングル"
            },
            ["toggle_mode"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Toggle",
                [SupportedLanguage.Korean] = "토글",
                [SupportedLanguage.Japanese] = "トグル"
            },
            ["modifier_keys"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Modifier Keys",
                [SupportedLanguage.Korean] = "조합키",
                [SupportedLanguage.Japanese] = "修飾キー"
            },
            ["items"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Items",
                [SupportedLanguage.Korean] = "아이템",
                [SupportedLanguage.Japanese] = "アイテム"
            },
            ["shortcut_key"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Shortcut Key",
                [SupportedLanguage.Korean] = "단축키",
                [SupportedLanguage.Japanese] = "ショートカットキー"
            },
            ["initial_state"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Initial State",
                [SupportedLanguage.Korean] = "초기 상태",
                [SupportedLanguage.Japanese] = "初期状態"
            },
            ["add_category"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Add Category",
                [SupportedLanguage.Korean] = "카테고리 추가",
                [SupportedLanguage.Japanese] = "カテゴリー追加"
            },
            ["reset_to_default"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Reset to Default",
                [SupportedLanguage.Korean] = "기본값으로 재설정",
                [SupportedLanguage.Japanese] = "デフォルトにリセット"
            },
            ["debug_logs"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Show Debug Logs",
                [SupportedLanguage.Korean] = "디버그 로그 표시",
                [SupportedLanguage.Japanese] = "デバッグログ表示"
            },
            
            // === 공통 메시지 ===
            ["wearing"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Wearing",
                [SupportedLanguage.Korean] = "착용 중",
                [SupportedLanguage.Japanese] = "着用中"
            },
            ["not_wearing"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Not Wearing",
                [SupportedLanguage.Korean] = "미착용",
                [SupportedLanguage.Japanese] = "未着用"
            },
            ["drag_and_drop"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Drag & Drop GameObjects here",
                [SupportedLanguage.Korean] = "게임오브젝트를 여기에 드래그 앤 드롭",
                [SupportedLanguage.Japanese] = "ゲームオブジェクトをここにドラッグ＆ドロップ"
            },
            
            // === 캡처 버튼 및 기타 ===
            ["capture"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Capture",
                [SupportedLanguage.Korean] = "캡처",
                [SupportedLanguage.Japanese] = "キャプチャ"
            },
            ["stop"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Stop",
                [SupportedLanguage.Korean] = "중지",
                [SupportedLanguage.Japanese] = "停止"
            },
            ["none"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "None",
                [SupportedLanguage.Korean] = "없음",
                [SupportedLanguage.Japanese] = "なし"
            },
            ["initial_on"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Initial ON",
                [SupportedLanguage.Korean] = "초기 ON",
                [SupportedLanguage.Japanese] = "初期ON"
            },
            
            // === 조합키 버튼 ===
            ["ctrl_active"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "✅ Ctrl",
                [SupportedLanguage.Korean] = "✅ Ctrl",
                [SupportedLanguage.Japanese] = "✅ Ctrl"
            },
            ["ctrl_inactive"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "⌨️ Ctrl",
                [SupportedLanguage.Korean] = "⌨️ Ctrl",
                [SupportedLanguage.Japanese] = "⌨️ Ctrl"
            },
            ["alt_active"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "✅ Alt",
                [SupportedLanguage.Korean] = "✅ Alt",
                [SupportedLanguage.Japanese] = "✅ Alt"
            },
            ["alt_inactive"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "⌨️ Alt",
                [SupportedLanguage.Korean] = "⌨️ Alt",
                [SupportedLanguage.Japanese] = "⌨️ Alt"
            },
            ["shift_active"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "✅ Shift",
                [SupportedLanguage.Korean] = "✅ Shift",
                [SupportedLanguage.Japanese] = "✅ Shift"
            },
            ["shift_inactive"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "⌨️ Shift",
                [SupportedLanguage.Korean] = "⌨️ Shift",
                [SupportedLanguage.Japanese] = "⌨️ Shift"
            },
            
            // === 경고 메시지 ===
            ["play_mode_required"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Play mode required for this operation",
                [SupportedLanguage.Korean] = "이 작업을 위해서는 플레이 모드가 필요합니다",
                [SupportedLanguage.Japanese] = "この操作にはプレイモードが必要です"
            },
            ["key_capture_started"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Key capture started: {0} Element {1} - Press a key!",
                [SupportedLanguage.Korean] = "키 캡처 모드 시작: {0} Element {1} - 키를 눌러주세요!",
                [SupportedLanguage.Japanese] = "キーキャプチャ開始: {0} Element {1} - キーを押してください！"
            },
            ["key_assigned"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Shortcut key assigned: {0} Element {1} = {2}",
                [SupportedLanguage.Korean] = "단축키 설정됨: {0} Element {1} = {2}",
                [SupportedLanguage.Japanese] = "ショートカットキー設定: {0} Element {1} = {2}"
            },
            ["drag_drop_added"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Added via drag & drop: {0}",
                [SupportedLanguage.Korean] = "드래그 앤 드롭으로 추가됨: {0}",
                [SupportedLanguage.Japanese] = "ドラッグ＆ドロップで追加: {0}"
            },
            ["default_categories_created"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "3 default categories created",
                [SupportedLanguage.Korean] = "기본 카테고리 3개 생성됨",
                [SupportedLanguage.Japanese] = "デフォルトカテゴリー3個作成"
            },
            
            // === 모드 토글 버튼 ===
            ["toggle_mode_on"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Toggle",
                [SupportedLanguage.Korean] = "토글 모드",
                [SupportedLanguage.Japanese] = "トグル"
                
            },
            ["toggle_mode_off"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Single",
                [SupportedLanguage.Korean] = "단일 모드",
                [SupportedLanguage.Japanese] = "シングル"
            },
            
            // === 초기 상태 버튼 ===
            ["initial_state_on"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "ON",
                [SupportedLanguage.Korean] = "ON",
                [SupportedLanguage.Japanese] = "ON"
            },
            ["initial_state_off"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "OFF",
                [SupportedLanguage.Korean] = "OFF",
                [SupportedLanguage.Japanese] = "OFF"
            },
            
            // === VModularManager 전용 ===
            ["outfit_tool_title"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "👘 Avatar Outfit Tool",
                [SupportedLanguage.Korean] = "👘 아바타 옷 입히기 도구",
                [SupportedLanguage.Japanese] = "👘 アバター衣装ツール"
            },
            ["outfit_tool_desc"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Simple tool to apply/remove VModular components at once",
                [SupportedLanguage.Korean] = "VModular 컴포넌트를 한 번에 적용/해제하는 간편한 툴",
                [SupportedLanguage.Japanese] = "VModularコンポーネントを一括適用/削除する簡単ツール"
            },
            ["apply_all"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "📝 Apply All",
                [SupportedLanguage.Korean] = "📝 전체 적용하기",
                [SupportedLanguage.Japanese] = "📝 全て適用"
            },
            ["restore"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "↶ Restore",
                [SupportedLanguage.Korean] = "↶ 되돌리기",
                [SupportedLanguage.Japanese] = "↶ 復元"
            },
            ["convenience_features"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "🛠️ Convenience Features",
                [SupportedLanguage.Korean] = "🛠️ 편의성 기능",
                [SupportedLanguage.Japanese] = "🛠️ 便利機能"
            },
            ["remove_missing_scripts"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "🗑️ Remove Missing Scripts",
                [SupportedLanguage.Korean] = "🗑️ 미싱 스크립트 제거",
                [SupportedLanguage.Japanese] = "🗑️ 欠落スクリプト削除"
            },
            ["component_status"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "📋 Component Status",
                [SupportedLanguage.Korean] = "📋 컴포넌트 상태",
                [SupportedLanguage.Japanese] = "📋 コンポーネント状態"
            },
            ["avatar"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Avatar",
                [SupportedLanguage.Korean] = "아바타",
                [SupportedLanguage.Japanese] = "アバター"
            },
            ["select_avatar_first"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Please select an avatar first.",
                [SupportedLanguage.Korean] = "먼저 아바타를 선택해주세요.",
                [SupportedLanguage.Japanese] = "まずアバターを選択してください。"
            },
            ["component_count"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "SingleBone: {0}, MultiBone: {1}",
                [SupportedLanguage.Korean] = "SingleBone: {0}개, MultiBone: {1}개",
                [SupportedLanguage.Japanese] = "SingleBone: {0}個、MultiBone: {1}個"
            },
            ["backup_exists"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "💾 Backup Available",
                [SupportedLanguage.Korean] = "💾 백업 있음",
                [SupportedLanguage.Japanese] = "💾 バックアップ有り"
            },
            ["no_components"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "No components found.",
                [SupportedLanguage.Korean] = "컴포넌트가 없습니다.",
                [SupportedLanguage.Japanese] = "コンポーネントがありません。"
            },
            ["single_bone"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "SingleBone",
                [SupportedLanguage.Korean] = "SingleBone",
                [SupportedLanguage.Japanese] = "SingleBone"
            },
            ["multi_bone"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "MultiBone",
                [SupportedLanguage.Korean] = "MultiBone",
                [SupportedLanguage.Japanese] = "MultiBone"
            },
            ["error"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Error",
                [SupportedLanguage.Korean] = "오류",
                [SupportedLanguage.Japanese] = "エラー"
            },
            ["completed"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Completed",
                [SupportedLanguage.Korean] = "완료",
                [SupportedLanguage.Japanese] = "完了"
            },
            ["missing_scripts_removed_success"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Missing scripts removed successfully!",
                [SupportedLanguage.Korean] = "미싱 스크립트 제거 완료!",
                [SupportedLanguage.Japanese] = "欠落スクリプト削除完了！"
            },
            ["scripts_removed_count"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Removed scripts: {0}",
                [SupportedLanguage.Korean] = "제거된 스크립트: {0}개",
                [SupportedLanguage.Japanese] = "削除されたスクリプト: {0}個"
            },
            ["no_missing_scripts_found"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "No missing scripts found.",
                [SupportedLanguage.Korean] = "미싱 스크립트가 발견되지 않았습니다.",
                [SupportedLanguage.Japanese] = "欠落スクリプトが見つかりませんでした。"
            },
            ["selected"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "✓ Selected: {0}",
                [SupportedLanguage.Korean] = "✓ 선택됨: {0}",
                [SupportedLanguage.Japanese] = "✓ 選択済み: {0}"
            },
            ["select_avatar_with_vmodular"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Select an avatar with VModular components.",
                [SupportedLanguage.Korean] = "VModular 컴포넌트가 있는 아바타를 선택하세요.",
                [SupportedLanguage.Japanese] = "VModularコンポーネントを持つアバターを選択してください。"
            },
            ["ok"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "OK",
                [SupportedLanguage.Korean] = "확인",
                [SupportedLanguage.Japanese] = "確認"
            },
            ["active"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "🟢 Active",
                [SupportedLanguage.Korean] = "🟢 활성",
                [SupportedLanguage.Japanese] = "🟢 有効"
            },
            ["inactive"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "⚫ Inactive",
                [SupportedLanguage.Korean] = "⚫ 비활성",
                [SupportedLanguage.Japanese] = "⚫ 無効"
            },
            ["toggle_active"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "🔄",
                [SupportedLanguage.Korean] = "🔄",
                [SupportedLanguage.Japanese] = "🔄"
            },
            ["enable_all_components"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "🟢 Enable All",
                [SupportedLanguage.Korean] = "🟢 전체 활성화",
                [SupportedLanguage.Japanese] = "🟢 全て有効"
            },
            ["disable_all_components"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "⚫ Disable All",
                [SupportedLanguage.Korean] = "⚫ 전체 비활성화",
                [SupportedLanguage.Japanese] = "⚫ 全て無効"
            },
            
            // === 사용법 가이드 ===
            ["how_to_use"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "📖 How to Use V Modular",
                [SupportedLanguage.Korean] = "📖 V모듈러 사용법",
                [SupportedLanguage.Japanese] = "📖 V モジュラー 使用方法"
            },
            ["basic_precautions"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "⚠️ Basic Precautions",
                [SupportedLanguage.Korean] = "⚠️ 기본 주의사항",
                [SupportedLanguage.Japanese] = "⚠️ 基本注意事項"
            },
            ["multibone_precaution"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "• MultiBone only works with parts specifically designed with identical bone names to the avatar.",
                [SupportedLanguage.Korean] = "• 멀티본은 아바타와 파츠의 본 이름이 동일하게 전용으로 작업된 파츠에만 작동합니다.",
                [SupportedLanguage.Japanese] = "• マルチボーンは、アバターとパーツのボーン名が同一に専用作業されたパーツでのみ動作します。"
            },
            ["hierarchy_requirement"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "• Part objects must be placed directly under the Avatar object.",
                [SupportedLanguage.Korean] = "• 아바타 오브젝트 바로 밑에 파츠 오브젝트를 넣어야 합니다.",
                [SupportedLanguage.Japanese] = "• アバターオブジェクトの直下にパーツオブジェクトを配置する必要があります。"
            },
            ["setup_guide"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "🔧 Setup Guide",
                [SupportedLanguage.Korean] = "🔧 설정 가이드",
                [SupportedLanguage.Japanese] = "🔧 設定ガイド"
            },
            ["step_1"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Step 1: Component Setup",
                [SupportedLanguage.Korean] = "1단계: 컴포넌트 설정",
                [SupportedLanguage.Japanese] = "ステップ1: コンポーネント設定"
            },
            ["multibone_setup"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "1-1. For outfits/hair with multiple bones: Add VModularMultiBone component to the top of the part object.",
                [SupportedLanguage.Korean] = "1-1. 의상/충돌체가 있는 헤어 등 본 구조가 여러개인 오브젝트: 그 파츠 오브젝트 제일 위에 VModularMultiBone 컴포넌트를 추가해주세요.",
                [SupportedLanguage.Japanese] = "1-1. 衣装/コリジョンのある髪など複数のボーン構造を持つオブジェクト: そのパーツオブジェクトの最上位にVModularMultiBoneコンポーネントを追加してください。"
            },
            ["singlebone_setup"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "1-2. For parts with single bone structure: Add VModularOneBone component, then specify the target position and attachment bone.",
                [SupportedLanguage.Korean] = "1-2. 본 구조가 하나로 이뤄진 파츠: VModularOneBone 컴포넌트를 부착한 후 부착될 대상인 위치와, 부착될 본을 지정해주세요.",
                [SupportedLanguage.Japanese] = "1-2. 単一ボーン構造のパーツ: VModularOneBoneコンポーネントを装着後、装着対象の位置と装着ボーンを指定してください。"
            },
            ["step_2"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Step 2: Avatar Selection",
                [SupportedLanguage.Korean] = "2단계: 아바타 지정",
                [SupportedLanguage.Japanese] = "ステップ2: アバター指定"
            },
            ["avatar_selection_guide"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "2. After completing component settings, drag and drop the Avatar object into the Avatar field above.",
                [SupportedLanguage.Korean] = "2. 각각의 컴포넌트의 설정이 완료되었다면, 아바타 오브젝트를 위의 아바타 필드에 끌어서 넣어주세요.",
                [SupportedLanguage.Japanese] = "2. 各コンポーネントの設定が完了したら、アバターオブジェクトを上のアバターフィールドにドラッグ&ドロップしてください。"
            },
            ["step_3"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Step 3: Apply/Restore",
                [SupportedLanguage.Korean] = "3단계: 적용/복원",
                [SupportedLanguage.Japanese] = "ステップ3: 適用/復元"
            },
            ["apply_restore_guide"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "3. Use 'Apply All' / 'Restore' buttons to dress up and revert the avatar.",
                [SupportedLanguage.Korean] = "3. '전체 적용하기' / '되돌리기' 버튼을 이용해 의상을 입히고 다시 되돌릴 수 있습니다.",
                [SupportedLanguage.Japanese] = "3. '全て適用' / '復元' ボタンを使って衣装を着せ、元に戻すことができます。"
            },
            ["hierarchy_example"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "📁 Hierarchy Example",
                [SupportedLanguage.Korean] = "📁 계층구조 예시",
                [SupportedLanguage.Japanese] = "📁 階層構造の例"
            },
            ["hierarchy_structure"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "MyAvatar\n├── Armature (Avatar's bone structure)\n├── OutfitPart1 ← Add VModularMultiBone here\n│   ├── Outfit bones...\n├── HairPart ← Add VModularMultiBone here\n│   ├── Hair bones...\n└── AccessoryPart ← Add VModularOneBone here\n    └── Single accessory bone",
                [SupportedLanguage.Korean] = "MyAvatar\n├── Armature (아바타의 본 구조)\n├── OutfitPart1 ← 여기에 VModularMultiBone 추가\n│   ├── 의상 본들...\n├── HairPart ← 여기에 VModularMultiBone 추가\n│   ├── 헤어 본들...\n└── AccessoryPart ← 여기에 VModularOneBone 추가\n    └── 단일 악세사리 본",
                [SupportedLanguage.Japanese] = "MyAvatar\n├── Armature (アバターのボーン構造)\n├── OutfitPart1 ← ここにVModularMultiBone追加\n│   ├── 衣装ボーン群...\n├── HairPart ← ここにVModularMultiBone追加\n│   ├── 髪ボーン群...\n└── AccessoryPart ← ここにVModularOneBone追加\n    └── 単一アクセサリボーン"
            },
            ["component_difference"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "💡 Component Differences",
                [SupportedLanguage.Korean] = "💡 컴포넌트 차이점",
                [SupportedLanguage.Japanese] = "💡 コンポーネントの違い"
            },
            ["multibone_description"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "• VModularMultiBone: For complex parts (outfits, hair with physics)",
                [SupportedLanguage.Korean] = "• VModularMultiBone: 복잡한 파츠용 (의상, 물리가 있는 헤어)",
                [SupportedLanguage.Japanese] = "• VModularMultiBone: 複雑なパーツ用 (衣装、物理のある髪)"
            },
            ["singlebone_description"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "• VModularOneBone: For simple accessories (hats, glasses, earrings)",
                [SupportedLanguage.Korean] = "• VModularOneBone: 간단한 악세사리용 (모자, 안경, 귀걸이)",
                [SupportedLanguage.Japanese] = "• VModularOneBone: シンプルなアクセサリー用 (帽子、眼鏡、イヤリング)"
            },
            ["head_bone_rule"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "⚠️ Important: If hair parts don't have a 'Head' bone, use VModularOneBone instead!",
                [SupportedLanguage.Korean] = "⚠️ 중요: 헤어 파츠에 'Head' 본이 없다면 VModularOneBone을 사용하세요!",
                [SupportedLanguage.Japanese] = "⚠️ 重要: ヘアパーツに'Head'ボーンがない場合はVModularOneBoneを使用してください！"
            },
            
            // === VModularUIGenerator 전용 ===
            ["ui_generator_title"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "V Modular UI Generator",
                [SupportedLanguage.Korean] = "V 모듈러 UI 생성기",
                [SupportedLanguage.Japanese] = "V モジュラー UI ジェネレーター"
            },
            ["how_to_use_ui_generator"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "📖 How to Use V Modular UI Generator",
                [SupportedLanguage.Korean] = "📖 V모듈러 UI 생성기 사용법",
                [SupportedLanguage.Japanese] = "📖 V モジュラー UI生成ツールの使い方"
            },
            ["ui_generator_overview"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "This tool generates runtime UI based on ShortcutObjectManager settings. The UI allows users to toggle avatar parts during gameplay.",
                [SupportedLanguage.Korean] = "이 도구는 ShortcutObjectManager 설정에 따라 런타임 UI를 생성합니다. UI를 통해 게임플레이 중 아바타 파츠를 토글할 수 있습니다.",
                [SupportedLanguage.Japanese] = "このツールはShortcutObjectManagerの設定に基づいてランタイムUIを生成します。UIを通してゲームプレイ中にアバターパーツをトグルできます。"
            },
            ["ui_generator_step1"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "1️⃣ Add ShortcutObjectManager",
                [SupportedLanguage.Korean] = "1️⃣ ShortcutObjectManager 추가",
                [SupportedLanguage.Japanese] = "1️⃣ ShortcutObjectManagerを追加"
            },
            ["shortcut_manager_setup"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Add ShortcutObjectManager component to your avatar GameObject and configure categories and objects.",
                [SupportedLanguage.Korean] = "아바타 GameObject에 ShortcutObjectManager 컴포넌트를 추가하고 카테고리와 오브젝트를 설정하세요.",
                [SupportedLanguage.Japanese] = "アバターGameObjectにShortcutObjectManagerコンポーネントを追加し、カテゴリとオブジェクトを設定してください。"
            },
            ["ui_generator_step2"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "2️⃣ Configure Objects",
                [SupportedLanguage.Korean] = "2️⃣ 오브젝트 설정",
                [SupportedLanguage.Japanese] = "2️⃣ オブジェクト設定"
            },
            ["drag_objects_guide"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Drag and drop your VModular components and other GameObjects into ShortcutObjectManager categories. Set shortcut keys and modes (Single/Toggle).",
                [SupportedLanguage.Korean] = "VModular 컴포넌트와 다른 GameObject들을 ShortcutObjectManager 카테고리로 끌어다 놓으세요. 단축키와 모드(Single/Toggle)를 설정하세요.",
                [SupportedLanguage.Japanese] = "VModularコンポーネントや他のGameObjectをShortcutObjectManagerのカテゴリにドラッグアンドドロップしてください。ショートカットキーとモード（Single/Toggle）を設定してください。"
            },
            ["ui_generator_step3"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "3️⃣ Select Target & Generate",
                [SupportedLanguage.Korean] = "3️⃣ 타겟 선택 및 생성",
                [SupportedLanguage.Japanese] = "3️⃣ ターゲット選択と生成"
            },
            ["select_generate_guide"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Select your avatar as Target Object, choose UI position (Left/Right), and click 'Generate UI' to create the runtime interface.",
                [SupportedLanguage.Korean] = "아바타를 Target Object로 선택하고, UI 위치(Left/Right)를 선택한 후 'UI 생성'을 클릭하여 런타임 인터페이스를 만드세요.",
                [SupportedLanguage.Japanese] = "アバターをTarget Objectとして選択し、UI位置（Left/Right）を選択して「UI生成」をクリックしてランタイムインターフェースを作成してください。"
            },
            ["important_notes"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "📌 Important Notes",
                [SupportedLanguage.Korean] = "📌 중요 사항",
                [SupportedLanguage.Japanese] = "📌 重要事項"
            },
            ["runtime_ui_note"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "• Generated UI works only in Play Mode\n• ShortcutObjectManager must be configured first\n• UI position can be changed before generation",
                [SupportedLanguage.Korean] = "• 생성된 UI는 플레이 모드에서만 작동합니다\n• ShortcutObjectManager를 먼저 설정해야 합니다\n• UI 위치는 생성 전에 변경 가능합니다",
                [SupportedLanguage.Japanese] = "• 生成されたUIはプレイモードでのみ動作します\n• ShortcutObjectManagerを先に設定する必要があります\n• UI位置は生成前に変更可能です"
            },
            ["modes_explanation"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "💡 Category Modes",
                [SupportedLanguage.Korean] = "💡 카테고리 모드",
                [SupportedLanguage.Japanese] = "💡 カテゴリモード"
            },
            ["single_mode_desc"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "• Single Mode: Only one item can be active per category (good for hair, outfits)",
                [SupportedLanguage.Korean] = "• Single 모드: 카테고리당 하나의 아이템만 활성화 (헤어, 의상에 적합)",
                [SupportedLanguage.Japanese] = "• Singleモード: カテゴリごとに1つのアイテムのみ有効化（髪、衣装に適しています）"
            },
            ["toggle_mode_desc"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "• Toggle Mode: Items can be toggled independently (good for accessories)",
                [SupportedLanguage.Korean] = "• Toggle 모드: 아이템을 독립적으로 토글 (악세사리에 적합)",
                [SupportedLanguage.Japanese] = "• Toggleモード: アイテムを独立してトグル（アクセサリーに適しています）"
            },
            ["ui_generation_modes"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "🔧 V Modular UI Generation Modes",
                [SupportedLanguage.Korean] = "🔧 V모듈러 UI 생성 모드",
                [SupportedLanguage.Japanese] = "🔧 V モジュラー UI生成モード"
            },
            ["default_mode_title"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "• Default Mode (Recommended)",
                [SupportedLanguage.Korean] = "• 기본 모드 (권장)",
                [SupportedLanguage.Japanese] = "• デフォルトモード（推奨）"
            },
            ["default_mode_desc"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Uses ShortcutObjectManager scripts directly. Full functionality with keyboard shortcuts and advanced features.",
                [SupportedLanguage.Korean] = "ShortcutObjectManager 스크립트를 직접 사용합니다. 키보드 단축키와 고급 기능을 모두 사용할 수 있습니다.",
                [SupportedLanguage.Japanese] = "ShortcutObjectManagerスクリプトを直接使用します。キーボードショートカットと高度な機能をすべて使用できます。"
            },
            ["no_script_mode_title"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "• No Script Mode",
                [SupportedLanguage.Korean] = "• 노 스크립트 모드",
                [SupportedLanguage.Japanese] = "• ノースクリプトモード"
            },
            ["no_script_mode_detailed_desc"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Generates UI that works without any scripts running. Perfect for environments where scripts are disabled or restricted.",
                [SupportedLanguage.Korean] = "스크립트 실행 없이도 작동하는 UI를 생성합니다. 스크립트가 비활성화되거나 제한된 환경에서 완벽합니다.",
                [SupportedLanguage.Japanese] = "スクリプト実行なしでも動作するUIを生成します。スクリプトが無効化または制限された環境で完璧です。"
            },
            ["target_object_selection"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Target Object Selection",    
                [SupportedLanguage.Korean] = "대상 오브젝트 선택",
                [SupportedLanguage.Japanese] = "ターゲットオブジェクト選択"
            },
            ["scanned_components"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Scanned Components",
                [SupportedLanguage.Korean] = "스캔된 컴포넌트",
                [SupportedLanguage.Japanese] = "スキャンされたコンポーネント"
            },
            ["ui_generation"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "UI Generation",
                [SupportedLanguage.Korean] = "UI 생성",
                [SupportedLanguage.Japanese] = "UI 生成"
            },
            ["color_settings"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Color Settings",
                [SupportedLanguage.Korean] = "색상 설정",
                [SupportedLanguage.Japanese] = "カラー設定"
            },
            ["target_gameobject"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Target GameObject",
                [SupportedLanguage.Korean] = "대상 게임오브젝트",
                [SupportedLanguage.Japanese] = "ターゲットゲームオブジェクト"
            },
            ["single_bone_components"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Single Bone Components",
                [SupportedLanguage.Korean] = "싱글본 컴포넌트",
                [SupportedLanguage.Japanese] = "シングルボーンコンポーネント"
            },
            ["multi_bone_components"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Multi Bone Components",
                [SupportedLanguage.Korean] = "멀티본 컴포넌트",
                [SupportedLanguage.Japanese] = "マルチボーンコンポーネント"
            },
            ["shortcut_manager"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Shortcut Manager",
                [SupportedLanguage.Korean] = "단축키 매니저",
                [SupportedLanguage.Japanese] = "ショートカットマネージャー"
            },
            ["found"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "✓ Found",
                [SupportedLanguage.Korean] = "✓ 발견됨",
                [SupportedLanguage.Japanese] = "✓ 発見"
            },
            ["not_found"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "✗ Not found",
                [SupportedLanguage.Korean] = "✗ 발견되지 않음",
                [SupportedLanguage.Japanese] = "✗ 見つかりません"
            },
            ["rescan_components"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Rescan Components",
                [SupportedLanguage.Korean] = "컴포넌트 재스캔",
                [SupportedLanguage.Japanese] = "コンポーネント再スキャン"
            },
            ["show_component_details"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Show Component Details",
                [SupportedLanguage.Korean] = "컴포넌트 상세 보기",
                [SupportedLanguage.Japanese] = "コンポーネント詳細表示"
            },
            ["generate_update_ui"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Generate/Update UI",
                [SupportedLanguage.Korean] = "UI 생성/업데이트",
                [SupportedLanguage.Japanese] = "UI 生成/更新"
            },
            ["delete_ui"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Delete UI",
                [SupportedLanguage.Korean] = "UI 삭제",
                [SupportedLanguage.Japanese] = "UI 削除"
            },
            ["generated_ui"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Generated UI",
                [SupportedLanguage.Korean] = "생성된 UI",
                [SupportedLanguage.Japanese] = "生成されたUI"
            },
            ["ui_canvas"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "UI Canvas",
                [SupportedLanguage.Korean] = "UI 캔버스",
                [SupportedLanguage.Japanese] = "UI キャンバス"
            },
            ["no_script_mode"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "No Script Mode",
                [SupportedLanguage.Korean] = "노 스크립트 모드",
                [SupportedLanguage.Japanese] = "ノースクリプトモード"
            },
            ["no_script_mode_desc"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Works without scripts after build",
                [SupportedLanguage.Korean] = "빌드 시 스크립트 없이 작동",
                [SupportedLanguage.Japanese] = "ビルド時にスクリプトなしで動作"
            },
            ["ui_position"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "UI Position",
                [SupportedLanguage.Korean] = "UI 위치",
                [SupportedLanguage.Japanese] = "UI 位置"
            },
            ["left"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Left",
                [SupportedLanguage.Korean] = "왼쪽",
                [SupportedLanguage.Japanese] = "左"
            },
            ["right"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Right",
                [SupportedLanguage.Korean] = "오른쪽",
                [SupportedLanguage.Japanese] = "右"
            },
            ["ui_theme_colors"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "UI Theme Colors",
                [SupportedLanguage.Korean] = "UI 테마 색상",
                [SupportedLanguage.Japanese] = "UI テーマカラー"
            },
            ["background_colors"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Background Colors",
                [SupportedLanguage.Korean] = "배경 색상",
                [SupportedLanguage.Japanese] = "背景色"
            },
            ["accent_colors"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Accent Colors",
                [SupportedLanguage.Korean] = "포인트 색상",
                [SupportedLanguage.Japanese] = "アクセントカラー"
            },
            ["text_colors"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Text Colors",
                [SupportedLanguage.Korean] = "텍스트 색상",
                [SupportedLanguage.Japanese] = "テキストカラー"
            },
            ["color_presets"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Color Presets",
                [SupportedLanguage.Korean] = "색상 프리셋",
                [SupportedLanguage.Japanese] = "カラープリセット"
            },
            ["texture_settings"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Texture Settings",
                [SupportedLanguage.Korean] = "텍스처 설정",
                [SupportedLanguage.Japanese] = "テクスチャ設定"
            },
            ["main_panel_background"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Main Panel Background",
                [SupportedLanguage.Korean] = "메인 패널 배경",
                [SupportedLanguage.Japanese] = "メインパネル背景"
            },
            ["header_background"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Header Background",
                [SupportedLanguage.Korean] = "헤더 배경",
                [SupportedLanguage.Japanese] = "ヘッダー背景"
            },
            ["button_background"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Button Background",
                [SupportedLanguage.Korean] = "버튼 배경",
                [SupportedLanguage.Japanese] = "ボタン背景"
            },
            ["general_background"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "General Background",
                [SupportedLanguage.Korean] = "일반 배경",
                [SupportedLanguage.Japanese] = "一般背景"
            },
            ["border_color"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Border Color",
                [SupportedLanguage.Korean] = "테두리 색상",
                [SupportedLanguage.Japanese] = "境界線色"
            },
            ["accent_color"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Accent Color",
                [SupportedLanguage.Korean] = "포인트 색상",
                [SupportedLanguage.Japanese] = "アクセントカラー"
            },
            ["primary_text"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Primary Text",
                [SupportedLanguage.Korean] = "주 텍스트",
                [SupportedLanguage.Japanese] = "メインテキスト"
            },
            ["secondary_text"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Secondary Text",
                [SupportedLanguage.Korean] = "보조 텍스트",
                [SupportedLanguage.Japanese] = "サブテキスト"
            },
            ["text_on_accent"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Text on Accent",
                [SupportedLanguage.Korean] = "포인트 색상 위 텍스트",
                [SupportedLanguage.Japanese] = "アクセント上テキスト"
            },
            ["white_theme"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "White",
                [SupportedLanguage.Korean] = "화이트",
                [SupportedLanguage.Japanese] = "ホワイト"
            },
            ["dark_theme"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Dark",
                [SupportedLanguage.Korean] = "다크",
                [SupportedLanguage.Japanese] = "ダーク"
            },
            ["beige_theme"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Beige",
                [SupportedLanguage.Korean] = "베이지",
                [SupportedLanguage.Japanese] = "ベージュ"
            },
            ["pink_theme"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Pink",
                [SupportedLanguage.Korean] = "핑크",
                [SupportedLanguage.Japanese] = "ピンク"
            },
            ["panel_texture"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Panel Texture",
                [SupportedLanguage.Korean] = "패널 텍스처",
                [SupportedLanguage.Japanese] = "パネルテクスチャ"
            },
            ["button_texture"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Button Texture",
                [SupportedLanguage.Korean] = "버튼 텍스처",
                [SupportedLanguage.Japanese] = "ボタンテクスチャ"
            },
            ["header_texture"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Header Texture",
                [SupportedLanguage.Korean] = "헤더 텍스처",
                [SupportedLanguage.Japanese] = "ヘッダーテクスチャ"
            },
            ["toggle_button_texture"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Toggle Button Texture",
                [SupportedLanguage.Korean] = "토글 버튼 텍스처",
                [SupportedLanguage.Japanese] = "トグルボタンテクスチャ"
            },
            ["delete_ui_confirm"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Are you sure you want to delete the generated UI?",
                [SupportedLanguage.Korean] = "생성된 UI를 삭제하시겠습니까?",
                [SupportedLanguage.Japanese] = "生成されたUIを削除しますか？"
            },
            ["yes"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Yes",
                [SupportedLanguage.Korean] = "네",
                [SupportedLanguage.Japanese] = "はい"
            },
            ["no"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "No",
                [SupportedLanguage.Korean] = "아니오",
                [SupportedLanguage.Japanese] = "いいえ"
            },
            ["target_object_not_selected"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Target object is not selected!",
                [SupportedLanguage.Korean] = "대상 오브젝트가 선택되지 않았습니다!",
                [SupportedLanguage.Japanese] = "ターゲットオブジェクトが選択されていません！"
            },
            ["no_script_mode_info"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "No Script Mode: Button clicks are directly connected with Unity Events and work after build.\n• SingleBone: Only one active in the same group\n• MultiBone: Individual toggle",
                [SupportedLanguage.Korean] = "노 스크립트 모드: 버튼 클릭이 Unity Event로 직접 연결되어 빌드 후에도 작동합니다.\n• SingleBone: 같은 그룹 중 하나만 활성화\n• MultiBone: 개별 토글",
                [SupportedLanguage.Japanese] = "ノースクリプトモード: ボタンクリックがUnity Eventで直接接続され、ビルド後も動作します。\n• SingleBone: 同じグループの1つだけアクティブ\n• MultiBone: 個別トグル"
            },
            ["settings_change_regenerate"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "⚠️ Settings changes will apply when UI is regenerated",
                [SupportedLanguage.Korean] = "⚠️ 설정 변경사항은 UI 재생성 시 적용됩니다",
                [SupportedLanguage.Japanese] = "⚠️ 設定変更はUI再生成時に適用されます"
            },
            
            // === 빌드 준비 기능 ===
            ["build_preparation"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "🔧 Build Preparation",
                [SupportedLanguage.Korean] = "🔧 빌드 준비",
                [SupportedLanguage.Japanese] = "🔧 ビルド準備"
            },
            ["non_script_mode"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Non-Script Mode:",
                [SupportedLanguage.Korean] = "Non스크립트 모드:",
                [SupportedLanguage.Japanese] = "ノンスクリプトモード:"
            },
            ["non_script_mode_desc"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "(Remove VModular/Shortcut scripts)",
                [SupportedLanguage.Korean] = "(V모듈러/숏컷 스크립트 제거)",
                [SupportedLanguage.Japanese] = "(Vモジュラー/ショートカットスクリプト削除)"
            },
            ["start_build_preparation"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "🚀 Start Build Preparation",
                [SupportedLanguage.Korean] = "🚀 빌드 준비 시작",
                [SupportedLanguage.Japanese] = "🚀 ビルド準備開始"
            },
            ["build_prep_confirm"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Build Preparation",
                [SupportedLanguage.Korean] = "빌드 준비",
                [SupportedLanguage.Japanese] = "ビルド準備"
            },
            ["build_prep_message"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Start build preparation?\n\n1. Clone original and deactivate\n2. Apply all to clone\n3. Rename clone to 'Character'\n4. Remove VModular/Shortcut scripts\n\nContinue?",
                [SupportedLanguage.Korean] = "빌드 준비를 시작합니다.\n\n1. 원본을 복제하고 비활성화\n2. 복제본에 모두 입히기 적용\n3. 복제본 이름을 'Character'로 변경\n4. V모듈러/숏컷 스크립트 제거\n\n계속하시겠습니까?",
                [SupportedLanguage.Japanese] = "ビルド準備を開始します。\n\n1. オリジナルを複製して非アクティブ化\n2. 複製に全て適用\n3. 複製名を'Character'に変更\n4. Vモジュラー/ショートカットスクリプト削除\n\n続行しますか？"
            },
            ["build_prep_message_no_script"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Start build preparation?\n\n1. Clone original and deactivate\n2. Apply all to clone\n3. Rename clone to 'Character'\n\nContinue?",
                [SupportedLanguage.Korean] = "빌드 준비를 시작합니다.\n\n1. 원본을 복제하고 비활성화\n2. 복제본에 모두 입히기 적용\n3. 복제본 이름을 'Character'로 변경\n\n계속하시겠습니까?",
                [SupportedLanguage.Japanese] = "ビルド準備を開始します。\n\n1. オリジナルを複製して非アクティブ化\n2. 複製に全て適用\n3. 複製名を'Character'に変更\n\n続行しますか？"
            },
            ["start"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Start",
                [SupportedLanguage.Korean] = "시작",
                [SupportedLanguage.Japanese] = "開始"
            },
            ["cancel"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Cancel",
                [SupportedLanguage.Korean] = "취소",
                [SupportedLanguage.Japanese] = "キャンセル"
            },
            ["build_prep_complete"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Build Preparation Complete",
                [SupportedLanguage.Korean] = "빌드 준비 완료",
                [SupportedLanguage.Japanese] = "ビルド準備完了"
            },
            ["build_prep_result"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Build preparation completed!\n\n✅ Original: {0} (deactivated)\n✅ Clone: Character (activated)\n✅ All applied\n{1}\n\nUse Character for build.",
                [SupportedLanguage.Korean] = "빌드 준비가 완료되었습니다!\n\n✅ 원본: {0} (비활성화됨)\n✅ 복제본: Character (활성화됨)\n✅ 모두 입히기 적용 완료\n{1}\n\n이제 Character를 빌드에 사용하세요.",
                [SupportedLanguage.Japanese] = "ビルド準備が完了しました！\n\n✅ オリジナル: {0} (非アクティブ)\n✅ 複製: Character (アクティブ)\n✅ 全て適用完了\n{1}\n\nCharacterをビルドに使用してください。"
            },
            ["scripts_removed"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "✅ VModular/Shortcut scripts removed",
                [SupportedLanguage.Korean] = "✅ V모듈러/숏컷 스크립트 제거 완료",
                [SupportedLanguage.Japanese] = "✅ Vモジュラー/ショートカットスクリプト削除完了"
            },
            
            // === 빌드 준비 가이드 ===
            ["build_prep_guide"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "🚀 Build Preparation Guide",
                [SupportedLanguage.Korean] = "🚀 빌드 준비 가이드",
                [SupportedLanguage.Japanese] = "🚀 ビルド準備ガイド"
            },
            ["build_prep_overview"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "This feature creates a build-ready avatar with all outfits automatically applied. Perfect for creating final character files for games or VRChat.",
                [SupportedLanguage.Korean] = "이 기능은 모든 의상이 자동으로 적용된 빌드 준비 아바타를 생성합니다. 게임이나 VRChat용 최종 캐릭터 파일 생성에 완벽합니다.",
                [SupportedLanguage.Japanese] = "この機能は、すべての衣装が自動適用されたビルド準備アバターを作成します。ゲームやVRChat用の最終キャラクターファイル作成に最適です。"
            },
            ["build_prep_process"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "📋 Build Preparation Process",
                [SupportedLanguage.Korean] = "📋 빌드 준비 프로세스",
                [SupportedLanguage.Japanese] = "📋 ビルド準備プロセス"
            },
            ["build_prep_step1"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "1️⃣ Clone & Deactivate Original",
                [SupportedLanguage.Korean] = "1️⃣ 원본 복제 및 비활성화",
                [SupportedLanguage.Japanese] = "1️⃣ オリジナル複製と非アクティブ化"
            },
            ["build_prep_step1_desc"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Creates a copy of your avatar and deactivates the original to preserve it.",
                [SupportedLanguage.Korean] = "아바타의 복사본을 생성하고 원본을 보존하기 위해 비활성화합니다.",
                [SupportedLanguage.Japanese] = "アバターのコピーを作成し、オリジナルを保存するために非アクティブ化します。"
            },
            ["build_prep_step2"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "2️⃣ Apply All Outfits",
                [SupportedLanguage.Korean] = "2️⃣ 모든 의상 적용",
                [SupportedLanguage.Japanese] = "2️⃣ すべての衣装適用"
            },
            ["build_prep_step2_desc"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Automatically applies all VModular components to the cloned avatar.",
                [SupportedLanguage.Korean] = "복제된 아바타에 모든 V모듈러 컴포넌트를 자동으로 적용합니다.",
                [SupportedLanguage.Japanese] = "複製されたアバターにすべてのVモジュラーコンポーネントを自動適用します。"
            },
            ["build_prep_step3"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "3️⃣ Rename to 'Character'",
                [SupportedLanguage.Korean] = "3️⃣ 'Character'로 이름 변경",
                [SupportedLanguage.Japanese] = "3️⃣ 'Character'に名前変更"
            },
            ["build_prep_step3_desc"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Renames the prepared avatar to 'Character' for easy identification.",
                [SupportedLanguage.Korean] = "준비된 아바타를 쉽게 식별할 수 있도록 'Character'로 이름을 변경합니다.",
                [SupportedLanguage.Japanese] = "準備されたアバターを簡単に識別できるように'Character'に名前を変更します。"
            },
            ["build_prep_step4"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "4️⃣ Script Removal (Optional)",
                [SupportedLanguage.Korean] = "4️⃣ 스크립트 제거 (선택사항)",
                [SupportedLanguage.Japanese] = "4️⃣ スクリプト削除（オプション）"
            },
            ["build_prep_step4_desc"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Removes VModular and Shortcut scripts when Non-Script Mode is enabled.",
                [SupportedLanguage.Korean] = "Non스크립트 모드가 활성화된 경우 V모듈러 및 숏컷 스크립트를 제거합니다.",
                [SupportedLanguage.Japanese] = "ノンスクリプトモードが有効な場合、Vモジュラーとショートカットスクリプトを削除します。"
            },
            ["build_prep_modes"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "⚙️ Build Preparation Modes",
                [SupportedLanguage.Korean] = "⚙️ 빌드 준비 모드",
                [SupportedLanguage.Japanese] = "⚙️ ビルド準備モード"
            },
            ["default_build_mode"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "• Default Mode (Recommended)",
                [SupportedLanguage.Korean] = "• 기본 모드 (권장)",
                [SupportedLanguage.Japanese] = "• デフォルトモード（推奨）"
            },
            ["default_build_mode_desc"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Creates a build-ready avatar with all VModular scripts intact. Perfect for most game engines.",
                [SupportedLanguage.Korean] = "모든 V모듈러 스크립트가 그대로 유지된 빌드 준비 아바타를 생성합니다. 대부분의 게임 엔진에 적합합니다.",
                [SupportedLanguage.Japanese] = "すべてのVモジュラースクリプトがそのまま保持されたビルド準備アバターを作成します。ほとんどのゲームエンジンに適しています。"
            },
            ["non_script_build_mode"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "• Non-Script Mode",
                [SupportedLanguage.Korean] = "• Non스크립트 모드",
                [SupportedLanguage.Japanese] = "• ノンスクリプトモード"
            },
            ["non_script_build_mode_desc"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "Removes all VModular and Shortcut scripts for environments that don't support custom scripts (like VSF). The avatar will work without any VModular functionality.",
                [SupportedLanguage.Korean] = "커스텀 스크립트를 지원하지 않는 환경(VSF 등)을 위해 모든 V모듈러 및 숏컷 스크립트를 제거합니다. 아바타는 V모듈러 기능 없이 작동합니다.",
                [SupportedLanguage.Japanese] = "カスタムスクリプトをサポートしない環境（VSFなど）のために、すべてのVモジュラーとショートカットスクリプトを削除します。アバターはVモジュラー機能なしで動作します。"
            },
            ["build_prep_ui_generator_note"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "📌 Important: UI Generator Usage",
                [SupportedLanguage.Korean] = "📌 중요: UI 제너레이터 사용법",
                [SupportedLanguage.Japanese] = "📌 重要: UIジェネレーター使用方法"
            },
            ["build_prep_ui_generator_desc"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "If you plan to use the UI Generator, you must use it BEFORE running Build Preparation. The UI Generator should be applied to the original avatar, then Build Preparation will create a ready-to-use character with both outfits and UI applied.",
                [SupportedLanguage.Korean] = "UI 제너레이터를 사용할 계획이라면, 빌드 준비를 실행하기 전에 먼저 사용해야 합니다. UI 제너레이터는 원본 아바타에 적용한 후, 빌드 준비를 통해 의상과 UI가 모두 적용된 사용 준비된 캐릭터를 생성합니다.",
                [SupportedLanguage.Japanese] = "UIジェネレーターを使用する予定がある場合は、ビルド準備を実行する前に先に使用する必要があります。UIジェネレーターはオリジナルアバターに適用した後、ビルド準備を通じて衣装とUIがすべて適用された使用準備完了キャラクターを生成します。"
            },
            ["build_prep_workflow"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "🔄 Recommended Workflow",
                [SupportedLanguage.Korean] = "🔄 권장 워크플로우",
                [SupportedLanguage.Japanese] = "🔄 推奨ワークフロー"
            },
            ["build_prep_workflow_step1"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "1. Set up all VModular components on your avatar",
                [SupportedLanguage.Korean] = "1. 아바타에 모든 V모듈러 컴포넌트 설정",
                [SupportedLanguage.Japanese] = "1. アバターにすべてのVモジュラーコンポーネントを設定"
            },
            ["build_prep_workflow_step2"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "2. (Optional) Use UI Generator if you need runtime UI",
                [SupportedLanguage.Korean] = "2. (선택사항) 런타임 UI가 필요하면 UI 제너레이터 사용",
                [SupportedLanguage.Japanese] = "2. （オプション）ランタイムUIが必要な場合はUIジェネレーターを使用"
            },
            ["build_prep_workflow_step3"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "3. Run Build Preparation to create final character",
                [SupportedLanguage.Korean] = "3. 빌드 준비를 실행하여 최종 캐릭터 생성",
                [SupportedLanguage.Japanese] = "3. ビルド準備を実行して最終キャラクターを作成"
            },
            ["build_prep_workflow_step4"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "4. Use the 'Character' object for your build",
                [SupportedLanguage.Korean] = "4. 빌드에 'Character' 오브젝트 사용",
                [SupportedLanguage.Japanese] = "4. ビルドに'Character'オブジェクトを使用"
            },
            
            // === WARUDO 전용 가이드 ===
            ["warudo_build_guide"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "🎮 WARUDO Build Guide",
                [SupportedLanguage.Korean] = "🎮 WARUDO 빌드 가이드",
                [SupportedLanguage.Japanese] = "🎮 WARUDOビルドガイド"
            },
            ["warudo_compatibility"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "✅ WARUDO Compatible",
                [SupportedLanguage.Korean] = "✅ WARUDO 호환",
                [SupportedLanguage.Japanese] = "✅ WARUDO互換"
            },
            ["vrc_incompatibility"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "❌ Not compatible with VRChat",
                [SupportedLanguage.Korean] = "❌ VRChat과 호환되지 않음",
                [SupportedLanguage.Japanese] = "❌ VRChatとは互換性なし"
            },
            ["warudo_folder_requirement"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "📁 WARUDO Build Requirement",
                [SupportedLanguage.Korean] = "📁 WARUDO 빌드 요구사항",
                [SupportedLanguage.Japanese] = "📁 WARUDOビルド要件"
            },
            ["warudo_folder_instruction"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "When building for WARUDO, the 'VModular_InAvatar' folder must be moved to your build mode folder before building.",
                [SupportedLanguage.Korean] = "WARUDO에서 빌드할 때는 빌드할 모드 폴더에 'VModular_InAvatar' 폴더가 옮겨진 채로 빌드해야 합니다.",
                [SupportedLanguage.Japanese] = "WARUDOでビルドする際は、ビルドするMODフォルダに'VModular_InAvatar'フォルダが移動された状態でビルドする必要があります。"
            },
            ["warudo_script_setup"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "⚙️ First-Time Script Build Setup",
                [SupportedLanguage.Korean] = "⚙️ 첫 스크립트 빌드 설정",
                [SupportedLanguage.Japanese] = "⚙️ 初回スクリプトビルド設定"
            },
            ["warudo_script_instruction"] = new Dictionary<SupportedLanguage, string>
            {
                [SupportedLanguage.English] = "If this is your first time building scripts, go to Unity Preferences > External Tools, set 'External Script Editor' to 'Visual Studio 2019', and click 'Regenerate project files'.",
                [SupportedLanguage.Korean] = "맨 처음 스크립트를 빌드하는 경우, Unity의 Preferences > External Tools로 이동하여 'External Script Editor'를 'Visual Studio 2019'로 설정하고 'Regenerate project files' 버튼을 눌러야 합니다.",
                [SupportedLanguage.Japanese] = "初めてスクリプトをビルドする場合は、UnityのPreferences > External Toolsに移動し、'External Script Editor'を'Visual Studio 2019'に設定し、'Regenerate project files'ボタンをクリックしてください。"
            }
        };
    
    // 번역된 텍스트 가져오기
    public static string Get(string key)
    {
        if (translations.ContainsKey(key) && translations[key].ContainsKey(CurrentLanguage))
        {
            return translations[key][CurrentLanguage];
        }
        
        // 폴백: 키 이름 그대로 반환
        return key;
    }
    
    // 언어 선택 GUI 그리기
    public static void DrawLanguageSelector()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(Get("language"), GUILayout.Width(60));
        
        SupportedLanguage newLang = (SupportedLanguage)EditorGUILayout.EnumPopup(CurrentLanguage, GUILayout.Width(100));
        if (newLang != CurrentLanguage)
        {
            CurrentLanguage = newLang;
        }
        EditorGUILayout.EndHorizontal();
    }
}
