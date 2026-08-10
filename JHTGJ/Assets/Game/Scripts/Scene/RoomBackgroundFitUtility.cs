using UnityEngine;

namespace JHTGJ.Scene
{
    public static class RoomBackgroundFitUtility
    {
        public static void FitToCamera(Camera camera, SpriteRenderer backgroundRenderer)
        {
            if (camera == null || backgroundRenderer == null || backgroundRenderer.sprite == null)
                return;

            var sprite = backgroundRenderer.sprite;
            var spriteWidth = sprite.rect.width / sprite.pixelsPerUnit;
            var spriteHeight = sprite.rect.height / sprite.pixelsPerUnit;
            if (spriteWidth <= 0f || spriteHeight <= 0f)
                return;

            var cameraHeight = camera.orthographicSize * 2f;
            var cameraWidth = cameraHeight * camera.aspect;
            var scale = Mathf.Max(cameraWidth / spriteWidth, cameraHeight / spriteHeight);
            backgroundRenderer.transform.localScale = new Vector3(scale, scale, 1f);
        }
    }
}
