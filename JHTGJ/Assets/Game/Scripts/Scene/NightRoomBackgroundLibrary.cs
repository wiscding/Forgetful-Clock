using UnityEngine;

namespace JHTGJ.Scene
{
    [CreateAssetMenu(fileName = "NightRoomBackgroundLibrary", menuName = "JHTGJ/Night Room Background Library")]
    public class NightRoomBackgroundLibrary : ScriptableObject
    {
        [SerializeField] Sprite frontHall;
        [SerializeField] Sprite bedroom;
        [SerializeField] Sprite bathroom;
        [SerializeField] Sprite kitchen;
        [SerializeField] Sprite backGarden;
        [SerializeField] Sprite rooftop;
        [SerializeField] Sprite livingRoom;
        [SerializeField] Sprite diningRoom;

        static NightRoomBackgroundLibrary cached;

        public static NightRoomBackgroundLibrary Instance
        {
            get
            {
                if (cached == null)
                    cached = Resources.Load<NightRoomBackgroundLibrary>("NightRoomBackgroundLibrary");
                return cached;
            }
        }

        public Sprite GetBackground(RoomType roomType)
        {
            switch (roomType)
            {
                case RoomType.FrontHall: return frontHall;
                case RoomType.Bedroom: return bedroom;
                case RoomType.Bathroom: return bathroom;
                case RoomType.Kitchen: return kitchen;
                case RoomType.BackGarden: return backGarden;
                case RoomType.Rooftop: return rooftop;
                case RoomType.LivingRoom: return livingRoom;
                case RoomType.DiningRoom: return diningRoom;
                default: return null;
            }
        }
    }
}
