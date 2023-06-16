using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

/// <summary>
/// each paint groups holds the location for the paint textures for each submesh of a single mesh
/// </summary>
public class PaintGroupView : MonoBehaviour
{
    [SerializeField] private TMP_Text title;
    private string path;

    public void Init(string path)
    {
        this.path = path;

        title.text = Path.GetFileName(path);
    }

    public void LoadAllPaintViews()
    {
        PaintViewManager.I.LoadAllPaintViews(path);
        PaintGroupManager.I.Hide();
    }

    public void ShowPaintViews()
    {
        PaintViewManager.I.GetAllPaintViews(path).ToList();
        PaintGroupManager.I.Hide();
    }

    public void Delete()
    {
        if (Directory.Exists(path))
            Directory.Delete(path, true);
        else
            Debug.Log($"couldn't find the directory {path} to delete!");

        Destroy(gameObject);
    }
}