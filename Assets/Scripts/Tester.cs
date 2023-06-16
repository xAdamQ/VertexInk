using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class Tester : MonoBehaviour
{
    public MeshFilter MeshFilter;

    [ContextMenu("Test")]
    public void Test()
    {
        var m = MeshFilter.sharedMesh;
        // uvs = Unwrapping.GeneratePerTriangleUV(m);
        // var message = uv.Aggregate(string.Empty, (current, next) => current + (next.ToString() + '\n'));
        // Debug.Log(message);

        var m2 = new Mesh()
        {
            vertices = m.vertices,
            triangles = m.triangles,
            uv = uvs
        };

        MeshFilter.mesh = m2;
    }

    private Vector2[] uvs;

    [ContextMenu("Test2")]
    public void Test2()
    {
        var m = MeshFilter.sharedMesh;

        var uvs = new Vector2[m.vertices.Length];

        for (var i = 0; i < m.vertices.Length; i++)
        {
        }

        var message = m.vertices.Aggregate(string.Empty, (current, next) => current + (next.ToString() + '\n'));
        Debug.Log(message);
    }

    [ContextMenu("create pc")]
    public void CreatePointCloudMesh()
    {
        var vertexCount = 1000;
        var vertices = new Vector3[vertexCount];
        var colors = new Color[vertexCount];

        var indices = new int[vertexCount];

        for (var i = 0; i < vertexCount; i++)
        {
            vertices[i] = new Vector3(Random.value, Random.value, Random.value);
            colors[i] = new Color(Random.value, Random.value, Random.value, 1);
            indices[i] = i;
        }

        var m = new Mesh
        {
            vertices = vertices,
            colors = colors
        };

        m.SetIndices(indices, MeshTopology.Points, 0);
        m.RecalculateBounds();

        GetComponent<MeshFilter>().mesh = m;
    }

    public GameObject meshGo;
    [ContextMenu("create unwrapped mesh")]
    public void CreateUnwrappedMesh()
    {
        var mesh = meshGo.GetComponent<MeshFilter>().mesh;
        meshGo.GetComponent<MeshFilter>().mesh = MeshUtils.CreateUnwrappedMesh(mesh);
    }


    private void OnDrawGizmos()
    {
        // if (uvs != null)
        // foreach (var vector2 in uvs)
        // Gizmos.DrawSphere(vector2, .01f);
    }
}