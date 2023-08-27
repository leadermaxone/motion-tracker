using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

using Gyroscope = UnityEngine.InputSystem.Gyroscope;
using System.Collections;
using UnityEngine.Events;




public class SceneManager : MonoBehaviour
{
    public GameObject recordStepsButton;
    public GameObject recordStillButton;
    public GameObject analyseStepsButton;
    public GameObject analyseStillButton;
    public GameObject checkStillButton;
    public GameObject stillStatus;
    public bool isRecordingSteps = false;
    public bool isRecordingStill = false;

    public float stepHighThreshold;
    public float stepLowThreshold;
    public float stepAvg;

    public float stillHighThreshold;
    public UnityEvent<float> UpdateStillHighThresholdUI = new UnityEvent<float>();

    public float stillLowThreshold;
    public UnityEvent<float> UpdateStillLowThresholdUI = new UnityEvent<float>();

    public float stillAvg;

    public DD_DataDiagram diagramAccelerationX;
    public DD_DataDiagram diagramAccelerationY;
    public DD_DataDiagram diagramAccelerationZ;
    public DD_DataDiagram diagramAccelerationMagnitude;

    GameObject lineAccelerationX;
    GameObject lineAccelerationX_NotFiltered;

    GameObject lineAccelerationY;
    GameObject lineAccelerationY_NotFiltered;

    GameObject lineAccelerationZ;
    GameObject lineAccelerationZ_NotFiltered;

    GameObject lineAccelerationMagnitude;
    GameObject lineAccelerationMagnitude_NotFiltered;

    Color colorX = Color.red;
    Color colorX_NotFiltered = Color.grey;

    Color colorY = Color.green;
    Color colorY_NotFiltered = Color.grey;

    Color colorZ = Color.blue;
    Color colorZ_NotFiltered = Color.grey;    
    
    Color colorMagnitude = Color.magenta;
    Color colorMagnitude_NotFiltered = Color.grey;

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


    public float accelerometerUpdateInterval;
    public float lowPassKernelWidthInSeconds;


    private float lowPassFilterFactor;
    Vector3 previousAccelerometerValue;


    Stack<Vector3> accelerometerFilteredValues = new Stack<Vector3>();
    Stack<Vector3> accelerometerRawValues = new Stack<Vector3>();
    Stack<float> accelerometerMagnitudeRawValues = new Stack<float>();
    Stack<float> accelerometerMagnitudeFilteredValues = new Stack<float>();
    private Vector3 accelerometerCurrentRawValue;
    private Vector3 accelerometerCurrentFilteredValue;

    private Coroutine stillCoroutine;
    private bool isCheckingStandingStill = false;
    private bool hasStartedWaitingForStill = false;

    void Start()
    {

        //text = GetComponent<TextMeshProUGUI>();

        //text2 = GetComponent<TextMeshProUGUI>();
        checkStillButton.SetActive(false);
        stillStatus.SetActive(false);
  
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
                previousAccelerometerValue = accelerometerCurrentRawValue;

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
        lineAccelerationMagnitude = diagramAccelerationMagnitude.AddLine(colorMagnitude.ToString(), colorMagnitude);
        lineAccelerationMagnitude_NotFiltered = diagramAccelerationMagnitude.AddLine(colorMagnitude_NotFiltered.ToString(), colorMagnitude_NotFiltered);

        StartCoroutine(ZoomAndDrag(diagramAccelerationX));
        StartCoroutine(ZoomAndDrag(diagramAccelerationY));
        StartCoroutine(ZoomAndDrag(diagramAccelerationZ));
        StartCoroutine(ZoomAndDrag(diagramAccelerationMagnitude));
    }



    public IEnumerator ZoomAndDrag(DD_DataDiagram diagram)
    {
        yield return new WaitForSeconds(0.1f);
        diagram.RaiseMoveEvent(0, 40f);
        diagram.RaiseZoomEvent(-1.5f, -1.5f);
    }

    public void OnAnalyseStillPressed()
    {
        AnalyseStill();
    }

    public void OnRecordStepsPressed()
    {
        if(!isRecordingSteps) 
        { 
            isRecordingSteps = true;
            ClearRegisteredData();
            analyseStepsButton.SetActive(false);
            recordStepsButton.GetComponentInChildren<TextMeshProUGUI>().text = "STOP RECORDING";
        }
        else
        {
            isRecordingSteps = false;
            analyseStepsButton.SetActive(true);
            recordStepsButton.GetComponentInChildren<TextMeshProUGUI>().text = "TRAIN STEPS";

        }
    }    
    public void OnRecordStillPressed()
    {
        if(!isRecordingStill) 
        {
            isRecordingStill = true;
            ClearRegisteredData();
            //analyseStillButton.SetActive(false);
            recordStillButton.GetComponentInChildren<TextMeshProUGUI>().text = "STOP RECORDING";
            isCheckingStandingStill = false;
            checkStillButton.SetActive(false);

        }
        else
        {
            isRecordingStill = false;
            //analyseStillButton.SetActive(true);
            recordStillButton.GetComponentInChildren<TextMeshProUGUI>().text = "TRAIN STAY STILL";
        }
    }

