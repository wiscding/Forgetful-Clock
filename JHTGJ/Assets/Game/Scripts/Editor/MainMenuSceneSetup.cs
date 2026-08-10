#if UNITY_EDITOR
using JHTGJ.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JHTGJ.EditorTools
{
    public static class MainMenuSceneSetup
    {
        [MenuItem("JHTGJ/Setup Menu And Game Scenes")]
        public static void Setup()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            CreateMainMenuScene();
            CleanupGameScene();
            ConfigureBuildSettings();

            EditorUtility.DisplayDialog(
                "场景拆分完成",
                "MainMenu 为主菜单场景\nSampleScene 为游戏场景\n\nBuild Settings 已设置：先 MainMenu，后 SampleScene",
                "OK");
        }

        static void CreateMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            foreach (var root in scene.GetRootGameObjects())
                Object.DestroyImmediate(root);

            MainMenuUICreator.BuildInCurrentScene();
            RemoveIfExists("PauseMenuCanvas");

            EditorSceneManager.SaveScene(scene, SceneLoader.MainMenuScenePath);
            Debug.Log("[JHTGJ] Saved MainMenu scene.");
        }

        static void CleanupGameScene()
        {
            var scene = EditorSceneManager.OpenScene(SceneLoader.GameScenePath, OpenSceneMode.Single);
            RemoveIfExists("MainMenuCanvas");
            RemoveIfExists("PauseMenuCanvas");
            RemoveIfExists("GameSettings");
            RemoveIfExists("GameFlow");

            var eventSystem = Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem != null)
                Object.DestroyImmediate(eventSystem.gameObject);

            PauseMenuUICreator.Create();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[JHTGJ] Cleaned UI objects from game scene.");
        }

        static void RemoveIfExists(string objectName)
        {
            var go = GameObject.Find(objectName);
            if (go != null)
                Object.DestroyImmediate(go);
        }

        static void ConfigureBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(SceneLoader.MainMenuScenePath, true),
                new EditorBuildSettingsScene(SceneLoader.GameScenePath, true)
            };
        }
    }
}
#endif
