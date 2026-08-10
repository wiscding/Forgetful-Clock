#if UNITY_EDITOR
using JHTGJ.Interaction;
using JHTGJ.Scene;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityScene = UnityEngine.SceneManagement.Scene;

namespace JHTGJ.EditorTools
{
    public static class FloorLinkNetworkSetup
    {
        public static readonly RoomType[] LinkedRooms =
        {
            RoomType.LivingRoom,
            RoomType.Hallway,
            RoomType.DiningRoom,
            RoomType.Basement,
            RoomType.Rooftop
        };

        static readonly float[] LinkSlotX = { -5f, -2f, 2f, 5f };

        [MenuItem("JHTGJ/Add Floor Link Teleports (Game Scene)")]
        public static void AddToGameSceneFromMenu()
        {
            if (!EnsureGameScene())
                return;

            var created = AddToScene(SceneManager.GetActiveScene());
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorUtility.DisplayDialog(
                "Floor Links",
                $"已在 {created} 个房间添加/更新上下层互通传送点。\n" +
                "原有门与剧情交互未改动。\n\n" +
                "可在各房间 InteractPoints/FloorLinkPoints 下调整位置。",
                "OK");
        }

        public static int AddToScene(UnityScene scene)
        {
            var count = 0;
            foreach (var roomType in LinkedRooms)
            {
                var roomRoot = FindRoomRoot(scene, roomType);
                if (roomRoot == null)
                {
                    Debug.LogWarning($"[JHTGJ] 未找到 Room_{roomType}，跳过楼层传送。");
                    continue;
                }

                var room = roomRoot.GetComponent<Room>();
                var floorY = room != null ? room.FloorY : WhiteboxRoomBuilder.FloorY;
                var folder = GetOrCreateFloorLinkFolder(roomRoot.transform);
                AddLinks(folder, roomType, floorY);
                count++;
            }

            return count;
        }

        public static void AddLinks(Transform parent, RoomType fromRoom, float floorY)
        {
            var slot = 0;
            foreach (var targetRoom in LinkedRooms)
            {
                if (targetRoom == fromRoom)
                    continue;

                var x = LinkSlotX[Mathf.Min(slot, LinkSlotX.Length - 1)];
                slot++;
                var standOffset = x < 0f ? 3f : -3f;
                var linkName = GetLinkObjectName(targetRoom);

                if (parent.Find(linkName) != null)
                    continue;

                CreateFloorLink(parent, linkName, targetRoom, x, floorY, standOffset);
            }
        }

        static GameObject FindRoomRoot(UnityScene scene, RoomType type)
        {
            var name = $"Room_{type}";
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == name)
                    return root;

                var found = FindByName(root.transform, name);
                if (found != null)
                    return found.gameObject;
            }

            return GameObject.Find(name);
        }

        static Transform FindByName(Transform parent, string name)
        {
            if (parent.name == name)
                return parent;

            for (var i = 0; i < parent.childCount; i++)
            {
                var found = FindByName(parent.GetChild(i), name);
                if (found != null)
                    return found;
            }

            return null;
        }

        static Transform GetOrCreateFloorLinkFolder(Transform roomRoot)
        {
            var interactPoints = roomRoot.Find("InteractPoints");
            if (interactPoints == null)
            {
                var go = new GameObject("InteractPoints");
                go.transform.SetParent(roomRoot, false);
                interactPoints = go.transform;
            }

            var folder = interactPoints.Find("FloorLinkPoints");
            if (folder != null)
                return folder;

            var folderGo = new GameObject("FloorLinkPoints");
            folderGo.transform.SetParent(interactPoints, false);
            return folderGo.transform;
        }

        static void CreateFloorLink(
            Transform parent,
            string objectName,
            RoomType targetRoom,
            float localX,
            float floorY,
            float standOffset)
        {
            var go = new GameObject(objectName);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(localX, floorY + 0.2f, 0f);

            var point = go.AddComponent<InteractPoint>();
            var so = new SerializedObject(point);
            so.FindProperty("interactId").stringValue = objectName;
            so.FindProperty("label").stringValue = GetLinkLabel(targetRoom);
            so.FindProperty("kind").enumValueIndex = (int)InteractionKind.ChangeRoom;
            so.FindProperty("targetRoom").enumValueIndex = (int)targetRoom;
            so.FindProperty("targetSpawnSide").enumValueIndex = (int)SpawnSide.Left;
            so.FindProperty("standXOffset").floatValue = standOffset;
            so.ApplyModifiedPropertiesWithoutUndo();
            point.ApplyMarker();

            Undo.RegisterCreatedObjectUndo(go, $"Create {objectName}");
        }

        public static string GetLinkObjectName(RoomType targetRoom) => $"FloorLink_To{targetRoom}";

        public static string GetLinkLabel(RoomType targetRoom) => $"→ {GetRoomDisplayName(targetRoom)}";

        public static string GetRoomDisplayName(RoomType type)
        {
            switch (type)
            {
                case RoomType.LivingRoom: return "客厅";
                case RoomType.Hallway: return "走廊";
                case RoomType.DiningRoom: return "餐厅";
                case RoomType.Basement: return "地下室";
                case RoomType.Rooftop: return "天台";
                default: return type.ToString();
            }
        }

        static bool EnsureGameScene()
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.name == JHTGJ.Core.SceneLoader.GameSceneName)
                return true;

            if (!EditorUtility.DisplayDialog(
                    "需要在游戏场景",
                    "楼层传送点应添加到 SampleScene。\n\n是否打开 SampleScene 并继续？",
                    "打开 SampleScene",
                    "取消"))
                return false;

            EditorSceneManager.OpenScene(JHTGJ.Core.SceneLoader.GameScenePath);
            return true;
        }
    }
}
#endif
