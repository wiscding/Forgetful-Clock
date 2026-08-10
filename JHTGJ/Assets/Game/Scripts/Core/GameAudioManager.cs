using UnityEngine;

namespace JHTGJ.Core
{
    public class GameAudioManager : MonoBehaviour
    {
        public static GameAudioManager Instance { get; private set; }

        const string LibraryResourcesPath = "AudioLibrary";

        [SerializeField] AudioLibrary library;
        [SerializeField] AudioSource bgmSource;
        [SerializeField] AudioSource sfxSource;

        BgmTrack? currentBgmTrack;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            EnsureLibrary();
            EnsureSources();
            RefreshVolumes();
        }

        void Start()
        {
            EnsureLibrary();
            TryPlayMenuBgmIfNeeded();
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void SetLibrary(AudioLibrary audioLibrary)
        {
            library = audioLibrary;
        }

        public void AbsorbFrom(GameAudioManager other)
        {
            if (other == null || other == this)
                return;

            if (library == null && other.library != null)
                library = other.library;
        }

        public void PlayBgm(BgmTrack track)
        {
            EnsureLibrary();
            if (library == null)
                return;

            if (currentBgmTrack == track && bgmSource != null && bgmSource.isPlaying)
                return;

            var clip = library.GetBgm(track);
            if (clip == null || bgmSource == null)
                return;

            currentBgmTrack = track;
            bgmSource.clip = clip;
            bgmSource.loop = true;
            bgmSource.Play();
        }

        public void StopBgm()
        {
            currentBgmTrack = null;
            if (bgmSource != null)
                bgmSource.Stop();
        }

        public void PlayButtonClick()
        {
            EnsureLibrary();
            if (library == null || library.ButtonClickSfx == null || sfxSource == null)
                return;

            sfxSource.PlayOneShot(library.ButtonClickSfx);
        }

        public void RefreshVolumes()
        {
            var settings = GameSettingsManager.Instance;
            var sfxVolume = settings != null ? settings.SfxVolume : 1f;

            if (sfxSource != null)
                sfxSource.volume = sfxVolume;
        }

        public void RefreshStoryBgm(int currentDay, bool storyEnded, bool endingScrollActive, bool conflictPhase = false)
        {
            if (endingScrollActive)
            {
                PlayBgm(BgmTrack.Ending);
                return;
            }

            if (storyEnded)
                return;

            if (currentDay >= 5)
            {
                PlayBgm(BgmTrack.LastDay);
                return;
            }

            if (conflictPhase)
            {
                PlayBgm(BgmTrack.Conflict);
                return;
            }

            PlayBgm(BgmTrack.Normal);
        }

        public void PlayConflictBgm()
        {
            PlayBgm(BgmTrack.Conflict);
        }

        void EnsureLibrary()
        {
            if (library != null)
                return;

            library = Resources.Load<AudioLibrary>(LibraryResourcesPath);
        }

        void TryPlayMenuBgmIfNeeded()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene.name != SceneLoader.MainMenuSceneName)
                return;

            PlayBgm(BgmTrack.Menu);
        }

        void EnsureSources()
        {
            if (bgmSource == null)
            {
                bgmSource = gameObject.AddComponent<AudioSource>();
                bgmSource.loop = true;
                bgmSource.playOnAwake = false;
            }

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
            }
        }
    }
}
