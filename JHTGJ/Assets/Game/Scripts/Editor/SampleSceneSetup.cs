#if UNITY_EDITOR
using System.Collections.Generic;
using JHTGJ.Character;
using JHTGJ.Interaction;
using JHTGJ.Scene;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JHTGJ.EditorTools
{
    public static class SampleSceneSetup
    {
        const string ScenePath = "Assets/Game/Scenes/SampleScene.unity";

        static readonly RoomType[] AllRoomTypes =
        {
            RoomType.Kitchen,
            RoomType.Bedroom,
            RoomType.Basement,
            RoomType.FrontHall,
            RoomType.LivingRoom,
            RoomType.DiningRoom,
            RoomType.Bathroom,
            RoomType.Hallway,
            RoomType.Storage,
            RoomType.Rooftop,
            RoomType.BackGarden
        };

        [MenuItem("JHTGJ/Setup Villa Whitebox Scene")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            try
            {
                var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                CleanupLegacyObjects(scene);
                var createdCount = BuildVilla();
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);

                var message = $"已创建 {createdCount} 个房间 + 玩家。\n默认出生：前厅 (FrontHall)\n调试切换：F1~F11";
                Debug.Log($"[JHTGJ] Villa whitebox scene setup complete. Rooms: {createdCount}");
                EditorUtility.DisplayDialog("Villa Setup", message, "OK");
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
                EditorUtility.DisplayDialog("Villa Setup 失败", ex.Message, "OK");
            }
        }

        static void CleanupLegacyObjects(UnityEngine.SceneManagement.Scene scene)
        {
            var toDestroy = new List<GameObject>();

            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == "Room" || root.name == "Floor" || root.name.StartsWith("Room_"))
                    toDestroy.Add(root);
            }

            foreach (var go in toDestroy)
                UnityEngine.Object.DestroyImmediate(go);
        }

        static int BuildVilla()
        {
            var builtRooms = new List<(RoomType type, GameObject root)>();

            foreach (var type in AllRoomTypes)
            {
                var room = WhiteboxRoomBuilder.BuildRoom(type);
                Undo.RegisterCreatedObjectUndo(room, $"Create {type}");
                room.SetActive(false);
                builtRooms.Add((type, room));
                Debug.Log($"[JHTGJ] Created {room.name}");
            }

            var startRoom = RoomType.FrontHall;
            var startEntry = builtRooms.Find(r => r.type == startRoom);
            if (startEntry.root == null)
                throw new System.InvalidOperationException($"Start room not found: {startRoom}");

            startEntry.root.SetActive(true);

            var systems = GetOrCreate("GameSystems");
            var manager = systems.GetComponent<VillaSceneManager>();
            if (manager == null)
                manager = systems.AddComponent<VillaSceneManager>();

            var protagonist = ProtagonistBuilder.CreateOrUpdate(startEntry.root.GetComponent<Room>(), manager);
            var movement = protagonist.GetComponent<SideViewCharacterController>();

            var so = new SerializedObject(manager);
            so.FindProperty("protagonist").objectReferenceValue = movement;
            so.FindProperty("startRoom").enumValueIndex = (int)startRoom;
            so.FindProperty("enableDebugShortcuts").boolValue = true;

            var roomsProp = so.FindProperty("rooms");
            roomsProp.arraySize = builtRooms.Count;
            for (var i = 0; i < builtRooms.Count; i++)
                SetRoomEntry(roomsProp.GetArrayElementAtIndex(i), builtRooms[i].type, builtRooms[i].root);

            so.ApplyModifiedPropertiesWithoutUndo();

            PauseMenuUICreator.EnsureInGameScene();
            StorySystemSetup.EnsureInGameScene();

            Selection.activeGameObject = protagonist;
            EditorGUIUtility.PingObject(protagonist);

            return builtRooms.Count;
        }

        static void SetRoomEntry(SerializedProperty entry, RoomType type, GameObject root)
        {
            entry.FindPropertyRelative("type").enumValueIndex = (int)type;
            entry.FindPropertyRelative("root").objectReferenceValue = root;
            entry.FindPropertyRelative("room").objectReferenceValue = root.GetComponent<Room>();
        }

        static GameObject GetOrCreate(string name)
        {
            var existing = GameObject.Find(name);
            return existing != null ? existing : new GameObject(name);
        }
    }
}
#endif
