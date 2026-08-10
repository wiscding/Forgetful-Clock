#if UNITY_EDITOR
using JHTGJ.Story;
using UnityEditor;
using UnityEngine;

namespace JHTGJ.EditorTools
{
    [CustomEditor(typeof(StoryCampaign))]
    public class StoryCampaignEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(
                "多天剧情配置：\n" +
                "· Days 列表第 0 项 = 第 1 天，第 1 项 = 第 2 天，以此类推\n" +
                "· 每一项拖入一个 Day Story Schedule 资产\n" +
                "· 玩完一天的所有阶段后，自动进入下一天的 Schedule\n" +
                "· Loop After Last Day：全部天数走完后是否从第 1 天重新循环\n\n" +
                "添加新一天：Project 右键 → Create → JHTGJ → Day Story Schedule，\n" +
                "填好阶段与对话后，拖入 Days 列表末尾。",
                MessageType.Info);

            DrawDefaultInspector();
        }
    }
}
#endif
