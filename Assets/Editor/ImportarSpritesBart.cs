using UnityEditor;
using UnityEngine;

public sealed class ImportarSpritesBart : AssetPostprocessor
{
    private void OnPostprocessTexture(Texture2D textura)
    {
        if (assetImporter.userData != "Bart magenta")
            return;

        Color32[] pixeles = textura.GetPixels32();
        for (int i = 0; i < pixeles.Length; i++)
        {
            Color32 pixel = pixeles[i];
            if (pixel.r > 225 && pixel.g < 50 && pixel.b > 225)
                pixeles[i] = new Color32(0, 0, 0, 0);
        }

        if (textura.format == TextureFormat.RGB24)
            textura.Reinitialize(textura.width, textura.height, TextureFormat.RGBA32, false);

        textura.SetPixels32(pixeles);
    }
}
