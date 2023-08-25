using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

using Gyroscope = UnityEngine.InputSystem.Gyroscope;
using System.Collections;



public class SensorsReader : MonoBehaviour
{
    public TextMeshProUGUI recordButtonTMP;
    public bool isRecording = false;

    public DD_DataDiagram diagramAccelerationX;

    public DD_DataDiagram diagramAccelerationY;

    public DD_DataDiagram diagramAccelerationZ;

    GameObject lineAccelerationX;
    GameObject lineAccelerationX_NotFiltered;

    GameObject lineAccelerationY;
    GameObject lineAccelerationY_NotFiltered;

    GameObject lineAccelerationZ;
    GameObject lineAccelerationZ_NotFiltered;

    Color colorX = Color.red;
    Color colorX_NotFiltered = Color.grey;

    Color colorY = Color.green;
    Color colorY_NotFiltered = Color.grey;

    Color colorZ = Color.blue;
    Color colorZ_NotFiltered = Color.grey;


    private float currentTime = 0.0f;

    private float period = 2.0f; // Controls the speed of the oscillation



    private bool sensorsEnabled = false;
    public TextMeshProUGUI text, text2;
    public GameObject PhoneModel1, PhoneModel2, PhoneModel3, PhoneModel4;
    public GameObject AccelerationArrow;


    Vector3 angularVelocity;
    Vector3 acceleration;
    Vector3 accelerationValue;
    Quaternion accelerationArrowLookRotation;
    Vector3 accelerationScaleVector = new Vector3(1, 1, 0.5f);

    Vector3 attitudeEuler;
    Quaternion attitudeValue;
    Vector3 attitudeValueEuler;
    Vector3 gravity;


    public float rotationSpeedFactor = 10f; // Adjust this value to control rotation speed



    public GameObject accelerometerUpdateIntervalController;

    public GameObject lowPassKernelWidthInSecondsController;



    public float accelerometerUpdateInterval;

    public float lowPassKernelWidthInSeconds;



    private float lowPassFilterFactor;

    Vector3 prevValue;



    Stack<Vector3> accelerometerFilteredValues = new Stack<Vector3>();

    Stack<Vector3> accelerometerRawValues = new Stack<Vector3>();

    private Vector3 accelerometerCurrentRawValue;

    private Vector3 accelerometerCurrentFilteredValue;

    void Start()
    {

        //text = GetComponent<TextMeshProUGUI>();

        //text2 = GetComponent<TextMeshProUGUI>();

  
        acceleration = Vector3.zero;
        accelerationValue = Vector3.zero;

        attitudeEuler = Vector3.zero;
        attitudeValueEuler = Vector3.zero;



        lowPassFilterFactor = accelerometerUpdateInterval / lowPassKernelWidthInSeconds;


        if (!sensorsEnabled)
        {
            connectSensors();

            try

            {

                accelerometerCurrentRawValue = LinearAccelerationSensor.current.acceleration.ReadValue();
                prevValue = accelerometerCurrentRawValue;

                //accelerometerRawValues.Push(accelerometerCurrentRawValue);

                //accelerometerFilteredValues.Push(accelerometerCurrentRawValue);

            }

            catch (Exception e)

            {

                Debug.Log("Error accessing Sensors " + e);

            }

        }



        lineAccelerationX = diagramAccelerationX.AddLine(colorX.ToString(), colorX);
        lineAccelerationX_NotFiltered = diagramAccelerationX.AddLine(colorX_NotFiltered.ToString(), colorX_NotFiltered);
        lineAccelerationY = diagramAccelerationY.AddLine(colorY.ToString(), colorY);
        lineAccelerationY_NotFiltered = diagramAccelerationY.AddLine(colorY_NotFiltered.ToString(), colorY_NotFiltered);
        lineAccelerationZ = diagramAccelerationZ.AddLine(colorZ.ToString(), colorZ);
        lineAccelerationZ_NotFiltered = diagramAccelerationZ.AddLine(colorZ_NotFiltered.ToString(), colorZ_NotFiltered);
        Debug.Log("Adding line " + lineAccelerationX);

        StartCoroutine(ZoomAndDrag(diagramAccelerationX));
        StartCoroutine(ZoomAndDrag(diagramAccelerationY));
        StartCoroutine(ZoomAndDrag(diagramAccelerationZ));
    }



