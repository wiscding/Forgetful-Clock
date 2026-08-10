using UnityEngine.SceneManagement;

namespace JHTGJ.Core
{
    public static class SceneLoader
    {
        public const string MainMenuSceneName = "MainMenu";
        public const string GameSceneName = "SampleScene";

        public const string MainMenuScenePath = "Assets/Game/Scenes/MainMenu.unity";
        public const string GameScenePath = "Assets/Game/Scenes/SampleScene.unity";

        public static void LoadGameScene()
        {
            SceneManager.LoadScene(GameSceneName);
        }

        public static void LoadMainMenuScene()
        {
            SceneManager.LoadScene(MainMenuSceneName);
        }
    }
}
