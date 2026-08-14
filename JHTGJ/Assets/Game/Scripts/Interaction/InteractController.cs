using JHTGJ.Character;
using JHTGJ.Scene;
using JHTGJ.Story;
using JHTGJ.UI;
using UnityEngine;
using UnityEngine.UI;

namespace JHTGJ.Interaction
{
    public class InteractController : MonoBehaviour
    {
        [SerializeField] SideViewCharacterController protagonist;
        [SerializeField] VillaSceneManager sceneManager;
        [SerializeField] StoryEventTreeManager storyManager;
        [SerializeField] RoomSelectionUI roomSelectionUI;
        [SerializeField] InteractionPromptUI interactionPromptUI;
        [SerializeField] float standTolerance = 1.5f;

        PauseMenuUI pauseMenuUI;

        void Awake()
        {
            if (protagonist == null)
                protagonist = GetComponent<SideViewCharacterController>();
            if (storyManager == null)
                storyManager = FindObjectOfType<StoryEventTreeManager>();
            if (roomSelectionUI == null)
                roomSelectionUI = FindObjectOfType<RoomSelectionUI>(true);
            if (interactionPromptUI == null)
                interactionPromptUI = FindObjectOfType<InteractionPromptUI>(true);
        }

        void Update()
        {
            RefreshInteractionPrompt();

            if (ShouldBlockInteraction())
                return;

            if (!Input.GetKeyDown(KeyCode.E) && !Input.GetKeyDown(KeyCode.Space))
                return;

            if (!TryResolveInteractionTarget(
                    out var emergencyStop,
                    out var selector,
                    out var characterPoint,
                    out var point))
                return;

            if (emergencyStop != null &&
                storyManager != null &&
                storyManager.TryHandleInteraction(emergencyStop))
                return;

            if (selector != null)
            {
                OpenRoomSelector(selector);
                return;
            }

            if (characterPoint != null)
            {
                storyManager?.TryHandleInteraction(characterPoint);
                return;
            }

            if (point == null)
                return;

            if (point.Kind == InteractionKind.ChangeRoom && sceneManager != null)
            {
                sceneManager.SwitchRoom(point.TargetRoom, point.TargetSpawnSide);
                return;
            }

            storyManager?.TryHandleInteraction(point);
        }

        void RefreshInteractionPrompt()
        {
            EnsureInteractionPromptUI();
            if (interactionPromptUI == null)
                return;

            if (ShouldBlockInteraction() ||
                !TryResolveInteractionTarget(
                    out var emergencyStop,
                    out var selector,
                    out var characterPoint,
                    out var point))
            {
                interactionPromptUI.Hide();
                return;
            }

            var worldPosition = GetPromptWorldPosition(emergencyStop, selector, characterPoint, point);
            interactionPromptUI.ShowAtWorldPosition(worldPosition);
        }

        static Vector3 GetPromptWorldPosition(
            InteractPoint emergencyStop,
            RoomSelectorInteractPoint selector,
            StoryCharacterInteractPoint characterPoint,
            InteractPoint point)
        {
            if (emergencyStop != null)
                return GetMarkerPromptPosition(emergencyStop.transform.position, emergencyStop.StandX);

            if (selector != null)
                return GetMarkerPromptPosition(selector.transform.position, selector.StandX);

            if (characterPoint != null)
                return GetCharacterPromptPosition(characterPoint);

            if (point != null)
                return GetMarkerPromptPosition(point.transform.position, point.StandX);

            return Vector3.zero;
        }

        static Vector3 GetMarkerPromptPosition(Vector3 markerPosition, float standX)
        {
            var y = markerPosition.y + 0.75f;
            return new Vector3(standX, y, markerPosition.z);
        }

        static Vector3 GetCharacterPromptPosition(StoryCharacterInteractPoint characterPoint)
        {
            var topY = characterPoint.transform.position.y + 1.5f;
            foreach (var renderer in characterPoint.GetComponentsInChildren<SpriteRenderer>(true))
            {
                if (renderer == null || !renderer.enabled || renderer.sprite == null)
                    continue;

                topY = Mathf.Max(topY, renderer.bounds.max.y);
            }

            return new Vector3(characterPoint.StandX, topY + 0.35f, characterPoint.transform.position.z);
        }

        bool ShouldBlockInteraction()
        {
            if (protagonist == null)
                return true;

            if (roomSelectionUI != null && roomSelectionUI.IsShowing)
                return true;

            if (storyManager != null && storyManager.IsPlayingStory)
                return true;

            if (OpeningCgPlayer.Instance != null && OpeningCgPlayer.Instance.IsPlaying)
                return true;

            if (pauseMenuUI == null)
                pauseMenuUI = FindObjectOfType<PauseMenuUI>(true);

            return pauseMenuUI != null && pauseMenuUI.IsOpen;
        }

