using UnityEngine;

namespace JHTGJ.Core
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlaceholderSprite : MonoBehaviour
    {
        [SerializeField] Color color = Color.white;

        void Awake()
        {
            var renderer = GetComponent<SpriteRenderer>();
            if (renderer.sprite != null)
                return;

            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();

            renderer.sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, 1f, 1f),
                new Vector2(0.5f, 0.5f),
                1f);
        }
    }
}
