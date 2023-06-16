using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// this is the base class that provides the slider functionality with controlled value setting for the the sliders
/// </summary>
public abstract class SliderController : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private float maxValue;

    protected abstract float ControlledValue { get; set; }
    protected abstract bool IsInt { get; }

    private void Awake()
    {
        slider.onValueChanged.AddListener(IsInt ? OnValueChangedInt : OnValueChanged);
        slider.value = ControlledValue / maxValue;
    }

    private void OnValueChanged(float value)
    {
        var endValue = value * maxValue;

        ControlledValue = endValue;
        valueText.text = endValue.ToString("f2");
    }

    private void OnValueChangedInt(float value)
    {
        var endValue = (int)Mathf.Round(value * maxValue);

        ControlledValue = endValue;
        valueText.text = endValue.ToString();
    }
}