using UnityEngine;

namespace JHTGJ.Scene
{
    [CreateAssetMenu(fileName = "Day2NightEventLibrary", menuName = "JHTGJ/Day 2 Night Event Library")]
    public class Day2NightEventLibrary : ScriptableObject
    {
        [SerializeField] Sprite kitchenBackground;

        static Day2NightEventLibrary cached;

        public static Day2NightEventLibrary Instance
        {
            get
            {
                if (cached == null)
                    cached = Resources.Load<Day2NightEventLibrary>("Day2NightEventLibrary");
                return cached;
            }
        }

        public Sprite KitchenBackground => kitchenBackground;
    }
}
