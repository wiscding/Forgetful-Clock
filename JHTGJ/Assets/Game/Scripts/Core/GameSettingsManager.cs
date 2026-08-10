using UnityEngine;

namespace JHTGJ.Core
{
    public class GameSettingsManager : MonoBehaviour
    {
        public static GameSettingsManager Instance { get; private set; }

        const string MasterVolumeKey = "JHTGJ_MasterVolume";
        const string SfxVolumeKey = "JHTGJ_SfxVolume";
        const string ResolutionIndexKey = "JHTGJ_ResolutionIndex";

        [SerializeField] float masterVolume = 1f;
        [SerializeField] float sfxVolume = 1f;
        [SerializeField] int resolutionIndex = -1;

        public float MasterVolume => masterVolume;
        public float SfxVolume => sfxVolume;
        public int ResolutionIndex => resolutionIndex;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                TransferAudioManagerToPersistedInstance();
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureAudioManager();
            LoadSettings();
            ApplyAll();
        }

        void EnsureAudioManager()
        {
            if (GetComponent<GameAudioManager>() == null)
                gameObject.AddComponent<GameAudioManager>();
        }

        void TransferAudioManagerToPersistedInstance()
        {
            var sourceAudio = GetComponent<GameAudioManager>();
            if (sourceAudio == null || Instance == null)
                return;

            var targetAudio = Instance.GetComponent<GameAudioManager>();
            if (targetAudio == null)
                targetAudio = Instance.gameObject.AddComponent<GameAudioManager>();

            targetAudio.AbsorbFrom(sourceAudio);
        }

        public void SetMasterVolume(float value)
        {
            masterVolume = Mathf.Clamp01(value);
            AudioListener.volume = masterVolume;
            PlayerPrefs.SetFloat(MasterVolumeKey, masterVolume);
        }

        public void SetSfxVolume(float value)
        {
            sfxVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(SfxVolumeKey, sfxVolume);
            GameAudioManager.Instance?.RefreshVolumes();
        }

        public void SetResolutionIndex(int index)
        {
            var resolutions = Screen.resolutions;
            if (index < 0 || index >= resolutions.Length)
                return;

            resolutionIndex = index;
            PlayerPrefs.SetInt(ResolutionIndexKey, resolutionIndex);
            ApplyResolution(index);
        }

        public void ApplyAll()
        {
            AudioListener.volume = masterVolume;
            GameAudioManager.Instance?.RefreshVolumes();
            if (resolutionIndex >= 0 && resolutionIndex < Screen.resolutions.Length)
                ApplyResolution(resolutionIndex);
        }

        public void SaveSettings()
        {
            PlayerPrefs.Save();
        }

        void LoadSettings()
        {
            masterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
            sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, 1f);
            resolutionIndex = PlayerPrefs.GetInt(ResolutionIndexKey, GetDefaultResolutionIndex());
        }

        static int GetDefaultResolutionIndex()
        {
            var resolutions = Screen.resolutions;
            if (resolutions.Length == 0)
                return -1;

            var current = Screen.currentResolution;
            for (var i = 0; i < resolutions.Length; i++)
            {
                var r = resolutions[i];
                if (r.width == current.width && r.height == current.height)
                    return i;
            }

            return resolutions.Length - 1;
        }

        static void ApplyResolution(int index)
        {
            var resolutions = Screen.resolutions;
            if (index < 0 || index >= resolutions.Length)
                return;

            var resolution = resolutions[index];
            Screen.SetResolution(
                resolution.width,
                resolution.height,
                FullScreenMode.Windowed,
                resolution.refreshRateRatio);

            Debug.Log($"[Settings] 分辨率：{resolution.width} x {resolution.height}");
        }
    }
}
