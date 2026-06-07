using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ThirdTutorialHumanGeneratorSceneUtility
{
    private const string ThirdTutorialScenePath = "Assets/SliceSufferServe_Project/Scenes/Actual/ThirdTutorial.unity";
    private const string KnightHumanGuid = "a4f53d1498984805b723d7217d2f1d01";

    [MenuItem("Tools/Slice Suffer Serve/Tutorial/Apply Third Tutorial Human Defaults")]
    public static void ApplyKnightOnlyDefaults()
    {
        EditorSceneManager.OpenScene(ThirdTutorialScenePath, OpenSceneMode.Single);

        string knightPath = AssetDatabase.GUIDToAssetPath(KnightHumanGuid);
        HumanBody knightHuman = AssetDatabase.LoadAssetAtPath<HumanBody>(knightPath);
        if (knightHuman == null)
        {
            throw new System.InvalidOperationException($"Could not load knight human prefab from GUID {KnightHumanGuid}.");
        }

        HumanGenerator[] generators = Object.FindObjectsByType<HumanGenerator>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < generators.Length; i++)
        {
            SerializedObject serializedGenerator = new SerializedObject(generators[i]);
            SerializedProperty humanPrefabs = serializedGenerator.FindProperty("humanPrefabs");
            humanPrefabs.arraySize = 1;
            humanPrefabs.GetArrayElementAtIndex(0).objectReferenceValue = knightHuman;
            serializedGenerator.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(generators[i]);
            PrefabUtility.RecordPrefabInstancePropertyModifications(generators[i]);
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"Applied knight-only HumanGenerator defaults to {generators.Length} generators in ThirdTutorial.");
    }
}
