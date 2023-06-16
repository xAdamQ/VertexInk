using UnityEngine;

/// <summary>
/// this is the lower left slider that controls the quality of the overall texture sizes of the active painted model
/// each submesh will have the base and paint texture with this size
/// </summary>
public class PaintQualityControllerController : SliderController
{
    protected override float ControlledValue
    {
        get => PaintManager.I.TextureSize;
        set => PaintManager.I.TextureSize = (int)Mathf.Round(value);
    }

    protected override bool IsInt => true;
}