#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityToolbarExtender; // Не забудь импортировать пакет unity-toolbar-extender

public class ItchIoUploaderWindow : EditorWindow
{
    // Поля для ввода
    private string field1 = "";
    private string field2 = "";
    private string field3 = "";
    private string field4 = "";
    private string field5 = "";

    // Добавляем кнопку справа в Toolbar
    [InitializeOnLoadMethod]
    static void AddToolbarButton()
    {
        ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUI);
    }

    static void OnToolbarGUI()
    {
        GUILayout.FlexibleSpace(); // Сдвигаем кнопку максимально вправо
        if (GUILayout.Button("Upload on Itch.Io", GUILayout.Width(130), GUILayout.Height(22)))
        {
            ItchIoUploaderWindow.ShowWindow();
        }
    }

    [MenuItem("Tools/Itch.Io Uploader")]
    public static void ShowWindow()
    {
        ItchIoUploaderWindow window = GetWindow<ItchIoUploaderWindow>("Itch.Io Upload");
        window.minSize = new Vector2(400, 300);
    }

    private void OnGUI()
    {
        GUILayout.Label("Upload project to Itch.Io", EditorStyles.boldLabel);
        GUILayout.Label("Fill out the settings below before uploading your build.", EditorStyles.wordWrappedLabel);
        GUILayout.Space(10);

        EditorGUILayout.BeginVertical("box");
        DrawField("Game Title:", ref field1);
        DrawField("Itch.io Page URL:", ref field2);
        DrawField("Version:", ref field3);
        DrawField("Build Path:", ref field4);
        DrawField("API Key:", ref field5);
        EditorGUILayout.EndVertical();

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Upload", GUILayout.Height(35)))
        {
            Debug.Log("Uploading to Itch.io...");
            // Здесь позже можно добавить реальную интеграцию с Butler API
        }
    }

    private void DrawField(string label, ref string field)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label, GUILayout.Width(150));
        field = EditorGUILayout.TextField(field);
        EditorGUILayout.EndHorizontal();
    }
}
#endif
