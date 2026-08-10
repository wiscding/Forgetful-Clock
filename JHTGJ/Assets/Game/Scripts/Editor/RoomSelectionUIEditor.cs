#if UNITY_EDITOR
using JHTGJ.UI;
using UnityEditor;
using UnityEngine;

namespace JHTGJ.EditorTools
{
    [CustomEditor(typeof(RoomSelectionUI))]
    public class RoomSelectionUIEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(
                "房间选择面板。可在 Hierarchy 中直接调整：\n" +
                "· SelectionPanel：面板大小、颜色、位置\n" +
                "· Title / Hint：标题与提示文字\n" +
                "· ButtonContainer：按钮间距（Vertical Layout Group）\n" +
                "· OptionTemplate：选项按钮样式（运行时克隆）\n" +
                "· Fade Duration：淡入淡出时长\n\n" +
                "若 2F/4F 等文字显示不全，请执行 JHTGJ → Fix Room Selection UI (Legacy Text)。",
                MessageType.Info);

            DrawDefaultInspector();
        }
    }
}
#endif
