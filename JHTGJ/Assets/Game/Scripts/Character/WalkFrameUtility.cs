using System.Collections.Generic;
using UnityEngine;

namespace JHTGJ.Character
{
    public static class WalkFrameUtility
    {
        public static Sprite[] PrepareWalkFrames(Sprite[] frames, Sprite idle)
        {
            if (frames == null || frames.Length == 0)
                return System.Array.Empty<Sprite>();

            var result = new List<Sprite>(frames.Length);
            foreach (var frame in frames)
            {
                if (frame == null || frame == idle)
                    continue;

                if (result.Count > 0 && result[result.Count - 1] == frame)
                    continue;

                result.Add(frame);
            }

            if (result.Count > 0)
                return result.ToArray();

            foreach (var frame in frames)
            {
                if (frame != null)
                    return new[] { frame };
            }

            return System.Array.Empty<Sprite>();
        }
    }
}
