using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

using Gyroscope = UnityEngine.InputSystem.Gyroscope;
using System.Collections;
using UnityEngine.Events;




public class SensorsReader : MonoBehaviour
{
    public float _stillDelayS = 0.01f;
    public event Action OnStill;
    public event Action OnMoving;

    public bool isRecordingSteps = false;
    public bool isRecordingStill = false;

    public float stepHighThreshold;
    public float stepLowThreshold;
    public float stepAvg;

    public float stillHighThreshold;
    public UnityEvent<float> OnStillHighThresholdChanged = new UnityEvent<float>();

    public float stillLowThreshold;
    public UnityEvent<float> OnStillLowThresholdChanged = new UnityEvent<float>();

    public float stillAvg;

    
   


    private bool sensorsEnabled = false;


    Vector3 acceleration;
    Vector3 accelerationProjectedXZ;
    Quaternion accelerationArrowLookRotation;
    Vector3 accelerationScaleVector = new Vector3(1, 1, 0.5f);

    Vector3 attitudeEulerProjectedXZ;
    Quaternion attitudeValue;
    Vector3 attitudeValueEuler;




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

     
  
        acceleration = Vector3.zero;
        accelerationProjectedXZ = Vector3.zero;

        attitudeEulerProjectedXZ = Vector3.zero;
        attitudeValueEuler = Vector3.zero;



        lowPassFilterFactor = accelerometerUpdateInterval / lowPassKernelWidthInSeconds;


