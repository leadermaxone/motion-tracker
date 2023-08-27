    using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class customValueSetter : MonoBehaviour
{
    public TextMeshProUGUI value;
    public TextMeshProUGUI valueName;
    public string valueNameString;
    public float precision = 0.1f;
    public float startValue;

    public UnityEvent<float> onValueChanged = new UnityEvent<float>();
    // Start is called before the first frame update
    void Start()
    {
        valueName.text = valueNameString;
        value.text = startValue.ToString("F2");
        onValueChanged.Invoke(startValue);

        Debug.Log($"Value Setter Start. valueNameString is {valueNameString} and startValue is {startValue.ToString()}");
        Debug.Log($"Value Setter Start. valueName.text is {valueName.text} and value.text is {value.text}");
    }

    public void OnPlusClicked()
    {
        if (value.text != null) 
        {
            float newValue = float.Parse(value.text) + precision;
            value.text = newValue.ToString("F2");
            onValueChanged.Invoke(newValue);
        }
    }
    public void OnMinusClicked()
    {
        if (value.text != null) 
        {
            float newValue = float.Parse(value.text) - precision;
            value.text = newValue.ToString("F2");
            onValueChanged.Invoke(newValue);
        }
    }
    public void SetValue(float newValue)
    {
        Debug.Log($"Set Value {newValue} for {this.gameObject.name}");
        value.text = newValue.ToString("F2");
    }
}
