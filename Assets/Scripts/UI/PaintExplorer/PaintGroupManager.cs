using System;
using System.IO;
using UnityEngine;

/// <summary>
/// this controls the top level UI for the paint explorer, it list the paint groups, and saves current annotations
/// </summary>
public class PaintGroupManager : MonoModule<PaintGroupManager>
{
    private string groupsPath => Application.persistentDataPath + "/paints";

    [SerializeField] private GameObject groupPrefab, view;
    [SerializeField] private Transform groupContainer;

    private void Start()
    {
        MakeAllGroups();
    }

    public void Hide()
    {
        view.SetActive(false);
    }

    public void Show()
    {
        view.SetActive(true);
    }


    private void EmptyContainer()
    {
        foreach (Transform child in groupContainer)
            Destroy(child.gameObject);
    }

    private void MakeAllGroups()
    {
        EmptyContainer();

        foreach (var directory in Directory.GetDirectories(groupsPath))
            Instantiate(groupPrefab, groupContainer)
                .GetComponent<PaintGroupView>()
                .Init(directory);
    }

    public void SaveActivePaints()
    {
        var time = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");

        if (!InputManager.I.ActivePaintableContainer.activeSelf)
            return;

        foreach (var paintable in InputManager.I.ActivePaintableContainer.GetComponents<Paintable>())
        {
            var path = groupsPath + "/" + paintable.mesh.GetHashCode() + '~' + time;

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);

                Instantiate(groupPrefab, groupContainer)
                    .GetComponent<PaintGroupView>()
                    .Init(path);
            }

            var texture = paintable.paintTexture.ToTexture();
            TexUtils.SaveTexture(texture, path, paintable.submeshIndex.ToString());

            Debug.Log(paintable.name + " -- " + paintable.name);
        }

        Application.ExternalEval("_JS_FileSystem_Sync();");
    }
}