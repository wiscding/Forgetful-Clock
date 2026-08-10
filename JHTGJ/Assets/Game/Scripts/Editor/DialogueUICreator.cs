#if UNITY_EDITOR
using JHTGJ.Core;
using JHTGJ.Story;
using JHTGJ.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JHTGJ.EditorTools
{
    public static class DialogueUICreator
    {
        [MenuItem("JHTGJ/Create Dialogue UI (Game Scene)")]
        public static void Create()
        {
            CreateInternal(forceRecreate: true);
        }

        [MenuItem("JHTGJ/Create Dialogue UI If Missing (Game Scene)")]
        public static void CreateIfMissing()
        {
            CreateInternal(forceRecreate: false);
        }

        static void CreateInternal(bool forceRecreate)
        {
            if (!EnsureGameScene())
                return;

            var existing = GameObject.Find("DialogueCanvas");
            if (existing != null && !forceRecreate)
            {
                Debug.Log("[JHTGJ] DialogueCanvas already exists; skipped.");
                Selection.activeGameObject = existing;
                EditorGUIUtility.PingObject(existing);
                return;
            }

            MainMenuUICreator.EnsureMainCamera();

            var schedule = AssetDatabase.LoadAssetAtPath<DayStorySchedule>(DayStoryScheduleCreator.DefaultAssetPath);
            var canvas = DialogueUIBuilder.Build(schedule).gameObject;

            DialogueUIFixer.FixExisting(canvas);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            if (canvas != null)
            {
                Selection.activeGameObject = canvas;
                EditorGUIUtility.PingObject(canvas);
            }

            Debug.Log(forceRecreate
                ? "[JHTGJ] Dialogue UI recreated in game scene."
                : "[JHTGJ] Dialogue UI created in game scene.");
        }

        static bool EnsureGameScene()
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.name == SceneLoader.GameSceneName)
                return true;

            if (!EditorUtility.DisplayDialog(
                    "需要在游戏场景",
                    "对话 UI 应放在 SampleScene（游戏场景）。\n\n是否打开 SampleScene 并继续？",
                    "打开 SampleScene",
                    "取消"))
                return false;

            EditorSceneManager.OpenScene(SceneLoader.GameScenePath);
            return true;
        }
    }
}
#endif
