using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


namespace InstaPaint
{
    /// <summary>
    /// I used to make some tests in this class
    /// </summary>
    public class MeshTester : MonoBehaviour
    {
        public Camera camera;
        public int brushSize = 1;
        private readonly HashSet<GameObject> manipulatedGos = new();

        private void Start()
        {
            camera = Camera.main;
        }

        private void Update()
        {
            //shoot a raycast from the camera to any object in the scene
            if (Pointer.current.press.wasPressedThisFrame)
            {
                var ray = camera.ScreenPointToRay(Mouse.current.position.ReadValue());

                //debug the ray
                Debug.DrawRay(ray.origin, ray.direction * 10000, Color.red, 1f);

                //if the raycast hits an object
                if (Physics.Raycast(ray, out var hit, 10000))
                {
                    if (!manipulatedGos.Contains(hit.collider.gameObject))
                        InitManipulation(hit.collider.gameObject);

                    //if the object is the mesh
                    //get the mesh renderer
                    Debug.Log(hit.triangleIndex);
                    Debug.Log(hit.textureCoord);
                    Debug.Log(hit.barycentricCoordinate);

                    TryPaint(hit);
                }
            }
        }

        private void InitManipulation(GameObject go)
        {
            manipulatedGos.Add(go);

            var rend = go.GetComponent<Renderer>();
            var originalTexture = (Texture2D)rend.material.mainTexture;
            var texture = new Texture2D(originalTexture.width, originalTexture.height);
            texture.SetPixels(originalTexture.GetPixels());
            texture.filterMode = FilterMode.Point;
            texture.Apply();

            rend.material.mainTexture = texture;
        }

        private void TryPaint(RaycastHit hit)
        {
            var rend = hit.transform.GetComponent<Renderer>();
            var meshCollider = hit.collider as MeshCollider;

            if (rend == null || rend.sharedMaterial == null || rend.sharedMaterial.mainTexture == null || meshCollider == null)
                return;

            var tex = (Texture2D)rend.material.mainTexture;
            var pixelUV = hit.textureCoord;
            var pixelXY = new Vector2Int((int)(pixelUV.x * tex.width), (int)(pixelUV.y * tex.height));

            for (var y = 0; y < brushSize; y++)
            {
                for (var x = 0; x < brushSize; x++)
                {
                    var point = pixelXY + new Vector2Int(x, y);
                    if (point.x >= tex.width)
                        point.x -= tex.width;
                    if (point.y >= tex.height)
                        point.y -= tex.height;
                    tex.SetPixel(point.x, point.y, Color.red);
                    // Debug.Log($@"colored {point}");
                }
            }

            tex.Apply();
        }

        private void OnValidate()
        {
            if (brushSize < 1)
                brushSize = 1;
        }
    }
}