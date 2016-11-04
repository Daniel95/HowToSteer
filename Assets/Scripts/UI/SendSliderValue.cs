using UnityEngine;
using System;
using UnityEngine.UI;
using System.Collections.Generic;

public class SendSliderValue : MonoBehaviour {

    public Action<float> OnValueChange;

    private Slider slider;

    private void Awake() {
        slider = GetComponent<Slider>();
    }

    public void ValueChange() {
        if (OnValueChange != null)
            OnValueChange(slider.value);
    }

    public float GetSliderValue() {
        return slider.value;
    }
}
