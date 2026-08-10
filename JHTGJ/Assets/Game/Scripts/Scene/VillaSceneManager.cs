using System;
using System.Collections.Generic;
using JHTGJ.Character;
using JHTGJ.Core;
using JHTGJ.Story;
using JHTGJ.UI;
using UnityEngine;

namespace JHTGJ.Scene
{
    public class VillaSceneManager : MonoBehaviour
    {
        [Serializable]
        public class RoomEntry
        {
            public RoomType type;
            public GameObject root;
            public Room room;
        }

        [SerializeField] List<RoomEntry> rooms = new List<RoomEntry>();
        [SerializeField] SideViewCharacterController protagonist;
        [SerializeField] RoomType startRoom = RoomType.FrontHall;
        [SerializeField] bool enableDebugShortcuts =
#if UNITY_EDITOR
            true;
#else
            false;
#endif

        RoomType currentType;
        readonly Dictionary<RoomType, Sprite> defaultBackgroundSprites = new Dictionary<RoomType, Sprite>();
        bool defaultBackgroundsCached;
        Vector2Int lastScreenSize;

        void Awake()
        {
            EnsureDefaultBackgroundsCached();
            EnsurePauseMenu();
        }

        void EnsurePauseMenu()
        {
            var pauseMenu = FindObjectOfType<PauseMenuUI>(true);
            if (pauseMenu != null)
            {
                if (!pauseMenu.gameObject.activeSelf)
                    pauseMenu.gameObject.SetActive(true);

                if (PauseMenuLegacyTextUtility.NeedsLegacyConversion(pauseMenu.gameObject))
                    PauseMenuLegacyTextUtility.ConvertCanvas(pauseMenu.gameObject);
                else
                    PauseMenuLegacyTextUtility.FixCanvasScale(pauseMenu.transform);

                return;
            }

            PauseMenuBuilder.Build(this);
        }

        void Start()
        {
            lastScreenSize = new Vector2Int(Screen.width, Screen.height);
            RefitAllRoomBackgrounds(Camera.main);

            if (GameSession.ShouldWaitForOpeningCg())
                return;

            SwitchRoom(GameSession.ResolveStartRoom(startRoom));
        }

        void Update()
        {
            var screenSize = new Vector2Int(Screen.width, Screen.height);
            if (screenSize != lastScreenSize)
            {
                lastScreenSize = screenSize;
                RefitAllRoomBackgrounds(Camera.main);
            }

            if (!DebugShortcutsUtility.IsActive(enableDebugShortcuts))
                return;

            TryDebugSwitch(KeyCode.F1, RoomType.FrontHall);
            TryDebugSwitch(KeyCode.F2, RoomType.LivingRoom);
            TryDebugSwitch(KeyCode.F3, RoomType.DiningRoom);
            TryDebugSwitch(KeyCode.F4, RoomType.Kitchen);
            TryDebugSwitch(KeyCode.F5, RoomType.Hallway);
            TryDebugSwitch(KeyCode.F6, RoomType.Bedroom);
            TryDebugSwitch(KeyCode.F7, RoomType.Bathroom);
            TryDebugSwitch(KeyCode.F8, RoomType.Storage);
            TryDebugSwitch(KeyCode.F9, RoomType.Basement);
            TryDebugSwitch(KeyCode.F10, RoomType.Rooftop);
            TryDebugSwitch(KeyCode.F11, RoomType.BackGarden);
        }

        void TryDebugSwitch(KeyCode key, RoomType type)
        {
            if (Input.GetKeyDown(key))
                SwitchRoom(type);
        }

        public void SwitchRoom(RoomType type, SpawnSide spawnSide = SpawnSide.Left)
        {
            var entry = rooms.Find(r => r.type == type);
            if (entry == null || entry.room == null)
            {
                Debug.LogWarning($"Room not found: {type}");
                return;
            }

            foreach (var roomEntry in rooms)
            {
                if (roomEntry.root != null)
                    roomEntry.root.SetActive(roomEntry.type == type);
            }

            currentType = type;

            if (protagonist != null)
            {
                protagonist.gameObject.SetActive(true);
                protagonist.SetGameplayVisible(true);
                protagonist.SetRoom(entry.room, spawnSide);
            }

            RefitRoomBackground(entry, Camera.main);

            var storyManager = FindObjectOfType<StoryEventTreeManager>();
            if (storyManager != null)
                storyManager.RefreshInteractMarkers();
        }

        public RoomType CurrentRoomType => currentType;

        public Room GetRoom(RoomType type)
        {
            var entry = rooms.Find(r => r.type == type);
            return entry?.room;
        }

        public bool TrySetRoomBackground(RoomType type, Sprite sprite)
        {
            if (sprite == null)
                return false;

            var entry = rooms.Find(r => r.type == type);
            if (entry?.root == null)
                return false;

            var background = entry.root.transform.Find("Background");
            if (background == null)
                return false;

            if (!background.TryGetComponent<SpriteRenderer>(out var renderer))
                return false;

            renderer.sprite = sprite;
            RefitRoomBackground(entry, Camera.main);
            return true;
        }

        public void RefitAllRoomBackgrounds(Camera camera = null)
        {
            if (camera == null)
                camera = Camera.main;

            if (camera == null)
                return;

            foreach (var entry in rooms)
                RefitRoomBackground(entry, camera);
        }

        static void RefitRoomBackground(RoomEntry entry, Camera camera)
        {
            if (entry?.root == null)
                return;

            var background = entry.root.transform.Find("Background");
            if (background == null ||
                !background.TryGetComponent<SpriteRenderer>(out var renderer) ||
                renderer.sprite == null)
                return;

            RoomBackgroundFitUtility.FitToCamera(camera, renderer);
        }

        public void ApplyRoomBackgroundSet(bool useNightBackgrounds, NightRoomBackgroundLibrary nightLibrary = null)
        {
            EnsureDefaultBackgroundsCached();

            foreach (var entry in rooms)
            {
                if (entry.type == RoomType.Storage)
                    continue;

                Sprite sprite = null;
                if (useNightBackgrounds && nightLibrary != null)
                    sprite = nightLibrary.GetBackground(entry.type);

                if (sprite == null && !defaultBackgroundSprites.TryGetValue(entry.type, out sprite))
                    continue;

                if (sprite != null)
                    TrySetRoomBackground(entry.type, sprite);
            }
        }

        void EnsureDefaultBackgroundsCached()
        {
            if (defaultBackgroundsCached)
                return;

            defaultBackgroundsCached = true;
            defaultBackgroundSprites.Clear();

            foreach (var entry in rooms)
            {
                if (entry.root == null)
                    continue;

                var background = entry.root.transform.Find("Background");
                if (background == null ||
                    !background.TryGetComponent<SpriteRenderer>(out var renderer) ||
                    renderer.sprite == null)
                    continue;

                defaultBackgroundSprites[entry.type] = renderer.sprite;
            }
        }
    }
}
