using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class AssetScanner
{
    public static List<ValidationResults> Scan(bool autoFix, bool checkTextures, int textureSize)
    {
        Debug.Log("Scanning project assets...");

        List<ValidationResults> results = new List<ValidationResults>();

        if (checkTextures)
        {
            TextureRules.CheckTextures(results, textureSize);
        }

        return results;


    }
}