        if (!sensorsEnabled)
        {
            EnableSensors();

            try

            {

                accelerometerCurrentRawValue = LinearAccelerationSensor.current.acceleration.ReadValue();
                previousAccelerometerValue = accelerometerCurrentRawValue;

            
            }

            catch (Exception e)

            {

                Debug.Log("Error accessing Sensors " + e);

            }

        }
    }

    public void Setup(float stillDelayS, Action OnStillCallback, Action OnMovingCallback)
    {
        _stillDelayS = stillDelayS;
        OnStill += OnStillCallback;
        OnMoving += OnMovingCallback;
    }

    public void StartRecordSteps()
    {
        if(!isRecordingSteps) 
        { 
            isRecordingSteps = true;
            ClearRegisteredData();
        }
        else
        {
            isRecordingSteps = false;
        }
    }    
    public void StartRecordStill()
    {
        if(!isRecordingStill) 
        {
            isRecordingStill = true;
            ClearRegisteredData();
            isCheckingStandingStill = false;
        }
        else
        {
            isRecordingStill = false;
        }
    }

    public void StartCheckStandingStill()
    {
        if(!isCheckingStandingStill)
        {
            isCheckingStandingStill = true;
        }
        else
        {
            OnStopCheckForStill();
            isCheckingStandingStill = false;
        }
    }


    private void AnalyseStillData()
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
            OnStillHighThresholdChanged.Invoke(stillHighThreshold);
            OnStillLowThresholdChanged.Invoke(stillLowThreshold);
            Debug.Log($"Analysis Still Complete: low {stillLowThreshold} - high {stillHighThreshold}");
        }
        else
        {
            Debug.Log($"Analysis Still Error: nothing to analyse");
        }
    }

    private IEnumerator WaitForStill()
    {
        yield return new WaitForSeconds(_stillDelayS);
        // invoke user logic
        Debug.Log($"Player Still for long enough!");
        OnStill.Invoke();
    }

    private void OnStopCheckForStill()
    {
        if(stillCoroutine != null)
        {
            StopCoroutine(stillCoroutine);
        }
    }

    private void CheckStandingStill(Vector3 acceleration)
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
                OnMoving.Invoke();
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
        
        try

        {
            CalculateAccelerometerValue();



            accelerationProjectedXZ.y = -(float)Math.Round(accelerometerCurrentFilteredValue.z, 2);
            accelerationProjectedXZ.z = (float)Math.Round(accelerometerCurrentFilteredValue.y, 2);
            accelerationProjectedXZ.x = (float)Math.Round(accelerometerCurrentFilteredValue.x, 2);
            

            if(isCheckingStandingStill)
            {
                CheckStandingStill(acceleration);
            }

            attitudeValue = AttitudeSensor.current.attitude.ReadValue();

            attitudeValueEuler = attitudeValue.eulerAngles;
            attitudeEulerProjectedXZ.y = -(float)Math.Round(attitudeValueEuler.z, 1);
            attitudeEulerProjectedXZ.z = -(float)Math.Round(attitudeValueEuler.y, 1);
            attitudeEulerProjectedXZ.x = -(float)Math.Round(attitudeValueEuler.x, 1);
                     
        }
        catch (Exception e) 
        {
            Debug.Log("error Update "+ e);
        }
    }

    public void SetStepHighThreshold(float newValue)
    {
        stepHighThreshold = newValue;
    }
    public void SetStepLowThreshold(float newValue)
    {
        stepLowThreshold = newValue;
    }
    public void SetStillHighThreshold(float newValue)
    {
        stillHighThreshold = newValue;
    }
    public void SetStillLowThreshold(float newValue)
    {
        stillLowThreshold = newValue;
    }

    public void OnAccelerometerUpdateIntervalChanged(float newValue)
    {
        accelerometerUpdateInterval = newValue;
        lowPassFilterFactor = accelerometerUpdateInterval / lowPassKernelWidthInSeconds;
    }

    public void OnLowPassKernelWidthInSecondsChanged(float newValue)
    {
        lowPassKernelWidthInSeconds = newValue;
        lowPassFilterFactor = accelerometerUpdateInterval / lowPassKernelWidthInSeconds;
    }

    void EnableSensors()
    {
        //if (Gyroscope.current != null)
        //{
        //    InputSystem.EnableDevice(Gyroscope.current);

        //}
        //if (Accelerometer.current != null)
        //{
        //    InputSystem.EnableDevice(Accelerometer.current);

        //}
        if (AttitudeSensor.current != null)
        {
            InputSystem.EnableDevice(AttitudeSensor.current);

        }
        //if (GravitySensor.current != null)
        //{
        //    InputSystem.EnableDevice(GravitySensor.current);

        //}    
        if (LinearAccelerationSensor.current != null)
        {
            InputSystem.EnableDevice(LinearAccelerationSensor.current);

        }

        if(
            //Gyroscope.current != null && Gyroscope.current.enabled &&
            //Accelerometer.current != null &&  Accelerometer.current.enabled &&
            AttitudeSensor.current != null &&  AttitudeSensor.current.enabled &&
            //GravitySensor.current != null &&  GravitySensor.current.enabled &&
            LinearAccelerationSensor.current != null && LinearAccelerationSensor.current.enabled
           )
        {
            sensorsEnabled = true;
        }
    }
    public void DisableSensors()
    {
        //if (Gyroscope.current != null)
        //{
        //    InputSystem.DisableDevice(Gyroscope.current);

        //}
        //if (Accelerometer.current != null)
        //{
        //    InputSystem.DisableDevice(Accelerometer.current);

        //}
        if (AttitudeSensor.current != null)
        {
            InputSystem.DisableDevice(AttitudeSensor.current);

        }
        //if (GravitySensor.current != null)
        //{
        //    InputSystem.DisableDevice(GravitySensor.current);

        //}    
        if (LinearAccelerationSensor.current != null)
        {
            InputSystem.DisableDevice(LinearAccelerationSensor.current);

        }

        if (
            //Gyroscope.current != null && !Gyroscope.current.enabled &&
            //Accelerometer.current != null &&  !Accelerometer.current.enabled &&
            AttitudeSensor.current != null && !AttitudeSensor.current.enabled &&
            //GravitySensor.current != null &&  !GravitySensor.current.enabled &&
            LinearAccelerationSensor.current != null && !LinearAccelerationSensor.current.enabled
           )
        {
            sensorsEnabled = false;
        }
    }
}