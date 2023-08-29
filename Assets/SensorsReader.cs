using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;




public class SensorsReader : MonoBehaviour
{
    public float StillDelayS
    {
        get => _stillDelayS;
        set => _stillDelayS = value;
    }
    private float _stillDelayS = 0.01f;
    private event Action OnStill;
    private event Action OnMoving;
    internal event Action<float> OnStillHighThresholdChanged;

    private bool isRecordingSteps = false;
    private bool isRecordingStill = false;

    // TODO property or explicit setter?
    public float StepHighThreshold
    {
        get => _stepHighThreshold;
        set => _stepHighThreshold = value;
    }
    private float _stepHighThreshold;
    public float StepLowThreshold
    {
        get => _stepLowThreshold;
        set => _stepLowThreshold = value;
    }
    private float _stepLowThreshold;

    public float StillHighThreshold
    {
        get => _stillHighThreshold; 
        set => _stillHighThreshold = value;
    }
    private float _stillHighThreshold;

    private bool sensorsEnabled = false;

    public Vector3 AccelerationRaw
    {
        get => _currentAccelerationRaw;
    }
    private Vector3 _currentAccelerationRaw;
    public Vector3 AccelerationFiltered
    {
        get => _currentAccelerationFiltered;
    }
    private Vector3 _currentAccelerationFiltered;
    public Vector3 AccelerationFilteredProjectedXZ 
    { 
        get => _currentAccelerationFilteredProjectedXZ; 
    }
    private Vector3 _currentAccelerationFilteredProjectedXZ;

    private Vector3 _previousAccelerationFiltered;

    public Quaternion Attitude
    {
        get => _attitude;
    }
    Quaternion _attitude;

    public Vector3 AttitudeEulerProjectedXZ
    {
        get => _attitudeEulerProjectedXZ;
    }
    Vector3 _attitudeEulerProjectedXZ;
    private Vector3 _attitudeValueEuler;



    public float AccelerometerUpdateInterval
    {
        get => _accelerometerUpdateInterval;
        set
        {
            _accelerometerUpdateInterval = value;
            _lowPassFilterFactor = _accelerometerUpdateInterval / _lowPassKernelWidthInSeconds;
        }
    }
    private float _accelerometerUpdateInterval;

    public float LowPassKernelWidthInSeconds
    {
        get => _lowPassKernelWidthInSeconds;
        set
        {
            _lowPassKernelWidthInSeconds = value;
            _lowPassFilterFactor = _accelerometerUpdateInterval / _lowPassKernelWidthInSeconds;
        }
    }
    private float _lowPassKernelWidthInSeconds;

    private float _lowPassFilterFactor;


    private Stack<Vector3> _accelerationFilteredValues = new Stack<Vector3>();
    private Stack<Vector3> _accelerationRawValues = new Stack<Vector3>();
    private Stack<float> _accelerationMagnitudeRawValues = new Stack<float>();
    private Stack<float> _accelerationMagnitudeFilteredValues = new Stack<float>();


    private Coroutine _stillCoroutine;
    private bool _isCheckingStandingStill = false;
    private bool _hasStartedWaitingForStill = false;

    void Start()
    {

     
  
        _currentAccelerationFiltered = Vector3.zero;
        _currentAccelerationRaw = Vector3.zero;
        _currentAccelerationFilteredProjectedXZ = Vector3.zero;

        _attitudeEulerProjectedXZ = Vector3.zero;
        _attitudeValueEuler = Vector3.zero;



        _lowPassFilterFactor = _accelerometerUpdateInterval / _lowPassKernelWidthInSeconds;


       
    }

    public void Setup(float stillDelayS, Action OnStillCallback, Action OnMovingCallback)
    {
        //TODO use param class?

        _stillDelayS = stillDelayS;
        OnStill += OnStillCallback;
        OnMoving += OnMovingCallback;

        if (!sensorsEnabled)
        {
            try
            {
                EnableSensors();
                _currentAccelerationRaw = LinearAccelerationSensor.current.acceleration.ReadValue();
                _previousAccelerationFiltered = _currentAccelerationRaw;
            }
            catch (Exception e)
            {
                sensorsEnabled = false;
                Debug.Log("Error accessing Sensors " + e);
            }
        }
    }


    public void SetStepRecorder(bool status)
    {
        if(status)
        {
            ClearRegisteredData();
        }
        isRecordingSteps = status;
    }    
    public void SetStillRecorder(bool status)
    {
        if (status)
        {
            ClearRegisteredData();
        }
        isRecordingStill = status;
    }

