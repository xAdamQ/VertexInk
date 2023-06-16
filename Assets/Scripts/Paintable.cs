using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

/// <summary>
/// this is the core of this project, a component of this is needed for each paintable submesh
/// it creates the materials, the textures, and issues the paint command buffers to the gpu if the object is
/// selected
/// </summary>
public class Paintable : MonoBehaviour
{
    private Material paintMaterial, dissolveEdgesMaterial, alphaBlendMat;
    private RenderTexture bridgeRenderTarget, mixRenderTarget;
    public RenderTexture paintTexture;
    private RenderTexture uvIslandTarget, fixedIslands;
    private Renderer renderer;
    private Texture originalTexture;
    [NonSerialized] public Mesh mesh;

    private CommandBuffer paintBuffer, previewBuffer;

    private Coroutine PaintCoroutine;
    private int textureSize => PaintManager.I.TextureSize;

    public int submeshIndex;
    private Material renderMaterial
    {
        get => renderer.materials[submeshIndex];
        set => renderer.materials[submeshIndex] = value;
    }

    private string mainTextureName = "_MainTex";

    private bool started;

    public void Init(int submeshIndex, string mainTextureName = null)
    {
        this.submeshIndex = submeshIndex;

        if (!string.IsNullOrEmpty(mainTextureName))
            this.mainTextureName = mainTextureName;

        renderer = GetComponent<MeshRenderer>();
        mesh = GetComponent<MeshFilter>().mesh;

        if (!GetComponent<MeshCollider>())
            gameObject.AddComponent<MeshCollider>();

        CreateRenderTextures();

        ClearTextures(bridgeRenderTarget, paintTexture, uvIslandTarget);

        InitMaterials();

        SetDebugRenderers();

        InitBuffers();

        ExportUvIslands();
        InitEdgeDissolve();

        Activate();

        Graphics.Blit(originalTexture, mixRenderTarget);

        PaintManager.I.UpdateEraseFlag();

        started = true;
    }

    private RenderTexture MakeStandardRT()
    {
        return new RenderTexture(textureSize, textureSize, 0)
        {
            anisoLevel = 0,
            useMipMap = false,
            filterMode = FilterMode.Bilinear,
            // wrapMode = TextureWrapMode.Clamp,
            // antiAliasing = 2,
            // format = RenderTextureFormat.ARGB32,
            // autoGenerateMips = true,
        };
    }

    private void CreateRenderTextures()
    {
        bridgeRenderTarget = MakeStandardRT();
        paintTexture = MakeStandardRT();
        mixRenderTarget = MakeStandardRT();
        uvIslandTarget = MakeStandardRT();
        fixedIslands = MakeStandardRT();
    }

    private void InitMaterials()
    {
        paintMaterial = new Material(PaintManager.I.paintShader);
        dissolveEdgesMaterial = new Material(PaintManager.I.dissolveShader);
        alphaBlendMat = new Material(PaintManager.I.alphaBlendShader);

        paintMaterial.SetTexture("_PaintTex", paintTexture);

        if (!renderMaterial)
            renderMaterial = new Material(PaintManager.I.defaultMaterial);

        // if (!renderMaterial.HasTexture(mainTextureName))
        //     renderMaterial.SetTexture(mainTextureName, Instantiate(PaintManager.I.whiteTexture));
        // originalTexture = renderMaterial.GetTexture(mainTextureName);
        // renderMaterial.SetTexture(mainTextureName, mixRenderTarget);

        if (!renderMaterial.mainTexture)
            renderMaterial.mainTexture = Instantiate(PaintManager.I.whiteTexture);

        originalTexture = renderMaterial.mainTexture;
        renderMaterial.mainTexture = mixRenderTarget;
    }

    private void InitBuffers()
    {
        paintBuffer = new CommandBuffer();
        paintBuffer.SetRenderTarget(bridgeRenderTarget);

        // paintBuffer.SetRenderTarget(new RenderTargetIdentifier[] { bridgeRenderTarget.colorBuffer, mixRenderTarget.colorBuffer },
        //     bridgeRenderTarget.depthBuffer);
        //this function requires a depth buffer to write/read the depth, but since we have a single
        //layer, this doesn't even matter
        //so if the rendering target was the screen for example, we have multiple things to render
        //and it would've made since to have a depth buffer to know which pixel is in front of which

        //this step will simulates creating a new game object to run the shader on
        paintBuffer.DrawMesh(mesh, Matrix4x4.identity, paintMaterial, submeshIndex);

        //copy the new state of the render target to persist in the paint texture to use it in the next frame
        paintBuffer.Blit(bridgeRenderTarget, fixedIslands, dissolveEdgesMaterial);
        paintBuffer.Blit(fixedIslands, paintTexture);
        paintBuffer.Blit(paintTexture, mixRenderTarget, alphaBlendMat);

        //the preview buffer differs from the paint buffer in that it doesn't copy the new state paint texture
        //so the changes are not preserved
        previewBuffer = new CommandBuffer();
        previewBuffer.SetRenderTarget(bridgeRenderTarget);
        previewBuffer.DrawMesh(mesh, Matrix4x4.identity, paintMaterial, submeshIndex);
        previewBuffer.Blit(bridgeRenderTarget, fixedIslands, dissolveEdgesMaterial);
        previewBuffer.Blit(fixedIslands, mixRenderTarget, alphaBlendMat);
    }

