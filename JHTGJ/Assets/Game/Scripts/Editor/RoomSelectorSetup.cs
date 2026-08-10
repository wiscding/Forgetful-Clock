#if UNITY_EDITOR
using JHTGJ.Core;
using JHTGJ.Interaction;
using JHTGJ.Scene;
using JHTGJ.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace JHTGJ.EditorTools
{
    public static class RoomSelectorSetup
    {
        const string SelectorObjectName = "RoomSelector_Travel";
        const string SelectorFolderName = "RoomSelectorPoints";
        const string LivingRoomElevatorName = "Interact_Elevator";
        const float LivingRoomElevatorLocalX = 6.5f;

        [MenuItem("JHTGJ/Fix Room Selection UI Duplicate Buttons")]
        public static void FixDuplicateButtonsInScene()
        {
            if (!EnsureGameScene())
                return;

            FixOptionTemplatePlacement();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorUtility.DisplayDialog(
                "已修复",
                "已将 OptionTemplate 移出 ButtonContainer 并隐藏。\n\n若仍有问题，可执行 Create Room Selection UI 重建面板。",
                "OK");
        }

        public static void FixOptionTemplatePlacement()
        {
            var template = GameObject.Find("OptionTemplate");
            if (template == null)
                return;

            var panel = template.transform.parent;
            while (panel != null && panel.name != "SelectionPanel")
                panel = panel.parent;

            if (panel != null && template.transform.parent != panel)
                template.transform.SetParent(panel, false);

            template.SetActive(false);

            var overlay = GameObject.Find("FadeOverlay");
            if (overlay != null)
            {
                if (overlay.TryGetComponent<CanvasGroup>(out var group))
                    group.blocksRaycasts = false;
                if (overlay.TryGetComponent<UnityEngine.UI.Image>(out var image))
                    image.raycastTarget = false;
            }

            var label = template.GetComponentInChildren<Text>();
            if (label != null)
                label.text = "选项";
        }

        [MenuItem("JHTGJ/Create Room Selection UI (Game Scene)")]
        public static void CreateUiFromMenu()
        {
            if (!EnsureGameScene())
                return;

            CreateOrFindUi(forceRecreate: true);
            var canvas = GameObject.Find("RoomSelectionCanvas");
            if (canvas != null)
                RoomSelectionUIFixer.FixExisting(canvas);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("[JHTGJ] Room selection UI created.");
        }

        [MenuItem("JHTGJ/Add Living Room Elevator (Game Scene)")]
        public static void AddLivingRoomElevatorFromMenu()
        {
            if (!EnsureGameScene())
                return;

            EnsureLivingRoomElevator();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorUtility.DisplayDialog(
                "客厅电梯",
                "已在客厅添加电梯互动点。\n\n按 E 可选择：2F / -1F",
                "OK");
        }

        public static void EnsureDiningRoomTravelDoor()
        {
            var ui = CreateOrFindUi(forceRecreate: false);
            WireProtagonist(ui);
            ConvertDiningRoomTravelDoor();
        }

        public static void EnsureLivingRoomElevator()
        {
            var ui = CreateOrFindUi(forceRecreate: false);
            WireProtagonist(ui);
            AddLivingRoomElevator();
        }

        static void AddLivingRoomElevator()
        {
            var livingRoom = GameObject.Find("Room_LivingRoom");
            if (livingRoom == null)
            {
                Debug.LogWarning("[JHTGJ] 未找到 Room_LivingRoom，跳过客厅电梯。");
                return;
            }

            var interactPoints = livingRoom.transform.Find("InteractPoints");
            if (interactPoints != null)
                RemoveLivingRoomLegacyTravelSelector(interactPoints);

            AddSelector(
                RoomType.LivingRoom,
                "电梯",
                LivingRoomElevatorLocalX,
                LivingRoomElevatorName,
                Entry("2F", RoomType.Hallway),
                Entry("-1F", RoomType.Basement));
        }

        static void RemoveLivingRoomLegacyTravelSelector(Transform interactPoints)
        {
            var legacy = interactPoints.Find(SelectorObjectName);
            if (legacy != null)
                Object.DestroyImmediate(legacy.gameObject);

            var folder = interactPoints.Find(SelectorFolderName);
            if (folder == null)
                return;

            legacy = folder.Find(SelectorObjectName);
            if (legacy != null)
                Object.DestroyImmediate(legacy.gameObject);
        }

        [MenuItem("JHTGJ/Add Rooftop And Basement Travel Points (Game Scene)")]
        public static void AddRooftopBasementFromMenu()
        {
            if (!EnsureGameScene())
                return;

            AddRooftopAndBasementSelectors();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorUtility.DisplayDialog(
                "Travel Points",
                "已添加：\n· 天台 → 3F\n· 地下室 → 客厅\n\n可在各房间 RoomSelector_Travel 调整位置。",
                "OK");
        }

        [MenuItem("JHTGJ/Convert Dining Room Door To Room Selector (Game Scene)")]
        public static void ConvertDiningRoomDoorFromMenu()
        {
            if (!EnsureGameScene())
                return;

            if (!ConvertDiningRoomTravelDoor())
            {
                EditorUtility.DisplayDialog(
                    "未找到",
                    "未找到 Room_DiningRoom/InteractPoints/Door_ToRooftop。\n\n请确认场景结构后重试。",
                    "OK");
                return;
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorUtility.DisplayDialog(
                "已转换",
                "餐厅 Door_ToRooftop 已改为房间选择点。\n\n按 E 可选择：4F / 2F",
                "OK");
        }

        [MenuItem("JHTGJ/Setup Room Selector System (Game Scene)")]
        public static void SetupFromMenu()
        {
            if (!EnsureGameScene())
                return;

            SetupInActiveScene();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorUtility.DisplayDialog(
                "Room Selector",
                "已创建房间选择面板，并在客厅/走廊/餐厅/天台/地下室各添加互动点。\n\n" +
                "可在 Inspector 中调整：\n" +
                "· RoomSelectionCanvas → 面板样式\n" +
                "· 客厅 Interact_Elevator → 电梯位置与目的地\n" +
                "· 各房间 RoomSelector_Travel → 位置与目的地列表\n" +
                "· 餐厅 Door_ToRooftop → 4F/2F 选择",
                "OK");
        }

        public static void SetupInActiveScene()
        {
            GameUIFontUpdater.UpdateSilently();

            var ui = CreateOrFindUi(forceRecreate: false);
            WireProtagonist(ui);

            AddLivingRoomElevator();

            AddSelector(RoomType.Hallway, "选择目的地", 0f,
                Entry("1F", RoomType.LivingRoom),
                Entry("3F", RoomType.DiningRoom));

            ConvertDiningRoomTravelDoor();

            AddRooftopAndBasementSelectors();
        }

        public static bool ConvertDiningRoomTravelDoor()
        {
            var diningRoom = GameObject.Find("Room_DiningRoom");
            if (diningRoom == null)
            {
                Debug.LogWarning("[JHTGJ] 未找到 Room_DiningRoom，跳过 Door_ToRooftop 转换。");
                return false;
            }

            var interactPoints = diningRoom.transform.Find("InteractPoints");
            if (interactPoints == null)
            {
                Debug.LogWarning("[JHTGJ] 未找到餐厅 InteractPoints，跳过 Door_ToRooftop 转换。");
                return false;
            }

            var door = interactPoints.Find("Door_ToRooftop");
            if (door == null)
            {
                Debug.LogWarning("[JHTGJ] 未找到 Door_ToRooftop，跳过转换。");
                return false;
            }

            var room = diningRoom.GetComponent<Room>();
            var floorY = room != null ? room.FloorY : WhiteboxRoomBuilder.FloorY;
            var localX = door.localPosition.x;

            var interact = door.GetComponent<InteractPoint>();
            if (interact != null)
                Object.DestroyImmediate(interact);

            var selector = door.GetComponent<RoomSelectorInteractPoint>();
            if (selector == null)
                selector = door.gameObject.AddComponent<RoomSelectorInteractPoint>();

            ConfigureSelector(selector, "选择目的地", localX, floorY,
                Entry("4F", RoomType.Rooftop),
                Entry("2F", RoomType.Hallway));

            RemoveDuplicateDiningRoomSelector(interactPoints);
            return true;
        }

        static void RemoveDuplicateDiningRoomSelector(Transform interactPoints)
        {
            var folder = interactPoints.Find(SelectorFolderName);
            if (folder == null)
                return;

            var duplicate = folder.Find(SelectorObjectName);
            if (duplicate != null)
                Object.DestroyImmediate(duplicate.gameObject);
        }

        static void AddRooftopAndBasementSelectors()
        {
            AddSelector(RoomType.Rooftop, "选择目的地", 0f,
                Entry("3F", RoomType.DiningRoom));

            AddSelector(RoomType.Basement, "选择目的地", 0f,
                Entry("1F", RoomType.LivingRoom));
        }

        static RoomSelectionUI CreateOrFindUi(bool forceRecreate)
        {
            var existing = Object.FindObjectOfType<RoomSelectionUI>(true);
            if (existing != null && !forceRecreate)
            {
                if (PauseMenuLegacyTextUtility.NeedsLegacyConversion(existing.gameObject))
                    RoomSelectionUIFixer.FixExisting(existing.gameObject);
                return existing;
            }

            return RoomSelectionUIBuilder.Build();
        }

        static void WireProtagonist(RoomSelectionUI ui)
        {
            var protagonist = GameObject.Find("Protagonist");
            if (protagonist == null)
                return;

            var interact = protagonist.GetComponent<InteractController>();
            if (interact == null)
                return;

            var so = new SerializedObject(interact);
            so.FindProperty("roomSelectionUI").objectReferenceValue = ui;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void AddSelector(
            RoomType roomType,
            string label,
            float localX,
            params (string label, RoomType target)[] destinations) =>
            AddSelector(roomType, label, localX, SelectorObjectName, destinations);

        static void AddSelector(
            RoomType roomType,
            string label,
            float localX,
            string objectName,
            params (string label, RoomType target)[] destinations)
        {
            var roomRoot = GameObject.Find($"Room_{roomType}");
            if (roomRoot == null)
            {
                Debug.LogWarning($"[JHTGJ] 未找到 Room_{roomType}，跳过 RoomSelector。");
                return;
            }

            var room = roomRoot.GetComponent<Room>();
            var floorY = room != null ? room.FloorY : WhiteboxRoomBuilder.FloorY;
            var folder = GetOrCreateSelectorFolder(roomRoot.transform);
            var interactPoints = roomRoot.transform.Find("InteractPoints");

            var existing = folder.Find(objectName);
            if (existing == null && interactPoints != null)
                existing = interactPoints.Find(objectName);

            if (existing != null)
            {
                ConfigureSelector(existing.GetComponent<RoomSelectorInteractPoint>(), label, localX, floorY, destinations);
                return;
            }

            var go = new GameObject(objectName);
            go.transform.SetParent(folder, false);
            go.transform.localPosition = new Vector3(localX, floorY + 0.2f, 0f);

            var selector = go.AddComponent<RoomSelectorInteractPoint>();
            ConfigureSelector(selector, label, localX, floorY, destinations);
            Undo.RegisterCreatedObjectUndo(go, $"Create {objectName} in {roomType}");
        }

        static Transform GetOrCreateSelectorFolder(Transform roomRoot)
        {
            var interactPoints = roomRoot.Find("InteractPoints");
            if (interactPoints == null)
            {
                var go = new GameObject("InteractPoints");
                go.transform.SetParent(roomRoot, false);
                interactPoints = go.transform;
            }

            var folder = interactPoints.Find(SelectorFolderName);
            if (folder != null)
                return folder;

            var folderGo = new GameObject(SelectorFolderName);
            folderGo.transform.SetParent(interactPoints, false);
            return folderGo.transform;
        }

        static void ConfigureSelector(
            RoomSelectorInteractPoint selector,
            string label,
            float localX,
            float floorY,
            params (string label, RoomType target)[] destinations)
        {
            if (selector == null)
                return;

            var so = new SerializedObject(selector);
            so.FindProperty("label").stringValue = label;
            so.FindProperty("standXOffset").floatValue = localX < 0f ? 3f : localX > 0f ? -3f : 0f;

            var list = so.FindProperty("destinations");
            list.ClearArray();
            foreach (var destination in destinations)
            {
                list.InsertArrayElementAtIndex(list.arraySize);
                var entry = list.GetArrayElementAtIndex(list.arraySize - 1);
                entry.FindPropertyRelative("label").stringValue = destination.label;
                entry.FindPropertyRelative("targetRoom").enumValueIndex = (int)destination.target;
                entry.FindPropertyRelative("spawnSide").enumValueIndex = (int)SpawnSide.Left;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            selector.transform.localPosition = new Vector3(localX, floorY + 0.2f, 0f);
            selector.ApplyMarker();
        }

        static (string label, RoomType target) Entry(string label, RoomType target) => (label, target);

        static bool EnsureGameScene()
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.name == SceneLoader.GameSceneName)
                return true;

            if (!EditorUtility.DisplayDialog(
                    "需要在游戏场景",
                    "房间选择系统应配置在 SampleScene。\n\n是否打开 SampleScene 并继续？",
                    "打开 SampleScene",
                    "取消"))
                return false;

            EditorSceneManager.OpenScene(SceneLoader.GameScenePath);
            return true;
        }
    }
}
#endif
