using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;




public class SensorsReader : MonoBehaviour
{
    public event Action OnStill;
    public event Action OnMoving;
    internal event Action<float> OnStillDelayChanged;
    internal event Action<float> OnStillHighThresholdChanged;
    internal event Action<float> OnStillMaxDistanceFromAverageChanged;
    internal event Action<float> OnStillWaveStepDeltaChanged;
    internal event Action<float> OnStepThresholdChanged;
    internal event Action<float> OnAccelerometerFrequencyChanged;
    internal event Action<float> OnStillMovingAverageWindowSizeChanged;
    internal event Action<float> OnAccelerometerUpdateIntervalChanged;
    internal event Action<float> OnLowPassKernelWidthInSecondsChanged;
    internal event Action<float, float> OnStateMachineStepDetected
    {
        add {
            if (_waveStateController != null)
            {
                _waveStateController.OnStepDetected += value;
            }
            else
            {
                Debug.Log("OnStateMachineStepDetected ADD warning : state machine is null");
            }
            _onStateMachineStepDetected += value;
        }
        remove {
            if (_waveStateController != null)
            {
                _waveStateController.OnStepDetected -= value;
            }
            else
            {
                Debug.Log("OnStateMachineStepDetected REMOVE error : state machine is null");
            }
            _onStateMachineStepDetected -= value;

        }
    }
    private event Action<float, float> _onStateMachineStepDetected;

    public float StillDelayS
    {
        get => _stillDelayS;
        set
        {
            _stillDelayS = value;
            if (OnStillDelayChanged != null)
                OnStillDelayChanged.Invoke(value);
        }
    }
    private float _stillDelayS;

    private bool isRecordingSteps = false;
    public bool IsRecordingStill
    {
        get => _isRecordingStill;
        set
        {
            if (value)
            {
                ClearRegisteredData();
            }
            _isRecordingStill = value;
        }
    }
    private bool _isRecordingStill = false;

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

    public float StepThreshold
    {
        get
        {
            if (IsStepRecognitionMachineEnabled && WaveStateController != null)
            {
                return WaveStateController.StepThreshold;
            }
            else
            {
                //Debug.Log("GetStepThreshold ERROR- step recognition machine is disabled");
                return -1;
            }
        }
        set
        {
            if (IsStepRecognitionMachineEnabled && WaveStateController != null)
            {
                WaveStateController.StepThreshold = (int)value;
                if (OnStepThresholdChanged != null)
                    OnStepThresholdChanged.Invoke((int)value);
            }
            else
            {
                Debug.Log("SetStepThreshold ERROR- step recognition machine is disabled");
            }
        }
    }
    public float StillHighThreshold
    {
        get => _stillHighThreshold;
        set => _stillHighThreshold = value;
    }
    private float _stillHighThreshold;

    public float StillAvg
    {
        get => _stillAvg;
    }
    private float _stillAvg;
    public float StillMovingAverageWindowSize
    {
        get => _stillMovAvgSize;
        set {
            _stillMovAvgSize = (int)value;
            if (OnStillMovingAverageWindowSizeChanged != null)
                OnStillMovingAverageWindowSizeChanged.Invoke((int)value);
        }
    }
    private int _stillMovAvgSize;
    private float _stillMovSum;
    private Queue<float> _stillMovAvgData;
    public float StillMovingAvg
    {
        get => _stillMovAvg;
    }
    private float _stillMovAvg;
    public float StillMaxDistanceBetweenAverages
    {
        get => _stillMaxDistAvg;
        set => _stillMaxDistAvg = value;
    }
    private float _stillMaxDistAvg;

    public float StillWaveStepDelta
    {
        get => _stillWaveStepDelta;
        set
        {
            _stillWaveStepDelta = value;
            if (OnStillWaveStepDeltaChanged != null)
                OnStillWaveStepDeltaChanged.Invoke(value);
        }
    }
    private float _stillWaveStepDelta;

