using System.Linq;
using TMPro;
using TriLibCore;
using TriLibCore.General;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// this is responsible for loading the 3d models
/// </summary>
public class ObjectLoader : MonoModule<ObjectLoader>
{
    private AssetLoaderOptions assetLoaderOptions;

    protected override void Awake()
    {
        base.Awake();

        loadButton.onClick.AddListener(Load);

        assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions();

        assetLoaderOptions.ImportColors = true;
        assetLoaderOptions.ImportMaterials = true;
        assetLoaderOptions.ImportMeshes = true;
        assetLoaderOptions.ImportTextures = true;
        assetLoaderOptions.ImportNormals = true;
        assetLoaderOptions.ImportTangents = true;

        assetLoaderOptions.PivotPosition = PivotPosition.Center;

        assetLoaderOptions.AddAssetUnloader = false;
        assetLoaderOptions.ReadAndWriteEnabled = true;
        assetLoaderOptions.UseUnityNativeTextureLoader = true;
        assetLoaderOptions.UseUnityNativeNormalCalculator = true;

        assetLoaderOptions.GCHelperCollectionInterval = 15;
    }

    public GameObject initialModel;
    private void Start()
    {
        // var ModelPath = AssetDatabase.GetAssetPath(ModelAsset);
        // AssetLoader.LoadModelFromFile(ModelPath, OnLoad, OnMaterialsLoad, OnProgress, OnError, null, assetLoaderOptions);

        HandleLoaded(Instantiate(initialModel));
    }

    // public void Load(byte[] binaryData)
    // {
    //     var textStream = new MemoryStream(binaryData);
    //
    //     var parent = new OBJLoader().Load(textStream);
    //
    //     var obj = GetFirstModel(parent.transform);
    //
    //     obj.transform.SetParent(null);
    //
    //     Destroy(parent);
    //
    //     PaintManager.I.MakePaintable(obj);
    //
    //     PlaceObject(obj);
    // }

    public string tstUrl;
    [ContextMenu("tst load")]
    private void tstLoad()
    {
        // Load("https://ricardoreis.net/trilib/demos/sample/TriLibSampleModel.zip");
        Load(tstUrl);
    }


    public void Load(string url)
    {
        Debug.Log("download url is: " + url);
        // var webRequest = AssetDownloader.CreateWebRequest(url);
        var webRequest = new UnityWebRequest(url);

        AssetDownloader.LoadModelFromUri(webRequest, OnLoad, OnMaterialsLoad, OnProgress, OnError, null, assetLoaderOptions);
        // StartCoroutine(GetFileRoutine(@"file:///" + fileLink));
    }

    public void Load()
    {
        var assetLoaderFilePicker = AssetLoaderFilePicker.Create();
        assetLoaderFilePicker.LoadModelFromFilePickerAsync("Select a Model file", OnLoad, OnMaterialsLoad, OnProgress, OnBeginLoad, OnError, null, assetLoaderOptions);
    }

    [SerializeField] private Button loadButton;
    [SerializeField] private TMP_Text loadText;

    /// <summary>
    /// Called when the the Model begins to load.
    /// </summary>
    /// <param name="filesSelected">Indicates if any file has been selected.</param>
    private void OnBeginLoad(bool filesSelected)
    {
        loadButton.interactable = !filesSelected;
        loadText.enabled = filesSelected;
    }

    /// <summary>
    /// Called when any error occurs.
    /// </summary>
    /// <param name="obj">The contextualized error, containing the original exception and the context passed to the method where the error was thrown.</param>
    private void OnError(IContextualizedError obj)
    {
        Debug.LogError($"An error occurred while loading your Model: {obj.GetInnerException()}");
        loadButton.interactable = true;
        loadText.text = "failed to load object";
    }

    /// <summary>
    /// Called when the Model loading progress changes.
    /// </summary>
    /// <param name="assetLoaderContext">The context used to load the Model.</param>
    /// <param name="progress">The loading progress.</param>
    private void OnProgress(AssetLoaderContext assetLoaderContext, float progress)
    {
        loadText.text = $"Progress: {progress:P}";
    }

    /// <summary>
    /// Called when the Model (including Textures and Materials) has been fully loaded.
    /// </summary>
    /// <remarks>The loaded GameObject is available on the assetLoaderContext.RootGameObject field.</remarks>
    /// <param name="assetLoaderContext">The context used to load the Model.</param>
    private void OnMaterialsLoad(AssetLoaderContext assetLoaderContext)
    {
        if (assetLoaderContext.RootGameObject != null)
        {
            Debug.Log("Model fully loaded.");
        }
        else
        {
            Debug.Log("Model could not be loaded.");
        }

        loadButton.interactable = true;
        loadText.enabled = false;

        HandleLoaded(assetLoaderContext.RootGameObject);
    }
    /// <summary>
    /// Called when the Model Meshes and hierarchy are loaded.
    /// </summary>
    /// <remarks>The loaded GameObject is available on the assetLoaderContext.RootGameObject field.</remarks>
    /// <param name="assetLoaderContext">The context used to load the Model.</param>
    private void OnLoad(AssetLoaderContext assetLoaderContext)
    {
        Debug.Log("Model loaded. Loading materials.");
    }

    private void HandleLoaded(GameObject root)
    {
        PaintManager.I.CancelPointCloud();
        Destroy(InputManager.I.ActiveObject);

        var container = new GameObject("active object");

        root.transform.SetParent(container.transform);

        foreach (var meshRenderer in root.GetComponentsInChildren<MeshRenderer>())
            InitGo(meshRenderer.gameObject);

        InputManager.I.ActiveObject = container;
    }

    private void InitGo(GameObject go)
    {
        PaintManager.I.MakePaintable(go);

        PlaceObject(go);

        loadButton.interactable = true;
    }

    private GameObject GetFirstModel(Transform t)
    {
        if (t.GetComponent<MeshRenderer>())
            return t.gameObject;

        return (from Transform child in t select GetFirstModel(child))
            .FirstOrDefault(m => m != null);
    }

    [SerializeField] private Transform placingPoint;
    [SerializeField] private float maxSize;

    public void PlaceObject(GameObject go)
    {
        var renderer = go.GetComponent<Renderer>();
        var bounds = renderer.bounds;

        var size = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);

        if (size == 0)
        {
            Debug.Log("model has no size");
            return;
        }

        var scale = maxSize / size;
        go.transform.localScale = Vector3.one * scale;

        //refresh bounds after scaling
        var center = renderer.bounds.center;
        var spacing = go.transform.position - center;
        go.transform.position = placingPoint.position + spacing;
    }
}