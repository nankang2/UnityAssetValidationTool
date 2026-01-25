using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TextureRules : MonoBehaviour
{
    public static void CheckTextures(List<ValidationResults> results, int textureSize)
    {
        // Find textures
        string[] textureGuids = AssetDatabase.FindAssets("t:Texture");

        foreach (string guid in textureGuids)
        {
            // Extract importer
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer == null)
            {
                continue;
            }

            if (importer.maxTextureSize > textureSize)
            {
                results.Add(new ValidationResults
                {
                    assetPath = path,
                    severity = ValidationSeverity.Warning,
                    message = $"Texture max size is {importer.maxTextureSize}px (recommended ≤ {textureSize})",
                    selected = false,
                    autoFixAction = () =>
                    {
                        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
                        if (imp == null) return;

                        imp.maxTextureSize = textureSize;
                        imp.SaveAndReimport();
                    }
                });
            }
        }
    }
}
