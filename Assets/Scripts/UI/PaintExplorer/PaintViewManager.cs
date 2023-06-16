using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// this is the UI container that shows all the paint textures for the whole mesh
/// </summary>
public class PaintViewManager : MonoModule<PaintViewManager>
{
    [SerializeField] private GameObject textureViewPrefab, view;
    [SerializeField] private Transform textureViewContainer;

    public void Back()
    {
        Hide();
        PaintGroupManager.I.Show();
    }

    private void Hide()
    {
        EmptyContainer();
        view.SetActive(false);
    }

    private void EmptyContainer()
    {
        foreach (Transform child in textureViewContainer)
            Destroy(child.gameObject);
    }

    public void LoadAllPaintViews(string path)
    {
        foreach (var paintView in GetAllPaintViews(path))
            paintView.Set();
    }

    public IEnumerable<PaintView> GetAllPaintViews(string path)
    {
        view.SetActive(true);

        var i = 0;
        foreach (var texture in TexUtils.LoadAllTexturesAt(path))
        {
            var pv = Instantiate(textureViewPrefab, textureViewContainer)
                .GetComponent<PaintView>();

            pv.Init(texture, i);

            yield return pv;

            i++;
        }
    }
}