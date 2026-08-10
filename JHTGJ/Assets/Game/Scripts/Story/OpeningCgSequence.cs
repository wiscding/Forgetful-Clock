using System;
using System.Collections.Generic;
using UnityEngine;

namespace JHTGJ.Story
{
    [Serializable]
    public class OpeningCgSlideDefinition
    {
        [SerializeField] Sprite image;
        [SerializeField] List<DialogueLine> lines = new List<DialogueLine>();

        public Sprite Image => image;
        public IReadOnlyList<DialogueLine> Lines => lines;
    }

    [CreateAssetMenu(fileName = "OpeningCgSequence", menuName = "JHTGJ/Opening CG Sequence")]
    public class OpeningCgSequence : ScriptableObject
    {
        [SerializeField] List<OpeningCgSlideDefinition> slides = new List<OpeningCgSlideDefinition>();

        public IReadOnlyList<OpeningCgSlideDefinition> Slides => slides;
    }
}
