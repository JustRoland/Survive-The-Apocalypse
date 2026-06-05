using System;
using UnityEngine;

public class SoundManager : MonoBehaviour, IManager
{
    private NoiseTrigger _noiseTrigger;
    
    public NoiseTrigger NoiseTrigger => _noiseTrigger;

    public void Initialize()
    {
        _noiseTrigger = GetComponent<NoiseTrigger>();
        NoiseTrigger.Initialize();
    }

    public void Terminate()
    {
        NoiseTrigger.Terminate();
    }
}
