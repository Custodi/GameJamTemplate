using System;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Debug = UnityEngine.Debug;

public class UnityBuilder : MonoBehaviour
{
   /* //Paths to builds
    private static string projectName => Application.productName;
    private static string buildsRootPath => Path.Combine(Directory.GetCurrentDirectory(), "Builds");
    private static string buildWebPath => Path.Combine(buildsRootPath, "WebBuild");
    private static string buildWebZipPath => Path.Combine(buildsRootPath, projectName + "_WebGL.zip");
    //For Butler strings
    private static string itchUsername => "ink-fox"; // Username on itch.io
    private static string itchGameName => "gamejamtemplate"; // URL-slug of game
    private static string itchChannel => "web"; //channel to delivery

    [MenuItem("Build/3. GetCommand")]
    public static void GetCommand()
    {
        Debug.Log($"push \"{buildWebZipPath}\" {itchUsername}/{itchGameName}:{itchChannel}");
    }
    [MenuItem("Build/0. Build AND Push")]
    public static async UniTask BuildAndPush()
    {
        await BuildWebGLAndZip();
        PushToItch();
    }
    [MenuItem("Build/1. WebGL Build and Zip")]
    public static async UniTask BuildWebGLAndZip()
    {
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        await UniTask.WaitUntil(() => !EditorApplication.isCompiling && !EditorApplication.isUpdating);

        if (!Directory.Exists(buildsRootPath))
        {
            Directory.CreateDirectory(buildsRootPath);
        }

        if (Directory.Exists(buildWebPath))
        {
            Directory.Delete(buildWebPath, true);
            Debug.Log("Удалена предыдущая папка сборки: " + buildWebPath);
        }
        // Даем время файловой системе обработать удаление
        System.Threading.Thread.Sleep(100);

        if (File.Exists(buildWebZipPath))
        {
            File.Delete(buildWebZipPath);
            Debug.Log("Удален предыдущий ZIP-архив: " + buildWebZipPath);
        }

        // 3. Особые конфигурации билда
        SetWebGLBuildSettings();

        // 4. Сборка WebGL
        Debug.Log("Начинаем сборку WebGL...");
        BuildPlayerOptions buildOptions = new BuildPlayerOptions();
        buildOptions.scenes = GetScenePaths();
        buildOptions.locationPathName = buildWebPath;
        buildOptions.target = BuildTarget.WebGL;
        buildOptions.options = BuildOptions.None;

        BuildPipeline.BuildPlayer(buildOptions);
        Debug.Log("Сборка WebGL завершена: " + buildWebPath);

        // 5 Архивирование
        Debug.Log("Создаем ZIP-архив...");
        ZipFolder(buildWebPath, buildWebZipPath);
        Debug.Log("ZIP-архив создан: " + buildWebZipPath);
    }

    private static void SetWebGLBuildSettings()
    {
        // Сохраняем текущие настройки
        var previousTemplate = PlayerSettings.WebGL.template;
        var previousSplashScreen = PlayerSettings.SplashScreen.show;

        try
        {
            // Явно устанавливаем ВСЕ нужные настройки
            //PlayerSettings.WebGL.template = "PROJECT:MyCustomTemplate"; // Замените на ваш шаблон
            // PlayerSettings.SplashScreen.show = false;
            UnityEngine.Debug.Log("Настройки WebGL применены");
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogWarning($"Не удалось применить некоторые настройки: {e.Message}");
        }
    }
    [MenuItem("Build/2. Push Zip on Itch.io")]
    public static void PushToItch()
    {
        try
        {
            if (!File.Exists(buildWebZipPath))
            {
                Debug.LogError($"ZIP файл не найден: {buildWebZipPath}");
                return;
            }
            // Формируем команду butler
            string butlerCommand = $"push \"{buildWebZipPath}\" {itchUsername}/{itchGameName}:{itchChannel}";

            Debug.Log($"Запускаем butler: {butlerCommand}");

            // Запускаем butler
            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.FileName = "butler";
            processInfo.Arguments = butlerCommand;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardOutput = true;
            processInfo.RedirectStandardError = true;
            processInfo.CreateNoWindow = true;

            using (Process process = new Process())
            {
                process.StartInfo = processInfo;
                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        Debug.Log($"Butler: {e.Data}");
                };
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        Debug.LogError($"Butler error: {e.Data}");
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    Debug.Log("Успешно загружено на itch.io!");
                }
                else
                {
                    Debug.LogError($"Ошибка загрузки. Код: {process.ExitCode}");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Ошибка при загрузке на itch.io: {e.Message}");
        }
    }
    private static string[] GetScenePaths()
    {
        string[] scenes = new string[EditorBuildSettings.scenes.Length];
        for (int i = 0; i < scenes.Length; i++)
        {
            scenes[i] = EditorBuildSettings.scenes[i].path;
        }
        return scenes;
    }

    private static void ZipFolder(string folderPath, string zipPath)
    {
        try
        {
            // Используем System.IO.Compression для создания ZIP
            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }

            System.IO.Compression.ZipFile.CreateFromDirectory(folderPath, zipPath);
            UnityEngine.Debug.Log("ZIP создан успешно: " + zipPath);
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError("Ошибка при создании ZIP: " + e.Message);

            // Альтернативный способ через 7-Zip (если установлен)
            //TryZipWithWinRar(folderPath, zipPath);
        }
    }

    private static void TryZipWithWinRar(string folderPath, string zipPath)
    {
        try
        {
            // Путь к WinRAR — стандартный по умолчанию, можно подправить при необходимости
            string winRarPath = @"D:\WinRAR\WinRAR.exe";

            if (!File.Exists(winRarPath))
            {
                Debug.LogError("WinRAR не найден. Установите WinRAR или проверьте путь к WinRAR.exe.");
                return;
            }

            // Если архив уже существует — удаляем
            if (File.Exists(zipPath))
                File.Delete(zipPath);

            // Команда для WinRAR:
            // a — добавить в архив
            // -afzip — указать формат ZIP (а не RAR)
            // -r — рекурсивно добавить всё содержимое папки
            string arguments = $"a -afzip \"{zipPath}\" \"{folderPath}\\*\" -r";

            ProcessStartInfo processInfo = new ProcessStartInfo
            {
                FileName = winRarPath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (Process process = new Process { StartInfo = processInfo })
            {
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    Debug.Log("ZIP создан через WinRAR: " + zipPath);
                }
                else
                {
                    Debug.LogError($"WinRAR вернул ошибку (код {process.ExitCode}): {error}\n{output}");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Ошибка при использовании WinRAR: " + e.Message);
        }
    }
   */
}