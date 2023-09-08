using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;




public class SceneManager : MonoBehaviour
{
    public SensorsReader sensorReader;
    public GameObject recordButton;
    public GameObject analyseButton;
    public GameObject checkStillButton;
    public GameObject stepMachineButton;
    public GameObject maxDistanceBetweenAveragesButton;
    public GameObject highThresholdButton;
    public GameObject stillStatus;
    public GameObject stateMachineStepDetectionStatus;
    public GameObject waveAmplitudeCheckButton;

    public UnityEvent<float> OnDelayForStillChangedFromSensor = new UnityEvent<float>();
    public UnityEvent<float> OnHighThresholdChangedFromSensor = new UnityEvent<float>();
    public UnityEvent<float> OnMaxDistanceBetweenAveragesChangedFromSensor = new UnityEvent<float>();
    public UnityEvent<float> OnMaxWaveAmplitudeChangedFromSensor = new UnityEvent<float>();
    public UnityEvent<float> OnNumberOfPeaksForAStepChangedFromSensor = new UnityEvent<float>();
    public UnityEvent<float> OnAccelerometerFrequencyChangedFromSensor = new UnityEvent<float>();
    public UnityEvent<float> OnMovingAverageWindowSizeChangedFromSensor = new UnityEvent<float>();
    public UnityEvent<float> OnAccelerometerUpdateIntervalChangedFromSensor = new UnityEvent<float>();
    public UnityEvent<float> OnLowPassKernelWidthInSecondsChangedFromSensor = new UnityEvent<float>();

    /*
    public DD_DataDiagram diagramAccelerationX;
    public DD_DataDiagram diagramAccelerationY;
    public DD_DataDiagram diagramAccelerationZ;
    */
    public DD_DataDiagram diagramMagnitudeAndHighThresholdCheck;
    public DD_DataDiagram diagramStepAndAmplitudeCheck;
    public DD_DataDiagram diagramDistanceBetweenAveragesCheck;
    /*
    private GameObject lineAccelerationX;
    private GameObject lineAccelerationX_NotFiltered;

    private GameObject lineAccelerationY;
    private GameObject lineAccelerationY_NotFiltered;

    private GameObject lineAccelerationZ;
    private GameObject lineAccelerationZ_NotFiltered;
    */
    private GameObject lineMagnitude;
    private GameObject lineMagnitude_NotFiltered;
    private GameObject lineHighThreshold;

    private GameObject lineMagnitude2;
    private GameObject lineMovingAverage;
    private GameObject lineAmplitudeMax;
    private GameObject lineAmplitudeMin;
    private GameObject lineMaxDistanceBetweenAverages;

    private GameObject lineMagnitude3;
    private GameObject lineMovingAverage3;
    private GameObject lineStillAverage;

    private Color Red = Color.red;
    private Color Grey = Color.grey;
    private Color Green = Color.green;
    private Color Blue = Color.blue;
    private Color Magenta = Color.magenta;
    private Color White = Color.white;

    public TextMeshProUGUI textLeft;
    public TextMeshProUGUI textRight;
    //public GameObject PhoneModelAttitude;
    public GameObject PhoneModelAcceleration;
    public GameObject AccelerationArrow;
    Vector3 accelerationScaleVector = new Vector3(1, 1, 0.5f);

    private bool sensorReaderStarted = false;

