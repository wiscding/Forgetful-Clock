#if UNITY_EDITOR
using JHTGJ.Character;
using JHTGJ.Interaction;
using JHTGJ.Scene;
using JHTGJ.Story;
using JHTGJ.UI;
using UnityEditor;
using UnityEngine;

namespace JHTGJ.EditorTools
{
    public static class ProtagonistBuilder
    {
        const string ProtagonistName = "Protagonist";

        [MenuItem("GameObject/JHTGJ/Create Protagonist", false, 0)]
        public static void CreateFromMenu()
        {
            var protagonist = CreateOrUpdate(null, null);
            Undo.RegisterCreatedObjectUndo(protagonist, "Create Protagonist");
            Selection.activeGameObject = protagonist;
            EditorGUIUtility.PingObject(protagonist);
        }

        public static GameObject CreateOrUpdate(Room startRoom, VillaSceneManager sceneManager)
        {
            var protagonist = GameObject.Find(ProtagonistName);
            if (protagonist == null)
            {
                protagonist = new GameObject(ProtagonistName);
                protagonist.transform.localScale = Vector3.one;
            }
            else if (protagonist.transform.localScale.y > 1.01f)
            {
                protagonist.transform.localScale = Vector3.one;
            }

            if (protagonist.GetComponent<SpriteRenderer>() == null)
                protagonist.AddComponent<SpriteRenderer>();

            var movement = GetOrAdd<SideViewCharacterController>(protagonist);
            GetOrAdd<InputHandler>(protagonist);
            GetOrAdd<ProtagonistController>(protagonist);
            GetOrAdd<CharacterSpriteAnimator>(protagonist);
            var interact = GetOrAdd<InteractController>(protagonist);

            movement.EnsureVisibleInEditor();

            if (startRoom != null)
            {
                var moveSo = new SerializedObject(movement);
                moveSo.FindProperty("room").objectReferenceValue = startRoom;
                moveSo.FindProperty("moveSpeed").floatValue = 4f;
                moveSo.ApplyModifiedPropertiesWithoutUndo();
                protagonist.transform.position = startRoom.PlayerSpawnPosition;
            }

            var interactSo = new SerializedObject(interact);
            interactSo.FindProperty("protagonist").objectReferenceValue = movement;
            if (sceneManager != null)
                interactSo.FindProperty("sceneManager").objectReferenceValue = sceneManager;

            var storyManager = Object.FindObjectOfType<StoryEventTreeManager>();
            if (storyManager != null)
                interactSo.FindProperty("storyManager").objectReferenceValue = storyManager;

            var roomSelectionUi = Object.FindObjectOfType<RoomSelectionUI>(true);
            if (roomSelectionUi != null)
                interactSo.FindProperty("roomSelectionUI").objectReferenceValue = roomSelectionUi;

            var interactionPromptUi = Object.FindObjectOfType<InteractionPromptUI>(true);
            if (interactionPromptUi != null)
                interactSo.FindProperty("interactionPromptUI").objectReferenceValue = interactionPromptUi;

            interactSo.ApplyModifiedPropertiesWithoutUndo();

            return protagonist;
        }

        static T GetOrAdd<T>(GameObject go) where T : Component
        {
            var component = go.GetComponent<T>();
            return component != null ? component : go.AddComponent<T>();
        }
    }
}
#endif
