using System;
using UnityEngine;

public class Trader : Crafting
{
    private static readonly int AltIdle = Animator.StringToHash("altIdle");
    private SimpleTimer _timer;
    private Animator _animator;

    private new void Start()
    {
        base.Start();
        _animator = GetComponentInChildren<Animator>();
        _timer = new SimpleTimer(TimerType.Stopwatch);
        _timer.StartTimer();
        _timer.MinuteEvent.AddListener(AltIdleAnimation);
        
    }

    private void Update()
    {
        _timer.UpdateTimer(Time.deltaTime);
    }

    private void AltIdleAnimation()
    {
        _animator.SetTrigger(AltIdle);
    }
    
    
    
}
