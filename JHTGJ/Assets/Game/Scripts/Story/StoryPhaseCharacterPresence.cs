using System;
using JHTGJ.Character;
using JHTGJ.Scene;
using UnityEngine;

namespace JHTGJ.Story
{
    [Serializable]
    public class StoryPhaseCharacterPresence
    {
        [SerializeField] string interactId = "Interact_Partner";
        [SerializeField] RoomType room = RoomType.LivingRoom;
        [SerializeField] float localX;
        [SerializeField] bool useScenePosition;
        [SerializeField] FacingDirection facing = FacingDirection.Left;
        [SerializeField] CharacterAppearanceProfile appearanceProfile;
        [SerializeField] Sprite idleSprite;

        public string InteractId => interactId;
        public RoomType Room => room;
        public float LocalX => localX;
        public bool UseScenePosition => useScenePosition;
        public FacingDirection Facing => facing;
        public CharacterAppearanceProfile AppearanceProfile => appearanceProfile;
        public Sprite IdleSprite => idleSprite;
    }
}