    void Start()
    {
        SensorsReaderOptions sensorsReaderOptions = new SensorsReaderOptions
        {
            IsStepRecognitionMachineEnabled = false,
            MaxWaveAmplitude = 0.007f,
            IsWaveAmplitudeCheckActive = false,
            NumberOfPeaksForAStep = 1,

            IsMaxDistanceBetweenAveragesEnabled = true,
            MaxDistanceBetweenAverages = 0.015f,

            IsHighThresholdEnabled = true,
            HighThreshold = 0.05f,

            AccelerometerFrequency = 60,
            MovingAverageWindowSize = 20,
            AccelerometerUpdateInterval = 0.10f,
            LowPassKernelWidthInSeconds = 0.80f
        };
        sensorReader.OnStateMachineStepDetected += (localMin, localMax) => { OnStateMachineStepDetected(localMin, localMax); };
        sensorReader.OnDelayForStillChanged += (newValue) => { OnDelayForStillChangedFromSensor.Invoke(newValue); };
        sensorReader.OnHighThresholdChanged += (newThreshold) => { OnHighThresholdChangedFromSensor.Invoke(newThreshold); };
        sensorReader.OnMaxDistanceBetweenAveragesChanged += (newThreshold) => { OnMaxDistanceBetweenAveragesChangedFromSensor.Invoke(newThreshold); };
        sensorReader.OnMaxWaveAmplitudeChanged += (newValue) => { OnMaxWaveAmplitudeChangedFromSensor.Invoke(newValue); };
        sensorReader.OnNumberOfPeaksForAStepChanged += (newValue) => { OnNumberOfPeaksForAStepChangedFromSensor.Invoke(newValue); };
        sensorReader.OnAccelerometerFrequencyChanged += (newValue) => { OnAccelerometerFrequencyChangedFromSensor.Invoke(newValue); };
        sensorReader.OnMovingAverageWindowSizeChanged += (newValue) => { OnMovingAverageWindowSizeChangedFromSensor.Invoke(newValue); };
        sensorReader.OnAccelerometerUpdateIntervalChanged += (newValue) => { OnAccelerometerUpdateIntervalChangedFromSensor.Invoke(newValue); };
        sensorReader.OnLowPassKernelWidthInSecondsChanged += (newValue) => { OnLowPassKernelWidthInSecondsChangedFromSensor.Invoke(newValue); };

        sensorReader.SetupAndStartSensors(0.1f, OnStillCallback, OnMovingCallback, sensorsReaderOptions);
        sensorReaderStarted = true;



        stillStatus.SetActive(false);
        stateMachineStepDetectionStatus.SetActive(false);

        waveAmplitudeCheckButton.GetComponent<CustomButtonBehaviour>().SetUIState(sensorsReaderOptions.IsWaveAmplitudeCheckActive);
        highThresholdButton.GetComponent<CustomButtonBehaviour>().SetUIState(sensorsReaderOptions.IsHighThresholdEnabled);
        checkStillButton.GetComponent<CustomButtonBehaviour>().SetUIState(false);
        recordButton.GetComponent<CustomButtonBehaviour>().SetUIState(false);
        maxDistanceBetweenAveragesButton.GetComponent<CustomButtonBehaviour>().SetUIState(sensorsReaderOptions.IsMaxDistanceBetweenAveragesEnabled);
        stepMachineButton.GetComponent<CustomButtonBehaviour>().SetUIState(sensorsReaderOptions.IsStepRecognitionMachineEnabled);


        /*
        lineAccelerationX = diagramAccelerationX.AddLine(colorX.ToString(), colorX);
        lineAccelerationX_NotFiltered = diagramAccelerationX.AddLine(colorX_NotFiltered.ToString(), colorX_NotFiltered);
        lineAccelerationY = diagramAccelerationY.AddLine(colorY.ToString(), colorY);
        lineAccelerationY_NotFiltered = diagramAccelerationY.AddLine(colorY_NotFiltered.ToString(), colorY_NotFiltered);
        lineAccelerationZ = diagramAccelerationZ.AddLine(colorZ.ToString(), colorZ);
        lineAccelerationZ_NotFiltered = diagramAccelerationZ.AddLine(colorZ_NotFiltered.ToString(), colorZ_NotFiltered);
        */
        lineMagnitude = diagramMagnitudeAndHighThresholdCheck.AddLine(Magenta.ToString(), Magenta);
        lineMagnitude_NotFiltered = diagramMagnitudeAndHighThresholdCheck.AddLine(Grey.ToString(), Grey);
        lineHighThreshold = diagramMagnitudeAndHighThresholdCheck.AddLine(White.ToString(), White);
        
        lineMagnitude2 = diagramStepAndAmplitudeCheck.AddLine(Magenta.ToString(), Magenta);
        lineMovingAverage = diagramStepAndAmplitudeCheck.AddLine(Green.ToString(), Green);
        /*
        lineAmplitudeMax = diagramStepAndAmplitudeCheck.AddLine(Red.ToString(), Red);
        lineAmplitudeMin = diagramStepAndAmplitudeCheck.AddLine(Red.ToString(), Red);
        */

        lineMagnitude3 = diagramDistanceBetweenAveragesCheck.AddLine(Magenta.ToString(), Magenta);
        lineMovingAverage3 = diagramDistanceBetweenAveragesCheck.AddLine(Green.ToString(), Green);
        lineMaxDistanceBetweenAverages = diagramDistanceBetweenAveragesCheck.AddLine(Blue.ToString(), Blue);
        lineStillAverage = diagramDistanceBetweenAveragesCheck.AddLine(White.ToString(), White);



        /*
        StartCoroutine(ZoomAndDrag(diagramAccelerationX));
        StartCoroutine(ZoomAndDrag(diagramAccelerationY));
        StartCoroutine(ZoomAndDrag(diagramAccelerationZ));
        */
        StartCoroutine(ZoomAndDrag(diagramMagnitudeAndHighThresholdCheck));
        StartCoroutine(ZoomAndDrag(diagramStepAndAmplitudeCheck));
        StartCoroutine(ZoomAndDrag(diagramDistanceBetweenAveragesCheck));
    }

