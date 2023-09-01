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
    private string precisionString;
    private int precisionDecimal;
    private string precisionF;
    public float startValue;

    public UnityEvent<float> onValueChanged = new UnityEvent<float>();
    // Start is called before the first frame update
    void Start()
    {
        precisionString = precision.ToString(System.Globalization.CultureInfo.InvariantCulture);
        Debug.Log("precision string >" + precisionString + "< length: "+ precisionString.Length+" indexof: "+ precisionString.IndexOf("."));
        precisionDecimal = precisionString.Length - precisionString.IndexOf(".") - 1;
        Debug.Log("precisionDecimal is >" + precisionDecimal + "<");
        valueName.text = valueNameString;
        precisionF = "F" + precisionDecimal.ToString();
        Debug.Log("precision F is >" + precisionF+"<");
        value.text = startValue.ToString(precisionF);
        onValueChanged.Invoke(startValue);

        //Debug.Log($"Value Setter Start. valueNameString is {valueNameString} and startValue is {startValue.ToString()}");
        //Debug.Log($"Value Setter Start. valueName.text is {valueName.text} and value.text is {value.text}");
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
        Debug.Log($"Set Value {newValue} for {this.gameObject.name}");
        value.text = newValue.ToString(precisionF);
    }
}
