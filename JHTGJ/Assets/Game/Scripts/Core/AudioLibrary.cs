using UnityEngine;

namespace JHTGJ.Core
{
    [CreateAssetMenu(fileName = "AudioLibrary", menuName = "JHTGJ/Audio Library")]
    public class AudioLibrary : ScriptableObject
    {
        [Header("BGM")]
        [SerializeField] AudioClip menuBgm;
        [SerializeField] AudioClip normalBgm;
        [SerializeField] AudioClip endingBgm;
        [SerializeField] AudioClip openingBgm;
        [SerializeField] AudioClip lastDayBgm;
        [SerializeField] AudioClip conflictBgm;

        [Header("SFX")]
        [SerializeField] AudioClip buttonClickSfx;

        public AudioClip GetBgm(BgmTrack track)
        {
            switch (track)
            {
                case BgmTrack.Menu: return menuBgm;
                case BgmTrack.Normal: return normalBgm;
                case BgmTrack.Ending: return endingBgm;
                case BgmTrack.Opening: return openingBgm;
                case BgmTrack.LastDay: return lastDayBgm;
                case BgmTrack.Conflict: return conflictBgm;
                default: return null;
            }
        }

        public AudioClip ButtonClickSfx => buttonClickSfx;
    }
}
