using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;




public class SensorsReader : MonoBehaviour
{
    #region CallBacks
    internal event Action OnStill;
    private Coroutine _stillCoroutine;
    internal event Action OnMoving;
    internal event Action<float> OnDelayForStillChanged;
    internal event Action<float> OnHighThresholdChanged;
    internal event Action<float> OnMaxDistanceBetweenAveragesChanged;
    internal event Action<float> OnMaxWaveAmplitudeChanged;
    internal event Action<float> OnNumberOfPeaksForAStepChanged;
    internal event Action<float> OnAccelerometerFrequencyChanged;
    internal event Action<float> OnMovingAverageWindowSizeChanged;
    internal event Action<float> OnAccelerometerUpdateIntervalChanged;
    internal event Action<float> OnLowPassKernelWidthInSecondsChanged;
    internal event Action<float, float> OnStateMachineStepDetected
    {
        add {
            _onStateMachineStepDetected += value;
            if (_stepRecognitionMachine != null)
                _stepRecognitionMachine.OnStepDetected += value;
        }
        remove {
            _onStateMachineStepDetected -= value;
            if (_stepRecognitionMachine != null)
                _stepRecognitionMachine.OnStepDetected -= value;
        }
    }
    private event Action<float, float> _onStateMachineStepDetected;
    #endregion
    #region State Machine
    public StepRecognitionMachine StepRecognitionMachine
    {
        get => _stepRecognitionMachine;
    }
    private StepRecognitionMachine _stepRecognitionMachine;

    public bool IsStepRecognitionMachineEnabled
    {
        get => _isStepRecognitionMachineEnabled;
        set
        {
            if (value && _stepRecognitionMachine == null)
            {
                _stepRecognitionMachine = new StepRecognitionMachine(this);
                if (_onStateMachineStepDetected != null)
                {
                    _stepRecognitionMachine.OnStepDetected += _onStateMachineStepDetected;
                }
                _stepRecognitionMachine.NumberOfPeaksForAStep = (int)NumberOfPeaksForAStep;
                _stepRecognitionMachine.IsWaveAmplitudeCheckActive = IsWaveAmplitudeCheckActive;

            }
            else if (!value)
            {
                _stepRecognitionMachine = null;
            }
            _isStepRecognitionMachineEnabled = value;
        }
    }
    private bool _isStepRecognitionMachineEnabled;

    public float NumberOfPeaksForAStep
    {
        get => _numberOfPeaksForAStep;
        set
        {
            _numberOfPeaksForAStep = value;
            if (OnNumberOfPeaksForAStepChanged != null)
                OnNumberOfPeaksForAStepChanged.Invoke(value);

            if (_stepRecognitionMachine != null)
                _stepRecognitionMachine.NumberOfPeaksForAStep = (int)value;
        }
    }
    private float _numberOfPeaksForAStep;

    public WaveState CurrentWaveState
    {
        get
        {
            if (IsStepRecognitionMachineEnabled && StepRecognitionMachine != null)
            {
                return _stepRecognitionMachine.CurrentState;
            }
            else
            {
                //Debug.Log("GetCurrentWaveState ERROR- step recognition machine is disabled");
                return null;
            }
        }
    }

    public bool IsWaveAmplitudeCheckActive
    {
        get => _isWaveAmplitudeCheckActive;
        set
        {
            _isWaveAmplitudeCheckActive = value;
            if (_stepRecognitionMachine != null)
                _stepRecognitionMachine.IsWaveAmplitudeCheckActive = value;
        }
    }
    private bool _isWaveAmplitudeCheckActive;
    #endregion
    #region Moving Average
    public float StillAverage
    {
        get => _stillAverage;
    }
    private float _stillAverage;
    public float MovingAverageWindowSize
    {
        get => _movingAverageWindowSize;
        set {
            _movingAverageWindowSize = (int)value;
            if (OnMovingAverageWindowSizeChanged != null)
                OnMovingAverageWindowSizeChanged.Invoke((int)value);
        }
    }
    private int _movingAverageWindowSize;
    
