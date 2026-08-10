#if UNITY_EDITOR
using JHTGJ.Interaction;
using UnityEditor;
using UnityEngine;

namespace JHTGJ.EditorTools
{
    [CustomEditor(typeof(RoomSelectorInteractPoint))]
    public class RoomSelectorInteractPointEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(
                "按 E 后会淡出并弹出房间选择面板。\n" +
                "在 Destinations 列表中添加/删除可前往的房间。\n" +
                "拖动物体位置，并调整 Stand X Offset 让圆球对齐玩家站位。",
                MessageType.Info);

            DrawDefaultInspector();
        }
    }
}
#endif
