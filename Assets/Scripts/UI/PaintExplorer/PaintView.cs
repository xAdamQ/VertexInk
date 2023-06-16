using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// a paint view contains the data of the paint texture for a specific submesh
/// </summary>
public class PaintView : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text submeshIndexText;

    private Texture2D texture;
    private int submeshIndex;

    public void Init(Texture2D texture, int submeshIndex)
    {
        this.texture = texture;
        this.submeshIndex = submeshIndex;
        submeshIndexText.text = submeshIndex.ToString();

        image.sprite = texture.ToSprite();

        button.onClick.AddListener(Set);
    }

    public void Set()
    {
        if (!InputManager.I.ActivePaintableContainer.activeSelf)
            return;

        foreach (var paintable in InputManager.I.ActivePaintableContainer.GetComponents<Paintable>())
        {
            if (paintable.submeshIndex == submeshIndex)
                paintable.SetPaintTexture(texture);
        }
    }
}