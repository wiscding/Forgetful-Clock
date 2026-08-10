using System;
using System.Collections.Generic;
using JHTGJ.Interaction;
using JHTGJ.Scene;
using UnityEngine;

namespace JHTGJ.Story
{
    [Serializable]
    public class StoryEventDefinition
    {
        [SerializeField] string eventId;
        [SerializeField] string summary;
        [SerializeField] string buttonLabel;
        [SerializeField] InteractionKind interactKind = InteractionKind.TalkToPartner;
        [SerializeField] Sprite protagonistPortrait;
        [SerializeField] Sprite wifePortrait;
        [SerializeField] List<DialogueLine> lines = new List<DialogueLine>();

        [Header("对话结束后（可选）")]
        [SerializeField] bool changeRoomBackgroundAfterDialogue;
        [SerializeField] Sprite roomBackgroundSprite;
        [SerializeField] bool useCurrentRoomForBackground = true;
        [SerializeField] RoomType backgroundTargetRoom;

        public string EventId => eventId;
        public string Summary => summary;
        public string ButtonLabel => !string.IsNullOrWhiteSpace(buttonLabel) ? buttonLabel : summary;
        public InteractionKind InteractKind => interactKind;
        public Sprite ProtagonistPortrait => protagonistPortrait;
        public Sprite WifePortrait => wifePortrait;
        public IReadOnlyList<DialogueLine> Lines => lines;
        public bool ChangeRoomBackgroundAfterDialogue => changeRoomBackgroundAfterDialogue;
        public Sprite RoomBackgroundSprite => roomBackgroundSprite;
        public bool UseCurrentRoomForBackground => useCurrentRoomForBackground;
        public RoomType BackgroundTargetRoom => backgroundTargetRoom;

        public bool MatchesInteractPoint(string interactPointId, InteractionKind kind)
        {
            if (!string.IsNullOrWhiteSpace(eventId))
                return eventId == interactPointId;

            return interactKind == kind;
        }

        public bool TryGetPostDialogueBackgroundChange(
            RoomType currentRoom,
            out RoomType targetRoom,
            out Sprite backgroundSprite)
        {
            targetRoom = default;
            backgroundSprite = null;

            if (!changeRoomBackgroundAfterDialogue || roomBackgroundSprite == null)
                return false;

            targetRoom = useCurrentRoomForBackground ? currentRoom : backgroundTargetRoom;
            backgroundSprite = roomBackgroundSprite;
            return true;
        }
    }
}
