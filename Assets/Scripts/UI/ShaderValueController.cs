using UnityEngine;

/// <summary>
/// this enables making instances of sliders that control global shader values, like
/// the brush size and sharpness, and the point cloud size 
/// </summary>
public class ShaderValueController : SliderController
{
    [SerializeField] protected float controlledValue;
    [SerializeField] protected string shaderValueName;

    protected override float ControlledValue
    {
        get => controlledValue;
        set
        {
            Shader.SetGlobalFloat(shaderValueName, value);
            controlledValue = value;
        }
    }

    protected override bool IsInt => false;

    private void Start()
    {
        Shader.SetGlobalFloat(shaderValueName, controlledValue);
    }
}