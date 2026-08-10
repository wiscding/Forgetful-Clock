using UnityEngine;

namespace JHTGJ.Scene
{
    [CreateAssetMenu(fileName = "Day4DuskEventLibrary", menuName = "JHTGJ/Day 4 Dusk Event Library")]
    public class Day4DuskEventLibrary : ScriptableObject
    {
        [SerializeField] Sprite rooftopBackground;

        static Day4DuskEventLibrary cached;

        public static Day4DuskEventLibrary Instance
        {
            get
            {
                if (cached == null)
                    cached = Resources.Load<Day4DuskEventLibrary>("Day4DuskEventLibrary");
                return cached;
            }
        }

        public Sprite RooftopBackground => rooftopBackground;
    }
}
