using UnityEngine;

namespace JHTGJ.Art
{
    /// <summary>
    /// Side-view whitebox block. Scale defines size; color distinguishes object type.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(SpriteRenderer))]
    public class WhiteboxBlock : MonoBehaviour
    {
        [SerializeField] Color color = new Color(0.92f, 0.92f, 0.92f);
        [SerializeField] Vector2 size = new Vector2(1f, 2f);
        [SerializeField] int sortingOrder;

        static Sprite unitSprite;

        void Awake() => Apply();
        void OnValidate() => Apply();

        public void Apply()
        {
            ApplyWith(color, size, sortingOrder);
        }

        public void ApplyWith(Color blockColor, Vector2 blockSize, int order)
        {
            var renderer = GetComponent<SpriteRenderer>();
            renderer.sprite = GetUnitSprite();
            renderer.color = blockColor;
            renderer.sortingOrder = order;
            transform.localScale = new Vector3(blockSize.x, blockSize.y, 1f);
        }

        static Sprite GetUnitSprite()
        {
            if (unitSprite != null)
                return unitSprite;

            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();

            unitSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, 1f, 1f),
                new Vector2(0.5f, 0.5f),
                1f);

            return unitSprite;
        }
    }
}
