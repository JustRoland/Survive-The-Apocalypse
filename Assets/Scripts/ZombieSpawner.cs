using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ZombieSpawner : MonoBehaviour, INoiseListener
{
    private AudioSource _audioSource;
    
    [SerializeField] private Zombie zombiePrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Vector2 spawnRange = new(2f, 4f);
    [SerializeField] private Vector2 delayMinMax = new(10, 30);
    [SerializeField] private float speedVariation = .1f;
    [SerializeField] private int maxZombies = 5;
    [SerializeField] private AudioClip spawnSound;
    private readonly List<Zombie> _zombies = new();
    private bool _scheduledSpawn;


    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        GameManager.Instance.Sound.NoiseTrigger.Subscribe(this);
    }

    private void OnDisable()
    {
        GameManager.Instance.Sound.NoiseTrigger.Unsubscribe(this);
    }

    private void Update()
    {
        if (_scheduledSpawn) SpawnZombie();
    }

    // TODO: Convert to Zombie Factory.
    private void SpawnZombie()
    {
        _scheduledSpawn = false;
        if (_zombies.Count < maxZombies)
        {
            if (!_audioSource.isPlaying)
            {
                _audioSource.clip = spawnSound;
                _audioSource.Play();
            }
            
            int amount = Mathf.Min(Random.Range((int)spawnRange.x, (int)spawnRange.y), maxZombies - _zombies.Count);

            for (int i = 0; i < amount; i++)
            {
                var z = Instantiate(zombiePrefab, spawnPoints[Random.Range(0, spawnPoints.Length)].position,
                    Quaternion.identity);
                z.Initialize(spawner: this, chaseTarget: GameManager.Instance.Player.transform, speedVariation);
                _zombies.Add(z);
            }
        }
    }

    public void Remove(Zombie zombie) => _zombies.Remove(zombie);
    public Vector3 Position => transform.position;

    public void OnNoiseTriggered(Vector3 noiseLocation)
    {
        print("Noise heard by spawner.");
        _scheduledSpawn = true;
    }
}