using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PaintManager : MonoModule<PaintManager>
{
    public Material defaultMaterial, uvIslandMaterial;
    public Texture2D whiteTexture;
    public Renderer debugRenderer, debugRenderer2, debugRenderer3, debugRenderer4;
    public GameObject pointCloudPrefab;
    public Texture defaultBaseTexture;
    public GameObject PointCloudControls;

    public int TextureSize;
    [NonSerialized] public Shader paintShader, mixShader, positionShader, uvIslandShader, dissolveShader, alphaBlendShader;

    private int clearTexture;

    [SerializeField] private FlexibleColorPicker colorPicker;
    [SerializeField] private Toggle eraseToggle, paintOnlyPointCloud;

    protected override void Awake()
    {
        base.Awake();

        paintShader = Shader.Find("Painter/TexturePaint");
        mixShader = Shader.Find("Painter/MergeTextures");
        positionShader = Shader.Find("Painter/PointPosition");
        uvIslandShader = Shader.Find("Painter/UvIslands");
        dissolveShader = Shader.Find("Painter/DissolveEdges");
        alphaBlendShader = Shader.Find("Painter/AlphaBlend");

        uvIslandMaterial = new Material(uvIslandShader);

        PointCloudControls.SetActive(false);
    }

    public void UpdateEraseFlag()
    {
        Shader.SetGlobalInt("_Erase", eraseToggle.isOn ? 0 : 1);
    }

    private void Start()
    {
        Shader.SetGlobalColor("_PaintColor", colorPicker.color);

        eraseToggle.onValueChanged.AddListener(v => Shader.SetGlobalInt("_Erase", v ? 0 : 1));
        colorPicker.onColorChange.AddListener(c => { Shader.SetGlobalColor("_PaintColor", c); });
    }

    public Vector3? currentHitPoint;

    public void Paint(Vector2 screenPosition)
    {
        currentHitPoint = null;

        var ray = InputManager.I.Camera.ScreenPointToRay(screenPosition);

        var IsPaintableHit = Physics.Raycast(ray, out var hit) &&
                             hit.collider.GetComponent<Paintable>();

        if (!IsPaintableHit) return;

        currentHitPoint = hit.point;

        // Vector4 hitPoint = IsPaintableHit ? hit.point : Vector3.positiveInfinity;
        // if (Mouse.current.press.isPressed)
        //     hitPositionQueue.Enqueue(hitPoint);
    }

    public void MakePaintable(GameObject go, string mainTextureName = null)
    {
        var mesh = go.GetComponent<MeshFilter>().mesh;

        if (mesh.uv.Length == 0)
            go.GetComponent<MeshFilter>().mesh = MeshUtils.CreateUnwrappedMesh(mesh);


        // var paintable = go.AddComponent<Paintable>();
        // paintable.Init(1);

        for (var m = 0; m < mesh.subMeshCount; m++)
        {
            var paintable = go.AddComponent<Paintable>();
            paintable.Init(m, mainTextureName);
        }
    }

    public Queue<Vector4> hitPositionQueue = new();

    public Vector2 prevScreenClick;
    public float unitLength;
    private void InterpolateScreenClick(Vector2 hitPoint)
    {
        var distance = Vector2.Distance(hitPoint, prevScreenClick);
        var stepCount = distance / unitLength;
        var lerpUnit = 1f / stepCount;
        for (var l = 0f; l <= 1; l += lerpUnit)
            Paint(Vector2.Lerp(prevScreenClick, hitPoint, l));
    }

    private void Update()
    {
        Paint(Pointer.current.position.ReadValue());

        // if (Pointer.current.press.wasPressedThisFrame)
        // {
        //     prevScreenClick = Pointer.current.position.ReadValue();
        //     Paint(prevScreenClick);
        // }
        // else if (Pointer.current.press.isPressed)
        // {
        //     InterpolateScreenClick(Pointer.current.position.ReadValue());
        // }
    }
    public void CancelPointCloud()
    {
        isPointCloud = false;
        PointCloudControls.SetActive(false);
        paintOnlyPointCloud.gameObject.SetActive(true);
    }
    private bool isPointCloud;

    public void TogglePointCloud()
    {
        isPointCloud = !isPointCloud;

        foreach (var paintable in InputManager.I.ActivePaintableContainer.GetComponents<Paintable>())
            paintable.MakePaintPointCloud(paintOnlyPointCloud.isOn, isPointCloud);

        InputManager.I.ActivePaintableContainer.SetActive(!isPointCloud);
        PointCloudControls.SetActive(isPointCloud);
        paintOnlyPointCloud.gameObject.SetActive(!isPointCloud);
    }

    #region tests
    // public Transform tstStart, tstEnd, tstPoint;
    // public float tstWidth;
    // public GameObject tstPaintableObj;
    // [ContextMenu("tst")]
    // private void tstPaintable()
    // {
    //     MakePaintable(tstPaintableObj);
    // }
    //
    // bool IsPointOnSegment(Vector3 a, Vector3 b, Vector3 c, float width)
    // {
    //     var vector = b - a;
    //
    //     var dir = vector.normalized;
    //     var normal = Vector3.Cross(dir, Vector3.Cross(vector, dir));
    //     var mid = Vector3.Lerp(a, b, .5f);
    //
    //     var p = c - mid;
    //
    //     var dist = Mathf.Abs(Vector3.Dot(p, normal));
    //     return dist <= width / 2.0f;
    // }
    //
    // bool IsPointOnSegment2(Vector3 a, Vector3 b, Vector3 c, float width)
    // {
    //     var ac = c - a;
    //     var ab = b - a;
    //     var dot = Vector3.Dot(ac, ab);
    //
    //     var vector = b - a;
    //
    //     var dir = vector.normalized;
    //     var normal = Vector3.Cross(dir, Vector3.Cross(vector, dir));
    //     var mid = Vector3.Lerp(a, b, .5f);
    //
    //     var p = c - mid;
    //
    //     var dist = Mathf.Abs(Vector3.Dot(p, normal));
    //     return dist <= width / 2.0f;
    // }
    //
    // float DistOfLineDefinedBy2PointsAndPoint(Vector2 start, Vector2 end, Vector2 point)
    // {
    //     return Mathf.Abs((end.y - start.y) * point.x - (end.x - start.x) * point.y + end.x * start.y - end.y * start.x) /
    //            Mathf.Sqrt(Mathf.Pow(end.y - start.y, 2) + Mathf.Pow(end.x - start.x, 2));
    // }
    // float GetPerpendicularLineFromPointToSegment(Vector3 start, Vector3 end, Vector3 point)
    // {
    //     var vector = end - start;
    //     var dir = vector.normalized;
    //
    //     var mid = Vector3.Lerp(start, end, .5f);
    //
    //     return Vector3.Dot(point - mid, dir);
    // }
    #endregion
}