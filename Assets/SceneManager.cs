using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;




public class SceneManager : MonoBehaviour
{
    public SensorsReader sensorReader;
    public GameObject recordStillButton;
    public GameObject analyseStillButton;
    public GameObject checkStillButton;
    public GameObject stepMachineButton;
    public GameObject maxDistanceBetweenAveragesButton;
    public GameObject highThresholdButton;
    public GameObject stillStatus;
    public GameObject stateMachineStepDetectionStatus;
    public GameObject waveDeltaCheckButton;

    public UnityEvent<float> OnStillDelayChangedFromSensor = new UnityEvent<float>();
    public UnityEvent<float> OnStillHighThresholdChangedFromSensor = new UnityEvent<float>();
    public UnityEvent<float> OnStillMaxDistanceFromAverageChangedFromSensor = new UnityEvent<float>();
    public UnityEvent<float> OnStillWaveStepDeltaChangedFromSensor = new UnityEvent<float>();
    public UnityEvent<float> OnStepThresholdChangedFromSensor = new UnityEvent<float>();
    public UnityEvent<float> OnAccelerometerFrequencyChangedFromSensor = new UnityEvent<float>();
    public UnityEvent<float> OnStillMovingAverageWindowSizeChangedFromSensor = new UnityEvent<float>();
    public UnityEvent<float> OnAccelerometerUpdateIntervalChangedFromSensor = new UnityEvent<float>();
    public UnityEvent<float> OnLowPassKernelWidthInSecondsChangedFromSensor = new UnityEvent<float>();

    /*
    public DD_DataDiagram diagramAccelerationX;
    public DD_DataDiagram diagramAccelerationY;
    public DD_DataDiagram diagramAccelerationZ;
    */
    public DD_DataDiagram diagramAccelerationMagnitude;
    public DD_DataDiagram diagramAccelerationAvg;
    public DD_DataDiagram diagramAccelerationAvgDist;
    /*
    private GameObject lineAccelerationX;
    private GameObject lineAccelerationX_NotFiltered;

    private GameObject lineAccelerationY;
    private GameObject lineAccelerationY_NotFiltered;

    private GameObject lineAccelerationZ;
    private GameObject lineAccelerationZ_NotFiltered;
    */
    private GameObject lineAccelerationMagnitude;
    private GameObject lineAccelerationMagnitude_NotFiltered;
    private GameObject lineAccelerationMagnitudeThreshold;

    private GameObject lineAccelerationMagnitudeForAvg;
    private GameObject lineAccelerationMovingAverage;
    private GameObject lineAccelerationMovingAverageMax;
    private GameObject lineAccelerationMovingAverageMin;
    private GameObject lineAccelerationMaxDistanceBetweenAverages;

    private GameObject lineAccelerationMagnitudeForAvgDist;
    private GameObject lineAccelerationMovingAverageDist;
    private GameObject lineAccelerationStillAverageDist;

    private Color colorRed = Color.red;
    private Color colorGrey = Color.grey;
    private Color colorGreen = Color.green;
    private Color colorBlue = Color.blue;
    private Color colorMagenta = Color.magenta;
    private Color colorWhite = Color.white;

    public TextMeshProUGUI text, text2;
    public GameObject PhoneModelAttitude,PhoneModelAcceleration;
    public GameObject AccelerationArrow;
    Vector3 accelerationScaleVector = new Vector3(1, 1, 0.5f);

    private bool sensorReaderStarted = false;

