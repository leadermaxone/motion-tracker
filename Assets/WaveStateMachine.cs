using UnityEngine;

public class WaveStateController
{
    public SensorsReader sensorsReader;
    public WaveState currentState;
    public GoingUp goingUp;
    public GoingDown goingDown;
    public CheckStep checkStep;

    public WaveStateController(SensorsReader sensorsReader)
    {
        this.sensorsReader = sensorsReader;
        goingUp = new GoingUp(this, sensorsReader);
        goingDown = new GoingDown(this, sensorsReader);
        checkStep = new CheckStep(this, sensorsReader);
        currentState = goingUp;
    }
    public void TransitionToState(WaveState newState)
    {
        currentState.OnExit();
        currentState = newState;
        currentState.OnEnter();
    }
    public void RunState()
    {
        currentState.OnUpdate();
    }

    public bool HasStep()
    {
        if (checkStep.stepCounter == 1f)
        {
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
    public virtual void OnEnter() { }
    public virtual void OnUpdate() { }
    public virtual void OnExit() { }
}

public class GoingUp : WaveState
{
    public GoingUp(WaveStateController waveStateController, SensorsReader sensorsReader) : base(waveStateController, sensorsReader) { }
    public float localMax;
    public override void OnUpdate()
    {
        base.OnUpdate();
        if(sensorsReader.AccelerationFiltered.magnitude > sensorsReader.PreviousAccelerationFiltered.magnitude)
        {
            // going up, stay in state
            // check for threshold crossed
            if (sensorsReader.AccelerationFiltered.magnitude > sensorsReader.StillMovingAvg)
                crossedThreshold = true;
        }
        else
        {
            localMax = sensorsReader.AccelerationFiltered.magnitude;
            waveStateController.TransitionToState(waveStateController.goingDown);
        }
    }
}
public class GoingDown : WaveState
{
    public GoingDown(WaveStateController waveStateController, SensorsReader sensorsReader) : base(waveStateController, sensorsReader) { }
    public float localMin;
    public override void OnUpdate()
    {
        base.OnUpdate();
        if (sensorsReader.AccelerationFiltered.magnitude < sensorsReader.PreviousAccelerationFiltered.magnitude)
        {
            // going down, stay in state
            // check for threshold crossed
            if (sensorsReader.AccelerationFiltered.magnitude < sensorsReader.StillMovingAvg)
                crossedThreshold = true;
        }
        else
        {
            localMin = sensorsReader.AccelerationFiltered.magnitude;
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
                waveStateController.goingUp.localMax - sensorsReader.StillMovingAvg > sensorsReader.StillWaveStepDelta &&
                sensorsReader.StillMovingAvg - waveStateController.goingDown.localMin > sensorsReader.StillWaveStepDelta
                )
            {
                stepCounter += 0.5f;
            }
        }

        waveStateController.TransitionToState(waveStateController.goingUp);

    }
}


