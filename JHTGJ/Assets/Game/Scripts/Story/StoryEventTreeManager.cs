using JHTGJ.Core;
using JHTGJ.Interaction;
using JHTGJ.Scene;
using JHTGJ.UI;
using System.Collections;
using UnityEngine;

namespace JHTGJ.Story
{
    public class StoryEventTreeManager : MonoBehaviour
    {
        [SerializeField] StoryCampaign campaign;
        [SerializeField] DayStorySchedule schedule;
        [SerializeField] VillaSceneManager villaSceneManager;
        [SerializeField] DialogueUI dialogueUI;
        [SerializeField] StoryChoiceUI storyChoiceUI;
        [SerializeField] EndingScrollUI endingScrollUI;
        [SerializeField] int currentDay = 1;
        [SerializeField] int currentPhaseIndex;
        [SerializeField] bool phaseChoiceMade;
        [SerializeField] bool storyEnded;
        [SerializeField] bool enableDebugShortcuts =
#if UNITY_EDITOR
            true;
#else
            false;
#endif

        bool isPlayingStory;
        bool openingCgFinished = true;
        bool postCookingDiningActive;
        bool storyFlowReady;
        int autoPlayedWakeUpDay = -1;
        int autoPlayedWakeUpPhaseIndex = -1;

        public int CurrentDay => currentDay;
        public int CurrentPhaseIndex => currentPhaseIndex;
        public bool StoryEnded => storyEnded;
        public bool IsPlayingStory =>
            isPlayingStory ||
            (dialogueUI != null && dialogueUI.IsShowing) ||
            (storyChoiceUI != null && storyChoiceUI.IsShowing) ||
            (endingScrollUI != null && endingScrollUI.IsShowing);

        DayStorySchedule ActiveSchedule =>
            campaign != null ? campaign.GetScheduleForDay(currentDay) : schedule;

        public StoryPhaseDefinition CurrentPhase
        {
            get
            {
                var active = ActiveSchedule;
                return active != null &&
                       currentPhaseIndex >= 0 &&
                       currentPhaseIndex < active.Phases.Count
                    ? active.Phases[currentPhaseIndex]
                    : null;
            }
        }

        void Awake()
        {
            if (villaSceneManager == null)
                villaSceneManager = FindObjectOfType<VillaSceneManager>();

            EnsureDialogueUI();
            LoadProgressFromSave();
        }

        void Start()
        {
            if (OpeningCgPlayer.Instance != null && OpeningCgPlayer.Instance.IsPlaying)
            {
                openingCgFinished = false;
                OpeningCgPlayer.Finished += OnOpeningCgFinished;
            }
            else if (GameSession.HasPendingOpeningCg())
            {
                openingCgFinished = false;
                OpeningCgPlayer.Finished += OnOpeningCgFinished;
            }
            else
            {
                BeginStoryFlow();
            }
        }

        void OnDestroy()
        {
            OpeningCgPlayer.Finished -= OnOpeningCgFinished;
        }

        void Update()
        {
            if (!DebugShortcutsUtility.IsActive(enableDebugShortcuts))
                return;

            if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))
                return;

            if (IsPlayingStory || IsPauseMenuOpen())
                return;

