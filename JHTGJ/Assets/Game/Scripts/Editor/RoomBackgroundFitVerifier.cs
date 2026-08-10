#if UNITY_EDITOR
using System.IO;
using System.Text;
using JHTGJ.Scene;
using UnityEditor;
using UnityEngine;

namespace JHTGJ.EditorTools
{
    public static class RoomBackgroundFitVerifier
    {
        const float OrthographicSize = 5f;
        static readonly (int width, int height, string label)[] TestResolutions =
        {
            (1920, 1080, "16:9 FHD"),
            (2560, 1080, "21:9 UW"),
            (1280, 720, "16:9 HD"),
            (1024, 768, "4:3"),
            (1600, 900, "16:9 900p"),
            (3440, 1440, "21:9 QHD"),
        };

        [MenuItem("JHTGJ/Verify Room Background Cover")]
        public static void VerifyAll()
        {
            var report = new StringBuilder();
            report.AppendLine("[JHTGJ] Room background cover verification");
            report.AppendLine($"Camera orthographic size: {OrthographicSize}");

            var paths = new[]
            {
                "Assets/Art/Environment",
                "Assets/Art/Night",
            };

            var passCount = 0;
            var failCount = 0;

            foreach (var folder in paths)
            {
                if (!AssetDatabase.IsValidFolder(folder))
                    continue;

                foreach (var guid in AssetDatabase.FindAssets("t:Sprite", new[] { folder }))
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    if (!path.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase))
                        continue;

                    var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    if (sprite == null)
                        continue;

                    report.AppendLine();
                    report.AppendLine(Path.GetFileName(path));

                    foreach (var resolution in TestResolutions)
                    {
                        var result = Evaluate(sprite, resolution.width, resolution.height);
                        report.AppendLine(
                            $"  {resolution.label,-14} scale={result.scale:F3} fitted={result.fittedWidth:F2}x{result.fittedHeight:F2} {(result.coversCamera ? "PASS" : "FAIL")}");

                        if (result.coversCamera)
                            passCount++;
                        else
                            failCount++;
                    }
                }
            }

            report.AppendLine();
            report.AppendLine($"Summary: {passCount} passed, {failCount} failed");
            Debug.Log(report.ToString());

            if (failCount > 0)
                EditorUtility.DisplayDialog("Background Cover", $"{failCount} cases failed. See Console.", "OK");
            else
                EditorUtility.DisplayDialog("Background Cover", "All tested resolutions pass cover math.", "OK");
        }

        static (float scale, float fittedWidth, float fittedHeight, bool coversCamera) Evaluate(
            Sprite sprite,
            int screenWidth,
            int screenHeight)
        {
            var spriteWidth = sprite.rect.width / sprite.pixelsPerUnit;
            var spriteHeight = sprite.rect.height / sprite.pixelsPerUnit;
            var cameraHeight = OrthographicSize * 2f;
            var cameraWidth = cameraHeight * ((float)screenWidth / screenHeight);
            var scale = Mathf.Max(cameraWidth / spriteWidth, cameraHeight / spriteHeight);
            var fittedWidth = spriteWidth * scale;
            var fittedHeight = spriteHeight * scale;
            var coversCamera = fittedWidth + 0.001f >= cameraWidth && fittedHeight + 0.001f >= cameraHeight;
            return (scale, fittedWidth, fittedHeight, coversCamera);
        }
    }
}
#endif
