using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class InputManager : MonoModule<InputManager>
{
    [SerializeField] private float moveSpeed, scrollSpeed, xRotSpeed, yRotSpeed;
    [SerializeField] private float xRotMin, xRotMax, yPosMin, yPosMax, orthoSizeMin, orthoSizeMax, objectYRotSpeed, objectXZRotSpeed;
    public DefaultInputActions InputActions;

    private Transform rotationSlave;

    public Camera Camera;
    protected override void Awake()
    {
        base.Awake();

        InputActions = new();
        InputActions.Enable();

        Camera = GetComponent<Camera>();
    }

    private void OnDisable()
    {
        InputActions.Disable();
    }

    private void Start()
    {
        InputActions.Player.Look.performed += ctx =>
        {
            var t = transform;
            var delta = -ctx.ReadValue<Vector2>();
            var rot = t.eulerAngles;

            if (delta.y < 0 && rot.x <= xRotMin || delta.y > 0 && rot.x >= xRotMax) delta.y = 0;

            var deltaTime = Time.deltaTime;
            t.eulerAngles += new Vector3(delta.y * xRotSpeed * deltaTime, -delta.x * yRotSpeed * deltaTime, 0);
        };

        InputActions.Player.Move.performed += ctx =>
        {
            var delta = -ctx.ReadValue<Vector2>();

            if (delta == Vector2.zero) return;

            var deltaTime = Time.deltaTime;
            var t = transform;

            var newPoz = t.position + delta.x * transform.right * moveSpeed * deltaTime +
                         delta.y * rotationSlave.forward * moveSpeed * deltaTime;
            t.position = newPoz;
        };

        InputActions.Player.RotateObjectXZ.performed += ctx => RotateObjectXZ(ctx.ReadValue<Vector2>());

        rotationSlave = new GameObject("camera rotation slave").transform;
    }

    private void Zoom(float scroll)
    {
        if (scroll == 0) return;

        var deltaTime = Time.deltaTime;
        var t = transform;
        var pos = t.position;


        if (Camera.orthographic)
        {
            var newSize = Camera.orthographicSize + -scroll * scrollSpeed * deltaTime;
            if (newSize > orthoSizeMin && newSize < orthoSizeMax)
                Camera.orthographicSize = newSize;
        }
        else
        {
            //positive is forward, so it makes the y negative
            if ((scroll > 0 && pos.y > yPosMin) || (scroll < 0 && pos.y < yPosMax))
                t.position += scroll * t.forward * scrollSpeed * deltaTime;
        }
    }

    private GameObject activeObject;
    public GameObject ActiveObject
    {
        get => activeObject;
        set
        {
            activeObject = value;

            ActivePaintableContainer = value.GetComponentInChildren<Paintable>().gameObject;
            ActiveCenter = CalcCenter(value);
            // ActiveRenderer = value.GetComponent<Renderer>();
        }
    }

    public GameObject ActivePaintableContainer;
    private Vector3 ActiveCenter;

    private Vector3 CalcCenter(GameObject root)
    {
        var min = Vector3.positiveInfinity;
        var max = Vector3.negativeInfinity;

        foreach (var bounds in root.GetComponentsInChildren<Renderer>().Select(r => r.bounds))
        {
            min.x = Mathf.Min(bounds.min.x, min.x);
            min.y = Mathf.Min(bounds.min.y, min.y);
            min.z = Mathf.Min(bounds.min.z, min.z);

            max.x = Mathf.Max(bounds.max.x, max.x);
            max.y = Mathf.Max(bounds.max.y, max.y);
            max.z = Mathf.Max(bounds.max.z, max.z);
        }

        return (min + max) / 2f;
    }

    // public Renderer ActiveRenderer;

    private void RotateObjectXZ(Vector2 value)
    {
        var valueVector = new Vector3(value.y, 0, -value.x);
        var axis = valueVector.normalized;
        var force = valueVector.magnitude;

        ActiveObject.transform.RotateAround(ActiveCenter, axis, objectXZRotSpeed * force * Time.deltaTime);
    }
    private void RotateObjectY(float value)
    {
        var axis = new Vector3(0, value, 0).normalized;
        //this value is either 1 or -1
        var force = Mathf.Abs(value);

        ActiveObject.transform.RotateAround(ActiveCenter, axis, objectYRotSpeed * force * Time.deltaTime);
    }

    private void OnDestroy()
    {
        InputActions.Disable();
    }

    private void Update()
    {
        if (InputActions.Player.RotateObjectY.WasPerformedThisFrame())
            RotateObjectY(InputActions.Player.RotateObjectY.ReadValue<float>());
        else if (InputActions.Player.Zoom.WasPerformedThisFrame())
            Zoom(InputActions.Player.Zoom.ReadValue<float>());

        rotationSlave.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
    }
}