    private float _movingSum;
    private Queue<float> _movingAverageData;
    public float MovingAverage
    {
        get => _movingAverage;
    }
    private float _movingAverage;
    #endregion
    #region Thresholds and values
    public float MaxDistanceBetweenAverages
    {
        get => _maxDistanceBetweenAverages;
        set
        {
            _maxDistanceBetweenAverages = value;
            if (OnMaxDistanceBetweenAveragesChanged != null)
                OnMaxDistanceBetweenAveragesChanged.Invoke(value);
        }
    }
    private float _maxDistanceBetweenAverages;
    public float DefaultMaxDistanceFromStillAverage
    {
        get => _defaultMaxDistanceFromStillAverage;
        set => _defaultMaxDistanceFromStillAverage = value;
    }
    private float _defaultMaxDistanceFromStillAverage = 0.75f;
    public float MaxWaveAmplitude
    {
        get => _maxWaveAmplitude;
        set
        {
            _maxWaveAmplitude = value;
            if (OnMaxWaveAmplitudeChanged != null)
                OnMaxWaveAmplitudeChanged.Invoke(value);
        }
    }
    private float _maxWaveAmplitude;
    public float HighThreshold
    {
        get => _highThreshold;
        set
        {
            _highThreshold = value;
            if (OnHighThresholdChanged != null)
                OnHighThresholdChanged.Invoke(value);
        }
    }
    private float _highThreshold;
    public float DelayForStill_S
    {
        get => _delayForStill_S;
        set
        {
            _delayForStill_S = value;
            if (OnDelayForStillChanged != null)
                OnDelayForStillChanged.Invoke(value);
        }
    }
    private float _delayForStill_S;
    #endregion
    #region Sensor Values
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

    private Stack<Vector3> _accelerationFilteredValues = new Stack<Vector3>();
    private Stack<Vector3> _accelerationRawValues = new Stack<Vector3>();
    private Stack<float> _accelerationMagnitudeRawValues = new Stack<float>();
    private Stack<float> _accelerationMagnitudeFilteredValues = new Stack<float>();
    #endregion
    #region State Variables
    public bool IsRecording
    {
        get => _isRecording;
        set
        {
            if (value)
            {
                ClearRegisteredData();
            }
            _isRecording = value;
        }
    }
    private bool _isRecording = false;
    public bool IsCheckingStill
    {
        get => _isCheckingStill;
        set
        {
            _isCheckingStill = value;
            if (value == false)
            {
                OnStopCheckForStill();
            }
        }
    }
    private bool _isCheckingStill = false;
    private bool _hasStartedWaitingForStill = false;

    public bool IsMaxDistanceBetweenAveragesEnabled
    {
        get => _isMaxDistanceBetweenAveragesEnabled;
        set => _isMaxDistanceBetweenAveragesEnabled = value;
    }
    private bool _isMaxDistanceBetweenAveragesEnabled;

    public bool IsHighThresholdEnabled
    {
        get => _isHighThresholdEnabled;
        set => _isHighThresholdEnabled = value;
    }
    private bool _isHighThresholdEnabled;
    #endregion