        bool TryResolveInteractionTarget(
            out InteractPoint emergencyStop,
            out RoomSelectorInteractPoint selector,
            out StoryCharacterInteractPoint characterPoint,
            out InteractPoint point)
        {
            emergencyStop = null;
            selector = null;
            characterPoint = null;
            point = null;

            if (protagonist == null)
                return false;

            emergencyStop = FindEmergencyStopPoint(out _);
            if (emergencyStop != null)
                return true;

            selector = FindAvailableRoomSelector(out var selectorDistance);
            characterPoint = FindAvailableCharacterPoint(out var characterDistance);
            point = FindAvailablePoint(out var pointDistance);

            if (selector != null)
            {
                var bestDistance = characterPoint != null
                    ? Mathf.Min(characterDistance, pointDistance)
                    : pointDistance;

                if (point == null || selectorDistance <= bestDistance)
                    return true;

                selector = null;
            }

            if (characterPoint != null && (point == null || characterDistance <= pointDistance))
                return true;

            return point != null;
        }

        void OpenRoomSelector(RoomSelectorInteractPoint selector)
        {
            EnsureRoomSelectionUI();
            if (roomSelectionUI == null || sceneManager == null || selector == null)
                return;

            roomSelectionUI.Show(
                selector.Label,
                selector.Destinations,
                destination => sceneManager.SwitchRoom(destination.TargetRoom, destination.SpawnSide));
        }

        void EnsureRoomSelectionUI()
        {
            if (roomSelectionUI == null)
                roomSelectionUI = FindObjectOfType<RoomSelectionUI>(true);

            if (roomSelectionUI == null)
                roomSelectionUI = RoomSelectionUIBuilder.Build();
        }

        void EnsureInteractionPromptUI()
        {
            if (interactionPromptUI != null &&
                interactionPromptUI.GetComponentInChildren<Text>(true) != null)
                return;

            interactionPromptUI = FindObjectOfType<InteractionPromptUI>(true);
            if (interactionPromptUI != null &&
                interactionPromptUI.GetComponentInChildren<Text>(true) != null)
                return;

            interactionPromptUI = InteractionPromptUIBuilder.Build();
        }

        InteractPoint FindAvailablePoint(out float bestDistance)
        {
            bestDistance = float.MaxValue;
            var room = protagonist.CurrentRoom;
            if (room == null)
                return null;

            InteractPoint best = null;
            var playerX = protagonist.transform.position.x;

            foreach (var interactPoint in room.GetComponentsInChildren<InteractPoint>(false))
            {
                if (storyManager != null && !storyManager.CanInteract(interactPoint))
                    continue;

                var distance = GetInteractionDistance(playerX, interactPoint.StandX, interactPoint.transform.position.x);
                if (distance > standTolerance || distance >= bestDistance)
                    continue;

                best = interactPoint;
                bestDistance = distance;
            }

            return best;
        }

        InteractPoint FindEmergencyStopPoint(out float bestDistance)
        {
            bestDistance = float.MaxValue;
            var room = protagonist.CurrentRoom;
            if (room == null)
                return null;

            if (storyManager != null && storyManager.StoryEnded)
                return null;

            InteractPoint best = null;
            var playerX = protagonist.transform.position.x;

            foreach (var interactPoint in room.GetComponentsInChildren<InteractPoint>(false))
            {
                if (interactPoint.Kind != InteractionKind.EmergencyStop)
                    continue;

                var distance = GetInteractionDistance(playerX, interactPoint.StandX, interactPoint.transform.position.x);
                if (distance > standTolerance || distance >= bestDistance)
                    continue;

                best = interactPoint;
                bestDistance = distance;
            }

            return best;
        }

        StoryCharacterInteractPoint FindAvailableCharacterPoint(out float bestDistance)
        {
            bestDistance = float.MaxValue;
            var room = protagonist.CurrentRoom;
            if (room == null)
                return null;

            StoryCharacterInteractPoint best = null;
            var playerX = protagonist.transform.position.x;

            foreach (var characterPoint in room.GetComponentsInChildren<StoryCharacterInteractPoint>(false))
            {
                if (!characterPoint.IsPresenceActive)
                    continue;

                if (storyManager != null && !storyManager.CanInteract(characterPoint))
                    continue;

                var distance = GetInteractionDistance(
                    playerX,
                    characterPoint.StandX,
                    characterPoint.transform.position.x);
                if (distance > standTolerance || distance >= bestDistance)
                    continue;

                best = characterPoint;
                bestDistance = distance;
            }

            return best;
        }

        RoomSelectorInteractPoint FindAvailableRoomSelector(out float bestDistance)
        {
            bestDistance = float.MaxValue;
            var room = protagonist.CurrentRoom;
            if (room == null)
                return null;

            RoomSelectorInteractPoint best = null;
            var playerX = protagonist.transform.position.x;

            foreach (var selector in room.GetComponentsInChildren<RoomSelectorInteractPoint>(false))
            {
                if (selector.Destinations == null || selector.Destinations.Count == 0)
                    continue;

                var distance = GetInteractionDistance(
                    playerX,
                    selector.StandX,
                    selector.transform.position.x);
                if (distance > standTolerance || distance >= bestDistance)
                    continue;

                best = selector;
                bestDistance = distance;
            }

            return best;
        }

        static float GetInteractionDistance(float playerX, float standX, float markerX) =>
            Mathf.Min(Mathf.Abs(playerX - standX), Mathf.Abs(playerX - markerX));
    }
}
