using System;
using UnityEngine;

namespace JHTGJ.Character
{
    [CreateAssetMenu(fileName = "CharacterAppearanceProfile", menuName = "JHTGJ/Character Appearance Profile")]
    public class CharacterAppearanceProfile : ScriptableObject
    {
        [Serializable]
        public class LightingAppearance
        {
            public CharacterLightingType lighting = CharacterLightingType.Default;
            public Sprite idle;
            public Sprite[] walkFrames = Array.Empty<Sprite>();
        }

        [SerializeField] LightingAppearance defaultAppearance = new LightingAppearance();
        [SerializeField] LightingAppearance[] lightingAppearances = Array.Empty<LightingAppearance>();

        public bool TryGetAppearance(CharacterLightingType lighting, out Sprite idle, out Sprite[] walkFrames)
        {
            idle = null;
            walkFrames = Array.Empty<Sprite>();

            foreach (var entry in lightingAppearances)
            {
                if (entry == null || entry.lighting != lighting)
                    continue;

                idle = entry.idle;
                walkFrames = entry.walkFrames ?? Array.Empty<Sprite>();
                return idle != null || walkFrames.Length > 0;
            }

            if (lighting != CharacterLightingType.Default && defaultAppearance != null)
            {
                idle = defaultAppearance.idle;
                walkFrames = defaultAppearance.walkFrames ?? Array.Empty<Sprite>();
                return idle != null || walkFrames.Length > 0;
            }

            if (defaultAppearance != null)
            {
                idle = defaultAppearance.idle;
                walkFrames = defaultAppearance.walkFrames ?? Array.Empty<Sprite>();
                return idle != null || walkFrames.Length > 0;
            }

            return false;
        }
    }
}
