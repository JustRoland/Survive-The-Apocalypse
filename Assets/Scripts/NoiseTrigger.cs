using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface INoiseListener
{
    public Vector3 Position { get; }
    
    void OnNoiseTriggered(Vector3 noiseLocation);
}


public class NoiseTrigger : MonoBehaviour
{
    private readonly List<INoiseListener> _listeners = new();

    public void Subscribe(INoiseListener listener)
    {
        _listeners.Add(listener);
    }

    public void Unsubscribe(INoiseListener listener)
    {
        _listeners.Remove(listener);
    }

    public void Initialize()
    {
        _listeners.Clear();
    }

    public void Terminate()
    {
        
    }


    public void TriggerNoise(Vector2 noiseLocation, int intensity)
    {
        print($"Noise triggered at {noiseLocation}");
        foreach (var l in _listeners.Where(l => Vector3.Distance(l.Position, noiseLocation) <= intensity))
        {
            l.OnNoiseTriggered(noiseLocation);
        }
    }
}