    private void OnColorChanged()
    {
        // var mixMaterial2 = new Material(PaintManager.I.mixShader);
        //
        // mixMaterial2.SetTexture("_MainTex", dryPaintTexture);
        // mixMaterial2.SetTexture("_OverlayTex", paintTexture);
        //
        // ClearTextures(bridgeRenderTarget);
        // mixMaterial2.SetTexture("_OverlayTex2", bridgeRenderTarget);
        //
        // var cb = new CommandBuffer();
        //
        // cb.SetRenderTarget(mixRenderTarget);
        // cb.DrawMesh(mesh, Matrix4x4.identity, mixMaterial2);
        //
        // ClearTextures(paintTexture);
        //
        // Graphics.ExecuteCommandBuffer(cb);
    }

    private void Activate()
    {
        if (PaintCoroutine != null)
            StopCoroutine(PaintCoroutine);

        PaintCoroutine = StartCoroutine(PaintCommand());
    }

    private readonly WaitForEndOfFrame frameWait = new();
    private IEnumerator PaintCommand()
    {
        while (true)
        {
            //I used to queue the hit points and dequeue them here, because at some points I felt like
            //the hit points were not processed fast enough, but it turned out not be true
            //but the mouse movement between the frames were too fast, so we need a custom solution for 
            //interpolated the inner values
            //there are 2 solutions for this the 
            // var hitPoint = PaintManager.I.hitPositionQueue.Count > 0 ? PaintManager.I.hitPositionQueue.Dequeue() : (Vector4)Vector3.positiveInfinity;

            if (PaintManager.I.currentHitPoint != null)
            {
                paintMaterial.SetVector("_HitPoint", PaintManager.I.currentHitPoint.Value);
                Graphics.ExecuteCommandBuffer(Pointer.current.press.isPressed ? paintBuffer : previewBuffer);
            }

            yield return frameWait;
        }
    }

    private void OnDisable()
    {
        if (PaintCoroutine != null)
            StopCoroutine(PaintCoroutine);
    }

    private void OnEnable()
    {
        if (!started)
            return;

        Debug.Log("enable");
        PaintCoroutine = StartCoroutine(PaintCommand());
    }

    private void SetDebugRenderers()
    {
        PaintManager.I.debugRenderer.material.mainTexture = paintTexture;
        PaintManager.I.debugRenderer2.material.mainTexture = mixRenderTarget;
    }

 
    
    public PointCloudObject PointCloudObject;
    public void MakePaintPointCloud(bool isPaintOnly, bool pointCloudEnabled)
    {
        if (PointCloudObject)
            Destroy(PointCloudObject.gameObject);

        if (pointCloudEnabled)
        {
            PointCloudObject = Instantiate(PaintManager.I.pointCloudPrefab, transform.parent)
                .GetComponent<PointCloudObject>();

            PointCloudObject.Init(isPaintOnly ? paintTexture : mixRenderTarget, mesh, textureSize, submeshIndex, this);
        }
    }

    private static void ClearTextures(params RenderTexture[] textures)
    {
        foreach (var renderTexture in textures)
        {
            Graphics.SetRenderTarget(renderTexture);
            GL.Clear(false, true, Color.clear);
        }
    }

    private void ExportUvIslands()
    {
        var cb = new CommandBuffer();
        cb.SetRenderTarget(uvIslandTarget);
        cb.DrawMesh(mesh, Matrix4x4.identity, PaintManager.I.uvIslandMaterial, submeshIndex);
        Graphics.ExecuteCommandBuffer(cb);
    }

    private void InitEdgeDissolve()
    {
        dissolveEdgesMaterial.SetTexture("_MainTex", paintTexture);
        dissolveEdgesMaterial.SetTexture("_UvIslands", uvIslandTarget);

        alphaBlendMat.SetTexture("_MainTex", fixedIslands);
        alphaBlendMat.SetTexture("_BaseTex", originalTexture);
    }

    public void SetPaintTexture(Texture2D texture2D)
    {
        Graphics.Blit(texture2D, paintTexture);
        Graphics.Blit(paintTexture, mixRenderTarget, alphaBlendMat);
    }

    private void Update()
    {
        paintMaterial.SetMatrix("o2w", transform.localToWorldMatrix);
    }
}