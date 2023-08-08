using TMPro;using UnityEngine;using UnityEngine.InputSystem;using UnityEngine.UI;using System;using Gyroscope = UnityEngine.InputSystem.Gyroscope;public class SensorsReader : MonoBehaviour{    private bool sensorsEnabled = false;    public TextMeshProUGUI text, text2;    public GameObject PhoneModel1, PhoneModel2, PhoneModel3, PhoneModel4;    Vector3 gyroAttitude;    Vector3 gyroRotationUnbiased;    Vector3 angularVelocity;    Vector3 acceleration;    Vector3 accelerationValue;    Quaternion attitude;    Vector3 attitudeEuler;    Quaternion attitudeValue;    Vector3 attitudeValueEuler;    Vector3 gravity;

    GameObject phoneContainer;
    Quaternion rot;

    public float rotationSpeedFactor = 10f; // Adjust this value to control rotation speed

    void Start()    {
        //text = GetComponent<TextMeshProUGUI>();
        //text2 = GetComponent<TextMeshProUGUI>();

        acceleration = Vector3.zero;        accelerationValue = Vector3.zero;        attitudeEuler = Vector3.zero;        attitudeValueEuler = Vector3.zero;        if(!sensorsEnabled)
        {
            connectSensors();
        }        phoneContainer = new GameObject("Phone Container");        phoneContainer.transform.position = PhoneModel3.transform.position;        PhoneModel3.transform.SetParent(phoneContainer.transform);        phoneContainer.transform.rotation = Quaternion.Euler(0, 0, 0);        rot = new Quaternion(0, 0, 1, 0);    }    void Update()     {
        if (!sensorsEnabled)
        {
            connectSensors();
            return;
        }

        angularVelocity = Gyroscope.current.angularVelocity.ReadValue();        accelerationValue = Accelerometer.current.acceleration.ReadValue();        acceleration.y = accelerationValue.z;        acceleration.z = accelerationValue.y;        acceleration.x = accelerationValue.x;        attitudeValue = AttitudeSensor.current.attitude.ReadValue(); // ReadValue() returns a Quaternion
        attitudeValueEuler = attitudeValue.eulerAngles;        attitudeEuler.y = -(float)Math.Round(attitudeValueEuler.z, 1);        attitudeEuler.z = -(float)Math.Round(attitudeValueEuler.y, 1);        attitudeEuler.x = -(float)Math.Round(attitudeValueEuler.x, 1);                 gravity = GravitySensor.current.gravity.ReadValue();        PhoneModel1.transform.Rotate(rotationSpeedFactor * Time.deltaTime * angularVelocity);        PhoneModel2.transform.Rotate(rotationSpeedFactor * Time.deltaTime *  acceleration);
        PhoneModel3.transform.rotation = Quaternion.Euler(attitudeEuler);
        //PhoneModel3.transform.localRotation = attitudeValue * rot;        //PhoneModel3.transform.localRotation = attitudeValue;        PhoneModel4.transform.Rotate(rotationSpeedFactor * Time.deltaTime *  gravity);        text.text = $"Angular Velocity\nX={angularVelocity.x:#0.00} Y={angularVelocity.y:#0.00} Z={angularVelocity.z:#0.00}\n\n" +                        $"Acceleration\nX={acceleration.x:#0.00} Y={acceleration.y:#0.00} Z={acceleration.z:#0.00}\n\n" +                            $"Attitude\nX={attitudeValue.x:#0.00} Y={attitudeValue.y:#0.00} Z={attitudeValue.z:#0.00}\n\n" +                             $"Gravity\nX={gravity.x:#0.00} Y={gravity.y:#0.00} Z={gravity.z:#0.00}";        text2.text = $"attitudeValueEuler \nX={attitudeValueEuler.x:#0.00} Y={attitudeValueEuler.y:#0.00} Z={attitudeValueEuler.z:#0.00}\n\n" +                        $"attitudeEuler\nX={attitudeEuler.x:#0.00} Y={attitudeEuler.y:#0.00} Z={attitudeEuler.z:#0.00}\n\n"                         //+                           // $"Attitude\nX={attitude.x:#0.00} Y={attitude.y:#0.00} Z={attitude.z:#0.00}\n\n" +                            // $"Gravity\nX={gravity.x:#0.00} Y={gravity.y:#0.00} Z={gravity.z:#0.00}"
                            ;    }    void connectSensors()
    {
        if (Gyroscope.current != null)
        {
            InputSystem.EnableDevice(Gyroscope.current);
        }        if (Accelerometer.current != null)
        {
            InputSystem.EnableDevice(Accelerometer.current);
        }        if (AttitudeSensor.current != null)
        {
            InputSystem.EnableDevice(AttitudeSensor.current);
        }        if (GravitySensor.current != null)
        {
            InputSystem.EnableDevice(GravitySensor.current);
        }

        if(
            Gyroscope.current != null && Gyroscope.current.enabled &&
            Accelerometer.current != null &&  Accelerometer.current.enabled &&
            AttitudeSensor.current != null &&  AttitudeSensor.current.enabled &&
            GravitySensor.current != null &&  GravitySensor.current.enabled
           )
        {
            sensorsEnabled = true;
        }
    }}