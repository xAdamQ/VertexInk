using UnityEngine;

/// <summary>
/// add any amount of paintable components based on the submesh count
/// </summary>
public class AutoPaintable : MonoBehaviour
{
    public string MainTextureName;
    private void Start()
    {
        PaintManager.I.MakePaintable(gameObject, MainTextureName);
    }
}