    void Start()
    {
        SensorsReaderOptions sensorsReaderOptions = new SensorsReaderOptions
        {
            IsStepRecognitionMachineEnabled = false,
            StillWaveStepDelta = 0.007f,
            IsWaveStepDeltaCheckActive = false,
            StepThreshold = 1,

            IsMaxDistanceBetweenAveragesEnabled = true,
            StillMaxDistanceBetweenAverages = 0.015f,

            IsStillHighThresholdEnabled = true,
            StillHighThreshold = 0.05f,

            AccelerometerFrequency = 60,
            StillMovingAverageWindowSize = 20,
            AccelerometerUpdateInterval = 0.10f,
            LowPassKernelWidthInSeconds = 0.80f
        };
        sensorReader.OnStateMachineStepDetected += (localMin, localMax) => { OnStateMachineStepDetected(localMin, localMax); };
        sensorReader.OnStillDelayChanged += (newValue) => { OnStillDelayChangedFromSensor.Invoke(newValue); };
        sensorReader.OnStillHighThresholdChanged += (newThreshold) => { OnStillHighThresholdChangedFromSensor.Invoke(newThreshold); };
        sensorReader.OnStillMaxDistanceFromAverageChanged += (newThreshold) => { OnStillMaxDistanceFromAverageChangedFromSensor.Invoke(newThreshold); };
        sensorReader.OnStillWaveStepDeltaChanged += (newValue) => { OnStillWaveStepDeltaChangedFromSensor.Invoke(newValue); };
        sensorReader.OnStepThresholdChanged += (newValue) => { OnStepThresholdChangedFromSensor.Invoke(newValue); };
        sensorReader.OnAccelerometerFrequencyChanged += (newValue) => { OnAccelerometerFrequencyChangedFromSensor.Invoke(newValue); };
        sensorReader.OnStillMovingAverageWindowSizeChanged += (newValue) => { OnStillMovingAverageWindowSizeChangedFromSensor.Invoke(newValue); };
        sensorReader.OnAccelerometerUpdateIntervalChanged += (newValue) => { OnAccelerometerUpdateIntervalChangedFromSensor.Invoke(newValue); };
        sensorReader.OnLowPassKernelWidthInSecondsChanged += (newValue) => { OnLowPassKernelWidthInSecondsChangedFromSensor.Invoke(newValue); };

        sensorReader.SetupAndStartSensors(0.1f, OnStillCallback, OnMovingCallback, sensorsReaderOptions);
        sensorReaderStarted = true;



        stillStatus.SetActive(false);
        stateMachineStepDetectionStatus.SetActive(false);

        waveDeltaCheckButton.GetComponent<CustomButtonBehaviour>().SetUIState(sensorsReaderOptions.IsWaveStepDeltaCheckActive);
        highThresholdButton.GetComponent<CustomButtonBehaviour>().SetUIState(sensorsReaderOptions.IsStillHighThresholdEnabled);
        checkStillButton.GetComponent<CustomButtonBehaviour>().SetUIState(false);
        recordStillButton.GetComponent<CustomButtonBehaviour>().SetUIState(false);
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
        lineAccelerationMagnitude = diagramAccelerationMagnitude.AddLine(colorMagenta.ToString(), colorMagenta);
        lineAccelerationMagnitude_NotFiltered = diagramAccelerationMagnitude.AddLine(colorGrey.ToString(), colorGrey);
        lineAccelerationMagnitudeThreshold = diagramAccelerationMagnitude.AddLine(colorWhite.ToString(), colorWhite);
        
        lineAccelerationMagnitudeForAvg = diagramAccelerationAvg.AddLine(colorMagenta.ToString(), colorMagenta);
        lineAccelerationMovingAverage = diagramAccelerationAvg.AddLine(colorGreen.ToString(), colorGreen);
        /*
        lineAccelerationMovingAverageMax = diagramAccelerationAvg.AddLine(colorRed.ToString(), colorRed);
        lineAccelerationMovingAverageMin = diagramAccelerationAvg.AddLine(colorRed.ToString(), colorRed);
        */

        lineAccelerationMagnitudeForAvgDist = diagramAccelerationAvgDist.AddLine(colorMagenta.ToString(), colorMagenta);
        lineAccelerationMovingAverageDist = diagramAccelerationAvgDist.AddLine(colorGreen.ToString(), colorGreen);
        lineAccelerationMaxDistanceBetweenAverages = diagramAccelerationAvgDist.AddLine(colorBlue.ToString(), colorBlue);
        lineAccelerationStillAverageDist = diagramAccelerationAvgDist.AddLine(colorWhite.ToString(), colorWhite);



        /*
        StartCoroutine(ZoomAndDrag(diagramAccelerationX));
        StartCoroutine(ZoomAndDrag(diagramAccelerationY));
        StartCoroutine(ZoomAndDrag(diagramAccelerationZ));
        */
        StartCoroutine(ZoomAndDrag(diagramAccelerationMagnitude));
        StartCoroutine(ZoomAndDrag(diagramAccelerationAvg));
        StartCoroutine(ZoomAndDrag(diagramAccelerationAvgDist));
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

        PhoneModelAttitude.transform.rotation = Quaternion.Euler(sensorReader.AttitudeEulerProjectedXZ);

        WriteVisualLogs();
            
      
    }


