using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class MeshUtils
{
    /// <summary>
    /// this first creates a mesh were it's triangles shares no vertices, something like flat shading, except that
    /// we preserve the same normals
    /// the reason for creating such a mesh, is it enables us to unwrap the mesh by creating a naively simple uv map
    /// you will 2 methods for the uv creation, one of them is created by Unity and works on the unity editor only
    /// so I created a naive one that works on the build but it's not as good as the one created by Unity 
    /// </summary>
    public static Mesh CreateUnwrappedMesh(Mesh mesh)
    {
        var triangleVertexCount = mesh.triangles.Length;

        var vertices = new Vector3[triangleVertexCount];
        var triangles = new int[triangleVertexCount];
        var normals = new Vector3[triangleVertexCount];
        for (var i = 0; i < mesh.triangles.Length; i++)
        {
            var vIndex = mesh.triangles[i];
            var v = mesh.vertices[vIndex];

            triangles[i] = i;
            vertices[i] = v;
            normals[i] = mesh.normals[vIndex];
        }

        var m = new Mesh()
        {
            vertices = vertices,
            triangles = triangles,
            normals = normals,
        };

#if UNITY_EDITOR
        m.uv = Unwrapping.GeneratePerTriangleUV(m);
#else
        m.uv = GetUvPositions(triangleVertexCount).ToArray();
#endif

        return m;
    }

    /// <summary>
    /// creates a simple uv map, supposed no shared vertices between triangles
    /// </summary>
    private static IEnumerable<Vector2> GetUvPositions(int triangleVertexCount)
    {
        var points = triangleVertexCount / 6;

        //this means each dimension*dimension will hold the whole points or more
        var dimensionLength = Mathf.Ceil(Mathf.Sqrt(points));
        var unitLength = 1f / dimensionLength;
        for (int i = 0, y = 0; y < dimensionLength; y++)
        {
            for (var x = 0; x < dimensionLength; x++)
            {
                if (i >= points)
                    yield break;

                //bottom left of the square (2 triangles)
                var p = new Vector2(x, y) * unitLength;

                yield return p;
                yield return new Vector2(p.x, p.y + unitLength);
                yield return new Vector2(p.x + unitLength, p.y);

                yield return p + Vector2.one * unitLength;
                yield return new Vector2(p.x, p.y + unitLength);
                yield return new Vector2(p.x + unitLength, p.y);

                i++;
            }
        }
    }
}