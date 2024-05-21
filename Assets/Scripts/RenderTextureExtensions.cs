using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    ///     Extension methods for <see cref="RenderTexture" />.
    /// </summary>
    public static class RenderTextureExtensions
    {
        /// <summary>
        ///     Converts a <see cref="RenderTexture" /> to a <see cref="Texture2D" />.
        ///     Code nicked from https://stackoverflow.com/questions/44264468/convert-rendertexture-to-texture2d.
        /// </summary>
        /// <param name="renderTexture">Render texture to convert.</param>
        /// <param name="texture">Target texture.</param>
        public static void ToTexture2D(this RenderTexture renderTexture, Texture2D texture)
        {
            RenderTexture oldRt = RenderTexture.active;
            RenderTexture.active = renderTexture;

            texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture.Apply();

            RenderTexture.active = oldRt;
        }
    }
}