    private void OnStillCallback()
    {
        Debug.Log($"Player Still for long enough!");
        stillStatus.GetComponentInChildren<TextMeshProUGUI>().text = "STILL!!!";
        stillStatus.GetComponentInChildren<Image>().color = Color.green;
    }    
    private void OnMovingCallback()
    {
        stillStatus.GetComponentInChildren<TextMeshProUGUI>().text = "moving";
        stillStatus.GetComponentInChildren<Image>().color = Color.red;
    }

    void Update() 
    {
        if (!sensorReaderStarted)
        {
            return;
        }
     
        if(sensorReader.AccelerationFilteredProjectedXZ != Vector3.zero)
        {        
            AccelerationArrow.transform.LookAt(AccelerationArrow.transform.position + sensorReader.AccelerationFilteredProjectedXZ); ;
            AccelerationArrow.transform.localScale = accelerationScaleVector * (1f+ sensorReader.AccelerationFilteredProjectedXZ.magnitude );
        }
        else
        {
            AccelerationArrow.transform.localScale = Vector3.zero;
        }

        DrawDiagramLines(sensorReader.AccelerationFilteredProjectedXZ);

        //PhoneModelAttitude.transform.rotation = Quaternion.Euler(sensorReader.AttitudeEulerProjectedXZ);

        WriteVisualLogs();
            
      
    }


    public void OnEnableWaveAmplitudeCheck()
    {
        if (sensorReader.IsWaveAmplitudeCheckActive)
        {
            waveAmplitudeCheckButton.GetComponent<CustomButtonBehaviour>().SetUIState(false);
            sensorReader.IsWaveAmplitudeCheckActive = false;
            diagramStepAndAmplitudeCheck.DestroyLine(lineAmplitudeMax);
            diagramStepAndAmplitudeCheck.DestroyLine(lineAmplitudeMin);
        }
        else
        {
            waveAmplitudeCheckButton.GetComponent<CustomButtonBehaviour>().SetUIState(true);
            sensorReader.IsWaveAmplitudeCheckActive = true;
            lineAmplitudeMax = diagramStepAndAmplitudeCheck.AddLine(Red.ToString(), Red);
            lineAmplitudeMin = diagramStepAndAmplitudeCheck.AddLine(Red.ToString(), Red);
        }
    }
    public void SetWaveAmplitudeButtonUI(bool mode)
    {
        if (mode)
        {
            waveAmplitudeCheckButton.GetComponentInChildren<TextMeshProUGUI>().text = "Wave Amplitude Check: ON";
        }
        else
        {
            waveAmplitudeCheckButton.GetComponentInChildren<TextMeshProUGUI>().text = "Wave Amplitude Check: OFF";
        }
    }

    public void OnEnableStepRecognitionMachine()
    {
        if (sensorReader.IsStepRecognitionMachineEnabled)
        {
            stepMachineButton.GetComponent<CustomButtonBehaviour>().SetUIState(false);
            sensorReader.IsStepRecognitionMachineEnabled = false;
        }
        else
        {
            stepMachineButton.GetComponent<CustomButtonBehaviour>().SetUIState(true);
            sensorReader.IsStepRecognitionMachineEnabled = true;
        }
    }
    public void SetStepRecognitionMachineButtonUI(bool mode)
    {
        if (mode)
        {
            stepMachineButton.GetComponentInChildren<TextMeshProUGUI>().text = "Step Machine: ON";
        }
        else
        {
            stepMachineButton.GetComponentInChildren<TextMeshProUGUI>().text = "Step Machine: OFF";
        }
    }