    public IEnumerator ZoomAndDrag(DD_DataDiagram diagram)

    {

        yield return new WaitForSeconds(0.1f);

        diagram.RaiseMoveEvent(0, 40f);

        diagram.RaiseZoomEvent(-1.5f, -1.5f);

    }

    public void OnRecordPressed()
    {
        if(!isRecording) 
        { 
            isRecording = true;
            recordButtonTMP.text = "STOP RECORDING";
        }
        else
        {
            isRecording = false;
            recordButtonTMP.text = "START RECORDING";

        }
    }



    public void onAccelerometerUpdateIntervalChanged(float newValue)

    {

        accelerometerUpdateInterval = newValue;

        lowPassFilterFactor = accelerometerUpdateInterval / lowPassKernelWidthInSeconds;

    }

    public void lowPassKernelWidthInSecondsChanged(float newValue)

    {

        lowPassKernelWidthInSeconds = newValue;

        lowPassFilterFactor = accelerometerUpdateInterval / lowPassKernelWidthInSeconds;

    }

    Vector3 GetLowPassValue(Vector3 currentValue, Vector3 prevValue)

    {

        Debug.Log($"Low pass: Prev {prevValue} to current {currentValue}");

        return Vector3.Lerp(prevValue, currentValue, lowPassFilterFactor);

    }

    void CalculateAccelerometerValue()

    {



        accelerometerCurrentRawValue = LinearAccelerationSensor.current.acceleration.ReadValue();




        //prevValue = accelerometerFilteredValues.Peek();

        Debug.Log($"Reading new raw  accelerometerCurrentRawValue and peeking last filtered {prevValue}");

        accelerometerCurrentFilteredValue = GetLowPassValue(accelerometerCurrentRawValue, prevValue);
        prevValue = accelerometerCurrentFilteredValue;

        if(isRecording)
        {
            accelerometerRawValues.Push(accelerometerCurrentRawValue);
            accelerometerFilteredValues.Push(accelerometerCurrentFilteredValue);
        }
    }

