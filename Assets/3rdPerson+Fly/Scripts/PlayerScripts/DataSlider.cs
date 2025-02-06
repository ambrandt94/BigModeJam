using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class DataSlider : MonoBehaviour
{
    [SerializeField]
    private Component sourceObject;

    private Slider slider;
    public IHaveSliderData dataSource;

    private void Updateslider(float f)
    {
        float v = slider.value;
        DOTween.To(() => v, x => v = x, f, .2f)
            .OnUpdate(() => {
                slider.value = v;
            });
    }

    private void Awake()
    {
        slider = GetComponent<Slider>();
        dataSource = (IHaveSliderData)sourceObject;
        if (dataSource != null) {
            slider.maxValue = dataSource.MaxAmount;
            slider.SetValueWithoutNotify(dataSource.CurrentAmount);
            dataSource.OnSliderDataUpdate += Updateslider;
        }
    }
}


public interface IHaveSliderData
{
    public float CurrentAmount { get; set; }
    public float MaxAmount { get; set; }
    public Action<float> OnSliderDataUpdate { get; set; }
}