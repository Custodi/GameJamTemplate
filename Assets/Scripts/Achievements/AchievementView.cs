using DG.Tweening;
using UnityEngine;

public class AchievementView : MonoBehaviour
{
    [SerializeField] private float showDuration = 2.5f;
    [SerializeField] private float moveDuration = 0.5f;

    private RectTransform rectTransform;
    private Vector2 hiddenPosition;
    private Vector2 visiblePosition;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(Vector2 hiddenPos, Vector2 visiblePos)
    {
        hiddenPosition = hiddenPos;
        visiblePosition = visiblePos;
        rectTransform.anchoredPosition = hiddenPosition;
    }

    public void Play()
    {
        Sequence sequence = DOTween.Sequence();

        sequence.Append(rectTransform.DOAnchorPos(visiblePosition, moveDuration)
            .SetEase(Ease.OutCubic));

        sequence.AppendInterval(showDuration);

        sequence.Append(rectTransform.DOAnchorPos(hiddenPosition, moveDuration)
            .SetEase(Ease.InCubic));

        sequence.OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }
}
