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
        value.text = startValue.ToString();
    }

    public void onPlusClicked()
    {
        if (value.text != null) 
        {
            float newValue = float.Parse(value.text) + precision;
            value.text = newValue.ToString();
            onValueChanged.Invoke(newValue);
        }
    }
    public void onMinusClicked()
    {
        if (value.text != null) 
        {
            float newValue = float.Parse(value.text) - precision;
            value.text = newValue.ToString();
            onValueChanged.Invoke(newValue);
        }
    }
}