    public void SetupAndStartSensors(float stillDelayS, Action OnStillCallback, Action OnMovingCallback, SensorsReaderOptions? sensorsReaderOptions)
    {
        _currentAccelerationFiltered = Vector3.zero;
        _currentAccelerationRaw = Vector3.zero;
        _currentAccelerationFilteredProjectedXZ = Vector3.zero;
        _currentAccelerationFilteredProjectedXZ = Vector3.zero;
        _attitudeEulerProjectedXZ = Vector3.zero;
        _attitudeValueEuler = Vector3.zero;
        _movingAverage = 0f;
        _stillAverage = 0f;
        _movingAverageData = new Queue<float>();

        _lowPassFilterFactor = _accelerometerUpdateInterval / _lowPassKernelWidthInSeconds;

        DelayForStill_S = stillDelayS;
        OnStill += OnStillCallback;
        OnMoving += OnMovingCallback;

        SensorsReaderOptions options = sensorsReaderOptions ?? new SensorsReaderOptions();

        IsStepRecognitionMachineEnabled = options.IsStepRecognitionMachineEnabled;
        MaxWaveAmplitude = options.MaxWaveAmplitude;
        IsWaveAmplitudeCheckActive = options.IsWaveAmplitudeCheckActive;
        NumberOfPeaksForAStep = options.NumberOfPeaksForAStep;

        IsMaxDistanceBetweenAveragesEnabled = options.IsMaxDistanceBetweenAveragesEnabled;
        MaxDistanceBetweenAverages = options.MaxDistanceBetweenAverages;

        IsHighThresholdEnabled = options.IsHighThresholdEnabled;
        HighThreshold = options.HighThreshold;

        AccelerometerFrequency = options.AccelerometerFrequency;
        MovingAverageWindowSize = options.MovingAverageWindowSize;
        AccelerometerUpdateInterval = options.AccelerometerUpdateInterval;
        LowPassKernelWidthInSeconds = options.LowPassKernelWidthInSeconds;

        if (!sensorsEnabled)
        {
            try
            {
                EnableSensors();
                _currentAccelerationRaw = LinearAccelerationSensor.current.acceleration.ReadValue();
                _previousAccelerationFiltered = _currentAccelerationRaw;
                PrepareRunningAverage(_currentAccelerationRaw.magnitude);
            }
            catch (Exception e)
            {
                sensorsEnabled = false;
                Debug.Log("Error accessing Sensors " + e);
            }
        }
    }
    void Update()
    {

        if (sensorsEnabled)
        {
            CalculateAccelerometerValue();
            CalculateRunningAverage(_currentAccelerationFilteredMagnitude);
            if (_isCheckingStill)
            {
                CheckStill(_currentAccelerationFilteredMagnitude);
            }

            //CalculateAttitude();
            _previousAccelerationFilteredMagnitude = _currentAccelerationFilteredMagnitude;
        }
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

        if (_isRecording)
        {
            _accelerationRawValues.Push(_currentAccelerationRaw);
            _accelerationFilteredValues.Push(_currentAccelerationFiltered);
            _accelerationMagnitudeRawValues.Push(_currentAccelerationRaw.magnitude);
            _accelerationMagnitudeFilteredValues.Push(_currentAccelerationFiltered.magnitude);
        }
    }
    Vector3 GetLowPassValue(Vector3 currentValue, Vector3 prevValue)

    {
        return Vector3.Lerp(prevValue, currentValue, _lowPassFilterFactor);
    }
    public void CalculateAttitude()
    {
        _attitude = AttitudeSensor.current.attitude.ReadValue();

        _attitudeValueEuler = _attitude.eulerAngles;
        _attitudeEulerProjectedXZ.y = -(float)Math.Round(_attitudeValueEuler.z, 1);
        _attitudeEulerProjectedXZ.z = -(float)Math.Round(_attitudeValueEuler.y, 1);
        _attitudeEulerProjectedXZ.x = -(float)Math.Round(_attitudeValueEuler.x, 1);

    }
    
    public void AnalyseData()
    {
        if (_accelerationMagnitudeFilteredValues.Count > 0)
        {
            _highThreshold = 0f;
            _stillAverage = 0f;
            for (int i = 0; i < _accelerationMagnitudeFilteredValues.Count; i++)
            {
                var current = _accelerationMagnitudeFilteredValues.Pop();
                if (current > _highThreshold)
                {
                    _highThreshold = current;
                }
                _stillAverage += current;
            }
            if (OnHighThresholdChanged != null)
                OnHighThresholdChanged.Invoke(_highThreshold);

            _stillAverage = (float)Math.Round(_stillAverage / _accelerationMagnitudeFilteredValues.Count, 3);

            //PrepareRunningAverage(_stillAverage);

            _maxDistanceBetweenAverages = (float)Math.Round(_stillAverage + (_highThreshold - _stillAverage) * _defaultMaxDistanceFromStillAverage, 3);
            if (OnMaxDistanceBetweenAveragesChanged != null)
                OnMaxDistanceBetweenAveragesChanged(_maxDistanceBetweenAverages);

            Debug.Log($"Analysis Still Complete: high {_highThreshold} - _stillAverage {_stillAverage}");
            _accelerationMagnitudeFilteredValues.Clear();
        }
        else
        {
            Debug.Log($"Analysis Still Error: nothing to analyse");
        }
    }
    private void PrepareRunningAverage(float value)
    {
        _movingSum = 0;
        _movingAverageData.Clear();
        for (int i = 0; i < _movingAverageWindowSize; i++)
        {
            _movingAverageData.Enqueue(value);
            _movingSum += value;
        }
        _movingAverage = (float)Math.Round(_movingSum / _movingAverageWindowSize, 3);
    }
    private void CalculateRunningAverage(float newValue)
    {
        _movingSum -= _movingAverageData.Dequeue();
        _movingAverageData.Enqueue(newValue);
        _movingSum += newValue;
        _movingAverage = (float)Math.Round(_movingSum / _movingAverageWindowSize, 3);
    }

