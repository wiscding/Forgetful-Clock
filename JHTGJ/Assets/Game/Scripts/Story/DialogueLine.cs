using System;
using UnityEngine;

namespace JHTGJ.Story
{
    [Serializable]
    public class DialogueLine
    {
        [SerializeField] string speakerName = "主角";
        [TextArea(2, 6)]
        [SerializeField] string text;
        [SerializeField] Sprite protagonistPortraitOverride;
        [SerializeField] Sprite wifePortraitOverride;

        public string SpeakerName => speakerName;
        public string Text => text;
        public Sprite ProtagonistPortraitOverride => protagonistPortraitOverride;
        public Sprite WifePortraitOverride => wifePortraitOverride;
    }
}