    public void OnEnableStepDeltaCheck()
    {
        if (sensorReader.IsWaveStepDeltaCheckActive)
        {
            waveDeltaCheckButton.GetComponent<CustomButtonBehaviour>().SetUIState(false);
            sensorReader.IsWaveStepDeltaCheckActive = false;
            diagramAccelerationAvg.DestroyLine(lineAccelerationMovingAverageMax);
            diagramAccelerationAvg.DestroyLine(lineAccelerationMovingAverageMin);
        }
        else
        {
            waveDeltaCheckButton.GetComponent<CustomButtonBehaviour>().SetUIState(true);
            sensorReader.IsWaveStepDeltaCheckActive = true;
            lineAccelerationMovingAverageMax = diagramAccelerationAvg.AddLine(colorRed.ToString(), colorRed);
            lineAccelerationMovingAverageMin = diagramAccelerationAvg.AddLine(colorRed.ToString(), colorRed);
        }
    }
    public void SetStepDeltaCheckUI(bool mode)
    {
        if (mode)
        {
            waveDeltaCheckButton.GetComponentInChildren<TextMeshProUGUI>().text = "Wave Delta Check: ON";
        }
        else
        {
            waveDeltaCheckButton.GetComponentInChildren<TextMeshProUGUI>().text = "Wave Delta Check: OFF";
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
    public void SetStepRecognitionMachineUI(bool mode)
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
            diagramAccelerationAvgDist.DestroyLine(lineAccelerationMaxDistanceBetweenAverages);
        }
        else
        {
            maxDistanceBetweenAveragesButton.GetComponent<CustomButtonBehaviour>().SetUIState(true);
            sensorReader.IsMaxDistanceBetweenAveragesEnabled = true;
            lineAccelerationMaxDistanceBetweenAverages = diagramAccelerationAvgDist.AddLine(colorBlue.ToString(), colorBlue);
        }
    }
    public void SetMaxDistanceBetweenAveragesUI(bool mode)
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

    public void OnEnableHighThresholdPressed()
    {
        if (sensorReader.IsStillHighThresholdEnabled)
        {
            highThresholdButton.GetComponent<CustomButtonBehaviour>().SetUIState(false);
            sensorReader.IsStillHighThresholdEnabled = false;
            diagramAccelerationMagnitude.DestroyLine(lineAccelerationMagnitudeThreshold);
        }
        else
        {
            highThresholdButton.GetComponent<CustomButtonBehaviour>().SetUIState(true);
            sensorReader.IsStillHighThresholdEnabled = true;
            lineAccelerationMagnitudeThreshold = diagramAccelerationMagnitude.AddLine(colorWhite.ToString(), colorWhite);
        }
    }
    public void SetHighThresholdUI(bool mode)
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

    public void OnAnalyseStillPressed()
    {
        sensorReader.AnalyseStillData();
    }

    public void OnRecordStillPressed()
    {
        if (sensorReader.IsRecordingStill)
        {
            recordStillButton.GetComponent<CustomButtonBehaviour>().SetUIState(false);
            sensorReader.IsRecordingStill = false;
        }
        else
        {
            recordStillButton.GetComponent<CustomButtonBehaviour>().SetUIState(true);
            sensorReader.IsRecordingStill = true;
        }
    }
    public void SetRecordStillUI(bool mode)
    {
        if (mode)
        {
            recordStillButton.GetComponentInChildren<TextMeshProUGUI>().text = "STOP RECORDING";
        }
        else
        {
            recordStillButton.GetComponentInChildren<TextMeshProUGUI>().text = "TRAIN STAY STILL";
        }
    }