    public void OnCheckStandingStillPressed()
    {
        if(!isCheckingStandingStill)
        {
            checkStillButton.GetComponentInChildren<TextMeshProUGUI>().text = "STOP CHECKING";
            isCheckingStandingStill = true;
            stillStatus.SetActive(true);
            stillStatus.GetComponentInChildren<TextMeshProUGUI>().text = "moving";
        }
        else
        {
            OnStopCheckForStill();
            checkStillButton.GetComponentInChildren<TextMeshProUGUI>().text = "START CHECK STILL";
            stillStatus.GetComponentInChildren<Image>().color = Color.red;
            isCheckingStandingStill = false;
            stillStatus.SetActive(false);

        }
    }


    private void AnalyseStill()
    {
        if(accelerometerMagnitudeFilteredValues.Count > 0)
        {
            stillHighThreshold = 0f;
            stillLowThreshold= 100f;
            for (int i = 0; i<accelerometerMagnitudeFilteredValues.Count; i++)
            {
                var current = accelerometerMagnitudeFilteredValues.Pop();
                if(current > stillHighThreshold)
                { 
                    stillHighThreshold = current;
                }
                else if (current < stillLowThreshold)
                { 
                    stillLowThreshold = current; 
                }
            }
            UpdateStillHighThresholdUI.Invoke(stillHighThreshold);
            UpdateStillLowThresholdUI.Invoke(stillLowThreshold);
            Debug.Log($"Analysis Still Complete: low {stillLowThreshold} - high {stillHighThreshold}");
            checkStillButton.SetActive(true);
        }
        else
        {
            Debug.Log($"Analysis Still Error: nothing to analyse");
        }
    }

    private IEnumerator WaitForStill()
    {
        yield return new WaitForSeconds(3);
        // invoke user logic
        Debug.Log($"Player Still for long enough!");
        stillStatus.GetComponentInChildren<TextMeshProUGUI>().text = "STILL!!!";
        stillStatus.GetComponentInChildren<Image>().color = Color.green;
    }

    private void OnMoving()
    {
        stillStatus.GetComponentInChildren<TextMeshProUGUI>().text = "moving";
        stillStatus.GetComponentInChildren<Image>().color = Color.red;
    }
    private void OnStopCheckForStill()
    {
        if(stillCoroutine != null)
        {
            StopCoroutine(stillCoroutine);
        }
    }

    private void CheckStandingStillF(Vector3 acceleration)
    {
        //TODO convert to sqrMagnitude for better performances
        if(acceleration.magnitude < stillHighThreshold)
        {
            if(!hasStartedWaitingForStill)
            {
                hasStartedWaitingForStill = true;
                stillCoroutine = StartCoroutine(WaitForStill());
            }
        }
        else
        {
            if(hasStartedWaitingForStill)
            {
                hasStartedWaitingForStill = false;
                if (stillCoroutine != null)
                {
                    StopCoroutine(stillCoroutine);
                }
                OnMoving();
            }
        }
    }

    private void ClearRegisteredData()
    {
        accelerometerMagnitudeFilteredValues.Clear();
        accelerometerMagnitudeRawValues.Clear();
        accelerometerFilteredValues.Clear();
        accelerometerRawValues.Clear();
    }
    Vector3 GetLowPassValue(Vector3 currentValue, Vector3 prevValue)

    {

        //Debug.Log($"Low pass: Prev {prevValue} to current {currentValue}");

        return Vector3.Lerp(prevValue, currentValue, lowPassFilterFactor);

    }

    void CalculateAccelerometerValue()

    {

        accelerometerCurrentRawValue = LinearAccelerationSensor.current.acceleration.ReadValue();

        //previousAccelerometerValue = accelerometerFilteredValues.Peek();

        //Debug.Log($"Reading new raw  accelerometerCurrentRawValue and peeking last filtered {previousAccelerometerValue}");

        accelerometerCurrentFilteredValue = GetLowPassValue(accelerometerCurrentRawValue, previousAccelerometerValue);
        previousAccelerometerValue = accelerometerCurrentFilteredValue;
        if(isRecordingSteps || isRecordingStill)
        {
            accelerometerRawValues.Push(accelerometerCurrentRawValue);
            accelerometerFilteredValues.Push(accelerometerCurrentFilteredValue);
            accelerometerMagnitudeRawValues.Push(accelerometerCurrentRawValue.magnitude);
            accelerometerMagnitudeFilteredValues.Push(accelerometerCurrentFilteredValue.magnitude);
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
            //Debug.Log($"Looking towards acceleration {acceleration} of magnitude {acceleration.magnitude}");
            if(acceleration != Vector3.zero)
            {
                accelerationArrowLookRotation = Quaternion.LookRotation(acceleration);
                AccelerationArrow.transform.rotation = accelerationArrowLookRotation;
                AccelerationArrow.transform.localScale = accelerationScaleVector * (acceleration.magnitude == 0f ? 1f : (1f+acceleration.magnitude) );
            }

            DrawDiagramLines(acceleration);

            if(isCheckingStandingStill)
            {
                CheckStandingStillF(acceleration);
            }

            attitudeValue = AttitudeSensor.current.attitude.ReadValue(); // ReadValue() returns a Quaternion

            attitudeValueEuler = attitudeValue.eulerAngles;
            attitudeEuler.y = -(float)Math.Round(attitudeValueEuler.z, 1);
            attitudeEuler.z = -(float)Math.Round(attitudeValueEuler.y, 1);
            attitudeEuler.x = -(float)Math.Round(attitudeValueEuler.x, 1);
         
            PhoneModel3.transform.rotation = Quaternion.Euler(attitudeEuler);

            WriteVisualLogs();
            
        }
        catch (Exception e) 
        {
            Debug.Log("error Update "+ e);
        }
    }

