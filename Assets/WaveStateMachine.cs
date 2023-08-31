using System;

public class WaveStateController
{
    private IWaveState currentState;
    private void TransitionToState(IWaveState newState)
    {
        currentState.OnExit();
        currentState = newState;
        currentState.OnEnter();
    }
}

public enum WaveStateId
{
    GoingUp;
    GoingDown;
    CheckStep;
}

public interface IWaveState
{
    public bool crossedThreshold
    public void OnEnter();
    public void OnUpdate();
    public void OnExit();
}

public class GoingUp : IWaveState
{
    OnUpdate()
    {
        if(curAcc > prevAcc)
        {
            // going up, stay in state
            // check for threshold crossed
            if (currAcc > threshold)
                crossedThreshold = true;
        }
        else
        {
            localMax = currAcc;
            WaveStateController.ChangeState(GoingDown)
        }
    }
}
public class GoingDown : IWaveState
{
    OnUpdate()
    {
        if (curAcc < prevAcc)
        {
            // going down, stay in state
            // check for threshold crossed
            if (currAcc < threshold)
                crossedThreshold = true;
        }
        else
        {
            localMin = currAcc;
            WaveStateController.ChangeState(CheckStep)
        }
    }
}
public class CheckStep : IWaveState
{
    OnUpdate()
    {
        if (GoingUp.crossedThreshold && GoingDown.crossedThreshold)
        {
            if(localMax - localMin > stepThreshold)
            {
                StepCounter += 0.5;
            }
        }
        else
        {
            WaveStateController.ChangeState(GoingUp)
        }
    }
    OnExit()
    {
        if (StepCounter == 1)
        {
            //we have a full step
            OnStep.Invoke();
            GoingUp.crossedThreshold = false;
            GoingDown.crossedThreshold = false;
            localMin = -1;
            localMax = -1;
        }
    }
}

}
