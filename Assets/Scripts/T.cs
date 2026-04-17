#if UNITY_EDITOR
using Cysharp.Threading.Tasks;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class T : MonoBehaviour
{
    // --- Конфигурация ---
    private static bool isCICDMode = false; // <-- переключатель режима
    private static string projectName => Application.productName;
    private static string buildsRootPath => Path.Combine(Directory.GetCurrentDirectory(), "Builds");
    private static string buildWebPath => Path.Combine(buildsRootPath, "WebBuild");
    private static string buildWebZipPath => Path.Combine(buildsRootPath, projectName + "_WebGL.zip");

    // Itch.io
    private static string itchUsername => "ink-fox";
    private static string itchGameName => "gamejamtemplatetest";
    private static string itchChannel => "web";

    // --- Точка входа ---
    [MenuItem("Build/1. WebGL Build and Zip (Auto Mode)")]
    public static async UniTask BuildWebGLAndZipAuto()
    {
        // Если переменная окружения CI=true, включаем CI/CD режим автоматически
        if (Environment.GetEnvironmentVariable("CI") == "true")
        {
            isCICDMode = true;
        }

        if (isCICDMode)
        {
            Debug.Log("⚙️ Режим CI/CD: сборка без UI и диалогов");
            BuildForCICD();
        }
        else
        {
            Debug.Log("🧩 Режим обычной сборки: выполняется в Unity Editor");
            await BuildInteractiveAsync();
        }
    }

    // --- Обычный режим (через UI, без блокировки редактора) ---
    private static async UniTask BuildInteractiveAsync()
    {
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        await UniTask.WaitUntil(() => !EditorApplication.isCompiling && !EditorApplication.isUpdating);

        PrepareBuildFolders();

        SetWebGLBuildSettings();

        Debug.Log("Начинаем сборку WebGL...");
        await UniTask.SwitchToMainThread();

        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = GetScenePaths(),
            locationPathName = buildWebPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(buildOptions);

        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.LogError("❌ Ошибка сборки WebGL!");
            return;
        }

        await Task.Run(() =>
        {
            Debug.Log($"Создаём ZIP... (режим: {(isCICDMode ? "CI/CD" : "Локальный")})");
            var level = isCICDMode ? System.IO.Compression.CompressionLevel.Optimal : System.IO.Compression.CompressionLevel.Fastest;
            CreateZipWithProgress(buildWebPath, buildWebZipPath, level);
        });

        Debug.Log($"✅ Билд и архив готов: {buildWebZipPath}");
    }

    public static void CreateZipWithProgress(string sourceDir, string zipPath, System.IO.Compression.CompressionLevel level)
    {
        var files = Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories);
        using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
        {
            int processed = 0;
            foreach (var file in files)
            {
                string entryName = file.Substring(sourceDir.Length + 1).Replace("\\", "/");
                zip.CreateEntryFromFile(file, entryName, level);

                processed++;
                if (processed % 100 == 0)
                    Debug.Log($"📦 [{processed}/{files.Length}] файлов упаковано...");
            }
        }
    }


    // --- CI/CD режим ---
    private static void BuildForCICD()
    {
        try
        {
            PrepareBuildFolders();
            SetWebGLBuildSettings();

            var buildOptions = new BuildPlayerOptions
            {
                scenes = GetScenePaths(),
                locationPathName = buildWebPath,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(buildOptions);

            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Console.WriteLine("❌ Ошибка сборки WebGL (CI/CD).");
                Environment.Exit(1);
            }

            System.IO.Compression.ZipFile.CreateFromDirectory(buildWebPath, buildWebZipPath);
            Console.WriteLine($"✅ CI/CD билд готов: {buildWebZipPath}");

            // Можно автоматически запушить на Itch.io, если нужно:
            // PushToItch();
        }
        catch (Exception e)
        {
            Console.WriteLine($"❌ Ошибка CI/CD сборки: {e.Message}");
            Environment.Exit(1);
        }
    }

    // --- Вспомогательные методы ---
    private static void PrepareBuildFolders()
    {
        if (!Directory.Exists(buildsRootPath))
            Directory.CreateDirectory(buildsRootPath);

        if (Directory.Exists(buildWebPath))
        {
            Directory.Delete(buildWebPath, true);
            Debug.Log("Удалена предыдущая папка сборки: " + buildWebPath);
        }

        if (File.Exists(buildWebZipPath))
        {
            File.Delete(buildWebZipPath);
            Debug.Log("Удалён предыдущий ZIP: " + buildWebZipPath);
        }
    }

    private static void SetWebGLBuildSettings()
    {
        try
        {
            Debug.Log("Настройки WebGL применены");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Ошибка применения настроек: {e.Message}");
        }
    }

    private static string[] GetScenePaths()
    {
        string[] scenes = new string[EditorBuildSettings.scenes.Length];
        for (int i = 0; i < scenes.Length; i++)
            scenes[i] = EditorBuildSettings.scenes[i].path;
        return scenes;
    }

    [MenuItem("Build/2. Push Zip on Itch.io")]
    public static void PushToItch()
    {
        if (!File.Exists(buildWebZipPath))
        {
            Debug.LogError($"ZIP не найден: {buildWebZipPath}");
            return;
        }

        string cmd = $"push \"{buildWebZipPath}\" {itchUsername}/{itchGameName}:{itchChannel}";
        Debug.Log("Запускаем butler: " + cmd);

        var processInfo = new ProcessStartInfo
        {
            FileName = "butler",
            Arguments = cmd,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using (var process = Process.Start(processInfo))
        {
            process.WaitForExit();
            if (process.ExitCode == 0)
                Debug.Log("✅ Успешно загружено на itch.io!");
            else
                Debug.LogError($"❌ Ошибка загрузки: код {process.ExitCode}");
        }
    }
}
#endif
