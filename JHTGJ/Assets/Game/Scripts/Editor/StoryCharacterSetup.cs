#if UNITY_EDITOR
using JHTGJ.Character;
using JHTGJ.Core;
using JHTGJ.Interaction;
using JHTGJ.Scene;
using JHTGJ.Story;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JHTGJ.EditorTools
{
    public static class StoryCharacterSetup
    {
        const string WifeObjectName = "StoryCharacter_Wife";
        const string WifeInteractId = "Interact_Partner";
        const string WifeIdleSpritePath = "Assets/Art/Characters/女角色常服.png";

        [MenuItem("JHTGJ/Setup Story Character Wife (Game Scene)")]
        public static void SetupWifeFromMenu()
        {
            if (!EnsureGameScene())
                return;

            EnsureWifeCharacter();
            DisableDuplicateInteractMarkers(WifeInteractId);

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorUtility.DisplayDialog(
                "Story Character",
                "已创建 StoryCharacter_Wife。\n\n" +
                "请在 DayStorySchedule 各阶段的 Character Presences 中配置：\n" +
                "· Interact Id: Interact_Partner\n" +
                "· Room / Local X / 形象 Sprite 或 Appearance Profile\n\n" +
                "同 Interact Id 的旧黄色 InteractPoint 已自动禁用。",
                "OK");
        }

        public static StoryCharacterInteractPoint EnsureWifeCharacter()
        {
            var systems = GameObject.Find("GameSystems") ?? new GameObject("GameSystems");
            var existing = Object.FindObjectOfType<StoryCharacterInteractPoint>(true);
            if (existing != null && existing.gameObject.name != WifeObjectName)
                existing = null;

            var go = GameObject.Find(WifeObjectName);
            if (go == null)
            {
                go = new GameObject(WifeObjectName);
                go.transform.SetParent(systems.transform, false);
            }

            var point = go.GetComponent<StoryCharacterInteractPoint>();
            if (point == null)
                point = go.AddComponent<StoryCharacterInteractPoint>();

            var so = new SerializedObject(point);
            so.FindProperty("interactId").stringValue = WifeInteractId;
            so.FindProperty("label").stringValue = "妻子";
            so.FindProperty("kind").enumValueIndex = (int)InteractionKind.TalkToPartner;
            so.FindProperty("standXOffset").floatValue = -2.5f;
            so.FindProperty("defaultIdleSprite").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Sprite>(WifeIdleSpritePath);
            so.FindProperty("targetWorldHeight").floatValue = 7.5f;
            SetFacing(so.FindProperty("facing"), FacingDirection.Left);
            so.ApplyModifiedPropertiesWithoutUndo();

            go.SetActive(false);
            return point;
        }

        public static void RemoveDuplicateWifeCharacters()
        {
            StoryCharacterInteractPoint keep = null;
            foreach (var point in Object.FindObjectsOfType<StoryCharacterInteractPoint>(true))
            {
                if (point.gameObject.name != WifeObjectName)
                    continue;

                if (keep == null)
                {
                    keep = point;
                    continue;
                }

                Object.DestroyImmediate(point.gameObject);
            }
        }

        public static void DisableDuplicateInteractMarkers(string interactId)
        {
            foreach (var point in Object.FindObjectsOfType<InteractPoint>(true))
            {
                if (point.InteractId != interactId)
                    continue;

                point.gameObject.SetActive(false);
            }
        }

        static bool EnsureGameScene()
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.name == SceneLoader.GameSceneName)
                return true;

            if (!EditorUtility.DisplayDialog(
                    "需要在游戏场景",
                    "故事角色应配置在 SampleScene。\n\n是否打开 SampleScene 并继续？",
                    "打开 SampleScene",
                    "取消"))
                return false;

            EditorSceneManager.OpenScene(SceneLoader.GameScenePath);
            return true;
        }

        static void SetFacing(SerializedProperty property, FacingDirection facing)
        {
            for (var i = 0; i < property.enumNames.Length; i++)
            {
                if (property.enumNames[i] != facing.ToString())
                    continue;

                property.enumValueIndex = i;
                return;
            }
        }
    }
}
#endif
