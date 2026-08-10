using System;
using JHTGJ.Character;
using JHTGJ.Core;
using JHTGJ.Scene;
using JHTGJ.UI;
using UnityEngine;

namespace JHTGJ.Story
{
    public class OpeningCgPlayer : MonoBehaviour
    {
        public static OpeningCgPlayer Instance { get; private set; }
        public static event Action Finished;

        [SerializeField] OpeningCgSequence sequence;
        [SerializeField] OpeningCgUI openingCgUI;
        [SerializeField] VillaSceneManager villaSceneManager;

        public bool IsPlaying { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        void Start()
        {
            if (!GameSession.TryConsumeOpeningCg())
                return;

            if (villaSceneManager == null)
                villaSceneManager = FindObjectOfType<VillaSceneManager>();

            EnsureOpeningCgUI();

            if (sequence == null || scheduleHasNoSlides())
            {
                Debug.LogWarning("[OpeningCG] 未配置开场 CG，直接进入游戏。");
                Complete();
                return;
            }

            PlaySequence();
        }

        bool scheduleHasNoSlides() =>
            sequence.Slides == null || sequence.Slides.Count == 0;

        void EnsureOpeningCgUI()
        {
            if (openingCgUI == null)
                openingCgUI = FindObjectOfType<OpeningCgUI>(true);

            if (openingCgUI == null)
                openingCgUI = OpeningCgUIBuilder.Build();
        }

        void PlaySequence()
        {
            var slides = sequence.Slides;
            var images = new Sprite[slides.Count];
            var lines = new DialogueLine[slides.Count][];

            for (var i = 0; i < slides.Count; i++)
            {
                var slide = slides[i];
                images[i] = slide != null ? slide.Image : null;

                if (slide?.Lines == null || slide.Lines.Count == 0)
                {
                    lines[i] = System.Array.Empty<DialogueLine>();
                    continue;
                }

                var slideLines = new DialogueLine[slide.Lines.Count];
                for (var j = 0; j < slide.Lines.Count; j++)
                    slideLines[j] = slide.Lines[j];
                lines[i] = slideLines;
            }

            IsPlaying = true;
            SetGameplayVisible(false);
            GameAudioManager.Instance?.PlayBgm(BgmTrack.Opening);
            openingCgUI.Play(images, lines, Complete);
        }

        void Complete()
        {
            IsPlaying = false;
            EnsureProtagonistReady();

            if (villaSceneManager != null)
            {
                var room = GameSession.ResolveStartRoom(RoomType.Bedroom);
                villaSceneManager.SwitchRoom(room);
            }

            Finished?.Invoke();
            Debug.Log("[OpeningCG] 开场 CG 结束，进入第一天。");
        }

        static void EnsureProtagonistReady()
        {
            var protagonist = GameObject.Find("Protagonist");
            if (protagonist == null)
                return;

            if (!protagonist.activeSelf)
                protagonist.SetActive(true);

            var controller = protagonist.GetComponent<SideViewCharacterController>();
            if (controller != null)
            {
                controller.SetGameplayVisible(true);
                controller.SnapFeetToFloor();
            }
        }

        void SetGameplayVisible(bool visible)
        {
            if (visible)
            {
                EnsureProtagonistReady();
                return;
            }

            var protagonist = GameObject.Find("Protagonist");
            if (protagonist != null)
            {
                protagonist.GetComponent<SideViewCharacterController>()?.SetGameplayVisible(false);
                protagonist.SetActive(false);
            }

            if (villaSceneManager == null)
                return;

            foreach (RoomType roomType in System.Enum.GetValues(typeof(RoomType)))
            {
                var room = villaSceneManager.GetRoom(roomType);
                if (room != null)
                    room.gameObject.SetActive(false);
            }
        }
    }
}
