#if UNITY_EDITOR
using JHTGJ.Scene;
using UnityEditor;
using UnityEngine;

namespace JHTGJ.EditorTools
{
    public static class WhiteboxRoomCreator
    {
        [MenuItem("GameObject/JHTGJ/Whitebox Room/Front Hall", false, 10)]
        static void CreateFrontHall() => CreateRoom(RoomType.FrontHall);

        [MenuItem("GameObject/JHTGJ/Whitebox Room/Living Room", false, 11)]
        static void CreateLivingRoom() => CreateRoom(RoomType.LivingRoom);

        [MenuItem("GameObject/JHTGJ/Whitebox Room/Dining Room", false, 12)]
        static void CreateDiningRoom() => CreateRoom(RoomType.DiningRoom);

        [MenuItem("GameObject/JHTGJ/Whitebox Room/Kitchen", false, 13)]
        static void CreateKitchen() => CreateRoom(RoomType.Kitchen);

        [MenuItem("GameObject/JHTGJ/Whitebox Room/Hallway", false, 14)]
        static void CreateHallway() => CreateRoom(RoomType.Hallway);

        [MenuItem("GameObject/JHTGJ/Whitebox Room/Bedroom", false, 15)]
        static void CreateBedroom() => CreateRoom(RoomType.Bedroom);

        [MenuItem("GameObject/JHTGJ/Whitebox Room/Bathroom", false, 16)]
        static void CreateBathroom() => CreateRoom(RoomType.Bathroom);

        [MenuItem("GameObject/JHTGJ/Whitebox Room/Storage", false, 17)]
        static void CreateStorage() => CreateRoom(RoomType.Storage);

        [MenuItem("GameObject/JHTGJ/Whitebox Room/Basement", false, 18)]
        static void CreateBasement() => CreateRoom(RoomType.Basement);

        [MenuItem("GameObject/JHTGJ/Whitebox Room/Rooftop", false, 19)]
        static void CreateRooftop() => CreateRoom(RoomType.Rooftop);

        [MenuItem("GameObject/JHTGJ/Whitebox Room/Back Garden", false, 20)]
        static void CreateBackGarden() => CreateRoom(RoomType.BackGarden);

        static void CreateRoom(RoomType type)
        {
            var roomRoot = WhiteboxRoomBuilder.BuildRoom(type);
            Undo.RegisterCreatedObjectUndo(roomRoot, $"Create {type} Room");
            Selection.activeGameObject = roomRoot;
            EditorGUIUtility.PingObject(roomRoot);
        }
    }
}
#endif
