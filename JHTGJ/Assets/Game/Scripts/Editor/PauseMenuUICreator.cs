#if UNITY_EDITOR
using JHTGJ.Core;
using JHTGJ.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JHTGJ.EditorTools
{
    public static class PauseMenuUICreator
    {
        [MenuItem("JHTGJ/Create Pause Menu UI (Game Scene)")]
        public static void Create()
        {
            CreateInternal(forceRecreate: true);
        }

        [MenuItem("JHTGJ/Create Pause Menu UI If Missing (Game Scene)")]
        public static void CreateIfMissing()
        {
            CreateInternal(forceRecreate: false);
        }

        public static void EnsureInGameScene()
        {
            CreateInternal(forceRecreate: false);
        }

        public static void CreateInternal(bool forceRecreate)
        {
            if (!EnsureGameScene())
                return;

            var existing = GameObject.Find("PauseMenuCanvas");
            if (existing != null && !forceRecreate)
            {
                if (!existing.activeSelf)
                    existing.SetActive(true);

                if (PauseMenuLegacyTextUtility.NeedsLegacyConversion(existing))
                {
                    PauseMenuUIFixer.FixExisting(existing);
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                    Debug.Log("[JHTGJ] PauseMenuCanvas converted from TMP to Legacy Text.");
                }
                else
                {
                    Debug.Log("[JHTGJ] PauseMenuCanvas already exists; skipped to preserve scene edits.");
                }

                Selection.activeGameObject = existing;
                EditorGUIUtility.PingObject(existing);
                return;
            }

            if (existing != null)
                Object.DestroyImmediate(existing);

            MainMenuUICreator.EnsureMainCamera();
            PauseMenuBuilder.Build(Object.FindObjectOfType<JHTGJ.Scene.VillaSceneManager>());

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            var canvasGo = GameObject.Find("PauseMenuCanvas");
            if (canvasGo != null)
            {
                Selection.activeGameObject = canvasGo;
                EditorGUIUtility.PingObject(canvasGo);
            }

            Debug.Log(forceRecreate
                ? "[JHTGJ] Pause Menu UI recreated in game scene."
                : "[JHTGJ] Pause Menu UI created in game scene.");
        }

        static bool EnsureGameScene()
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.name == SceneLoader.GameSceneName)
                return true;

            if (!EditorUtility.DisplayDialog(
                    "需要在游戏场景",
                    "暂停菜单应放在 SampleScene（游戏场景），不在 MainMenu。\n\n是否打开 SampleScene 并继续？",
                    "打开 SampleScene",
                    "取消"))
                return false;

            EditorSceneManager.OpenScene(SceneLoader.GameScenePath);
            return true;
        }
    }
}
#endif