    public void OnEnableMaxDistanceBetweenAverages()
    {
        if (sensorReader.IsMaxDistanceBetweenAveragesEnabled)
        {
            maxDistanceBetweenAveragesButton.GetComponent<CustomButtonBehaviour>().SetUIState(false);
            sensorReader.IsMaxDistanceBetweenAveragesEnabled = false;
            diagramDistanceBetweenAveragesCheck.DestroyLine(lineMaxDistanceBetweenAverages);
        }
        else
        {
            maxDistanceBetweenAveragesButton.GetComponent<CustomButtonBehaviour>().SetUIState(true);
            sensorReader.IsMaxDistanceBetweenAveragesEnabled = true;
            lineMaxDistanceBetweenAverages = diagramDistanceBetweenAveragesCheck.AddLine(Blue.ToString(), Blue);
        }
    }
    public void SetMaxDistanceBetweenAveragesButtonUI(bool mode)
    {
        if (mode)
        {
            maxDistanceBetweenAveragesButton.GetComponentInChildren<TextMeshProUGUI>().text = "Max Dist AVG: ON";
        }
        else
        {
            maxDistanceBetweenAveragesButton.GetComponentInChildren<TextMeshProUGUI>().text = "Max Dist AVG: OFF";
        }
    }

    public void OnEnableHighThreshold()
    {
        if (sensorReader.IsHighThresholdEnabled)
        {
            highThresholdButton.GetComponent<CustomButtonBehaviour>().SetUIState(false);
            sensorReader.IsHighThresholdEnabled = false;
            diagramMagnitudeAndHighThresholdCheck.DestroyLine(lineHighThreshold);
        }
        else
        {
            highThresholdButton.GetComponent<CustomButtonBehaviour>().SetUIState(true);
            sensorReader.IsHighThresholdEnabled = true;
            lineHighThreshold = diagramMagnitudeAndHighThresholdCheck.AddLine(White.ToString(), White);
        }
    }
    public void SetHighThresholdButtonUI(bool mode)
    {
        if (mode)
        {
            highThresholdButton.GetComponentInChildren<TextMeshProUGUI>().text = "High Threshold: ON";
        }
        else
        {
            highThresholdButton.GetComponentInChildren<TextMeshProUGUI>().text = "High Threshold: OFF";
        }
    }

    public void OnAnalyseData()
    {
        sensorReader.AnalyseData();
    }

    public void OnRecord()
    {
        if (sensorReader.IsRecording)
        {
            recordButton.GetComponent<CustomButtonBehaviour>().SetUIState(false);
            sensorReader.IsRecording = false;
        }
        else
        {
            recordButton.GetComponent<CustomButtonBehaviour>().SetUIState(true);
            sensorReader.IsRecording = true;
        }
    }
    public void SetRecordButtonUI(bool mode)
    {
        if (mode)
        {
            recordButton.GetComponentInChildren<TextMeshProUGUI>().text = "STOP RECORDING";
        }
        else
        {
            recordButton.GetComponentInChildren<TextMeshProUGUI>().text = "START RECORDING";
        }
    }

    public void OnCheckStill()
    {
        if (sensorReader.IsCheckingStill)
        {
            checkStillButton.GetComponent<CustomButtonBehaviour>().SetUIState(false);
            sensorReader.IsCheckingStill = false;
        }
        else
        {
            checkStillButton.GetComponent<CustomButtonBehaviour>().SetUIState(true);
            sensorReader.IsCheckingStill = true;
        }
    }
    public void SetCheckStillButtonUI(bool mode)
    {
        if (mode)
        {
            checkStillButton.GetComponentInChildren<TextMeshProUGUI>().text = "STOP CHECKING";
            stillStatus.SetActive(true);
            stateMachineStepDetectionStatus.SetActive(true);
            stillStatus.GetComponentInChildren<TextMeshProUGUI>().text = "moving";
        }
        else
        {
            checkStillButton.GetComponentInChildren<TextMeshProUGUI>().text = "START CHECK STILL";
            stillStatus.GetComponentInChildren<Image>().color = Color.red;
            stillStatus.SetActive(false);
            stateMachineStepDetectionStatus.SetActive(false);
        }
    }

    public IEnumerator ZoomAndDrag(DD_DataDiagram diagram)
    {
        yield return new WaitForSeconds(0.1f);
        diagram.RaiseMoveEvent(0, 20f);
        diagram.RaiseZoomEvent(-2f, -2f);
    }
    public void OnZoomDiagramStepAmplitude_Plus()
    {
        diagramStepAndAmplitudeCheck.RaiseZoomEvent(-0.01f, -0.01f);
    }
    public void OnZoomDiagramStepAmplitude_Minus()
    {
        diagramStepAndAmplitudeCheck.RaiseZoomEvent(+0.01f, +0.01f);
    }
    public void OnZoomDiagramDistanceAverages_Plus()
    {
        diagramDistanceBetweenAveragesCheck.RaiseZoomEvent(-0.01f, -0.01f);
    }
    public void OnZoomDiagramDistanceAverages_Minus()
    {
        diagramDistanceBetweenAveragesCheck.RaiseZoomEvent(+0.01f, +0.01f);
    }


