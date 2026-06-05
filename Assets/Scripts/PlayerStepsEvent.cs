using System;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class PlayerStepsEvent : MonoBehaviour
{

    PlayerController _player;
    [SerializeField] private AudioClip[] stepsAudio;

    private void Awake()
    {
        _player = GetComponentInParent<PlayerController>();
    }

    public void PlayStepSound()
    {
        int random = Random.Range(0, stepsAudio.Length);
        _player.PlaySound(stepsAudio[random]);
    }
}
