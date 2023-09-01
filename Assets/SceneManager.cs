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
    private static SceneManager instance;
    public SensorsReader sensorReader;
    //public GameObject recordStepsButton;
    public GameObject recordStillButton;
    //public GameObject analyseStepsButton;
    public GameObject analyseStillButton;
    public GameObject checkStillButton;
    public GameObject stillStatus;
    public GameObject stateMachineStepDetectionStatus;

    private bool recordStillPressed = false;
    private bool checkStillPressed = false;

    public DD_DataDiagram diagramAccelerationX;
    public DD_DataDiagram diagramAccelerationY;
    public DD_DataDiagram diagramAccelerationZ;
    public DD_DataDiagram diagramAccelerationMagnitude;
    public DD_DataDiagram diagramAccelerationAvg;

    private GameObject lineAccelerationX;
    private GameObject lineAccelerationX_NotFiltered;

    private GameObject lineAccelerationY;
    private GameObject lineAccelerationY_NotFiltered;

    private GameObject lineAccelerationZ;
    private GameObject lineAccelerationZ_NotFiltered;

    private GameObject lineAccelerationMagnitude;
    private GameObject lineAccelerationMagnitude_NotFiltered;
    private GameObject lineAccelerationMagnitudeThreshold;

    private GameObject lineAccelerationMagnitudeForAvg;
    private GameObject lineAccelerationMovingAverage;
    private GameObject lineAccelerationMovingAverageMax;
    private GameObject lineAccelerationMovingAverageMin;
    private GameObject lineAccelerationMaxDistanceBetweenAverages;

    private Color colorX = Color.red;
    private Color colorX_NotFiltered = Color.grey;
    private Color colorY = Color.green;
    private Color colorY_NotFiltered = Color.grey;
    private Color colorZ = Color.blue;
    private Color colorZ_NotFiltered = Color.grey;    
    private Color colorMagnitude = Color.magenta;
    private Color colorMagnitude_NotFiltered = Color.grey;
    private Color colorMagnitudeThreshold = Color.white;

    private Color colorMovingAverage = Color.green;
    private Color colorMovingAverageMax = Color.red;
    private Color colorMovingAverageMin = Color.red;
    private Color colorMaxDistanceBetweenAverages = Color.blue;

    public TextMeshProUGUI text, text2, scrollViewText;
    public GameObject PhoneModelAttitude,PhoneModelAcceleration;
    public GameObject AccelerationArrow;
    Vector3 accelerationScaleVector = new Vector3(1, 1, 0.5f);

    public UnityEvent<float> OnStillHighThresholdChangedFromSensor = new UnityEvent<float>();
    public UnityEvent<float> OnStillMaxDistanceFromAverageChangedFromSensor = new UnityEvent<float>();

    public bool logsEnabled = false;
    public GameObject pauseLogsButton;

    private bool sensorReaderStarted = false;
    void Start()
    {
        instance = this;
        //text = GetComponent<TextMeshProUGUI>();

        //text2 = GetComponent<TextMeshProUGUI>();
        //checkStillButton.SetActive(false);
        stillStatus.SetActive(false);
        stateMachineStepDetectionStatus.SetActive(false);

        //lineAccelerationX = diagramAccelerationX.AddLine(colorX.ToString(), colorX);
        //lineAccelerationX_NotFiltered = diagramAccelerationX.AddLine(colorX_NotFiltered.ToString(), colorX_NotFiltered);
        //lineAccelerationY = diagramAccelerationY.AddLine(colorY.ToString(), colorY);
        //lineAccelerationY_NotFiltered = diagramAccelerationY.AddLine(colorY_NotFiltered.ToString(), colorY_NotFiltered);
        //lineAccelerationZ = diagramAccelerationZ.AddLine(colorZ.ToString(), colorZ);
        //lineAccelerationZ_NotFiltered = diagramAccelerationZ.AddLine(colorZ_NotFiltered.ToString(), colorZ_NotFiltered);

        lineAccelerationMagnitude = diagramAccelerationMagnitude.AddLine(colorMagnitude.ToString(), colorMagnitude);
        lineAccelerationMagnitude_NotFiltered = diagramAccelerationMagnitude.AddLine(colorMagnitude_NotFiltered.ToString(), colorMagnitude_NotFiltered);
        lineAccelerationMagnitudeThreshold = diagramAccelerationMagnitude.AddLine(colorMagnitudeThreshold.ToString(), colorMagnitudeThreshold);

        
        lineAccelerationMagnitudeForAvg = diagramAccelerationAvg.AddLine(colorMagnitude.ToString(), colorMagnitude);
        lineAccelerationMovingAverage = diagramAccelerationAvg.AddLine(colorMovingAverage.ToString(), colorMovingAverage);
        lineAccelerationMovingAverageMax = diagramAccelerationAvg.AddLine(colorMovingAverageMax.ToString(), colorMovingAverageMax);
        lineAccelerationMovingAverageMin = diagramAccelerationAvg.AddLine(colorMovingAverageMin.ToString(), colorMovingAverageMin);
        lineAccelerationMaxDistanceBetweenAverages = diagramAccelerationAvg.AddLine(colorMaxDistanceBetweenAverages.ToString(), colorMaxDistanceBetweenAverages);

        
     

        //StartCoroutine(ZoomAndDrag(diagramAccelerationX));
        //StartCoroutine(ZoomAndDrag(diagramAccelerationY));
        //StartCoroutine(ZoomAndDrag(diagramAccelerationZ));
        StartCoroutine(ZoomAndDrag(diagramAccelerationMagnitude));
        StartCoroutine(ZoomAndDrag(diagramAccelerationAvg));

        sensorReader.Setup(0.1f, OnStillCallback, OnMovingCallback);
        sensorReaderStarted = true;
        sensorReader.OnStillHighThresholdChanged += (newThreshold)=> { OnStillHighThresholdChangedFromSensor.Invoke(newThreshold); };
        sensorReader.OnStillMaxDistanceFromAverageChanged += (newThreshold)=> { OnStillMaxDistanceFromAverageChangedFromSensor.Invoke(newThreshold); };
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

    public IEnumerator ZoomAndDrag(DD_DataDiagram diagram)
    {
        yield return new WaitForSeconds(0.1f);
        diagram.RaiseMoveEvent(0, 20f);
        diagram.RaiseZoomEvent(-2f, -2f);
    }

    public void OnZoomDiagramPlus()
    {
        diagramAccelerationAvg.RaiseZoomEvent(-0.01f, -0.01f);
    }    
    public void OnZoomDiagramMinus()
    {
        diagramAccelerationAvg.RaiseZoomEvent(+0.01f, +0.01f);
    }

    public void OnAnalyseStillPressed()
    {
        sensorReader.AnalyseStillData();
    }

  
    public void OnRecordStillPressed()
    {
        if(!recordStillPressed) 
        {
            recordStillPressed = true;
            recordStillButton.GetComponentInChildren<TextMeshProUGUI>().text = "STOP RECORDING";
            //checkStillButton.SetActive(false);
            sensorReader.SetStillRecorder(true);
        }
        else
        {
            recordStillPressed = false;
            recordStillButton.GetComponentInChildren<TextMeshProUGUI>().text = "TRAIN STAY STILL";
            sensorReader.SetStillRecorder(false);
        }
    }

    public void OnCheckStandingStillPressed()
    {
        if(!checkStillPressed)
        {
            checkStillPressed = true;
            checkStillButton.GetComponentInChildren<TextMeshProUGUI>().text = "STOP CHECKING";
            stillStatus.SetActive(true);
            stateMachineStepDetectionStatus.SetActive(true);

            stillStatus.GetComponentInChildren<TextMeshProUGUI>().text = "moving";

            sensorReader.SetStandingStillRecognition(true);
        }
        else
        {
            checkStillPressed = false;

            checkStillButton.GetComponentInChildren<TextMeshProUGUI>().text = "START CHECK STILL";
            stillStatus.GetComponentInChildren<Image>().color = Color.red;
            stillStatus.SetActive(false);
            stateMachineStepDetectionStatus.SetActive(false);

            sensorReader.SetStandingStillRecognition(false);
        }
    }



    void Update() 
    {
        if (!sensorReaderStarted)
        {
            return;
        }
     

        //Debug.Log($"Looking towards _acceleration {_acceleration} of magnitude {_acceleration.magnitude}");
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

    public void OnPauseLogsClicked()
    {
        if(logsEnabled)
        {
            logsEnabled = false;
            pauseLogsButton.GetComponentInChildren<TextMeshProUGUI>().text = "Start Logs";
        }
        else
        {
            logsEnabled = true;
            pauseLogsButton.GetComponentInChildren<TextMeshProUGUI>().text = "Pause Logs";

        }
    }


    private void WriteVisualLogs()
    {
        text.text =
                        //$"Attitude\nX={sensorReader.Attitude.x:#0.00} Y={sensorReader.Attitude.y:#0.00} Z={sensorReader.Attitude.z:#0.00}\n\n" +
                        //$"attitudeEulerProjectedXZ\nX={sensorReader.AttitudeEulerProjectedXZ.x:#0.00} Y={sensorReader.AttitudeEulerProjectedXZ.y:#0.00} Z={sensorReader.AttitudeEulerProjectedXZ.z:#0.00}\n\n" +
                        $"CURRENT STATE ={sensorReader.WaveStateController.currentState.GetType().Name} \n"+
                        $"Moving Avg={sensorReader.StillMovingAvg:#0.00} \n"+
                        $"Max dist btw avg={sensorReader.StillMaxDistanceBetweenAverages:#0.00} \n"+
                        $"Still threshold High={sensorReader.StillHighThreshold:#0.00} \n"+
                        $"Accelerator Magnitude={sensorReader.AccelerationFilteredMagnitude:#0.00}\n\n" +
                        $"LowPassKernelWidthS {sensorReader.LowPassKernelWidthInSeconds:#0.00} \naccelerometerUpdateInterval={sensorReader.AccelerometerUpdateInterval:#0.00}";
        text2.text =
                         $"Acceleration Raw \nX={sensorReader.AccelerationRaw.x:#0.00} Y={sensorReader.AccelerationRaw.y:#0.00} Z={sensorReader.AccelerationRaw.z:#0.00}\n\n" +
                         $"Acceleration Filtered XZ\nX={sensorReader.AccelerationFiltered.x:#0.00} Y={sensorReader.AccelerationFiltered.y:#0.00}  Z= {sensorReader.AccelerationFiltered.z:#0.00}\n\n" +
                         $"Acceleration Filtered XZ\nX={sensorReader.AccelerationFilteredProjectedXZ.x:#0.00} Y={sensorReader.AccelerationFilteredProjectedXZ.y:#0.00}  Z= {sensorReader.AccelerationFilteredProjectedXZ.z:#0.00}\n\n";

        if(logsEnabled)
        {
            if(scrollViewText.text.Length > 2000)
            {
                scrollViewText.text = "";
            }
        
            scrollViewText.text += sensorReader.AccelerationFilteredMagnitude + " - " + sensorReader.WaveStateController.currentState.GetType().Name + "\n";
        }


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
        diagramAccelerationMagnitude.InputPoint(lineAccelerationMagnitudeThreshold, new Vector2(0.01f, sensorReader.StillHighThreshold));


        diagramAccelerationAvg.InputPoint(lineAccelerationMagnitudeForAvg, new Vector2(0.01f, sensorReader.AccelerationFilteredMagnitude));
        diagramAccelerationAvg.InputPoint(lineAccelerationMovingAverage, new Vector2(0.01f, sensorReader.StillMovingAvg));
        diagramAccelerationAvg.InputPoint(lineAccelerationMovingAverageMax, new Vector2(0.01f, sensorReader.StillMovingAvg+sensorReader.StillWaveStepDelta));
        diagramAccelerationAvg.InputPoint(lineAccelerationMovingAverageMin, new Vector2(0.01f, sensorReader.StillMovingAvg - sensorReader.StillWaveStepDelta));
        diagramAccelerationAvg.InputPoint(lineAccelerationMaxDistanceBetweenAverages, new Vector2(0.01f, sensorReader.StillMaxDistanceBetweenAverages));        

    }

    public static void StateMachineStepDetected(WaveStateController waveStateController)
    {
        instance.StartCoroutine(OnStateMachineStepDetected());
        var localMin = waveStateController.goingDown.localMin;
        for (float i = -0.05f; i < 0.05f; i += 0.01f)
        {
            instance.diagramAccelerationAvg.InputPoint(instance.lineAccelerationMaxDistanceBetweenAverages, new Vector2(0.01f, localMin + i));
        }
    }

    public static IEnumerator OnStateMachineStepDetected()
    {
        instance.stateMachineStepDetectionStatus.GetComponentInChildren<TextMeshProUGUI>().text = "STEP!!!";
        instance.stateMachineStepDetectionStatus.GetComponentInChildren<Image>().color = Color.green;
        yield return new WaitForSeconds(0.5f);
        instance.stateMachineStepDetectionStatus.GetComponentInChildren<TextMeshProUGUI>().text = "...";
        instance.stateMachineStepDetectionStatus.GetComponentInChildren<Image>().color = Color.red;
    }

    private void OnDestroy()
    {
        sensorReader.DisableSensors();
    }

}