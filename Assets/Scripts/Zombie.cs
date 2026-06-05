using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class Zombie : MonoBehaviour, INoiseListener
{
    private static readonly int IsWalking = Animator.StringToHash("isWalking");
    private static readonly int OnHit = Animator.StringToHash("onHit");
    private static readonly int OnDead = Animator.StringToHash("onDead");

    private static int _id;
    public readonly int ID = _id++;
    private ParticleSystem _emitter;
    private SpriteRenderer _spriteRenderer;
    private Rigidbody2D _rigidbody;
    private Animator _animator;
    private AudioSource _audioSource;
    
    [SerializeField] private Transform target;
    private Vector3 _targetPosition;
    private ZombieSpawner _spawner;

    [SerializeField] private float walkingSpeed = 1;
    [SerializeField] private float health = 100;
    private bool _isDead;
    
    [SerializeField] private UnityEvent onDeath = new ();
    
    [SerializeField] private AudioClip[] growlSounds;
    [SerializeField] private AudioClip[] hitSounds;
    [SerializeField] private AudioClip[] deathSounds;

    private CancellationTokenSource _cancellationToken;
        
    private void Awake()
    {
        _emitter = GetComponentInChildren<ParticleSystem>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _animator = GetComponentInChildren<Animator>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _audioSource = GetComponent<AudioSource>();
        GetComponentInChildren<ZombieDieEvent>().Initialize(this);
        
    }

    private void OnEnable()
    {
        _cancellationToken = new CancellationTokenSource();
    }

    private void OnDisable()
    {
        _cancellationToken.Cancel();
        _cancellationToken.Dispose();
    }

    private void Start()
    {
        GameManager.Instance.Sound.NoiseTrigger.Subscribe(this);
        
        PlayGrowlSound(_cancellationToken).Forget();
    }

    private void Update()
    {
        if (_isDead) return;
        
        //This is incredibly inefficient...
        if (target) _targetPosition = target.position;
        var walking = _targetPosition != Vector3.zero; 
        _animator.SetBool(IsWalking, walking);
        if (walking)
        {
            _rigidbody.position =
                Vector2.MoveTowards(_rigidbody.position, _targetPosition, walkingSpeed * Time.deltaTime);
            _spriteRenderer.flipX = _targetPosition.x < transform.position.x;
        }
    }

    public void Initialize(ZombieSpawner spawner, Transform chaseTarget, float speedVariation = 0)
    {
        _spawner = spawner;
        target = chaseTarget;
        walkingSpeed += Random.Range(-speedVariation, speedVariation);
    }

    public void Hit(float damage)
    {
        if (_isDead) return;
        MessageManager.Instance.NewMessage($"{damage} damage to {name}");
        _animator.SetTrigger(OnHit);
        PlaySound(hitSounds[Random.Range(0, hitSounds.Length)], true);
        _emitter.Play();
        health -= damage;


        if (health <= 0)
        {
            _isDead = true;
            _spawner?.Remove(this);
            _rigidbody.simulated = false;
            _animator.SetTrigger(OnDead);
            _cancellationToken.Cancel();
            PlaySound(deathSounds[Random.Range(0, deathSounds.Length)], true);
            GameManager.Instance.Sound.NoiseTrigger.Unsubscribe(this);
            onDeath.Invoke();
        }

    }

    public async UniTask PlayGrowlSound(CancellationTokenSource token)
    {
        while (true)
        {
            
            float delay = Random.Range(8f, 20f);
            
            await UniTask.WaitForSeconds(delay/2);
            
            if (token.IsCancellationRequested) return;
            PlaySound(growlSounds[Random.Range(0, growlSounds.Length)]);
            
            await UniTask.WaitForSeconds(delay/2);
            
            await UniTask.Yield();
        }
    }

    public void Despawn()
    {
        _emitter.Stop();
        onDeath.RemoveAllListeners();
        Destroy(gameObject);
    }

    public Vector3 Position => transform.position;
    public void OnNoiseTriggered(Vector3 noiseLocation)
    {
        print($"Noise heard by {name}");
        _targetPosition = noiseLocation;
    }

    public void PlaySound(AudioClip clip, bool priority = false)
    {
        if (_audioSource.isPlaying && !priority) return;
        _audioSource.clip = clip;
        _audioSource.Play();
    }
}