    public void OnCheckStandingStillPressed()
    {
        if (sensorReader.IsCheckingStandingStill)
        {
            checkStillButton.GetComponent<CustomButtonBehaviour>().SetUIState(false);
            sensorReader.IsCheckingStandingStill = false;
        }
        else
        {
            checkStillButton.GetComponent<CustomButtonBehaviour>().SetUIState(true);
            sensorReader.IsCheckingStandingStill = true;
        }
    }
    public void SetCheckStandingStillUI(bool mode)
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
    public void OnZoomDiagramAvgPlus()
    {
        diagramAccelerationAvg.RaiseZoomEvent(-0.01f, -0.01f);
    }
    public void OnZoomDiagramAvgMinus()
    {
        diagramAccelerationAvg.RaiseZoomEvent(+0.01f, +0.01f);
    }
    public void OnZoomDiagramAvgDistPlus()
    {
        diagramAccelerationAvgDist.RaiseZoomEvent(-0.01f, -0.01f);
    }
    public void OnZoomDiagramAvgDistMinus()
    {
        diagramAccelerationAvgDist.RaiseZoomEvent(+0.01f, +0.01f);
    }


    public void OnStillWaveStepDeltaChangedByUI(float newValue)
    {
        sensorReader.StillWaveStepDelta = newValue;
    }
    public void OnMaxDistanceBetweenAveragesChangedByUI(float newValue)
    {
        sensorReader.StillMaxDistanceBetweenAverages = newValue;
    }
    public void OnStillHighThresholdChangedByUI(float newValue)
    {
        sensorReader.StillHighThreshold = newValue;
    }
    public void OnAccelerometerUpdateIntervalChangedByUI(float newValue)
    {
        sensorReader.AccelerometerUpdateInterval = newValue;
    }
    public void OnLowPassKernelWidthInSecondsChangedByUI(float newValue)
    {
        sensorReader.LowPassKernelWidthInSeconds = newValue;
    }    
    public void OnStillDelayChangedByUI(float newValue)
    {
        sensorReader.StillDelayS = newValue;
    }
    public void OnMovingAverageWindowSizeChangedByUI(float value)
    {
        sensorReader.StillMovingAverageWindowSize = value;
    }
    public void OnAccelerometerFrequencyChangedByUI(float value)
    {
        sensorReader.AccelerometerFrequency = value;
    }
    public void OnStepThresholdChangedByUI(float value)
    {
        sensorReader.StepThreshold = value;
    }  
    public void OnStateMachineStepDetected(float localMin, float localMax)
    {
        StartCoroutine(OnStateMachineStepDetected());
        for (float i = -0.05f; i < 0.05f; i += 0.01f)
        {
            diagramAccelerationAvg.InputPoint(lineAccelerationMovingAverage, new Vector2(0.01f, localMin + i));
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
        text.text =
                        //$"Attitude\nX={sensorReader.Attitude.x:#0.00} Y={sensorReader.Attitude.y:#0.00} Z={sensorReader.Attitude.z:#0.00}\n\n" +
                        //$"attitudeEulerProjectedXZ\nX={sensorReader.AttitudeEulerProjectedXZ.x:#0.00} Y={sensorReader.AttitudeEulerProjectedXZ.y:#0.00} Z={sensorReader.AttitudeEulerProjectedXZ.z:#0.00}\n\n" +
                        $"State machine [{sensorReader.IsStepRecognitionMachineEnabled}]={sensorReader.CurrentWaveState?.GetType().Name}/{sensorReader.WaveStateController?.CurrentState.GetType().Name} \n" +
                        $"Wave max/min [{sensorReader.IsWaveStepDeltaCheckActive}/{sensorReader.WaveStateController?.IsWaveStepDeltaCheckActive}] \nX={sensorReader.StillWaveStepDelta}\n" +
                        $"# of Up/Down to count a step {sensorReader.StepThreshold}/{sensorReader.WaveStateController?.StepThreshold}\n" +
                        $"Max dist btw avg [{sensorReader.IsMaxDistanceBetweenAveragesEnabled}]={sensorReader.StillMaxDistanceBetweenAverages:#0.000} \n" +
                        $"Still threshold High [{sensorReader.IsStillHighThresholdEnabled}]={sensorReader.StillHighThreshold:#0.00} \n" +
                        $"Accelerometer Frequency ={sensorReader.AccelerometerFrequency:#0.00} \n Avg W Size={sensorReader.StillMovingAverageWindowSize}";
        text2.text =
                        $"High Threshold Check: {sensorReader.IsStillHighThresholdEnabled && sensorReader.AccelerationFilteredMagnitude > sensorReader.StillHighThreshold} \n" +
                        $"Max dist btw avg Check: {sensorReader.IsMaxDistanceBetweenAveragesEnabled && sensorReader.StillMovingAverage - sensorReader.StillAvg > sensorReader.StillMaxDistanceBetweenAverages} \n" +
                        $"State machine step Check: {sensorReader.IsStepRecognitionMachineEnabled && sensorReader.WaveStateController != null && sensorReader.WaveStateController.HasStep()} \n" +
                        $"Moving Avg={sensorReader.StillMovingAverage:#0.00} Still Avg={sensorReader.StillAvg:#0.00} \n"+
                        $"Accelerator Magnitude={sensorReader.AccelerationFilteredMagnitude:#0.00}\n" +
                         $"Acceleration Filtered XZ\nX={sensorReader.AccelerationFiltered.x:#0.00} Y={sensorReader.AccelerationFiltered.y:#0.00}  Z= {sensorReader.AccelerationFiltered.z:#0.00}\n" +
                        $"LowPassKernelWidthS {sensorReader.LowPassKernelWidthInSeconds:#0.00} \naccelerometerUpdateInterval={sensorReader.AccelerometerUpdateInterval:#0.00}";

    }

    private void DrawDiagramLines(Vector3 acceleration)
    {
        //diagramAccelerationX.InputPoint(lineAccelerationX, new Vector2(0.01f, sensorReader.AccelerationFilteredProjectedXZ.x));
        //diagramAccelerationX.InputPoint(lineAccelerationX_NotFiltered, new Vector2(0.01f, sensorReader.AccelerationRaw.x));

        //diagramAccelerationY.InputPoint(lineAccelerationY, new Vector2(0.01f, sensorReader.AccelerationFilteredProjectedXZ.y));
        //diagramAccelerationY.InputPoint(lineAccelerationY_NotFiltered, new Vector2(0.01f, sensorReader.AccelerationRaw.y));

        //diagramAccelerationZ.InputPoint(lineAccelerationZ, new Vector2(0.01f, sensorReader.AccelerationFilteredProjectedXZ.z));
        //diagramAccelerationZ.InputPoint(lineAccelerationZ_NotFiltered, new Vector2(0.01f, sensorReader.AccelerationRaw.z));
        diagramAccelerationMagnitude.InputPoint(lineAccelerationMagnitude, new Vector2(0.01f, sensorReader.AccelerationFilteredMagnitude));
        diagramAccelerationMagnitude.InputPoint(lineAccelerationMagnitude_NotFiltered, new Vector2(0.01f, sensorReader.AccelerationRaw.magnitude));
        if(sensorReader.IsStillHighThresholdEnabled)
            diagramAccelerationMagnitude.InputPoint(lineAccelerationMagnitudeThreshold, new Vector2(0.01f, sensorReader.StillHighThreshold));



        diagramAccelerationAvg.InputPoint(lineAccelerationMagnitudeForAvg, new Vector2(0.01f, sensorReader.AccelerationFilteredMagnitude));
        diagramAccelerationAvg.InputPoint(lineAccelerationMovingAverage, new Vector2(0.01f, sensorReader.StillMovingAverage));
        if(sensorReader.IsWaveStepDeltaCheckActive)
        {
            diagramAccelerationAvg.InputPoint(lineAccelerationMovingAverageMax, new Vector2(0.01f, sensorReader.StillMovingAverage+sensorReader.StillWaveStepDelta));
            diagramAccelerationAvg.InputPoint(lineAccelerationMovingAverageMin, new Vector2(0.01f, sensorReader.StillMovingAverage - sensorReader.StillWaveStepDelta));
        }


        diagramAccelerationAvgDist.InputPoint(lineAccelerationMagnitudeForAvgDist, new Vector2(0.01f, sensorReader.AccelerationFilteredMagnitude));
        if(sensorReader.IsMaxDistanceBetweenAveragesEnabled)
            diagramAccelerationAvgDist.InputPoint(lineAccelerationMaxDistanceBetweenAverages, new Vector2(0.01f, sensorReader.StillMaxDistanceBetweenAverages+sensorReader.StillAvg));
        diagramAccelerationAvgDist.InputPoint(lineAccelerationMovingAverageDist, new Vector2(0.01f, sensorReader.StillMovingAverage));
        diagramAccelerationAvgDist.InputPoint(lineAccelerationStillAverageDist, new Vector2(0.01f, sensorReader.StillAvg));

    }



    private void OnDestroy()
    {
        sensorReader.DisableSensors();
    }

}