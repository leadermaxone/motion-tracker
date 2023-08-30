using System;

public interface IWaveState
{
    public WaveStateId Id { get; }
    public bool Enabled { get; set; }
    public void OnEnter();
    public void OnUpdate();
    public void OnExit();
}



}
