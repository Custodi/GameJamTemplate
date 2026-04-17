using UnityEngine;
using UnityEditor;
using System.IO;

public class SpriteWhiteConverter : EditorWindow
{
    [SerializeField] private Texture2D[] sprites; // Array of sprites to convert, set manually or via drag & drop
    private Vector2 scrollPos;
    private bool isProcessing = false;

    [MenuItem("Tools/Convert Sprites to White")]
    public static void ShowWindow()
    {
        GetWindow<SpriteWhiteConverter>("Sprite White Converter");
    }

    private void OnGUI()
    {
        GUILayout.Label("🎨 Convert Sprites to White", EditorStyles.boldLabel);
        GUILayout.Space(8);
        GUILayout.Label("Drag and drop your sprites here (Texture2D):", EditorStyles.wordWrappedLabel);

        GUILayout.Space(10);

        // Display scrollable list
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(180));
        SerializedObject so = new SerializedObject(this);
        SerializedProperty prop = so.FindProperty("sprites");
        EditorGUILayout.PropertyField(prop, true);
        so.ApplyModifiedProperties();
        EditorGUILayout.EndScrollView();

        GUILayout.Space(10);

        EditorGUI.BeginDisabledGroup(isProcessing);

        if (sprites != null && sprites.Length > 0)
        {
            if (GUILayout.Button("🔄 Convert All Sprites", GUILayout.Height(30)))
            {
                ConvertSprites();
            }
        }
        else
        {
            GUILayout.Label("No sprites selected.", EditorStyles.centeredGreyMiniLabel);
        }

        EditorGUI.EndDisabledGroup();

        if (isProcessing)
        {
            GUILayout.Space(5);
            GUILayout.Label("Processing... please wait.", EditorStyles.helpBox);
        }
    }

    private void ConvertSprites()
    {
        if (sprites == null || sprites.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "Please add at least one sprite.", "OK");
            return;
        }

        isProcessing = true;

        try
        {
            for (int i = 0; i < sprites.Length; i++)
            {
                Texture2D tex = sprites[i];
                if (tex == null) continue;

                string assetPath = AssetDatabase.GetAssetPath(tex);
                string fullPath = Path.GetFullPath(assetPath);
                TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

                if (importer == null) continue;

                bool wasReadable = importer.isReadable;
                if (!wasReadable)
                {
                    importer.isReadable = true;
                    importer.SaveAndReimport();
                }

                Texture2D newTex = MakeWhite(tex);

                byte[] bytes;
                string ext = Path.GetExtension(assetPath).ToLower();
                if (ext == ".png")
                    bytes = newTex.EncodeToPNG();
                else if (ext == ".jpg" || ext == ".jpeg")
                    bytes = newTex.EncodeToJPG();
                else
                    continue;

                File.WriteAllBytes(fullPath, bytes);
                AssetDatabase.ImportAsset(assetPath);

                if (!wasReadable)
                {
                    importer.isReadable = false;
                    importer.SaveAndReimport();
                }

                EditorUtility.DisplayProgressBar(
                    "Converting Sprites",
                    $"Processing: {tex.name}",
                    (float)(i + 1) / sprites.Length
                );
            }

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Done!", "All selected sprites have been converted to white.", "OK");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error during conversion: " + e.Message);
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            isProcessing = false;
            AssetDatabase.Refresh();
        }
    }

    private Texture2D MakeWhite(Texture2D tex)
    {
        Texture2D result = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false);
        Color[] pixels = tex.GetPixels();

        for (int i = 0; i < pixels.Length; i++)
        {
            float a = pixels[i].a; // Keep alpha channel
            pixels[i] = new Color(1f, 1f, 1f, a);
        }

        result.SetPixels(pixels);
        result.Apply();
        return result;
    }
}
