using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TowerDefense.EditorTools
{
    /// <summary>
    /// 지정한 폴더 안에서 프로젝트 어디에서도 참조되지 않는(미사용) 에셋과 빈 폴더를 찾아 삭제하는 에디터 툴.
    ///
    /// 미사용 에셋 판단 기준:
    /// - Build Settings에 등록된 모든 씬의 재귀적 의존성
    /// - 대상 폴더 "바깥"에 있는 모든 에셋의 재귀적 의존성
    /// 위 두 집합("사용됨") 어디에도 포함되지 않는, 대상 폴더 안의 에셋을 "미사용"으로 간주한다.
    ///
    /// 빈 폴더 판단 기준:
    /// - (.meta 제외) 파일이 하나도 없고, 하위 폴더도 전부 비어 있는(재귀적으로) 폴더.
    /// - 중첩된 빈 폴더는 가장 바깥쪽(최상위) 빈 폴더만 목록에 표시한다.
    ///   (최상위 폴더를 삭제하면 그 안의 빈 하위 폴더도 함께 삭제되기 때문)
    ///
    /// 주의:
    /// - Resources 폴더에서 문자열 경로로 동적 로드되는 에셋, Addressables, 리플렉션으로만 참조되는
    ///   타입 등은 정적 의존성 분석으로 잡히지 않으므로 삭제 전 목록을 반드시 눈으로 확인할 것.
    /// - .cs 스크립트는 기본적으로 후보에서 제외한다(옵션으로 포함 가능하지만 위험도가 높음).
    /// </summary>
    public class UnusedAssetCleaner : EditorWindow
    {
        const string PrefsKeyFolder = "UnusedAssetCleaner.TargetFolder";
        const string PrefsKeyIncludeScripts = "UnusedAssetCleaner.IncludeScripts";

        string _targetFolder = "Assets";
        bool _includeScripts;
        bool _hasScanned;
        Vector2 _scrollPos;

        readonly List<string> _unusedAssets = new List<string>();
        readonly Dictionary<string, bool> _assetSelection = new Dictionary<string, bool>();

        readonly List<string> _emptyFolders = new List<string>();
        readonly Dictionary<string, bool> _folderSelection = new Dictionary<string, bool>();

        [MenuItem("Tools/Asset Cleanup/미사용 에셋 정리...")]
        static void Open()
        {
            var window = GetWindow<UnusedAssetCleaner>("미사용 에셋 정리");
            window.minSize = new Vector2(560, 460);
        }

        void OnEnable()
        {
            _targetFolder = EditorPrefs.GetString(PrefsKeyFolder, "Assets");
            _includeScripts = EditorPrefs.GetBool(PrefsKeyIncludeScripts, false);
        }

        void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "선택한 폴더 안에서, 프로젝트의 씬/프리팹/머티리얼 등 어디에서도 참조되지 않는 에셋과 빈 폴더를 찾습니다.\n" +
                "동적 로드(Resources.Load, Addressables, 리플렉션 등)로만 참조되는 에셋은 잡히지 않을 수 있으니 " +
                "삭제 전 목록을 반드시 확인하세요. 삭제는 휴지통을 거치지 않고 즉시 영구적으로 이루어지며 복구할 수 없습니다.",
                MessageType.Warning);

            EditorGUILayout.Space();
            DrawFolderSelector();

            EditorGUI.BeginChangeCheck();
            bool includeScripts = EditorGUILayout.ToggleLeft(
                "스크립트(.cs) 파일도 검사 대상에 포함 (주의: 오탐 가능성 높음)", _includeScripts);
            if (EditorGUI.EndChangeCheck())
            {
                _includeScripts = includeScripts;
                EditorPrefs.SetBool(PrefsKeyIncludeScripts, _includeScripts);
            }

            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(!AssetDatabase.IsValidFolder(_targetFolder)))
            {
                if (GUILayout.Button("스캔", GUILayout.Height(28)))
                {
                    Scan();
                }
            }

            EditorGUILayout.Space();

            if (_hasScanned)
            {
                DrawResults();
            }
        }

        void DrawFolderSelector()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("대상 폴더", GUILayout.Width(70));
            EditorGUILayout.SelectableLabel(_targetFolder, EditorStyles.textField,
                GUILayout.Height(EditorGUIUtility.singleLineHeight));

            if (GUILayout.Button("선택...", GUILayout.Width(70)))
            {
                string absoluteStart = string.IsNullOrEmpty(_targetFolder) || !AssetDatabase.IsValidFolder(_targetFolder)
                    ? Application.dataPath
                    : Path.GetFullPath(_targetFolder);

                string picked = EditorUtility.OpenFolderPanel("대상 폴더 선택", absoluteStart, "");
                if (!string.IsNullOrEmpty(picked))
                {
                    string projectRelative = ToProjectRelativePath(picked);
                    if (projectRelative == null)
                    {
                        EditorUtility.DisplayDialog("잘못된 폴더", "프로젝트의 Assets 폴더 내부만 선택할 수 있습니다.", "확인");
                    }
                    else
                    {
                        _targetFolder = projectRelative;
                        EditorPrefs.SetString(PrefsKeyFolder, _targetFolder);
                        ClearResults();
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            if (!AssetDatabase.IsValidFolder(_targetFolder))
            {
                EditorGUILayout.HelpBox("유효한 Assets 하위 폴더를 선택하세요.", MessageType.Warning);
            }
        }

        static string ToProjectRelativePath(string absolutePath)
        {
            string dataPath = Application.dataPath.Replace('\\', '/');
            string normalized = absolutePath.Replace('\\', '/');

            if (normalized == dataPath)
            {
                return "Assets";
            }

            if (normalized.StartsWith(dataPath + "/"))
            {
                return "Assets" + normalized.Substring(dataPath.Length);
            }

            return null;
        }

        void ClearResults()
        {
            _hasScanned = false;
            _unusedAssets.Clear();
            _assetSelection.Clear();
            _emptyFolders.Clear();
            _folderSelection.Clear();
        }

        void DrawResults()
        {
            DrawUnusedAssetsSection();
            EditorGUILayout.Space();
            DrawEmptyFoldersSection();
            EditorGUILayout.Space();

            int totalSelected = _assetSelection.Count(kv => kv.Value) + _folderSelection.Count(kv => kv.Value);
            using (new EditorGUI.DisabledScope(totalSelected == 0))
            {
                GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
                if (GUILayout.Button($"선택한 {totalSelected}개 영구 삭제", GUILayout.Height(30)))
                {
                    DeleteSelected();
                }
                GUI.backgroundColor = Color.white;
            }
        }

        void DrawUnusedAssetsSection()
        {
            EditorGUILayout.LabelField($"미사용 에셋: {_unusedAssets.Count}개", EditorStyles.boldLabel);

            if (_unusedAssets.Count == 0)
            {
                EditorGUILayout.HelpBox("미사용 에셋이 없습니다.", MessageType.Info);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("전체 선택", GUILayout.Width(80)))
            {
                SetAllSelection(_unusedAssets, _assetSelection, true);
            }
            if (GUILayout.Button("전체 해제", GUILayout.Width(80)))
            {
                SetAllSelection(_unusedAssets, _assetSelection, false);
            }
            GUILayout.FlexibleSpace();
            int selectedCount = _assetSelection.Count(kv => kv.Value);
            EditorGUILayout.LabelField($"{selectedCount}개 선택됨", GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Height(220));
            foreach (string path in _unusedAssets)
            {
                EditorGUILayout.BeginHorizontal();

                bool current = _assetSelection.TryGetValue(path, out bool value) && value;
                bool updated = EditorGUILayout.Toggle(current, GUILayout.Width(18));
                if (updated != current)
                {
                    _assetSelection[path] = updated;
                }

                var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                EditorGUILayout.ObjectField(obj, typeof(Object), false);

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        void DrawEmptyFoldersSection()
        {
            EditorGUILayout.LabelField($"빈 폴더: {_emptyFolders.Count}개", EditorStyles.boldLabel);

            if (_emptyFolders.Count == 0)
            {
                EditorGUILayout.HelpBox("빈 폴더가 없습니다.", MessageType.Info);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("전체 선택", GUILayout.Width(80)))
            {
                SetAllSelection(_emptyFolders, _folderSelection, true);
            }
            if (GUILayout.Button("전체 해제", GUILayout.Width(80)))
            {
                SetAllSelection(_emptyFolders, _folderSelection, false);
            }
            GUILayout.FlexibleSpace();
            int selectedCount = _folderSelection.Count(kv => kv.Value);
            EditorGUILayout.LabelField($"{selectedCount}개 선택됨", GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();

            foreach (string path in _emptyFolders)
            {
                EditorGUILayout.BeginHorizontal();

                bool current = _folderSelection.TryGetValue(path, out bool value) && value;
                bool updated = EditorGUILayout.Toggle(current, GUILayout.Width(18));
                if (updated != current)
                {
                    _folderSelection[path] = updated;
                }

                EditorGUILayout.LabelField(path);

                EditorGUILayout.EndHorizontal();
            }
        }

        static void SetAllSelection(List<string> paths, Dictionary<string, bool> selection, bool value)
        {
            foreach (string path in paths)
            {
                selection[path] = value;
            }
        }

        void Scan()
        {
            ClearResults();

            try
            {
                ScanUnusedAssets();
                ScanEmptyFolders();
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            _hasScanned = true;
        }

        void ScanUnusedAssets()
        {
            var usedAssets = new HashSet<string>();

            // 1) Build Settings에 등록된 씬들의 재귀적 의존성
            var scenePaths = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();

            for (int i = 0; i < scenePaths.Length; i++)
            {
                EditorUtility.DisplayProgressBar("스캔 중", $"씬 의존성 분석: {scenePaths[i]}",
                    (float)i / (scenePaths.Length + 1));

                foreach (string dep in AssetDatabase.GetDependencies(scenePaths[i], true))
                {
                    usedAssets.Add(dep);
                }
            }

            // 2) 대상 폴더 "바깥"에 있는 모든 에셋의 재귀적 의존성
            string normalizedTargetFolder = _targetFolder.Replace('\\', '/').TrimEnd('/') + "/";

            var outsideAssetPaths = AssetDatabase.GetAllAssetPaths()
                .Where(p => p.StartsWith("Assets/"))
                .Where(p => !p.StartsWith(normalizedTargetFolder) && p != _targetFolder)
                .Where(p => !AssetDatabase.IsValidFolder(p))
                .ToArray();

            for (int i = 0; i < outsideAssetPaths.Length; i++)
            {
                if (i % 25 == 0)
                {
                    EditorUtility.DisplayProgressBar("스캔 중",
                        $"외부 에셋 의존성 분석 ({i}/{outsideAssetPaths.Length})",
                        (float)i / outsideAssetPaths.Length);
                }

                usedAssets.Add(outsideAssetPaths[i]);
                foreach (string dep in AssetDatabase.GetDependencies(outsideAssetPaths[i], true))
                {
                    usedAssets.Add(dep);
                }
            }

            // 3) 대상 폴더 안의 에셋 중, 사용됨 목록에 없는 것을 미사용으로 판정
            var folderAssetPaths = AssetDatabase.FindAssets("", new[] { _targetFolder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Distinct()
                .Where(p => !AssetDatabase.IsValidFolder(p))
                .Where(p => _includeScripts || !p.EndsWith(".cs"))
                .Where(p => !p.EndsWith(".asmdef") && !p.EndsWith(".asmref"))
                .OrderBy(p => p);

            foreach (string path in folderAssetPaths)
            {
                if (!usedAssets.Contains(path))
                {
                    _unusedAssets.Add(path);
                    _assetSelection[path] = false;
                }
            }
        }

        void ScanEmptyFolders()
        {
            if (!AssetDatabase.IsValidFolder(_targetFolder))
            {
                return;
            }

            string absoluteTargetFolder = Path.GetFullPath(_targetFolder).Replace('\\', '/');
            string assetsRoot = Application.dataPath.Replace('\\', '/');

            var discovered = new List<string>();
            IsFolderEmptyRecursive(absoluteTargetFolder, absoluteTargetFolder, assetsRoot, discovered);

            // 중첩된 빈 폴더는 최상위 폴더만 남긴다 (부모를 지우면 자식도 함께 지워지므로)
            var topMost = discovered
                .Where(path => !discovered.Any(other => other != path && IsSubPathOf(path, other)))
                .OrderBy(p => p)
                .ToList();

            foreach (string path in topMost)
            {
                _emptyFolders.Add(path);
                _folderSelection[path] = false;
            }
        }

        /// <summary>
        /// absoluteFolderPath가 비어있는지(재귀적으로, .meta 제외) 판정.
        /// 대상 폴더 자기 자신(rootFolderPath)은 emptyFoldersOut에 추가하지 않고,
        /// 그 하위 폴더들만 후보로 추가한다.
        /// </summary>
        static bool IsFolderEmptyRecursive(string absoluteFolderPath, string rootFolderPath, string assetsRoot, List<string> emptyFoldersOut)
        {
            bool isRoot = string.Equals(absoluteFolderPath.TrimEnd('/'), rootFolderPath.TrimEnd('/'));
            bool isEmpty = true;

            string[] entries;
            try
            {
                entries = Directory.GetFileSystemEntries(absoluteFolderPath);
            }
            catch (IOException)
            {
                return true;
            }

            foreach (string entry in entries)
            {
                string normalizedEntry = entry.Replace('\\', '/');

                if (normalizedEntry.EndsWith(".meta"))
                {
                    continue;
                }

                if (Directory.Exists(normalizedEntry))
                {
                    bool subEmpty = IsFolderEmptyRecursive(normalizedEntry, rootFolderPath, assetsRoot, emptyFoldersOut);
                    if (!subEmpty)
                    {
                        isEmpty = false;
                    }
                }
                else
                {
                    // 실제 파일이 하나라도 있으면 비어있지 않음
                    isEmpty = false;
                }
            }

            if (isEmpty && !isRoot)
            {
                string relative = "Assets" + absoluteFolderPath.Substring(assetsRoot.Length);
                emptyFoldersOut.Add(relative);
            }

            return isEmpty;
        }

        static bool IsSubPathOf(string path, string potentialAncestor)
        {
            string normalizedAncestor = potentialAncestor.TrimEnd('/') + "/";
            return path.StartsWith(normalizedAncestor);
        }

        void DeleteSelected()
        {
            var assetsToDelete = _assetSelection.Where(kv => kv.Value).Select(kv => kv.Key).ToList();
            var foldersToDelete = _folderSelection.Where(kv => kv.Value).Select(kv => kv.Key).ToList();
            int total = assetsToDelete.Count + foldersToDelete.Count;

            if (total == 0)
            {
                return;
            }

            var preview = assetsToDelete.Concat(foldersToDelete).Take(15).ToList();
            bool confirmed = EditorUtility.DisplayDialog(
                "영구 삭제 확인",
                $"선택한 {total}개 항목(에셋 {assetsToDelete.Count}개, 빈 폴더 {foldersToDelete.Count}개)을 " +
                "휴지통을 거치지 않고 즉시 영구 삭제합니다.\n이 작업은 되돌릴 수 없습니다.\n\n" +
                string.Join("\n", preview) + (total > preview.Count ? $"\n... 외 {total - preview.Count}개" : ""),
                "영구 삭제", "취소");

            if (!confirmed)
            {
                return;
            }

            int deletedCount = 0;

            foreach (string path in assetsToDelete)
            {
                if (AssetDatabase.DeleteAsset(path))
                {
                    deletedCount++;
                }
                else
                {
                    Debug.LogWarning($"[UnusedAssetCleaner] 삭제 실패: {path}");
                }
            }

            // 폴더는 깊은 경로부터 지워야 상위가 먼저 사라져서 생기는 오류를 피할 수 있다
            foreach (string path in foldersToDelete.OrderByDescending(p => p.Length))
            {
                if (AssetDatabase.DeleteAsset(path))
                {
                    deletedCount++;
                }
                else
                {
                    Debug.LogWarning($"[UnusedAssetCleaner] 폴더 삭제 실패: {path}");
                }
            }

            AssetDatabase.Refresh();

            _unusedAssets.RemoveAll(p => assetsToDelete.Contains(p));
            foreach (string path in assetsToDelete)
            {
                _assetSelection.Remove(path);
            }

            _emptyFolders.RemoveAll(p => foldersToDelete.Contains(p));
            foreach (string path in foldersToDelete)
            {
                _folderSelection.Remove(path);
            }

            Debug.Log($"[UnusedAssetCleaner] {deletedCount}개 항목을 영구 삭제했습니다.");
        }
    }
}
