using UnityEngine;

namespace JHTGJ.Scene
{
    [CreateAssetMenu(fileName = "PostCookingDiningLibrary", menuName = "JHTGJ/Post Cooking Dining Library")]
    public class PostCookingDiningLibrary : ScriptableObject
    {
        [SerializeField] Sprite diningRoomBackground;

        static PostCookingDiningLibrary cached;

        public static PostCookingDiningLibrary Instance
        {
            get
            {
                if (cached == null)
                    cached = Resources.Load<PostCookingDiningLibrary>("PostCookingDiningLibrary");
                return cached;
            }
        }

        public Sprite DiningRoomBackground => diningRoomBackground;
    }
}
