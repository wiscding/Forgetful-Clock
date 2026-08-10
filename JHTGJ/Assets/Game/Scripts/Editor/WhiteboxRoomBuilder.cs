#if UNITY_EDITOR
using JHTGJ.Art;
using JHTGJ.Interaction;
using JHTGJ.Scene;
using UnityEditor;
using UnityEngine;

namespace JHTGJ.EditorTools
{
    public static class WhiteboxRoomBuilder
    {
        public const float WalkLeft = -9f;
        public const float WalkRight = 9f;
        public const float FloorY = -3f;

        public static GameObject BuildRoom(RoomType type)
        {
            var roomRoot = new GameObject($"Room_{type}");
            var room = roomRoot.AddComponent<Room>();

            CreateBounds(roomRoot.transform, room);
            CreateBackground(roomRoot.transform, GetBackgroundColor(type));
            CreateFloor(roomRoot.transform, room);
            CreateSpawn(roomRoot.transform, room);

            switch (type)
            {
                case RoomType.Kitchen: BuildKitchenProps(roomRoot.transform); break;
                case RoomType.Bedroom: BuildBedroomProps(roomRoot.transform); break;
                case RoomType.Basement: BuildBasementProps(roomRoot.transform); break;
                case RoomType.FrontHall: BuildFrontHallProps(roomRoot.transform); break;
                case RoomType.LivingRoom: BuildLivingRoomProps(roomRoot.transform); break;
                case RoomType.DiningRoom: BuildDiningRoomProps(roomRoot.transform); break;
                case RoomType.Bathroom: BuildBathroomProps(roomRoot.transform); break;
                case RoomType.Hallway: BuildHallwayProps(roomRoot.transform); break;
                case RoomType.Storage: BuildStorageProps(roomRoot.transform); break;
                case RoomType.Rooftop: BuildRooftopProps(roomRoot.transform); break;
                case RoomType.BackGarden: BuildBackGardenProps(roomRoot.transform); break;
            }

            return roomRoot;
        }

        static Color GetBackgroundColor(RoomType type)
        {
            switch (type)
            {
                case RoomType.Kitchen: return new Color(0.86f, 0.84f, 0.78f);
                case RoomType.Bedroom: return new Color(0.82f, 0.78f, 0.88f);
                case RoomType.Basement: return new Color(0.55f, 0.56f, 0.6f);
                case RoomType.FrontHall: return new Color(0.88f, 0.84f, 0.76f);
                case RoomType.LivingRoom: return new Color(0.9f, 0.82f, 0.72f);
                case RoomType.DiningRoom: return new Color(0.92f, 0.88f, 0.8f);
                case RoomType.Bathroom: return new Color(0.85f, 0.9f, 0.95f);
                case RoomType.Hallway: return new Color(0.78f, 0.78f, 0.8f);
                case RoomType.Storage: return new Color(0.7f, 0.68f, 0.62f);
                case RoomType.Rooftop: return new Color(0.65f, 0.82f, 0.95f);
                case RoomType.BackGarden: return new Color(0.72f, 0.88f, 0.72f);
                default: return new Color(0.82f, 0.84f, 0.88f);
            }
        }

        static void CreateBounds(Transform parent, Room room)
        {
            var bounds = new GameObject("Bounds");
            bounds.transform.SetParent(parent, false);

            var left = CreateBoundMarker(bounds.transform, "WalkLeft", WalkLeft, FloorY, WalkBoundMarker.BoundSide.Left);
            var right = CreateBoundMarker(bounds.transform, "WalkRight", WalkRight, FloorY, WalkBoundMarker.BoundSide.Right);

            var so = new SerializedObject(room);
            so.FindProperty("leftBound").objectReferenceValue = left;
            so.FindProperty("rightBound").objectReferenceValue = right;
            so.ApplyModifiedPropertiesWithoutUndo();
            room.SyncFromMarkers();
        }

        static WalkBoundMarker CreateBoundMarker(Transform parent, string name, float x, float y, WalkBoundMarker.BoundSide side)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(x, y, 0f);
            var marker = go.AddComponent<WalkBoundMarker>();
            var so = new SerializedObject(marker);
            so.FindProperty("side").enumValueIndex = (int)side;
            so.ApplyModifiedPropertiesWithoutUndo();
            return marker;
        }

