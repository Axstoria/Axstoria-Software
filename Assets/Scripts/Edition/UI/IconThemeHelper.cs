using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class IconThemeHelper
{
    private static readonly Dictionary<int, Texture2D> _brightnessCache = new();

    public static void ApplyInvertedIcons(VisualElement root, string buttonClass, byte brightness = 180)
    {
        root.Query<Button>(className: buttonClass).ForEach(btn =>
        {
            Texture2D original = btn.resolvedStyle.backgroundImage.texture as Texture2D;
            if (original == null)
                return;

            int key = original.GetHashCode() * 397 ^ brightness;
            if (!_brightnessCache.TryGetValue(key, out Texture2D result))
            {
                result = RemapTexture(original, brightness);
                _brightnessCache[key] = result;
            }

            btn.style.backgroundImage = new StyleBackground(result);
        });
    }

    public static void RestoreOriginalIcons(VisualElement root, string buttonClass)
    {
        root.Query<Button>(className: buttonClass).ForEach(btn =>
        {
            btn.style.backgroundImage = StyleKeyword.Null;
        });
    }

    private static Texture2D RemapTexture(Texture2D source, byte targetBrightness)
    {
        RenderTexture rt = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(source, rt);

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D result = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        result.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);

        Color32[] pixels = result.GetPixels32();
        for (int i = 0; i < pixels.Length; i++)
        {
            if (pixels[i].a == 0)
                continue;
            pixels[i] = new Color32(
                targetBrightness,
                targetBrightness,
                targetBrightness,
                pixels[i].a
            );
        }

        result.SetPixels32(pixels);
        result.Apply();
        return result;
    }
}
