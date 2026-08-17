using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MissingScriptCleaner : EditorWindow
{
    private readonly List<GameObject> _prefabs = new();

    [MenuItem("Tools/Prefab/Missing Script Cleaner")]
    private static void Open()
    {
        GetWindow<MissingScriptCleaner>("Missing Script Cleaner");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField(
            "Prefab Missing Script Cleaner",
            EditorStyles.boldLabel
        );

        EditorGUILayout.HelpBox(
            "Missing Script를 제거할 Prefab을 아래 영역으로 드래그 앤 드롭하세요.",
            MessageType.Info
        );

        EditorGUILayout.Space(5);

        // Drag & Drop 영역
        Rect dropArea = GUILayoutUtility.GetRect(
            0,
            100,
            GUILayout.ExpandWidth(true)
        );

        GUI.Box(
            dropArea,
            "Prefab을 여기에 드래그 앤 드롭",
            EditorStyles.helpBox
        );

        HandleDragAndDrop(dropArea);

        EditorGUILayout.Space(10);

        // 등록된 Prefab 목록
        EditorGUILayout.LabelField(
            $"등록된 Prefab ({_prefabs.Count})",
            EditorStyles.boldLabel
        );

        for (int i = 0; i < _prefabs.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();

            _prefabs[i] = (GameObject)EditorGUILayout.ObjectField(
                _prefabs[i],
                typeof(GameObject),
                false
            );

            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                _prefabs.RemoveAt(i);
                GUIUtility.ExitGUI();
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("목록 비우기"))
        {
            _prefabs.Clear();
        }

        GUI.enabled = _prefabs.Count > 0;

        if (GUILayout.Button("Missing Script 전부 제거"))
        {
            RemoveMissingScripts();
        }

        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();
    }

    private void HandleDragAndDrop(Rect dropArea)
    {
        Event currentEvent = Event.current;

        if (!dropArea.Contains(currentEvent.mousePosition))
        {
            return;
        }

        switch (currentEvent.type)
        {
            case EventType.DragUpdated:
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                currentEvent.Use();
                break;

            case EventType.DragPerform:
                DragAndDrop.AcceptDrag();

                foreach (Object draggedObject in DragAndDrop.objectReferences)
                {
                    if (draggedObject is not GameObject gameObject)
                    {
                        continue;
                    }

                    string path = AssetDatabase.GetAssetPath(gameObject);

                    if (string.IsNullOrEmpty(path))
                    {
                        continue;
                    }

                    if (!PrefabUtility.IsPartOfPrefabAsset(gameObject))
                    {
                        continue;
                    }

                    if (!_prefabs.Contains(gameObject))
                    {
                        _prefabs.Add(gameObject);
                    }
                }

                currentEvent.Use();
                Repaint();
                break;
        }
    }

    private void RemoveMissingScripts()
    {
        int totalRemoved = 0;
        int processedPrefabs = 0;

        try
        {
            foreach (GameObject prefab in _prefabs)
            {
                if (prefab == null)
                {
                    continue;
                }

                string prefabPath = AssetDatabase.GetAssetPath(prefab);

                if (string.IsNullOrEmpty(prefabPath))
                {
                    continue;
                }

                GameObject prefabRoot =
                    PrefabUtility.LoadPrefabContents(prefabPath);

                try
                {
                    int removedCount =
                        RemoveMissingScriptsRecursive(prefabRoot);

                    if (removedCount > 0)
                    {
                        PrefabUtility.SaveAsPrefabAsset(
                            prefabRoot,
                            prefabPath
                        );
                    }

                    totalRemoved += removedCount;
                    processedPrefabs++;

                    Debug.Log(
                        $"[Missing Script Cleaner] " +
                        $"{prefab.name}: {removedCount}개 제거"
                    );
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(prefabRoot);
                }
            }
        }
        finally
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        Debug.Log(
            $"[Missing Script Cleaner] 완료\n" +
            $"처리한 Prefab: {processedPrefabs}개\n" +
            $"제거한 Missing Script: {totalRemoved}개"
        );

        EditorUtility.DisplayDialog(
            "완료",
            $"Prefab {processedPrefabs}개 처리\n" +
            $"Missing Script {totalRemoved}개 제거",
            "확인"
        );
    }

    private static int RemoveMissingScriptsRecursive(GameObject root)
    {
        int removedCount = 0;

        Transform[] transforms =
            root.GetComponentsInChildren<Transform>(true);

        foreach (Transform transform in transforms)
        {
            GameObject gameObject = transform.gameObject;

            int missingCount =
                GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(
                    gameObject
                );

            if (missingCount <= 0)
            {
                continue;
            }

            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(
                gameObject
            );

            removedCount += missingCount;
        }

        return removedCount;
    }
}