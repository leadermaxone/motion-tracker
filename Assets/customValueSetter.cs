    using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class CustomValueSetter : MonoBehaviour
{
    public TextMeshProUGUI value;
    public TextMeshProUGUI valueName;
    public string valueNameString;
    public float precision = 0.1f;
    private string precisionString;
    private int precisionDecimal;
    private string precisionF;
    public float startValue;

    public UnityEvent<float> onValueChanged = new UnityEvent<float>();

    void Start()
    {
        precisionString = precision.ToString(System.Globalization.CultureInfo.InvariantCulture);
        if(precisionString.IndexOf(".")  != -1)
        {
            precisionDecimal = precisionString.Length - precisionString.IndexOf(".") - 1;
        }
        else
        {
            precisionDecimal = 0;
        }
        valueName.text = valueNameString;
        precisionF = "F" + precisionDecimal.ToString();
        //value.text = startValue.ToString(precisionF);
        //onValueChanged.Invoke(startValue);

    }

    public void OnPlusClicked()
    {
        if (value.text != null) 
        {
            float newValue = float.Parse(value.text) + precision;
            value.text = newValue.ToString(precisionF);
            onValueChanged.Invoke(newValue);
        }
    }
    public void OnMinusClicked()
    {
        if (value.text != null) 
        {
            float newValue = float.Parse(value.text) - precision;
            value.text = newValue.ToString(precisionF);
            onValueChanged.Invoke(newValue);
        }
    }
    public void SetValue(float newValue)
    {
        value.text = newValue.ToString(precisionF);
    }
}
