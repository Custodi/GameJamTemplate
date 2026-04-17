using System.Collections.Generic;
using UnityEngine;

public enum AchievementScreenPosition
{
    TopLeft,
    TopCenter,
    TopRight,

    MiddleLeft,
    MiddleRight,

    BottomLeft,
    BottomCenter,
    BottomRight
}

public class AchievementManager : MonoBehaviourSingleton<AchievementManager>
{
    [Header("Settings")]
    [SerializeField] private AchievementScreenPosition screenPosition;
    [SerializeField] private Canvas rootCanvas;
    [SerializeField] private AchievementView achievementViewPrefab;

    private HashSet<string> unlockedAchievements = new HashSet<string>();

    // -------------------------
    // Public API
    // -------------------------

    public void AddAchievement(AchievementConfig config)
    {
        if (unlockedAchievements.Contains(config.Id))
            return;

        unlockedAchievements.Add(config.Id);
        ShowAchievement(config);
    }

    public bool HasAchievement(string id)
    {
        return unlockedAchievements.Contains(id);
    }

    public void ResetAllAchievements()
    {
        unlockedAchievements.Clear();
    }

    // -------------------------
    // Internal logic
    // -------------------------

    private void ShowAchievement(AchievementConfig config)
    {
        AchievementView view = Instantiate(achievementViewPrefab, rootCanvas.transform);

        RectTransform canvasRect = rootCanvas.GetComponent<RectTransform>();

        Vector2 visiblePos = GetVisiblePosition(canvasRect);
        Vector2 hiddenPos = GetHiddenPosition(canvasRect, visiblePos);

        view.Initialize(hiddenPos, visiblePos);

        // Here you would bind Title / Description / Icon to UI
        view.Play();
    }

    private Vector2 GetVisiblePosition(RectTransform canvasRect)
    {
        float x = 0f;
        float y = 0f;

        float halfWidth = canvasRect.rect.width / 2f;
        float halfHeight = canvasRect.rect.height / 2f;
        float offset = 50f;

        switch (screenPosition)
        {
            case AchievementScreenPosition.TopLeft:
                x = -halfWidth + offset;
                y = halfHeight - offset;
                break;

            case AchievementScreenPosition.TopCenter:
                x = 0f;
                y = halfHeight - offset;
                break;

            case AchievementScreenPosition.TopRight:
                x = halfWidth - offset;
                y = halfHeight - offset;
                break;

            case AchievementScreenPosition.MiddleLeft:
                x = -halfWidth + offset;
                y = 0f;
                break;

            case AchievementScreenPosition.MiddleRight:
                x = halfWidth - offset;
                y = 0f;
                break;

            case AchievementScreenPosition.BottomLeft:
                x = -halfWidth + offset;
                y = -halfHeight + offset;
                break;

            case AchievementScreenPosition.BottomCenter:
                x = 0f;
                y = -halfHeight + offset;
                break;

            case AchievementScreenPosition.BottomRight:
                x = halfWidth - offset;
                y = -halfHeight + offset;
                break;
        }

        return new Vector2(x, y);
    }

    private Vector2 GetHiddenPosition(RectTransform canvasRect, Vector2 visiblePos)
    {
        Vector2 direction = visiblePos.normalized;
        return visiblePos + direction * 300f;
    }
}