        static void CreateBackground(Transform parent, Color color)
        {
            CreateBlock(parent, "Background", new Vector3(0f, 1f, 0f), new Vector2(18f, 10f), color, -10);
        }

        static void CreateFloor(Transform parent, Room room)
        {
            var floorColor = new Color(0.35f, 0.32f, 0.28f);
            var floor = CreateBlock(parent, "Floor", new Vector3(0f, FloorY - 0.5f, 0f), new Vector2(18f, 0.2f), floorColor, -5);

            var so = new SerializedObject(room);
            so.FindProperty("floorReference").objectReferenceValue = floor.transform;
            so.ApplyModifiedPropertiesWithoutUndo();
            room.SyncFromMarkers();
        }

        static void CreateSpawn(Transform parent, Room room)
        {
            var spawn = new GameObject("PlayerSpawn");
            spawn.transform.SetParent(parent, false);
            spawn.transform.localPosition = new Vector3(WalkLeft, FloorY, 0f);

            var so = new SerializedObject(room);
            so.FindProperty("playerSpawn").objectReferenceValue = spawn.transform;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void BuildFrontHallProps(Transform parent)
        {
            var props = CreateFolder(parent, "Props");
            var interact = CreateFolder(parent, "InteractPoints");

            CreateBlock(props, "ShoeCabinet", new Vector3(-5f, -2f, 0f), new Vector2(1.5f, 2f), new Color(0.65f, 0.5f, 0.35f), 0);
            CreateBlock(props, "Mirror", new Vector3(2f, -0.5f, 0f), new Vector2(1f, 2.5f), new Color(0.75f, 0.85f, 0.95f), 0);
            CreateBlock(props, "EntryMat", new Vector3(-8f, -2.8f, 0f), new Vector2(1.2f, 0.3f), new Color(0.5f, 0.35f, 0.25f), 0);

            CreateDoor(interact, "Door_ToLivingRoom", "→ 客厅", RoomType.LivingRoom, new Vector3(8f, FloorY + 0.2f, 0f), 0f, SpawnSide.Left);
        }

        static void BuildLivingRoomProps(Transform parent)
        {
            var props = CreateFolder(parent, "Props");
            var interact = CreateFolder(parent, "InteractPoints");

            CreateBlock(props, "Sofa", new Vector3(-2f, -2.2f, 0f), new Vector2(3f, 1.2f), new Color(0.75f, 0.45f, 0.4f), 0);
            CreateBlock(props, "TV", new Vector3(5f, -0.5f, 0f), new Vector2(2f, 1.5f), new Color(0.25f, 0.25f, 0.28f), 0);
            CreateBlock(props, "CoffeeTable", new Vector3(0f, -2.5f, 0f), new Vector2(1.8f, 0.6f), new Color(0.6f, 0.45f, 0.3f), 0);

            CreateDoor(interact, "Door_ToFrontHall", "← 前厅", RoomType.FrontHall, new Vector3(-8f, FloorY + 0.2f, 0f), 0f, SpawnSide.Right);
            CreateDoor(interact, "Door_ToBackGarden", "→ 后花园", RoomType.BackGarden, new Vector3(8f, FloorY + 0.2f, 0f), 0f, SpawnSide.Left);
            AddFloorLinks(interact, RoomType.LivingRoom);
        }

        static void BuildDiningRoomProps(Transform parent)
        {
            var props = CreateFolder(parent, "Props");
            var interact = CreateFolder(parent, "InteractPoints");

            CreateBlock(props, "DiningTable", new Vector3(0f, -2f, 0f), new Vector2(3.5f, 1f), new Color(0.65f, 0.48f, 0.32f), 0);
            CreateBlock(props, "Chairs", new Vector3(-3f, -2.5f, 0f), new Vector2(0.8f, 0.8f), new Color(0.55f, 0.4f, 0.28f), 0);
            CreateBlock(props, "Sideboard", new Vector3(6f, -1.8f, 0f), new Vector2(1.5f, 1.8f), new Color(0.58f, 0.42f, 0.3f), 0);

            CreateInteract(interact, "Interact_DiningTable", "餐桌", InteractionKind.EatTogether,
                new Vector3(0f, FloorY + 0.2f, 0f), 3f);
            CreateDoor(interact, "Door_ToKitchen", "← 厨房", RoomType.Kitchen, new Vector3(-8f, FloorY + 0.2f, 0f), 0f, SpawnSide.Right);
            CreateDoor(interact, "Door_ToBathroom", "→ 卫生间", RoomType.Bathroom, new Vector3(8f, FloorY + 0.2f, 0f), 0f, SpawnSide.Left);
            AddFloorLinks(interact, RoomType.DiningRoom);
        }

        static void BuildKitchenProps(Transform parent)
        {
            var props = CreateFolder(parent, "Props");
            var interact = CreateFolder(parent, "InteractPoints");

            CreateBlock(props, "Fridge", new Vector3(-6f, -1.5f, 0f), new Vector2(1.2f, 3f), new Color(0.55f, 0.75f, 0.95f), 0);
            CreateBlock(props, "Stove", new Vector3(4f, -1.8f, 0f), new Vector2(1.5f, 1.2f), new Color(0.95f, 0.55f, 0.35f), 0);
            CreateBlock(props, "Counter", new Vector3(0f, -2.2f, 0f), new Vector2(2.5f, 0.8f), new Color(0.7f, 0.55f, 0.35f), 0);

            CreateInteract(interact, "Interact_Fridge", "冰箱/做早餐", InteractionKind.CookBreakfast,
                new Vector3(-6f, FloorY + 0.2f, 0f), 3f);
            CreateInteract(interact, "Interact_Stove", "灶台/做饭", InteractionKind.CookLunch,
                new Vector3(4f, FloorY + 0.2f, 0f), -3f);
            CreateDoor(interact, "Door_ToDiningRoom", "→ 餐厅", RoomType.DiningRoom, new Vector3(8f, FloorY + 0.2f, 0f), 0f, SpawnSide.Left);
        }

        static void BuildBedroomProps(Transform parent)
        {
            var props = CreateFolder(parent, "Props");
            var interact = CreateFolder(parent, "InteractPoints");

            CreateBlock(props, "Bed", new Vector3(3f, -2.3f, 0f), new Vector2(2.5f, 1.2f), new Color(0.75f, 0.65f, 0.95f), 0);
            CreateBlock(props, "Partner", new Vector3(-3f, -2f, 0f), new Vector2(1f, 1.6f), new Color(0.9f, 0.7f, 0.75f), 1);
            CreateBlock(props, "Wheelchair", new Vector3(-3f, -2.8f, 0f), new Vector2(1.2f, 0.6f), new Color(0.4f, 0.4f, 0.45f), 0);
            CreateBlock(props, "Diary", new Vector3(-6f, -2.5f, 0f), new Vector2(0.5f, 0.35f), new Color(0.95f, 0.9f, 0.55f), 0);

            CreateInteract(interact, "Interact_Bed", "床/睡觉", InteractionKind.Sleep, new Vector3(3f, FloorY + 0.2f, 0f), -3f);
            CreateInteract(interact, "Interact_Partner", "伴侣", InteractionKind.TalkToPartner, new Vector3(-3f, FloorY + 0.2f, 0f), 3f);
            CreateInteract(interact, "Interact_Diary", "日记", InteractionKind.ReadDiary, new Vector3(-6f, FloorY + 0.2f, 0f), 3f);
            CreateDoor(interact, "Door_ToHallway", "→ 走廊", RoomType.Hallway, new Vector3(8f, FloorY + 0.2f, 0f), 0f, SpawnSide.Left);
        }

        static void BuildBathroomProps(Transform parent)
        {
            var props = CreateFolder(parent, "Props");
            var interact = CreateFolder(parent, "InteractPoints");

            CreateBlock(props, "Sink", new Vector3(-4f, -1.8f, 0f), new Vector2(1.2f, 1.5f), new Color(0.9f, 0.92f, 0.95f), 0);
            CreateBlock(props, "Bathtub", new Vector3(3f, -2.2f, 0f), new Vector2(2.5f, 1f), new Color(0.85f, 0.88f, 0.92f), 0);
            CreateBlock(props, "Mirror", new Vector3(-4f, 0f, 0f), new Vector2(0.8f, 1.2f), new Color(0.75f, 0.85f, 0.95f), 0);

            CreateInteract(interact, "Interact_Sink", "洗手台", InteractionKind.Clean, new Vector3(-4f, FloorY + 0.2f, 0f), 3f);
            CreateDoor(interact, "Door_ToDiningRoom", "← 餐厅", RoomType.DiningRoom, new Vector3(-8f, FloorY + 0.2f, 0f), 0f, SpawnSide.Right);
        }

        static void BuildHallwayProps(Transform parent)
        {
            var props = CreateFolder(parent, "Props");
            var interact = CreateFolder(parent, "InteractPoints");

            CreateBlock(props, "HallwayArt", new Vector3(0f, 0f, 0f), new Vector2(1f, 1.5f), new Color(0.6f, 0.55f, 0.5f), 0);
            CreateBlock(props, "StairRail", new Vector3(6f, -1f, 0f), new Vector2(0.3f, 3f), new Color(0.45f, 0.42f, 0.4f), 0);

            CreateDoor(interact, "Door_ToBedroom", "← 卧室", RoomType.Bedroom, new Vector3(-8f, FloorY + 0.2f, 0f), 0f, SpawnSide.Right);
            CreateDoor(interact, "Door_ToStorage", "→ 储物间", RoomType.Storage, new Vector3(8f, FloorY + 0.2f, 0f), 0f, SpawnSide.Left);
            AddFloorLinks(interact, RoomType.Hallway);
        }

        static void BuildStorageProps(Transform parent)
        {
            var props = CreateFolder(parent, "Props");
            var interact = CreateFolder(parent, "InteractPoints");

            CreateBlock(props, "ShelfA", new Vector3(-5f, -1f, 0f), new Vector2(1.5f, 3f), new Color(0.55f, 0.42f, 0.32f), 0);
            CreateBlock(props, "ShelfB", new Vector3(2f, -1f, 0f), new Vector2(1.5f, 3f), new Color(0.55f, 0.42f, 0.32f), 0);
            CreateBlock(props, "Boxes", new Vector3(5f, -2.5f, 0f), new Vector2(1.2f, 0.8f), new Color(0.65f, 0.55f, 0.4f), 0);

            CreateInteract(interact, "Interact_Storage", "储物架", InteractionKind.Search, new Vector3(2f, FloorY + 0.2f, 0f), -3f);
            CreateDoor(interact, "Door_ToHallway", "← 走廊", RoomType.Hallway, new Vector3(-8f, FloorY + 0.2f, 0f), 0f, SpawnSide.Right);
        }

        static void BuildBasementProps(Transform parent)
        {
            var props = CreateFolder(parent, "Props");
            var interact = CreateFolder(parent, "InteractPoints");

            CreateBlock(props, "TimeMachine", new Vector3(-3f, -1.2f, 0f), new Vector2(2f, 2.5f), new Color(0.45f, 0.55f, 0.85f), 0);
            CreateBlock(props, "EmergencyStop", new Vector3(4f, -2f, 0f), new Vector2(0.8f, 0.8f), new Color(0.95f, 0.25f, 0.25f), 0);
            CreateBlock(props, "Console", new Vector3(4f, -1.2f, 0f), new Vector2(1.5f, 1f), new Color(0.35f, 0.35f, 0.38f), 0);

            CreateInteract(interact, "Interact_TimeMachine", "时光机/重置", InteractionKind.ResetLoop, new Vector3(-3f, FloorY + 0.2f, 0f), 3f);
            CreateInteract(interact, "Interact_EmergencyStop", "紧急停止", InteractionKind.EmergencyStop, new Vector3(4f, FloorY + 0.2f, 0f), -3f);
            AddFloorLinks(interact, RoomType.Basement);
        }

        static void BuildRooftopProps(Transform parent)
        {
            var props = CreateFolder(parent, "Props");
            var interact = CreateFolder(parent, "InteractPoints");

            CreateBlock(parent, "Sky", new Vector3(0f, 3f, 0f), new Vector2(18f, 4f), new Color(0.55f, 0.78f, 0.95f, 0.5f), -8);
            CreateBlock(props, "Railing", new Vector3(0f, -1.5f, 0f), new Vector2(18f, 0.3f), new Color(0.5f, 0.5f, 0.52f), 0);
            CreateBlock(props, "Bench", new Vector3(-2f, -2.3f, 0f), new Vector2(2f, 0.8f), new Color(0.6f, 0.45f, 0.3f), 0);
            CreateBlock(props, "Antenna", new Vector3(6f, 0.5f, 0f), new Vector2(0.2f, 3f), new Color(0.4f, 0.4f, 0.42f), 0);

            CreateInteract(interact, "Interact_Sunset", "看日落", InteractionKind.WatchSunset, new Vector3(-2f, FloorY + 0.2f, 0f), 3f);
            AddFloorLinks(interact, RoomType.Rooftop);
        }

        static void BuildBackGardenProps(Transform parent)
        {
            var props = CreateFolder(parent, "Props");
            var interact = CreateFolder(parent, "InteractPoints");

            CreateBlock(parent, "Sky", new Vector3(0f, 3f, 0f), new Vector2(18f, 4f), new Color(0.6f, 0.85f, 0.95f, 0.4f), -8);
            CreateBlock(props, "Grass", new Vector3(0f, -2.8f, 0f), new Vector2(18f, 0.5f), new Color(0.35f, 0.65f, 0.35f), -4);
            CreateBlock(props, "Tree", new Vector3(4f, -0.5f, 0f), new Vector2(1.5f, 4f), new Color(0.3f, 0.6f, 0.35f), 0);
            CreateBlock(props, "GardenBench", new Vector3(-3f, -2.3f, 0f), new Vector2(2f, 0.8f), new Color(0.55f, 0.4f, 0.28f), 0);

            CreateInteract(interact, "Interact_GardenBench", "花园长椅", InteractionKind.Reconcile, new Vector3(-3f, FloorY + 0.2f, 0f), 3f);
            CreateDoor(interact, "Door_ToLivingRoom", "← 客厅", RoomType.LivingRoom, new Vector3(-8f, FloorY + 0.2f, 0f), 0f, SpawnSide.Right);
        }

        static void CreateDoor(Transform parent, string name, string label, RoomType target, Vector3 localPos, float standOffset, SpawnSide targetSpawnSide)
        {
            CreateInteract(parent, name, label, InteractionKind.ChangeRoom, localPos, standOffset, target, targetSpawnSide);
        }

        static void AddFloorLinks(Transform interactParent, RoomType fromRoom)
        {
            var folder = new GameObject("FloorLinkPoints");
            folder.transform.SetParent(interactParent, false);
            FloorLinkNetworkSetup.AddLinks(folder.transform, fromRoom, FloorY);
        }

        static Transform CreateFolder(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        static GameObject CreateBlock(Transform parent, string name, Vector3 localPos, Vector2 size, Color color, int sortingOrder)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.AddComponent<SpriteRenderer>();
            var block = go.AddComponent<WhiteboxBlock>();
            var so = new SerializedObject(block);
            so.FindProperty("color").colorValue = color;
            so.FindProperty("size").vector2Value = size;
            so.FindProperty("sortingOrder").intValue = sortingOrder;
            so.ApplyModifiedPropertiesWithoutUndo();
            block.Apply();
            return go;
        }

        static void CreateInteract(Transform parent, string name, string label, InteractionKind kind,
            Vector3 localPos, float standOffset, RoomType targetRoom = RoomType.Kitchen, SpawnSide targetSpawnSide = SpawnSide.Left)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;

            var point = go.AddComponent<InteractPoint>();
            var so = new SerializedObject(point);
            so.FindProperty("interactId").stringValue = name;
            so.FindProperty("label").stringValue = label;
            so.FindProperty("kind").enumValueIndex = (int)kind;
            so.FindProperty("targetRoom").enumValueIndex = (int)targetRoom;
            so.FindProperty("targetSpawnSide").enumValueIndex = (int)targetSpawnSide;
            so.FindProperty("standXOffset").floatValue = standOffset;
            so.ApplyModifiedPropertiesWithoutUndo();
            point.ApplyMarker();
        }
    }
}
#endif