    public void SetStandingStillRecognition(bool status)
    {

        _isCheckingStandingStill = status;
        if(status == false)
        {
            OnStopCheckForStill();
        }
    }


    public void AnalyseStillData()
    {
        if(_accelerationMagnitudeFilteredValues.Count > 0)
        {
            _stillHighThreshold = 0f;
            for (int i = 0; i<_accelerationMagnitudeFilteredValues.Count; i++)
            {
                var current = _accelerationMagnitudeFilteredValues.Pop();
                if(current > _stillHighThreshold)
                { 
                    _stillHighThreshold = current;
                }
            }
            OnStillHighThresholdChanged.Invoke(_stillHighThreshold);
            Debug.Log($"Analysis Still Complete: high {_stillHighThreshold}");
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
        if(_stillCoroutine != null)
        {
            StopCoroutine(_stillCoroutine);
        }
    }

    private void CheckStandingStill(Vector3 acceleration)
    {
        //TODO convert to sqrMagnitude for better performances
        if(acceleration.magnitude < _stillHighThreshold)
        {
            if(!_hasStartedWaitingForStill)
            {
                _hasStartedWaitingForStill = true;
                _stillCoroutine = StartCoroutine(WaitForStill());
            }
        }
        else
        {
            if(_hasStartedWaitingForStill)
            {
                _hasStartedWaitingForStill = false;
                if (_stillCoroutine != null)
                {
                    StopCoroutine(_stillCoroutine);
                }
                OnMoving.Invoke();
            }
        }
    }

    private void ClearRegisteredData()
    {
        _accelerationMagnitudeFilteredValues.Clear();
        _accelerationMagnitudeRawValues.Clear();
        _accelerationFilteredValues.Clear();
        _accelerationRawValues.Clear();
    }
    Vector3 GetLowPassValue(Vector3 currentValue, Vector3 prevValue)

    {

        //Debug.Log($"Low pass: Prev {prevValue} to current {currentValue}");

        return Vector3.Lerp(prevValue, currentValue, _lowPassFilterFactor);

    }

    void CalculateAccelerometerValue()

    {

        _currentAccelerationRaw = LinearAccelerationSensor.current.acceleration.ReadValue();

        _currentAccelerationFiltered = GetLowPassValue(_currentAccelerationRaw, _previousAccelerationFiltered);

        _previousAccelerationFiltered = _currentAccelerationFiltered;

        _currentAccelerationFilteredProjectedXZ.y = (float)Math.Round(_currentAccelerationFiltered.z, 2);
        _currentAccelerationFilteredProjectedXZ.z = (float)Math.Round(_currentAccelerationFiltered.y, 2);
        _currentAccelerationFilteredProjectedXZ.x = (float)Math.Round(_currentAccelerationFiltered.x, 2);

        if (isRecordingSteps || isRecordingStill)
        {
            _accelerationRawValues.Push(_currentAccelerationRaw);
            _accelerationFilteredValues.Push(_currentAccelerationFiltered);
            _accelerationMagnitudeRawValues.Push(_currentAccelerationRaw.magnitude);
            _accelerationMagnitudeFilteredValues.Push(_currentAccelerationFiltered.magnitude);
        }
                                                          
    }

    void Update() 
    {
        
            if(sensorsEnabled)
            {
                CalculateAccelerometerValue();

                if(_isCheckingStandingStill)
                {
                    CheckStandingStill(_currentAccelerationFiltered);
                }

                CalculateAttitude();
            }
    }

    public void CalculateAttitude()
    {
        _attitude = AttitudeSensor.current.attitude.ReadValue();

        _attitudeValueEuler = _attitude.eulerAngles;
        _attitudeEulerProjectedXZ.y = -(float)Math.Round(_attitudeValueEuler.z, 1);
        _attitudeEulerProjectedXZ.z = -(float)Math.Round(_attitudeValueEuler.y, 1);
        _attitudeEulerProjectedXZ.x = -(float)Math.Round(_attitudeValueEuler.x, 1);

    }



    public void SetAccelerometerUpdateIntervalChanged(float newValue)
    {
        _accelerometerUpdateInterval = newValue;
        _lowPassFilterFactor = _accelerometerUpdateInterval / _lowPassKernelWidthInSeconds;
    }

    public void SetLowPassKernelWidthInSecondsChanged(float newValue)
    {
        _lowPassKernelWidthInSeconds = newValue;
        _lowPassFilterFactor = _accelerometerUpdateInterval / _lowPassKernelWidthInSeconds;
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