    public bool IsWaveStepDeltaCheckActive
    {
        get
        {
            if (IsStepRecognitionMachineEnabled && WaveStateController != null)
            {
                return WaveStateController.IsWaveStepDeltaCheckActive;
            }
            else
            {
                //Debug.Log("GetWaveDeltaStepCheck ERROR- step recognition machine is disabled");
                return false;
            }
        }
        set
        {
            if (IsStepRecognitionMachineEnabled && WaveStateController != null)
            {
                WaveStateController.IsWaveStepDeltaCheckActive = value;
            }
            else
            {
                Debug.Log("SetWaveDeltaStepCheck ERROR- step recognition machine is disabled");
            }
        }
    }


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

    public float AccelerationFilteredMagnitude
    {
        get => _currentAccelerationFilteredMagnitude;
    }
    private float _currentAccelerationFilteredMagnitude;
    public Vector3 AccelerationFilteredProjectedXZ
    {
        get => _currentAccelerationFilteredProjectedXZ;
    }
    private Vector3 _currentAccelerationFilteredProjectedXZ;

    public Vector3 PreviousAccelerationFiltered
    {
        get => _previousAccelerationFiltered;
    }
    private Vector3 _previousAccelerationFiltered;

    public float PreviousAccelerationFilteredMagnitude
    {
        get => _previousAccelerationFilteredMagnitude;
    }
    private float _previousAccelerationFilteredMagnitude;

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
            if (OnAccelerometerUpdateIntervalChanged != null)
                OnAccelerometerUpdateIntervalChanged.Invoke(value);
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
            if (OnLowPassKernelWidthInSecondsChanged != null)
                OnLowPassKernelWidthInSecondsChanged.Invoke(value);
        }
    }
    private float _lowPassKernelWidthInSeconds;

    private float _lowPassFilterFactor;


    private Stack<Vector3> _accelerationFilteredValues = new Stack<Vector3>();
    private Stack<Vector3> _accelerationRawValues = new Stack<Vector3>();
    private Stack<float> _accelerationMagnitudeRawValues = new Stack<float>();
    private Stack<float> _accelerationMagnitudeFilteredValues = new Stack<float>();


    private Coroutine _stillCoroutine;
    public bool IsCheckingStandingStill
    {
        get => _isCheckingStandingStill;
        set
        {
            _isCheckingStandingStill = value;
            if (value == false)
            {
                OnStopCheckForStill();
            }
        }
    }
    private bool _isCheckingStandingStill = false;
    private bool _hasStartedWaitingForStill = false;

    public WaveStateController WaveStateController
    {
        get => _waveStateController;
    }
    private WaveStateController _waveStateController;

    public bool IsStepRecognitionMachineEnabled
    {
        get => _isStepRecognitionMachineEnabled;
        set
        {
            if(value && _waveStateController == null)
            {
                _waveStateController = new WaveStateController(this);
                if(_onStateMachineStepDetected != null)
                {
                    _waveStateController.OnStepDetected += _onStateMachineStepDetected;
                }
            }
            else if(!value)
            {
                _waveStateController = null;
            }
            _isStepRecognitionMachineEnabled = value;
        }
    }
    private bool _isStepRecognitionMachineEnabled;

    public bool IsMaxDistanceBetweenAveragesEnabled
    {
        get => _isMaxDistanceBetweenAveragesEnabled;
        set => _isMaxDistanceBetweenAveragesEnabled = value;
    }
    private bool _isMaxDistanceBetweenAveragesEnabled;

    public bool IsStillHighThresholdEnabled
    {
        get => _isStillHighThresholdEnabled;
        set => _isStillHighThresholdEnabled = value;
    }
    private bool _isStillHighThresholdEnabled;

    public float AccelerometerFrequency
    {
        get => LinearAccelerationSensor.current.samplingFrequency;
        set
        {
            LinearAccelerationSensor.current.samplingFrequency = value;
            if (OnAccelerometerFrequencyChanged != null)
                OnAccelerometerFrequencyChanged.Invoke(value);
        }
    }

    void Start()
    {

      
    }

    public void Setup(float stillDelayS, Action OnStillCallback, Action OnMovingCallback)
    {
        _currentAccelerationFiltered = Vector3.zero;
        _currentAccelerationRaw = Vector3.zero;
        _currentAccelerationFilteredProjectedXZ = Vector3.zero;
        _currentAccelerationFilteredProjectedXZ = Vector3.zero;

        _attitudeEulerProjectedXZ = Vector3.zero;
        _attitudeValueEuler = Vector3.zero;

        _stillMovAvg = 0f;
        _stillAvg = 0f;
        _stillMovAvgData = new Queue<float>();

        _lowPassFilterFactor = _accelerometerUpdateInterval / _lowPassKernelWidthInSeconds;

        //_waveStateController = new WaveStateController(this);
        _stillMovAvgSize = 60; //at 60hz it's 1/2 secs worth of data



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

    public void AnalyseStillData()
    {
        if(_accelerationMagnitudeFilteredValues.Count > 0)
        {
            _stillHighThreshold = 0f;
            _stillAvg = 0f;
            for (int i = 0; i < _accelerationMagnitudeFilteredValues.Count; i++)
            {
                var current = _accelerationMagnitudeFilteredValues.Pop();
                if (current > _stillHighThreshold)
                {
                    _stillHighThreshold = current;
                }
                _stillAvg += current;
            }
            if(OnStillHighThresholdChanged != null)
            {
                OnStillHighThresholdChanged.Invoke(_stillHighThreshold);
            }
            _stillAvg = (float)Math.Round(_stillAvg / _accelerationMagnitudeFilteredValues.Count, 3);
            PrepareRunningAverage(_stillAvg);
            _stillMaxDistAvg = (float)Math.Round(_stillAvg + (_stillHighThreshold - _stillAvg) * 0.75f, 3);
            OnStillMaxDistanceFromAverageChanged(_stillMaxDistAvg);
            Debug.Log($"Analysis Still Complete: high {_stillHighThreshold} - _stillAvg {_stillAvg}");
            _accelerationMagnitudeFilteredValues.Clear();
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
        if(OnStill != null)
        {
            OnStill.Invoke();
        }
    }

    private void OnStopCheckForStill()
    {
        if (_stillCoroutine != null)
        {
            StopCoroutine(_stillCoroutine);
        }
    }
    private void PrepareRunningAverage(float value)
    {
        _stillMovSum = 0;
        _stillMovAvgData.Clear();
        for (int i=0; i< _stillMovAvgSize; i++)
        {
            _stillMovAvgData.Enqueue(value);
            _stillMovSum += value;
        }
        _stillMovAvg = (float)Math.Round(_stillMovSum / _stillMovAvgSize, 3);
    }
    private void CalculateRunningAverage(float newValue)
    {
        _stillMovSum -= _stillMovAvgData.Dequeue();
        _stillMovAvgData.Enqueue(newValue);
        _stillMovSum += newValue;
        _stillMovAvg = (float)Math.Round(_stillMovSum / _stillMovAvgSize, 3);
    }
   

    private void CheckStandingStill(float accelerationMagnitude)
    {
        CalculateRunningAverage(accelerationMagnitude);
        //TODO convert to sqrMagnitude for better performances
        if (IsStepRecognitionMachineEnabled && _waveStateController != null)
        {
            _waveStateController.RunState();
        }
        if (
            _isStillHighThresholdEnabled && accelerationMagnitude > _stillHighThreshold
            || _isMaxDistanceBetweenAveragesEnabled && _stillMovAvg - _stillAvg > _stillMaxDistAvg
            || _isStepRecognitionMachineEnabled && _waveStateController != null && _waveStateController.HasStep()
            )
        {
            if (_hasStartedWaitingForStill)
            {
                _hasStartedWaitingForStill = false;
                if (_stillCoroutine != null)
                {
                    StopCoroutine(_stillCoroutine);
                }
                if(OnMoving != null)
                {
                    OnMoving.Invoke();
                }
            }
        }
        else
        {
            if (!_hasStartedWaitingForStill)
            {
                _hasStartedWaitingForStill = true;
                _stillCoroutine = StartCoroutine(WaitForStill());
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
        return Vector3.Lerp(prevValue, currentValue, _lowPassFilterFactor);
    }

    void CalculateAccelerometerValue()

    {

        _currentAccelerationRaw = LinearAccelerationSensor.current.acceleration.ReadValue();

        _currentAccelerationFiltered = GetLowPassValue(_currentAccelerationRaw, _previousAccelerationFiltered);
        _currentAccelerationFiltered.x = (float)Math.Round(_currentAccelerationFiltered.x, 3);
        _currentAccelerationFiltered.y = (float)Math.Round(_currentAccelerationFiltered.y, 3);
        _currentAccelerationFiltered.z = (float)Math.Round(_currentAccelerationFiltered.z, 3);

        _currentAccelerationFilteredMagnitude = (float)Math.Round(_currentAccelerationFiltered.magnitude, 3);

        _previousAccelerationFiltered = _currentAccelerationFiltered;

        _currentAccelerationFilteredProjectedXZ.y = (float)Math.Round(_currentAccelerationFiltered.z, 3);
        _currentAccelerationFilteredProjectedXZ.z = (float)Math.Round(_currentAccelerationFiltered.y, 3);
        _currentAccelerationFilteredProjectedXZ.x = (float)Math.Round(_currentAccelerationFiltered.x, 3);

        if (isRecordingSteps || _isRecordingStill)
        {
            _accelerationRawValues.Push(_currentAccelerationRaw);
            _accelerationFilteredValues.Push(_currentAccelerationFiltered);
            _accelerationMagnitudeRawValues.Push(_currentAccelerationRaw.magnitude);
            _accelerationMagnitudeFilteredValues.Push(_currentAccelerationFiltered.magnitude);
        }
    }

    void Update()
    {

        if (sensorsEnabled)
        {
            CalculateAccelerometerValue();

            if (_isCheckingStandingStill)
            {
                CheckStandingStill(_currentAccelerationFilteredMagnitude);
            }

            CalculateAttitude();
            _previousAccelerationFilteredMagnitude = _currentAccelerationFilteredMagnitude;
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








    public WaveState GetCurrentWaveState()
    {
        if (IsStepRecognitionMachineEnabled && WaveStateController != null)
        {
            return WaveStateController.CurrentState;
        }
        else
        {
            //Debug.Log("GetCurrentWaveState ERROR- step recognition machine is disabled");
            return null;
        }
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
        //if (GravitySensor.current != null)
        //{
        //    InputSystem.EnableDevice(GravitySensor.current);

        //}    
        if (AttitudeSensor.current != null)
        {
            InputSystem.EnableDevice(AttitudeSensor.current);

        }
        if (LinearAccelerationSensor.current != null)
        {
            InputSystem.EnableDevice(LinearAccelerationSensor.current);
            Debug.Log("ACCELERATION SAMPLING FREQ IS" + LinearAccelerationSensor.current.samplingFrequency);
        }

        if (
            //Gyroscope.current != null && Gyroscope.current.enabled &&
            //Accelerometer.current != null &&  Accelerometer.current.enabled &&
            //GravitySensor.current != null &&  GravitySensor.current.enabled &&
            AttitudeSensor.current != null && AttitudeSensor.current.enabled &&
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
        //if (GravitySensor.current != null)
        //{
        //    InputSystem.DisableDevice(GravitySensor.current);

        //}    
        if (AttitudeSensor.current != null)
        {
            InputSystem.DisableDevice(AttitudeSensor.current);

        }
        if (LinearAccelerationSensor.current != null)
        {
            InputSystem.DisableDevice(LinearAccelerationSensor.current);

        }

        if (
            //Gyroscope.current != null && !Gyroscope.current.enabled &&
            //Accelerometer.current != null &&  !Accelerometer.current.enabled &&
            //GravitySensor.current != null &&  !GravitySensor.current.enabled &&
            AttitudeSensor.current != null && !AttitudeSensor.current.enabled &&
            LinearAccelerationSensor.current != null && !LinearAccelerationSensor.current.enabled
           )
        {
            sensorsEnabled = false;
        }
    }
}