    private void CheckStill(float accelerationMagnitude)
    {
        //CalculateRunningAverage(accelerationMagnitude);
        //TODO convert to sqrMagnitude for better performances
        if (IsStepRecognitionMachineEnabled && _stepRecognitionMachine != null)
        {
            _stepRecognitionMachine.RunState();
        }
        if (
            _isHighThresholdEnabled && accelerationMagnitude > _highThreshold
            || _isMaxDistanceBetweenAveragesEnabled && _movingAverage - _stillAverage > _maxDistanceBetweenAverages
            || _isStepRecognitionMachineEnabled && _stepRecognitionMachine != null && _stepRecognitionMachine.HasStep()
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

    private IEnumerator WaitForStill()
    {
        yield return new WaitForSeconds(_delayForStill_S);
        // invoke user logic
        Debug.Log($"Player Still for long enough!");
        if (OnStill != null)
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
 
    private void ClearRegisteredData()
    {
        _accelerationMagnitudeFilteredValues.Clear();
        _accelerationMagnitudeRawValues.Clear();
        _accelerationFilteredValues.Clear();
        _accelerationRawValues.Clear();
    }
    void EnableSensors()
    {
        //if (Gyroscope.current != null)
        //    InputSystem.EnableDevice(Gyroscope.current);
        //if (Accelerometer.current != null)
        //    InputSystem.EnableDevice(Accelerometer.current);
        //if (GravitySensor.current != null)
        //    InputSystem.EnableDevice(GravitySensor.current);
        //if (AttitudeSensor.current != null)
        //    InputSystem.EnableDevice(AttitudeSensor.current);
        if (LinearAccelerationSensor.current != null)
            InputSystem.EnableDevice(LinearAccelerationSensor.current);

        if (
            //Gyroscope.current != null && Gyroscope.current.enabled &&
            //Accelerometer.current != null &&  Accelerometer.current.enabled &&
            //GravitySensor.current != null &&  GravitySensor.current.enabled &&
            //AttitudeSensor.current != null && AttitudeSensor.current.enabled &&
            LinearAccelerationSensor.current != null && LinearAccelerationSensor.current.enabled
           )
        {
            sensorsEnabled = true;
        }
    }
    public void DisableSensors()
    {
        //if (Gyroscope.current != null)
        //    InputSystem.DisableDevice(Gyroscope.current);
        //if (Accelerometer.current != null)
        //    InputSystem.DisableDevice(Accelerometer.current);
        //if (GravitySensor.current != null)
        //    InputSystem.DisableDevice(GravitySensor.current);
        //if (AttitudeSensor.current != null)
        //    InputSystem.DisableDevice(AttitudeSensor.current);
        if (LinearAccelerationSensor.current != null)
            InputSystem.DisableDevice(LinearAccelerationSensor.current);

        if (
            //Gyroscope.current != null && !Gyroscope.current.enabled &&
            //Accelerometer.current != null &&  !Accelerometer.current.enabled &&
            //GravitySensor.current != null &&  !GravitySensor.current.enabled &&
            //AttitudeSensor.current != null && !AttitudeSensor.current.enabled &&
            LinearAccelerationSensor.current != null && !LinearAccelerationSensor.current.enabled
           )
        {
            sensorsEnabled = false;
        }
    }
}
public class SensorsReaderOptions
{
    public bool IsStepRecognitionMachineEnabled { get; set; } = false;
    public float MaxWaveAmplitude { get; set; } = 0.007f;
    public bool IsWaveAmplitudeCheckActive { get; set; } = false;
    public float NumberOfPeaksForAStep { get; set; } = 1;
    public bool IsMaxDistanceBetweenAveragesEnabled { get; set; } = true;
    public float MaxDistanceBetweenAverages { get; set; } = 0.015f;
    public bool IsHighThresholdEnabled { get; set; } = true;
    public float HighThreshold { get; set; } = 0.05f;
    public float AccelerometerFrequency { get; set; } = 60;
    public float MovingAverageWindowSize { get; set; } = 20;
    public float AccelerometerUpdateInterval { get; set; } = 0.10f;
    public float LowPassKernelWidthInSeconds { get; set; } = 0.80f;
}