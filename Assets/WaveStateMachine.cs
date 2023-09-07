using System;
using UnityEngine;

public class WaveStateController
{
    public SensorsReader sensorsReader;
    public WaveState CurrentState
    {
        get => _currentState;
    }
    public WaveState _currentState;
    public GoingUp goingUp;
    public GoingDown goingDown;
    public CheckStep checkStep;
    public bool IsWaveStepDeltaCheckActive
    { 
        get => _isWaveStepDeltaCheckActive;
        set => _isWaveStepDeltaCheckActive = value;
    }
    private bool _isWaveStepDeltaCheckActive;

    public int StepThreshold
    {
        get => _stepThreshold;
        set => _stepThreshold = value;
    }
    private int _stepThreshold;

    public event Action<float,float> OnStepDetected;

    public WaveStateController(SensorsReader sensorsReader)
    {
        this.sensorsReader = sensorsReader;
        goingUp = new GoingUp(this, sensorsReader);
        goingDown = new GoingDown(this, sensorsReader);
        checkStep = new CheckStep(this, sensorsReader);
        _currentState = goingUp;
        //_isWaveStepDeltaCheckActive = false;
        //_stepThreshold = 2;
    }

    public void TransitionToState(WaveState newState)
    {
        _currentState.OnExit();
        _currentState = newState;
        _currentState.OnEnter();
    }
    public void RunState()
    {
        _currentState.OnUpdate();
    }

    public bool HasStep()
    {
        if (checkStep.stepCounter == _stepThreshold)
        {
            if(OnStepDetected != null)
            {
                OnStepDetected.Invoke(goingDown.localMin, goingUp.localMax);
            }
            //we have a full checkStep
            goingUp.crossedThreshold = false;
            goingDown.crossedThreshold = false;
            goingDown.localMin = -1;
            goingUp.localMax = -1;
            checkStep.stepCounter = 0;
            Debug.Log("STEP FROM STATE MACHINE!!!!");
            return true;
        }
        return false;
    }
}



public enum WaveStateId
{
    GoingUp,
    GoingDown,
    CheckStep
}

public class WaveState
{
    public WaveState(WaveStateController waveStateController, SensorsReader sensorsReader)
    {
        this.waveStateController = waveStateController;
        this.sensorsReader = sensorsReader;
    }
    public WaveStateController waveStateController;
    public SensorsReader sensorsReader;
    public bool crossedThreshold;
    private string direction;
    public virtual void OnEnter() { }
    public virtual void OnUpdate() {
        direction = sensorsReader.AccelerationFilteredMagnitude > sensorsReader.PreviousAccelerationFilteredMagnitude ? "UP" : "Down";
        Debug.Log($"From  {sensorsReader.PreviousAccelerationFilteredMagnitude} to {sensorsReader.AccelerationFilteredMagnitude} going {direction}");
    }
    public virtual void OnExit() { }
}

public class GoingUp : WaveState
{
    public GoingUp(WaveStateController waveStateController, SensorsReader sensorsReader) : base(waveStateController, sensorsReader) { }
    public float localMax;
    public override void OnEnter() 
    {
        crossedThreshold = false;
    }
    public override void OnUpdate()
    {
        base.OnUpdate();
        if(sensorsReader.AccelerationFilteredMagnitude > sensorsReader.PreviousAccelerationFilteredMagnitude)
        {
            // going up, stay in state
            // check for threshold crossed
            if (sensorsReader.AccelerationFilteredMagnitude > sensorsReader.StillMovingAvg)
                crossedThreshold = true;
        }
        else
        {
            localMax = sensorsReader.AccelerationFilteredMagnitude;
            waveStateController.TransitionToState(waveStateController.goingDown);
        }
    }
}
public class GoingDown : WaveState
{
    public GoingDown(WaveStateController waveStateController, SensorsReader sensorsReader) : base(waveStateController, sensorsReader) { }
    public float localMin;
    public override void OnEnter()
    {
        crossedThreshold = false;
    }
    public override void OnUpdate()
    {
        base.OnUpdate();
        if (sensorsReader.AccelerationFilteredMagnitude < sensorsReader.PreviousAccelerationFilteredMagnitude)
        {
            // going down, stay in state
            // check for threshold crossed
            if (sensorsReader.AccelerationFilteredMagnitude < sensorsReader.StillMovingAvg)
                crossedThreshold = true;
        }
        else
        {
            localMin = sensorsReader.AccelerationFilteredMagnitude;
            waveStateController.TransitionToState(waveStateController.checkStep);
        }
    }
}
public class CheckStep : WaveState
{
    public CheckStep(WaveStateController waveStateController, SensorsReader sensorsReader) : base(waveStateController, sensorsReader) { }
    public float stepCounter = 0f;
    public override void OnUpdate()
    {
        base.OnUpdate();
        if (waveStateController.goingUp.crossedThreshold && waveStateController.goingDown.crossedThreshold)
        {
            if(
                waveStateController.IsWaveStepDeltaCheckActive &&
                waveStateController.goingUp.localMax - sensorsReader.StillMovingAvg > sensorsReader.StillWaveStepDelta &&
                sensorsReader.StillMovingAvg - waveStateController.goingDown.localMin > sensorsReader.StillWaveStepDelta
                )
            {
                stepCounter += 1;
            }
            else if(!waveStateController.IsWaveStepDeltaCheckActive)
            {
                stepCounter += 1;
            }
        }
        else
        {
            stepCounter = 0f;
        }

        waveStateController.TransitionToState(waveStateController.goingUp);

    }
}