    public void OnMaxWaveAmplitudeChangedByUI(float newValue)
    {
        sensorReader.MaxWaveAmplitude = newValue;
    }
    public void OnMaxDistanceBetweenAveragesChangedByUI(float newValue)
    {
        sensorReader.MaxDistanceBetweenAverages = newValue;
    }
    public void OnHighThresholdChangedByUI(float newValue)
    {
        sensorReader.HighThreshold = newValue;
    }
    public void OnAccelerometerUpdateIntervalChangedByUI(float newValue)
    {
        sensorReader.AccelerometerUpdateInterval = newValue;
    }
    public void OnLowPassKernelWidthInSecondsChangedByUI(float newValue)
    {
        sensorReader.LowPassKernelWidthInSeconds = newValue;
    }    
    public void OnDelayForStillChangedByUI(float newValue)
    {
        sensorReader.DelayForStill_S = newValue;
    }
    public void OnMovingAverageWindowSizeChangedByUI(float value)
    {
        sensorReader.MovingAverageWindowSize = value;
    }
    public void OnAccelerometerFrequencyChangedByUI(float value)
    {
        sensorReader.AccelerometerFrequency = value;
    }
    public void OnNumberOfPeaksForAStepChangedByUI(float value)
    {
        sensorReader.NumberOfPeaksForAStep = value;
    }  
    public void OnStateMachineStepDetected(float localMin, float localMax)
    {
        StartCoroutine(OnStateMachineStepDetected());
        for (float i = -0.05f; i < 0.05f; i += 0.01f)
        {
            diagramStepAndAmplitudeCheck.InputPoint(lineMovingAverage, new Vector2(0.01f, localMin + i));
        }
    }

    public IEnumerator OnStateMachineStepDetected()
    {
        stateMachineStepDetectionStatus.GetComponentInChildren<TextMeshProUGUI>().text = "STEP!!!";
        stateMachineStepDetectionStatus.GetComponentInChildren<Image>().color = Color.green;
        yield return new WaitForSeconds(0.5f);
        stateMachineStepDetectionStatus.GetComponentInChildren<TextMeshProUGUI>().text = "...";
        stateMachineStepDetectionStatus.GetComponentInChildren<Image>().color = Color.red;
    }

    private void WriteVisualLogs()
    {
        /*textLeft.text =
                        //$"Attitude\nX={sensorReader.Attitude.x:#0.00} Y={sensorReader.Attitude.y:#0.00} Z={sensorReader.Attitude.z:#0.00}\n\n" +
                        //$"attitudeEulerProjectedXZ\nX={sensorReader.AttitudeEulerProjectedXZ.x:#0.00} Y={sensorReader.AttitudeEulerProjectedXZ.y:#0.00} Z={sensorReader.AttitudeEulerProjectedXZ.z:#0.00}\n\n" +
                        $"Wave max/min [{sensorReader.IsWaveAmplitudeCheckActive}/{sensorReader.StepRecognitionMachine?.IsWaveAmplitudeCheckActive}] \nX={sensorReader.MaxWaveAmplitude}\n" +
                        $"# of Up/Down to count a step {sensorReader.NumberOfPeaksForAStep}/{sensorReader.StepRecognitionMachine?.NumberOfPeaksForAStep}\n" +
                        $"Max dist btw avg [{sensorReader.IsMaxDistanceBetweenAveragesEnabled}]={sensorReader.MaxDistanceBetweenAverages:#0.000} \n" +
                        $"Still threshold High [{sensorReader.IsHighThresholdEnabled}]={sensorReader.HighThreshold:#0.00} \n" +
                        $"Accelerometer Frequency ={sensorReader.AccelerometerFrequency:#0.00} \n Avg W Size={sensorReader.MovingAverageWindowSize}";
        */
        textRight.text =
                        $"High Threshold Check: {sensorReader.IsHighThresholdEnabled && sensorReader.AccelerationFilteredMagnitude > sensorReader.HighThreshold} \n" +
                        $"Max dist btw avg Check: {sensorReader.IsMaxDistanceBetweenAveragesEnabled && sensorReader.MovingAverage - sensorReader.StillAverage > sensorReader.MaxDistanceBetweenAverages} \n" +
                        $"State machine step Check: {sensorReader.IsStepRecognitionMachineEnabled && sensorReader.StepRecognitionMachine != null && sensorReader.StepRecognitionMachine.HasStep()} \n" +
                        $"State machine [{sensorReader.IsStepRecognitionMachineEnabled}]={sensorReader.StepRecognitionMachine?.CurrentState.GetType().Name} \n" +
                        $"Moving Avg={sensorReader.MovingAverage:#0.00} Still Avg={sensorReader.StillAverage:#0.00} \n" +
                        $"Accelerator Magnitude={sensorReader.AccelerationFilteredMagnitude:#0.00}\n" +
                         $"Acceleration Filtered XZ\nX={sensorReader.AccelerationFiltered.x:#0.00} Y={sensorReader.AccelerationFiltered.y:#0.00}  Z= {sensorReader.AccelerationFiltered.z:#0.00}\n";
    }

