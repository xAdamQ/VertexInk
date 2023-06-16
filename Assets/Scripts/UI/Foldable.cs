using DG.Tweening;
using UnityEngine;

/// <summary>
/// a UI class that makes the contained elements in the content foldable
/// </summary>
public class Foldable : MonoBehaviour
{
    [SerializeField] private RectTransform content;
    [SerializeField] private bool folded;
    [SerializeField] private Ease ease;
    [SerializeField] private float duration = .4f;

    private float originalHeight;

    private void Start()
    {
        originalHeight = content.sizeDelta.y;

        var originalDuration = duration;
        duration = 0;

        if (folded)
            Fold();
        else
            Unfold();

        duration = originalDuration;
    }

    public void ToggleFold()
    {
        if (folded)
            Unfold();
        else
            Fold();

        folded = !folded;
    }

    private void Fold()
    {
        content.DOSizeDelta(new Vector2(content.sizeDelta.x, 0), duration).SetEase(ease);
    }

    private void Unfold()
    {
        content.DOSizeDelta(new Vector2(content.sizeDelta.x, originalHeight), duration);
    }
}