    void Update() 
    {
        if (!sensorsEnabled)
        {
            connectSensors();

            return;

        }
        try

        {

            angularVelocity = Gyroscope.current.angularVelocity.ReadValue();



            //accelerationValue = LinearAccelerationSensor.current.acceleration.ReadValue();





            CalculateAccelerometerValue();



            acceleration.y = -(float)Math.Round(accelerometerCurrentFilteredValue.z, 2);
            acceleration.z = (float)Math.Round(accelerometerCurrentFilteredValue.y, 2);
            acceleration.x = (float)Math.Round(accelerometerCurrentFilteredValue.x, 2);
            Debug.Log($"Looking towards acceleration {acceleration} of magnitude {acceleration.magnitude}");
            accelerationArrowLookRotation = Quaternion.LookRotation(acceleration);
            AccelerationArrow.transform.rotation = accelerationArrowLookRotation;
            AccelerationArrow.transform.localScale = accelerationScaleVector * (acceleration.magnitude == 0f ? 1f : (1f+acceleration.magnitude) );

            diagramAccelerationX.InputPoint(lineAccelerationX, new Vector2(0.01f, acceleration.x));
            diagramAccelerationX.InputPoint(lineAccelerationX_NotFiltered, new Vector2(0.01f, accelerometerCurrentRawValue.x));

            diagramAccelerationY.InputPoint(lineAccelerationY, new Vector2(0.01f, acceleration.y));
            diagramAccelerationY.InputPoint(lineAccelerationY_NotFiltered, new Vector2(0.01f, accelerometerCurrentRawValue.y));

            diagramAccelerationZ.InputPoint(lineAccelerationZ, new Vector2(0.01f, acceleration.z));
            diagramAccelerationZ.InputPoint(lineAccelerationZ_NotFiltered, new Vector2(0.01f, accelerometerCurrentRawValue.z));

            attitudeValue = AttitudeSensor.current.attitude.ReadValue(); // ReadValue() returns a Quaternion

            attitudeValueEuler = attitudeValue.eulerAngles;
            attitudeEuler.y = -(float)Math.Round(attitudeValueEuler.z, 1);
            attitudeEuler.z = -(float)Math.Round(attitudeValueEuler.y, 1);
            attitudeEuler.x = -(float)Math.Round(attitudeValueEuler.x, 1);
         

            //gravity = GravitySensor.current.gravity.ReadValue();

            //PhoneModel1.transform.Rotate(rotationSpeedFactor * Time.deltaTime * angularVelocity);
            //PhoneModel2.transform.Rotate(rotationSpeedFactor * Time.deltaTime *  acceleration);
            PhoneModel3.transform.rotation = Quaternion.Euler(attitudeEuler);
            //PhoneModel3.transform.localRotation = attitudeValue * rot;
            //PhoneModel3.transform.localRotation = attitudeValue;

            //PhoneModel4.transform.Rotate(rotationSpeedFactor * Time.deltaTime *  gravity);

            text.text = 
                        $"Attitude\nX={attitudeValue.x:#0.00} Y={attitudeValue.y:#0.00} Z={attitudeValue.z:#0.00}\n\n" +
                        $"attitudeValueEuler \nX={attitudeValueEuler.x:#0.00} Y={attitudeValueEuler.y:#0.00} Z={attitudeValueEuler.z:#0.00}\n\n" +
                        $"attitudeEuler\nX={attitudeEuler.x:#0.00} Y={attitudeEuler.y:#0.00} Z={attitudeEuler.z:#0.00}\n\n" +

                        $"Angular Velocity\nX={angularVelocity.x:#0.00} Y={angularVelocity.y:#0.00} Z={angularVelocity.z:#0.00}\n\n"

                        ;




            text2.text =   
                             $"Acceleration Raw \nX={accelerometerCurrentRawValue.x:#0.00} Y={accelerometerCurrentRawValue.y:#0.00} Z={accelerometerCurrentRawValue.z:#0.00}\n\n" +
                             $"acceleration filtered\nX={accelerometerCurrentFilteredValue.x:#0.00} Y={accelerometerCurrentFilteredValue.y:#0.00} Z={accelerometerCurrentFilteredValue.z:#0.00}\n\n"+

                             $"Accelerator Magnitude={acceleration.magnitude:#0.00}\n\n" +
                             $"LowPassKernelWidthS {lowPassKernelWidthInSeconds:#0.00} \naccelerometerUpdateInterval={accelerometerUpdateInterval:#0.00}"

                             //$"Gravity\nX={gravity.x:#0.00} Y={gravity.y:#0.00} Z={gravity.z:#0.00}"

                             ;





        }
        catch (Exception e) 
        {
            Debug.Log("error Update "+ e);
        }
        currentTime += Time.deltaTime;



        // Calculate the sine value between -1 and 1

        //float sineValue = Mathf.Sin(currentTime / period * Mathf.PI);

        //diagramAccelerationX.InputPoint(lineAccelerationX, new Vector2(0.01f, sineValue));
    }

    void connectSensors()
    {
        if (Gyroscope.current != null)
        {
            InputSystem.EnableDevice(Gyroscope.current);

        }
        if (Accelerometer.current != null)
        {
            InputSystem.EnableDevice(Accelerometer.current);

        }
        if (AttitudeSensor.current != null)
        {
            InputSystem.EnableDevice(AttitudeSensor.current);

        }
        if (GravitySensor.current != null)
        {
            InputSystem.EnableDevice(GravitySensor.current);

        }    
        if (LinearAccelerationSensor.current != null)
        {
            InputSystem.EnableDevice(LinearAccelerationSensor.current);

        }

        if(
            Gyroscope.current != null && Gyroscope.current.enabled &&
            Accelerometer.current != null &&  Accelerometer.current.enabled &&
            AttitudeSensor.current != null &&  AttitudeSensor.current.enabled &&
            GravitySensor.current != null &&  GravitySensor.current.enabled &&
            LinearAccelerationSensor.current != null && LinearAccelerationSensor.current.enabled
           )
        {
            sensorsEnabled = true;
        }
    }
}