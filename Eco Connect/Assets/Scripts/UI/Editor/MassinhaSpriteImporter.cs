using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Configura automaticamente os sprites do tema Massinha:
// tipo Sprite, sem compressão e com as bordas 9-slice certas.
public class MassinhaSpriteImporter : AssetPostprocessor
{
    public const string SpriteFolder = "Assets/UI/Massinha/Sprites";

    // Bordas 9-slice em pixels: esquerda, baixo, direita, cima
    private static readonly Dictionary<string, Vector4> Borders = new Dictionary<string, Vector4>
    {
        { "btn_cream",         new Vector4(44, 57, 44, 44) },
        { "btn_cream_pressed", new Vector4(44, 57, 44, 44) },
        { "btn_green",         new Vector4(44, 57, 44, 44) },
        { "btn_green_pressed", new Vector4(44, 57, 44, 44) },
        { "btn_sun",           new Vector4(44, 57, 44, 44) },
        { "btn_sun_pressed",   new Vector4(44, 57, 44, 44) },
        { "btn_tool_selected", new Vector4(44, 57, 44, 44) },
        { "card",              new Vector4(60, 78, 60, 60) },
        { "ribbon",            new Vector4(34, 45, 34, 34) },
        { "pill_white",        new Vector4(34, 34, 34, 34) },
        { "pill_sun",          new Vector4(34, 34, 34, 34) },
        { "slider_track",      new Vector4(24, 24, 24, 24) },
        { "slider_fill",       new Vector4(24, 24, 24, 24) },
        { "stamp_frame",       new Vector4(24, 24, 24, 24) },
    };

    private void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith(SpriteFolder))
            return;

        TextureImporter importer = (TextureImporter)assetImporter;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.spritePixelsPerUnit = 100;

        string name = System.IO.Path.GetFileNameWithoutExtension(assetPath);

        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        settings.spriteBorder = Borders.TryGetValue(name, out Vector4 border) ? border : Vector4.zero;
        importer.SetTextureSettings(settings);
    }
}
