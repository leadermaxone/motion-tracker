using System;
using UnityEngine;

public class StepRecognitionMachine
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
    public bool IsWaveAmplitudeCheckActive
    { 
        get => _isWaveAmplitudeCheckActive;
        set => _isWaveAmplitudeCheckActive = value;
    }
    private bool _isWaveAmplitudeCheckActive;

    public int NumberOfPeaksForAStep
    {
        get => _numberOfPeaksForAStep;
        set => _numberOfPeaksForAStep = value;
    }
    private int _numberOfPeaksForAStep;

    public event Action<float,float> OnStepDetected;

    public StepRecognitionMachine(SensorsReader sensorsReader)
    {
        this.sensorsReader = sensorsReader;
        goingUp = new GoingUp(this, sensorsReader);
        goingDown = new GoingDown(this, sensorsReader);
        checkStep = new CheckStep(this, sensorsReader);
        _currentState = goingUp;
        //_isWaveAmplitudeCheckActive = false;
        //_numberOfPeaksForAStep = 2;
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
        if (checkStep.numberOfUpDowns == _numberOfPeaksForAStep)
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
            checkStep.numberOfUpDowns = 0;
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
    public WaveState(StepRecognitionMachine stepMachine, SensorsReader sensorsReader)
    {
        this.stepMachine = stepMachine;
        this.sensorsReader = sensorsReader;
    }
    public StepRecognitionMachine stepMachine;
    public SensorsReader sensorsReader;
    public bool crossedThreshold;
    public virtual void OnEnter() { }
    public virtual void OnUpdate() { }
    public virtual void OnExit() { }
}

public class GoingUp : WaveState
{
    public GoingUp(StepRecognitionMachine waveStateController, SensorsReader sensorsReader) : base(waveStateController, sensorsReader) { }
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
            if (sensorsReader.AccelerationFilteredMagnitude > sensorsReader.MovingAverage)
                crossedThreshold = true;
        }
        else
        {
            localMax = sensorsReader.AccelerationFilteredMagnitude;
            stepMachine.TransitionToState(stepMachine.goingDown);
        }
    }
}
public class GoingDown : WaveState
{
    public GoingDown(StepRecognitionMachine waveStateController, SensorsReader sensorsReader) : base(waveStateController, sensorsReader) { }
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
            if (sensorsReader.AccelerationFilteredMagnitude < sensorsReader.MovingAverage)
                crossedThreshold = true;
        }
        else
        {
            localMin = sensorsReader.AccelerationFilteredMagnitude;
            stepMachine.TransitionToState(stepMachine.checkStep);
        }
    }
}
public class CheckStep : WaveState
{
    public CheckStep(StepRecognitionMachine waveStateController, SensorsReader sensorsReader) : base(waveStateController, sensorsReader) { }
    public float numberOfUpDowns = 0f;
    public override void OnUpdate()
    {
        base.OnUpdate();
        if (stepMachine.goingUp.crossedThreshold && stepMachine.goingDown.crossedThreshold)
        {
            // Alternatively check only for upper threshold, as it seems from data that soft steps are not symmetrical
            if(
                stepMachine.IsWaveAmplitudeCheckActive &&
                stepMachine.goingUp.localMax - sensorsReader.MovingAverage > sensorsReader.MaxWaveAmplitude &&
                sensorsReader.MovingAverage - stepMachine.goingDown.localMin > sensorsReader.MaxWaveAmplitude
                )
            {
                numberOfUpDowns += 1;
            }
            else if(!stepMachine.IsWaveAmplitudeCheckActive)
            {
                numberOfUpDowns += 1;
            }
        }
        else
        {
            numberOfUpDowns = 0f;
        }

        stepMachine.TransitionToState(stepMachine.goingUp);

    }
}