    public void OnStepHighThresholdChanged(float newValue)
    {
        stepHighThreshold = newValue;
    }
    public void OnStepLowThresholdChanged(float newValue)
    {
        stepLowThreshold = newValue;
    }
    public void OnStillHighThresholdChanged(float newValue)
    {
        stillHighThreshold = newValue;
    }
    public void OnStillLowThresholdChanged(float newValue)
    {
        stillLowThreshold = newValue;
    }

    public void OnAccelerometerUpdateIntervalChanged(float newValue)
    {
        accelerometerUpdateInterval = newValue;
        lowPassFilterFactor = accelerometerUpdateInterval / lowPassKernelWidthInSeconds;
    }

    public void LowPassKernelWidthInSecondsChanged(float newValue)
    {
        lowPassKernelWidthInSeconds = newValue;
        lowPassFilterFactor = accelerometerUpdateInterval / lowPassKernelWidthInSeconds;
    }

    private void WriteVisualLogs()
    {
        text.text =
                        $"Attitude\nX={attitudeValue.x:#0.00} Y={attitudeValue.y:#0.00} Z={attitudeValue.z:#0.00}\n\n" +
                        $"attitudeValueEuler \nX={attitudeValueEuler.x:#0.00} Y={attitudeValueEuler.y:#0.00} Z={attitudeValueEuler.z:#0.00}\n\n" +
                        $"attitudeEuler\nX={attitudeEuler.x:#0.00} Y={attitudeEuler.y:#0.00} Z={attitudeEuler.z:#0.00}\n\n" +

                        $"Still threshold \nLow={stillLowThreshold:#0.00} High={stillHighThreshold:#0.00} \n" +
                        $"Step threshold \nLow={stepLowThreshold:#0.00} High={stepHighThreshold:#0.00}"

                        ;




        text2.text =
                         $"Acceleration Raw \nX={accelerometerCurrentRawValue.x:#0.00} Y={accelerometerCurrentRawValue.y:#0.00} Z={accelerometerCurrentRawValue.z:#0.00}\n\n" +
                         $"acceleration filtered\nX={accelerometerCurrentFilteredValue.x:#0.00} Y={accelerometerCurrentFilteredValue.y:#0.00} Z={accelerometerCurrentFilteredValue.z:#0.00}\n\n" +

                         $"Accelerator Magnitude={acceleration.magnitude:#0.00}\n\n" +
                         $"LowPassKernelWidthS {lowPassKernelWidthInSeconds:#0.00} \naccelerometerUpdateInterval={accelerometerUpdateInterval:#0.00}"

                         //$"Gravity\nX={gravity.x:#0.00} Y={gravity.y:#0.00} Z={gravity.z:#0.00}"

                         ;
    }

    private void DrawDiagramLines(Vector3 acceleration)
    {
        diagramAccelerationX.InputPoint(lineAccelerationX, new Vector2(0.01f, acceleration.x));
        diagramAccelerationX.InputPoint(lineAccelerationX_NotFiltered, new Vector2(0.01f, accelerometerCurrentRawValue.x));

        diagramAccelerationY.InputPoint(lineAccelerationY, new Vector2(0.01f, acceleration.y));
        diagramAccelerationY.InputPoint(lineAccelerationY_NotFiltered, new Vector2(0.01f, accelerometerCurrentRawValue.y));

        diagramAccelerationZ.InputPoint(lineAccelerationZ, new Vector2(0.01f, acceleration.z));
        diagramAccelerationZ.InputPoint(lineAccelerationZ_NotFiltered, new Vector2(0.01f, accelerometerCurrentRawValue.z));

        diagramAccelerationMagnitude.InputPoint(lineAccelerationMagnitude, new Vector2(0.01f, acceleration.magnitude));
        diagramAccelerationMagnitude.InputPoint(lineAccelerationMagnitude_NotFiltered, new Vector2(0.01f, accelerometerCurrentRawValue.magnitude));

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