    private void DrawDiagramLines(Vector3 acceleration)
    {
        //diagramAccelerationX.InputPoint(lineAccelerationX, new Vector2(0.01f, sensorReader.AccelerationFilteredProjectedXZ.x));
        //diagramAccelerationX.InputPoint(lineAccelerationX_NotFiltered, new Vector2(0.01f, sensorReader.AccelerationRaw.x));

        //diagramAccelerationY.InputPoint(lineAccelerationY, new Vector2(0.01f, sensorReader.AccelerationFilteredProjectedXZ.y));
        //diagramAccelerationY.InputPoint(lineAccelerationY_NotFiltered, new Vector2(0.01f, sensorReader.AccelerationRaw.y));

        //diagramAccelerationZ.InputPoint(lineAccelerationZ, new Vector2(0.01f, sensorReader.AccelerationFilteredProjectedXZ.z));
        //diagramAccelerationZ.InputPoint(lineAccelerationZ_NotFiltered, new Vector2(0.01f, sensorReader.AccelerationRaw.z));
        diagramMagnitudeAndHighThresholdCheck.InputPoint(lineMagnitude, new Vector2(0.01f, sensorReader.AccelerationFilteredMagnitude));
        diagramMagnitudeAndHighThresholdCheck.InputPoint(lineMagnitude_NotFiltered, new Vector2(0.01f, sensorReader.AccelerationRaw.magnitude));
        if(sensorReader.IsHighThresholdEnabled)
            diagramMagnitudeAndHighThresholdCheck.InputPoint(lineHighThreshold, new Vector2(0.01f, sensorReader.HighThreshold));



        diagramStepAndAmplitudeCheck.InputPoint(lineMagnitude2, new Vector2(0.01f, sensorReader.AccelerationFilteredMagnitude));
        diagramStepAndAmplitudeCheck.InputPoint(lineMovingAverage, new Vector2(0.01f, sensorReader.MovingAverage));
        if(sensorReader.IsWaveAmplitudeCheckActive)
        {
            diagramStepAndAmplitudeCheck.InputPoint(lineAmplitudeMax, new Vector2(0.01f, sensorReader.MovingAverage+sensorReader.MaxWaveAmplitude));
            diagramStepAndAmplitudeCheck.InputPoint(lineAmplitudeMin, new Vector2(0.01f, sensorReader.MovingAverage - sensorReader.MaxWaveAmplitude));
        }


        diagramDistanceBetweenAveragesCheck.InputPoint(lineMagnitude3, new Vector2(0.01f, sensorReader.AccelerationFilteredMagnitude));
        if(sensorReader.IsMaxDistanceBetweenAveragesEnabled)
            diagramDistanceBetweenAveragesCheck.InputPoint(lineMaxDistanceBetweenAverages, new Vector2(0.01f, sensorReader.MaxDistanceBetweenAverages+sensorReader.StillAverage));
        diagramDistanceBetweenAveragesCheck.InputPoint(lineMovingAverage3, new Vector2(0.01f, sensorReader.MovingAverage));
        diagramDistanceBetweenAveragesCheck.InputPoint(lineStillAverage, new Vector2(0.01f, sensorReader.StillAverage));

    }



    private void OnDestroy()
    {
        sensorReader.DisableSensors();
    }

}