            for (var day = 1; day <= 5; day++)
            {
                if (DebugDayKeyDown(day))
                    DebugJumpToDay(day);
            }
        }

        static bool DebugDayKeyDown(int day)
        {
            var alpha = (KeyCode)((int)KeyCode.Alpha1 + day - 1);
            var keypad = (KeyCode)((int)KeyCode.Keypad1 + day - 1);
            return Input.GetKeyDown(alpha) || Input.GetKeyDown(keypad);
        }

        bool IsPauseMenuOpen()
        {
            var pause = FindObjectOfType<PauseMenuUI>(true);
            return pause != null && pause.IsOpen;
        }

        void DebugJumpToDay(int day)
        {
            day = Mathf.Clamp(day, 1, 5);
            currentDay = day;
            storyEnded = false;
            phaseChoiceMade = false;
            postCookingDiningActive = false;
            isPlayingStory = false;
            autoPlayedWakeUpDay = -1;
            autoPlayedWakeUpPhaseIndex = -1;
            currentPhaseIndex = ResolveDebugPhaseIndex(day);
            ApplyPhaseIndexWithSkips();

            if (villaSceneManager == null)
                villaSceneManager = FindObjectOfType<VillaSceneManager>();

            ApplyRoomBackgroundsForCurrentPhase();

            var room = day >= 4 ? RoomType.Basement : RoomType.Bedroom;
            villaSceneManager?.SwitchRoom(room);

            storyFlowReady = true;
            RefreshStorySceneState();
            TryAutoSaveProgress();

            var phase = CurrentPhase;
            var phaseLabel = phase != null ? phase.DisplayName : "?";
            Debug.Log(
                $"[Story Debug] 跳转到第 {currentDay} 天 · {phaseLabel}（PhaseIndex={currentPhaseIndex}）。" +
                " Ctrl+1~5 切换天数，F9 地下室。");
        }

        int ResolveDebugPhaseIndex(int day)
        {
            var active = ActiveSchedule;
            if (active == null || active.Phases.Count == 0)
                return 0;

            if (day == 5)
            {
                for (var i = 0; i < active.Phases.Count; i++)
                {
                    if (IsDayFiveFinalPhase(active.Phases[i]))
                        return i;
                }

                return active.Phases.Count - 1;
            }

            for (var i = 0; i < active.Phases.Count; i++)
            {
                var phase = active.Phases[i];
                if (phase != null && phase.PhaseType != StoryPhaseType.WakeUp)
                    return i;
            }

            return Mathf.Min(1, active.Phases.Count - 1);
        }

        void OnOpeningCgFinished()
        {
            OpeningCgPlayer.Finished -= OnOpeningCgFinished;
            openingCgFinished = true;
            BeginStoryFlow();
        }

        void BeginStoryFlow()
        {
            ApplyRoomBackgroundsForCurrentPhase();
            EnsureSavedRoomLoaded();
            storyFlowReady = true;
            RefreshStorySceneState();
            LogCurrentPhase();
        }

        void EnsureSavedRoomLoaded()
        {
            if (GameSession.ShouldWaitForOpeningCg())
                return;

            if (!SaveManager.TryLoad(out _, out var room, out _, out _, out var storyEnded) || storyEnded)
                return;

            if (villaSceneManager == null)
                villaSceneManager = FindObjectOfType<VillaSceneManager>();

            villaSceneManager?.SwitchRoom(room);
        }

        void RefreshStoryBgm(bool endingScrollActive = false)
        {
            var phase = CurrentPhase;
            var conflictPhase = phase != null &&
                                StoryConflictBgmRules.ShouldPlayConflictBgm(currentDay, phase.PhaseType);
            GameAudioManager.Instance?.RefreshStoryBgm(currentDay, storyEnded, endingScrollActive, conflictPhase);
        }

        void EnsureDialogueUI()
        {
            if (dialogueUI == null)
                dialogueUI = FindObjectOfType<DialogueUI>(true);

            if (dialogueUI == null)
                dialogueUI = DialogueUIBuilder.Build(ActiveSchedule);
        }

        void LoadProgressFromSave()
        {
            if (!SaveManager.TryLoad(out var day, out _, out var phaseIndex, out var choiceMade, out var ended))
                return;

            if (ended)
                return;

            currentDay = day;
            currentPhaseIndex = phaseIndex;
            phaseChoiceMade = choiceMade;
            storyEnded = false;
            postCookingDiningActive = SaveManager.IsPostCookingDiningActive();
            ApplyPhaseIndexWithSkips();
        }

        void ApplyPhaseIndexWithSkips()
        {
            var active = ActiveSchedule;
            if (active == null)
                return;

            while (currentPhaseIndex < active.Phases.Count)
            {
                var phase = active.Phases[currentPhaseIndex];
                if (phase != null &&
                    phase.PhaseType == StoryPhaseType.NightEvent &&
                    !active.IncludeNightEvent)
                {
                    currentPhaseIndex++;
                    continue;
                }

                break;
            }
        }

        public bool CanInteract(InteractPoint point) =>
            CanInteractWithStory(point != null ? point.InteractId : null, point != null ? point.Kind : default);

        public bool CanInteract(StoryCharacterInteractPoint point) =>
            CanInteractWithStory(point != null ? point.InteractId : null, point != null ? point.Kind : default);

        public bool TryHandleInteraction(InteractPoint point)
        {
            if (point == null)
                return false;

            if (point.Kind == InteractionKind.EmergencyStop)
            {
                if (IsPlayingStory || storyEnded)
                    return false;

                if (currentDay == 5)
                {
                    if (!IsDayFiveFinalPhase())
                        return false;

                    TryPlayEmergencyStopFlavor();
                    return true;
                }

                var phaseEvent = FindMatchingEvent(point.InteractId, point.Kind);
                if (phaseEvent != null)
                {
                    PlayStoryEvent(phaseEvent, CompleteEmergencyStopPhaseEvent);
                    return true;
                }

                TryPlayEmergencyStopFlavor();
                return true;
            }

            if (IsPlayingStory || storyEnded)
                return false;

            if (point.Kind == InteractionKind.ChangeRoom)
                return false;

            return TryHandleStoryInteraction(point.InteractId, point.Kind, AdvanceAfterPhaseChoice);
        }

        public bool TryHandleInteraction(StoryCharacterInteractPoint point)
        {
            if (point == null || IsPlayingStory || storyEnded)
                return false;

            return TryHandleStoryInteraction(point.InteractId, point.Kind, AdvanceAfterPhaseChoice);
        }

        bool CanInteractWithStory(string interactId, InteractionKind kind)
        {
            if (string.IsNullOrWhiteSpace(interactId))
                return false;

            if (kind == InteractionKind.EmergencyStop)
            {
                if (storyEnded || IsPlayingStory)
                    return false;

                if (currentDay == 5)
                    return IsDayFiveFinalPhase();

                if (FindMatchingEvent(interactId, kind) != null)
                    return true;

                return !HasPendingEmergencyStopPhase();
            }

            if (IsPlayingStory || storyEnded)
                return false;

            if (kind == InteractionKind.ChangeRoom)
                return true;

            if (phaseChoiceMade || ActiveSchedule == null)
                return false;

            var phase = CurrentPhase;
            if (phase != null && phase.PhaseType == StoryPhaseType.WakeUp)
                return false;

            if (phase != null && phase.PhaseType == StoryPhaseType.ButtonChoice)
                return false;

            return FindMatchingEvent(interactId, kind) != null;
        }

        bool TryHandleStoryInteraction(string interactId, InteractionKind kind, System.Action onComplete)
        {
            var storyEvent = FindMatchingEvent(interactId, kind);
            if (storyEvent == null)
                return false;

            PlayStoryEvent(storyEvent, onComplete);
            return true;
        }

        StoryEventDefinition FindMatchingEvent(InteractPoint point) =>
            point == null ? null : FindMatchingEvent(point.InteractId, point.Kind);

        StoryEventDefinition FindMatchingEvent(string interactId, InteractionKind kind)
        {
            var phase = CurrentPhase;
            if (phase == null)
                return null;

            foreach (var storyEvent in phase.Events)
            {
                if (storyEvent == null)
                    continue;

                if (storyEvent.MatchesInteractPoint(interactId, kind))
                    return storyEvent;
            }

            return null;
        }

        void PlayStoryEvent(StoryEventDefinition storyEvent, System.Action onComplete, bool hideWifePortrait = false)
        {
            EnsureDialogueUI();
            isPlayingStory = true;

            var active = ActiveSchedule;
            var phase = CurrentPhase;
            dialogueUI.ShowStory(
                storyEvent,
                ResolvePortrait(phase, active, protagonist: true),
                ResolvePortrait(phase, active, protagonist: false),
                () =>
                {
                    isPlayingStory = false;
                    ApplyPostDialogueStoryEffects(storyEvent);
                    onComplete?.Invoke();
                },
                hideWifePortrait);
        }

        void ApplyPostDialogueStoryEffects(StoryEventDefinition storyEvent)
        {
            ApplyPostDialogueRoomBackground(storyEvent);

            if (ShouldActivatePostCookingDining(storyEvent))
                SetPostCookingDiningActive(true);
        }

        static bool ShouldActivatePostCookingDining(StoryEventDefinition storyEvent)
        {
            if (storyEvent == null || storyEvent.EventId != "Interact_Fridge")
                return false;

            return storyEvent.InteractKind == InteractionKind.CookBreakfast;
        }

        void SetPostCookingDiningActive(bool active)
        {
            postCookingDiningActive = active;
            SaveManager.SetPostCookingDiningActive(active);
            ApplyRoomBackgroundsForCurrentPhase();
            RefreshCharacterPresences();
        }

        void ApplyPostDialogueRoomBackground(StoryEventDefinition storyEvent)
        {
            if (storyEvent == null || villaSceneManager == null)
                return;

            if (!storyEvent.TryGetPostDialogueBackgroundChange(
                    villaSceneManager.CurrentRoomType,
                    out var targetRoom,
                    out var backgroundSprite))
                return;

            if (targetRoom == RoomType.Storage)
                return;

            if (villaSceneManager.TrySetRoomBackground(targetRoom, backgroundSprite))
                return;

            Debug.LogWarning($"[Story] 无法更换 {targetRoom} 的背景图。");
        }

        static Sprite ResolvePortrait(StoryPhaseDefinition phase, DayStorySchedule schedule, bool protagonist)
        {
            if (phase != null)
            {
                var phasePortrait = protagonist ? phase.PhaseProtagonistPortrait : phase.PhaseWifePortrait;
                if (phasePortrait != null)
                    return phasePortrait;
            }

            var library = StoryPortraitLibrary.Instance;
            if (library != null && phase != null)
            {
                var libraryPortrait = protagonist
                    ? library.GetProtagonistPortrait(phase.PhaseType)
                    : library.GetWifePortrait(phase.PhaseType);
                if (libraryPortrait != null)
                    return libraryPortrait;
            }

            return protagonist
                ? schedule?.DefaultProtagonistPortrait
                : schedule?.DefaultWifePortrait;
        }

        void AdvanceAfterPhaseChoice()
        {
            currentPhaseIndex++;
            phaseChoiceMade = false;
            ApplyPhaseIndexWithSkips();

            var active = ActiveSchedule;
            if (active == null || currentPhaseIndex >= active.Phases.Count)
            {
                CompleteDayLoop();
                return;
            }

            ApplyRoomBackgroundsForCurrentPhase();
            RefreshStorySceneState();
            LogCurrentPhase();
            TryAutoSaveProgress();
        }

        void CompleteDayLoop()
        {
            SetPostCookingDiningActive(false);
            currentDay++;
            currentPhaseIndex = 0;
            phaseChoiceMade = false;
            autoPlayedWakeUpDay = -1;
            autoPlayedWakeUpPhaseIndex = -1;
            ApplyPhaseIndexWithSkips();
            Debug.Log($"[Story] 第 {currentDay} 天开始。");

            if (villaSceneManager != null)
                villaSceneManager.SwitchRoom(RoomType.Bedroom);

            ApplyRoomBackgroundsForCurrentPhase();
            TryAutoSaveProgress();
            RefreshStorySceneState();
            LogCurrentPhase();
        }

        void ApplyRoomBackgroundsForCurrentPhase()
        {
            if (villaSceneManager == null)
                return;

            var useNight = CurrentPhase != null &&
                           StoryPhaseTimeOfDay.UsesNightRoomBackground(CurrentPhase.PhaseType);
            villaSceneManager.ApplyRoomBackgroundSet(useNight, NightRoomBackgroundLibrary.Instance);
            ApplySpecialNightEventBackground();
            ApplyPostCookingDiningRoomBackground(useNight);
            ApplyStorageRoomBackground();
        }

        void ApplySpecialNightEventBackground()
        {
            if (villaSceneManager == null ||
                !TryGetSpecialNightEventConfig(out var config))
                return;

            if (config.Background != null)
                villaSceneManager.TrySetRoomBackground(config.Room, config.Background);
        }

        bool TryGetSpecialNightEventConfig(out SpecialNightEventConfig config) =>
            SpecialNightEventResolver.TryGetConfig(
                currentDay,
                CurrentPhase?.PhaseType ?? default,
                out config);

        void ApplyPostCookingDiningRoomBackground(bool useNight)
        {
            if (villaSceneManager == null || !postCookingDiningActive || useNight)
                return;

            var sprite = PostCookingDiningLibrary.Instance?.DiningRoomBackground;
            if (sprite != null)
                villaSceneManager.TrySetRoomBackground(RoomType.DiningRoom, sprite);
        }

        void ApplyStorageRoomBackground()
        {
            if (villaSceneManager == null)
                return;

            var sprite = ActiveSchedule?.StorageCleanBackground;
            if (sprite == null)
                return;

            villaSceneManager.TrySetRoomBackground(RoomType.Storage, sprite);
        }

        void EnsureEndingScrollUI()
        {
            if (endingScrollUI == null)
                endingScrollUI = FindObjectOfType<EndingScrollUI>(true);

            if (endingScrollUI == null)
                endingScrollUI = EndingScrollUIBuilder.Build();
        }

        void PlayEndingScroll(string content, System.Action onComplete)
        {
            EnsureEndingScrollUI();
            StartCoroutine(PlayEndingScrollAfterDialogue(content, onComplete));
        }

        IEnumerator PlayEndingScrollAfterDialogue(string content, System.Action onComplete)
        {
            yield return null;

            if (endingScrollUI == null)
                EnsureEndingScrollUI();

            if (endingScrollUI == null)
            {
                onComplete?.Invoke();
                yield break;
            }

            if (!endingScrollUI.gameObject.activeInHierarchy)
                endingScrollUI.gameObject.SetActive(true);

            RefreshStoryBgm(endingScrollActive: true);
            endingScrollUI.Show(content, onComplete);
        }

        void CompleteStoryEnding(string logMessage = "[Story] 时间循环已终止。")
        {
            storyEnded = true;
            RefreshStorySceneState();
            Debug.Log(logMessage);
        }

        void PlayEndingScrollAndComplete(string content, string logMessage)
        {
            PlayEndingScroll(content, () =>
            {
                CompleteStoryEnding(logMessage);
                ReturnToMainMenuAfterEnding();
            });
        }

        static void ReturnToMainMenuAfterEnding()
        {
            Time.timeScale = 1f;
            SaveManager.DeleteSave();
            SceneLoader.LoadMainMenuScene();
        }

        void TryPlayEmergencyStopFlavor()
        {
            var active = ActiveSchedule;
            if (active == null || active.EndingEvent == null)
            {
                Debug.LogWarning("[Story] 未配置急停按钮剧情。");
                return;
            }

            PlayStoryEvent(active.EndingEvent, OnEmergencyStopFlavorFinished, hideWifePortrait: true);
        }

        void OnEmergencyStopFlavorFinished()
        {
            if (currentDay == 3 || currentDay == 4)
            {
                PlayEndingScrollAndComplete(EndingScrollContent.Ending1, "[Story] 结局一。");
                return;
            }

            if (currentDay == 5)
            {
                PlayEndingScrollAndComplete(EndingScrollContent.Ending2, "[Story] 结局二。");
                return;
            }

            RefreshInteractMarkers();
        }

        static bool IsDayFiveFinalPhase(StoryPhaseDefinition phase) =>
            phase != null && phase.DisplayName == "最后";

        bool IsDayFiveFinalPhase() => IsDayFiveFinalPhase(CurrentPhase);

        void CompleteEmergencyStopPhaseEvent()
        {
            if (currentDay == 3 || currentDay == 4)
            {
                PlayEndingScrollAndComplete(EndingScrollContent.Ending1, "[Story] 结局一。");
                return;
            }

            if (currentDay == 5)
            {
                PlayEndingScrollAndComplete(EndingScrollContent.Ending2, "[Story] 结局二。");
                return;
            }

            RefreshInteractMarkers();
        }

        public void SaveProgress(RoomType room)
        {
            SaveManager.SaveGame(currentDay, room, currentPhaseIndex, phaseChoiceMade, storyEnded);
        }

        void TryAutoSaveProgress()
        {
            if (storyEnded)
                return;

            var room = villaSceneManager != null
                ? villaSceneManager.CurrentRoomType
                : RoomType.Bedroom;

            SaveProgress(room);

            var phase = CurrentPhase;
            var phaseLabel = phase != null ? phase.DisplayName : "新的一天";
            Debug.Log($"[AutoSave] 已自动保存：第 {currentDay} 天 · {phaseLabel}");
        }

        public void RefreshInteractMarkers()
        {
            RefreshInteractPointMarkers();
            RefreshCharacterPresences();
            RefreshButtonPhaseUi();
        }

        void RefreshStorySceneState()
        {
            RefreshInteractPointMarkers();
            RefreshCharacterPresences();
            RefreshButtonPhaseUi();
            TryAutoPlayWakeUpPhase();
        }

        void TryAutoPlayWakeUpPhase()
        {
            if (!storyFlowReady || !openingCgFinished)
                return;

            var phase = CurrentPhase;
            if (phase == null || storyEnded || isPlayingStory || phaseChoiceMade)
                return;

            if (phase.PhaseType != StoryPhaseType.WakeUp)
                return;

            if (autoPlayedWakeUpDay == currentDay && autoPlayedWakeUpPhaseIndex == currentPhaseIndex)
                return;

            var storyEvent = GetAutoPlayPhaseEvent(phase);
            if (storyEvent == null)
            {
                Debug.LogWarning("[Story] 醒来阶段未配置 Events，已自动跳过。");
                AdvanceAfterPhaseChoice();
                return;
            }

            autoPlayedWakeUpDay = currentDay;
            autoPlayedWakeUpPhaseIndex = currentPhaseIndex;
            PlayStoryEvent(storyEvent, AdvanceAfterPhaseChoice);
        }

        static StoryEventDefinition GetAutoPlayPhaseEvent(StoryPhaseDefinition phase)
        {
            if (phase == null)
                return null;

            foreach (var storyEvent in phase.Events)
            {
                if (storyEvent == null || storyEvent.Lines == null || storyEvent.Lines.Count == 0)
                    continue;

                return storyEvent;
            }

            return null;
        }

        void EnsureStoryChoiceUI()
        {
            if (storyChoiceUI == null)
                storyChoiceUI = FindObjectOfType<StoryChoiceUI>(true);

            if (storyChoiceUI == null)
                storyChoiceUI = StoryChoiceUIBuilder.Build();
        }

        void RefreshButtonPhaseUi()
        {
            EnsureStoryChoiceUI();
            if (storyChoiceUI == null)
                return;

            var phase = CurrentPhase;
            if (storyEnded ||
                isPlayingStory ||
                phaseChoiceMade ||
                phase == null ||
                phase.PhaseType != StoryPhaseType.ButtonChoice)
            {
                storyChoiceUI.Hide();
                return;
            }

            storyChoiceUI.Show(
                phase.DisplayName,
                phase.ChoicePrompt,
                phase.Events,
                OnButtonPhaseEventSelected);
        }

        void OnButtonPhaseEventSelected(StoryEventDefinition storyEvent)
        {
            if (storyChoiceUI != null)
                storyChoiceUI.Hide();

            if (storyEvent == null)
                return;

            PlayStoryEvent(storyEvent, AdvanceAfterPhaseChoice);
        }

        void RefreshInteractPointMarkers()
        {
            var points = FindObjectsOfType<InteractPoint>(true);
            foreach (var point in points)
                point.SetAvailable(CanInteract(point));
        }

        void RefreshCharacterPresences()
        {
            var phase = CurrentPhase;
            var characterPoints = FindObjectsOfType<StoryCharacterInteractPoint>(true);

            foreach (var point in characterPoints)
            {
                var presence = FindCharacterPresence(phase, point.InteractId);
                if (presence == null || storyEnded)
                {
                    point.SetPresenceActive(false);
                    continue;
                }

                var room = villaSceneManager != null ? villaSceneManager.GetRoom(presence.Room) : null;
                if (room == null)
                {
                    point.SetPresenceActive(false);
                    Debug.LogWarning($"[Story] 未找到角色所在房间：{presence.Room}（{point.InteractId}）");
                    continue;
                }

                point.SetPresenceActive(true);

                if (TryApplyEmbeddedBackgroundInteract(point, presence, room))
                    continue;

                point.ApplyPresence(presence, room);
            }
        }

        bool TryApplyEmbeddedBackgroundInteract(
            StoryCharacterInteractPoint point,
            StoryPhaseCharacterPresence presence,
            Room room)
        {
            if (TryApplySpecialNightEventEmbeddedInteract(point, presence, room))
                return true;

            if (ShouldUseEmbeddedDiningWifeInteract(presence))
            {
                point.ApplyEmbeddedBackgroundInteract(
                    room,
                    PostCookingDiningInteractLayout.WifeLocalX,
                    PostCookingDiningInteractLayout.StandXOffset,
                    PostCookingDiningInteractLayout.WifeFacing);
                return true;
            }

            return false;
        }

        bool TryApplySpecialNightEventEmbeddedInteract(
            StoryCharacterInteractPoint point,
            StoryPhaseCharacterPresence presence,
            Room room)
        {
            if (presence == null ||
                !TryGetSpecialNightEventConfig(out var config) ||
                presence.InteractId != "Interact_Partner" ||
                presence.Room != config.Room)
                return false;

            point.ApplyEmbeddedBackgroundInteract(
                room,
                config.WifeLocalX,
                config.StandXOffset,
                config.WifeFacing);
            return true;
        }

        bool ShouldUseEmbeddedDiningWifeInteract(StoryPhaseCharacterPresence presence)
        {
            if (!postCookingDiningActive || presence == null)
                return false;

            if (presence.InteractId != "Interact_Partner" || presence.Room != RoomType.DiningRoom)
                return false;

            var phase = CurrentPhase;
            return phase != null && !StoryPhaseTimeOfDay.UsesNightRoomBackground(phase.PhaseType);
        }

        static StoryPhaseCharacterPresence FindCharacterPresence(StoryPhaseDefinition phase, string interactId)
        {
            if (phase == null || string.IsNullOrWhiteSpace(interactId))
                return null;

            StoryPhaseCharacterPresence match = null;
            foreach (var presence in phase.CharacterPresences)
            {
                if (presence != null && presence.InteractId == interactId)
                    match = presence;
            }

            return match;
        }

        bool HasPendingEmergencyStopPhase()
        {
            var active = ActiveSchedule;
            if (active == null || currentPhaseIndex < 0)
                return false;

            for (var i = currentPhaseIndex; i < active.Phases.Count; i++)
            {
                var phase = active.Phases[i];
                if (phase == null)
                    continue;

                foreach (var storyEvent in phase.Events)
                {
                    if (storyEvent != null &&
                        storyEvent.InteractKind == InteractionKind.EmergencyStop)
                        return true;
                }
            }

            return false;
        }

        void LogCurrentPhase()
        {
            var phase = CurrentPhase;
            if (phase == null)
            {
                Debug.Log($"[Story] 第 {currentDay} 天：所有阶段已完成，等待下一次循环。");
                RefreshStoryBgm();
                return;
            }

            Debug.Log($"[Story] 第 {currentDay} 天 · {phase.DisplayName}（{phase.PhaseType}）");
            RefreshStoryBgm();
        }
    }
}
