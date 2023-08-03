using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PhoneModelController : MonoBehaviour
{
    public string label;
    public TextMeshProUGUI TMPlabel;
    // Start is called before the first frame update
    void Start()
    {
        TMPlabel.text = label;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
