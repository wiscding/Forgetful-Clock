#if UNITY_EDITOR
using JHTGJ.Character;
using JHTGJ.Interaction;
using JHTGJ.Scene;
using UnityEditor;
using UnityEngine;

namespace JHTGJ.EditorTools
{
    [CustomEditor(typeof(StoryCharacterInteractPoint))]
    public class StoryCharacterInteractPointEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(
                "故事角色互动点：在 DayStorySchedule 当前阶段的 Character Presences 中配置出现位置与形象。\n" +
                "Interact Id 需与阶段 Events 中的 Event Id 一致（如 Interact_Partner）。\n" +
                "玩家靠近 Stand X 位置按 E 进入对话。",
                MessageType.Info);

            DrawDefaultInspector();

            if (!GUILayout.Button("预览默认形象"))
                return;

            foreach (var targetObject in targets)
            {
                if (targetObject is not StoryCharacterInteractPoint point)
                    continue;

                var room = point.GetComponentInParent<Room>();
                if (room == null)
                    room = Object.FindObjectOfType<Room>();

                var profile = serializedObject.FindProperty("defaultAppearanceProfile").objectReferenceValue
                    as CharacterAppearanceProfile;
                var idle = serializedObject.FindProperty("defaultIdleSprite").objectReferenceValue as Sprite;
                var facingProp = serializedObject.FindProperty("facing");
                var facing = (FacingDirection)System.Enum.Parse(
                    typeof(FacingDirection),
                    facingProp.enumNames[facingProp.enumValueIndex]);

                point.ApplyAppearance(profile, idle, room);
                point.ApplyFacing(facing);
            }
        }
    }
}
#endif
