using System;
using UnityEngine;

public class ZombieDieEvent : MonoBehaviour
{
    private Zombie _zombie;

    public void Initialize(Zombie zombie)
    {
        _zombie = zombie;
    }

    public void Despawn() => _zombie.Despawn();



}
