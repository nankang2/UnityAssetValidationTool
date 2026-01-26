using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AssetValidatorWindow : EditorWindow
{
    private bool checkTextures = true;
    private bool checkMeshes = true;

    private int textureSize = 2048;
    private readonly int[] textureSizeOptions = { 1024, 2048, 4096 };
    private readonly string[] textureSizeLabels = { "1024", "2048", "4096" };
    private int textureSizeIndex = 1;

    private Vector2 scrollPos;
    private GUIStyle wordWrapLabel;

    List<ValidationResults> results = new List<ValidationResults> ();


    [MenuItem("Tools/Asset Validator")]
    public static void Open()
    {
        GetWindow<AssetValidatorWindow>("Asset Validator");
    }

    void OnGUI()
    {
        if (wordWrapLabel == null)
        {
            wordWrapLabel = new GUIStyle(EditorStyles.label);
            wordWrapLabel.wordWrap = true;
        }

        GUILayout.Label("Asset Validation Tool", EditorStyles.boldLabel);

        GUILayout.Space(5);

        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("Scan Options", EditorStyles.boldLabel);

        checkTextures = GUILayout.Toggle(checkTextures, "Validate Textures");
        //checkMeshes = GUILayout.Toggle(checkMeshes, "Validate Meshes");
        textureSizeIndex = EditorGUILayout.Popup("Max Texture Size", textureSizeIndex, textureSizeLabels);
        textureSize = textureSizeOptions[textureSizeIndex];
        EditorGUILayout.EndVertical();

        GUILayout.Space(10);

        // Scan textures
        if (GUILayout.Button("Scan Project"))
        {
            results = AssetScanner.Scan(checkTextures, textureSize);
        }

        scrollPos = GUILayout.BeginScrollView(scrollPos);
        // Display scanned results
        foreach (var result in results)
        {
            EditorGUILayout.BeginHorizontal("box");

            // Per-row checkbox
            result.selected = EditorGUILayout.Toggle(result.selected, GUILayout.Width(18));

            // Load asset
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(result.assetPath);

            // Thumbnail
            Texture2D preview = null;
            if (asset != null)
            {
                preview = AssetPreview.GetAssetPreview(asset);
                if (preview == null)
                    preview = AssetPreview.GetMiniThumbnail(asset); // usually non-null
            }

            if (GUILayout.Button(preview, GUILayout.Width(50), GUILayout.Height(50)))
            {
                Selection.activeObject = asset;
                EditorGUIUtility.PingObject(asset);
            }

            // Texture Info
            GUILayout.Label(result.severity.ToString(), GUILayout.Width(70));

            EditorGUILayout.BeginVertical("box");
            GUILayout.Label(result.assetPath, wordWrapLabel, GUILayout.MaxWidth(position.width - 140));
            GUILayout.Label(result.message, wordWrapLabel, GUILayout.MaxWidth(position.width - 140));
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();
        GUILayout.Space(8);
        EditorGUILayout.BeginHorizontal();

        //  Quick selection
        if (GUILayout.Button("Select All", GUILayout.Width(80)))
        {
            foreach (var r in results) if (r != null) r.selected = true;
        }
        if (GUILayout.Button("Select None", GUILayout.Width(80)))
        {
            foreach (var r in results) if (r != null) r.selected = false;
        }

        GUILayout.FlexibleSpace();

        // Fix Selected
        using (new EditorGUI.DisabledScope(results == null || results.Count == 0))
        {
            if (GUILayout.Button("Fix Selected", GUILayout.Height(26), GUILayout.Width(110)))
            {
                FixSelected();
            }
        }

        EditorGUILayout.EndHorizontal();

    }

    private void FixSelected()
    {
        // Collect first
        var toFix = new List<ValidationResults>();
        foreach (var r in results)
        {
            if (r == null) continue;
            if (!r.selected) continue;
            if (r.autoFixAction == null) continue;
            toFix.Add(r);
        }

        if (toFix.Count == 0)
            return;

        // Faster batch import
        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (var r in toFix)
                r.autoFixAction.Invoke();
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();
        }

        // Re-scan so warnings disappear
        results = AssetScanner.Scan(checkTextures, textureSize);
    }

}
