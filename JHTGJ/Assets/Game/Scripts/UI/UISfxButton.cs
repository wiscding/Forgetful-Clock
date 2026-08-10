using JHTGJ.Core;
using UnityEngine;
using UnityEngine.UI;

namespace JHTGJ.UI
{
    [RequireComponent(typeof(Button))]
    public class UISfxButton : MonoBehaviour
    {
        Button button;

        void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(OnClick);
        }

        void OnDestroy()
        {
            if (button != null)
                button.onClick.RemoveListener(OnClick);
        }

        void OnClick()
        {
            GameAudioManager.Instance?.PlayButtonClick();
        }
    }
}
