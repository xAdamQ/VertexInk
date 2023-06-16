using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PointCloudObject : MonoBehaviour
{
    private List<Vector3> vertices;
    private List<Color> colors;
    private List<int> indices;
    private Mesh mesh;
    private Material positionExportMaterial;
    private RenderTexture positionContainer, colorTexture;
    private int submeshIndex;
    private float scaleDownFactor;
    private Vector3 realCenter;
    private Paintable paintable;

    public void Init(RenderTexture colorTexture, Mesh mesh, int textureSize, int submeshIndex, Paintable paintable)
    {
        this.colorTexture = colorTexture;
        this.mesh = mesh;
        this.submeshIndex = submeshIndex;
        this.paintable = paintable;

        positionContainer = new RenderTexture(textureSize, textureSize, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        positionExportMaterial = new Material(PaintManager.I.positionShader);

        SetPointCloudFactors();
        MakePositionContainer();
        MakeMeshGeometry();
        MakeMesh();
        PlaceObject();

        // InputManager.I.ActiveObject = gameObject;
        PaintManager.I.debugRenderer4.material.mainTexture = positionContainer;
    }

    private void SetPointCloudFactors()
    {
        var bounds = mesh.bounds;

        realCenter = bounds.center;
        positionExportMaterial.SetVector("_RealCenterOffset", realCenter);

        var size = bounds.size;
        scaleDownFactor = 1f / Mathf.Max(size.x, size.y, size.z);
        positionExportMaterial.SetFloat("_ScaleDownFactor", scaleDownFactor);

        Debug.Log(realCenter);
        Debug.Log(size);
        Debug.Log(scaleDownFactor);
    }

    private void MakePositionContainer()
    {
        var cb = new CommandBuffer();
        cb.SetRenderTarget(positionContainer);
        cb.DrawMesh(mesh, transform.localToWorldMatrix, positionExportMaterial, submeshIndex);
        Graphics.ExecuteCommandBuffer(cb);
    }

    private void MakeMeshGeometry()
    {
        vertices = new List<Vector3>();
        colors = new List<Color>();

        indices = new List<int>();

        var pos = positionContainer.ToTexture();
        var color = colorTexture.ToTexture();

        var i = 0;
        for (var y = 0; y < positionContainer.width; y++)
        {
            for (var x = 0; x < positionContainer.height; x++)
            {
                var pRaw = pos.GetPixel(x, y);
                var c = color.GetPixel(x, y);

                // if (c == Color.clear)
                // c = Color.magenta;
                if (pRaw == Color.clear || c == Color.clear) continue;

                var p = (pRaw.ToVector3() * 2 - Vector3.one) / scaleDownFactor + realCenter;

                vertices.Add(p);
                colors.Add(c);
                indices.Add(i);

                i++;
            }
        }
    }

    private void MakeMesh()
    {
        var m = new Mesh
        {
            indexFormat = IndexFormat.UInt32,
            vertices = vertices.ToArray(),
            colors = colors.ToArray()
        };

        m.SetIndices(indices, MeshTopology.Points, 0);
        m.RecalculateBounds();


        GetComponent<MeshFilter>().mesh = m;
    }

    private void PlaceObject()
    {
        gameObject.transform.position = paintable.transform.position;
        gameObject.transform.localScale = paintable.transform.localScale;
    }

    private void MakePointsBruteForce()
    {
        foreach (var vector3 in vertices)
        {
            var c = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c.transform.position = vector3;
            c.transform.localScale = Vector3.one * .1f;
        }
    }
}