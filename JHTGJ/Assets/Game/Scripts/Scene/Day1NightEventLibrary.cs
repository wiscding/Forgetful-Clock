using UnityEngine;

namespace JHTGJ.Scene
{
    [CreateAssetMenu(fileName = "Day1NightEventLibrary", menuName = "JHTGJ/Day 1 Night Event Library")]
    public class Day1NightEventLibrary : ScriptableObject
    {
        [SerializeField] Sprite bedroomBackground;

        static Day1NightEventLibrary cached;

        public static Day1NightEventLibrary Instance
        {
            get
            {
                if (cached == null)
                    cached = Resources.Load<Day1NightEventLibrary>("Day1NightEventLibrary");
                return cached;
            }
        }

        public Sprite BedroomBackground => bedroomBackground;
    }
}
