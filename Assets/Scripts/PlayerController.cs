using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Input;
using Task_Manager.Scripts;

public class PlayerController : MonoBehaviour
{
    private static PlayerController _instance;
    
    private Rigidbody2D _rigidbody;
    public static InputSystem_Actions InputAction;
    private LineRenderer _lineRenderer;
    private SpriteRenderer _spriteRenderer;
    private Collider2D _collider;
    private Animator _animator;
    private AudioSource _audioSource;

    private static readonly int IsRunning = Animator.StringToHash("isRunning");
    private static readonly int IsAiming = Animator.StringToHash("isAiming");
    private static readonly int IsJumping = Animator.StringToHash("isJumping");
    private static readonly int OnShoot = Animator.StringToHash("onShoot");
    private static readonly int OnHit = Animator.StringToHash("onHit");

    public float VelocityX => _rigidbody.linearVelocityX;

    [SerializeField] private GameObject crosshairReference;
    private Animator _crosshairAnimator;
    [SerializeField] private Transform bulletOrigin;
    private Vector3 _bulletOriginPosition;
    [SerializeField] private float baseDamage = 25f;
    [SerializeField] private float preciseDamageMultiplierBody = 2f;
    [SerializeField] private float preciseDamageMultiplierHead = 4f;
    [SerializeField] private float meleeDamage = 75f;


    private bool _isGrounded;
    private bool _isOnTrain;
    [SerializeField] private float normalWalkSpeed = 6;
    [SerializeField] private float aimWalkSpeed = 2;
    [SerializeField] private float acceleration = 50;
    [SerializeField] private float jumpForce = 5;

    private float _currentVelocity;
    private IInteractable _currentInteractable;

    private bool _isAiming;
    [SerializeField] private float gunRange = 10;
    [SerializeField] private Gradient normalShotColor;
    [SerializeField] private Gradient criticalShotColor;
    public bool isBuilding;

    private IInteractable _interactableCache;
    private CompleteTaskScript _completeTask;
    private bool _t0, _t1, _t2, _t3, _t4, _t5, _t6, _t7, _t8, _t9; // ugly task trackers

    [SerializeField] private AudioClip gunShotSound;
    [SerializeField] private AudioClip gunSlapSound;
    [SerializeField] private AudioClip openMap;


    private void Awake()
    {
        if (_instance == null) _instance = this;
        else Destroy(gameObject);
        
        _rigidbody = GetComponent<Rigidbody2D>();
        InputAction = new InputSystem_Actions();
        _lineRenderer = GetComponent<LineRenderer>();
        _collider = GetComponent<Collider2D>();
        _animator = GetComponentInChildren<Animator>();
        _crosshairAnimator = crosshairReference.GetComponent<Animator>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _bulletOriginPosition = bulletOrigin.localPosition;
        _completeTask = GetComponent<CompleteTaskScript>();
        _audioSource = GetComponent<AudioSource>();

        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        InputAction.Player.Enable();
    }

    private void OnDisable()
    {
        InputAction.Player.Disable();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        switch (other.gameObject.layer)
        {
            case 7:
                _isGrounded = true;
                break;
            case 6:
                _isGrounded = true;
                _isOnTrain = true;
                if (!_t9) _completeTask.CompleteTask(9);
                break;
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        switch (other.gameObject.layer)
        {
            case 7:
                _isGrounded = false;
                break;
            case 6:
                _isGrounded = false;
                _isOnTrain = false;
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out IInteractable interactable))
        {
            interactable.OnPlayerEnter();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.TryGetComponent(out IInteractable interactable))
        {
            interactable.OnPlayerStay();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out IInteractable interactable))
        {
            interactable.OnPlayerExit();
        }
    }


    private void Update()
    {
        var mousePos = Camera.main.ScreenToWorldPoint(InputAction.Player.Mouse.ReadValue<Vector2>());
        _animator.SetBool(IsJumping, !_isGrounded);

        Move();
        Jump();
        Attack(mousePos);
        Aim(mousePos);
        Melee();


        if (isBuilding)
        {
            var pos = mousePos - new Vector3(0, 0, -10);
            GameManager.Instance.Build.ValidateBuildingSocket(pos);
            return;
        }

        Interact();
        OpenInventory();
        OpenMap();
        Pause();
    }


    private void Move()
    {
        var speed = _isAiming ? aimWalkSpeed : normalWalkSpeed;
        float targetVelocity = InputAction.Player.Move.ReadValue<float>() * speed;
        _currentVelocity = Mathf.Lerp(_currentVelocity, targetVelocity,
            1f - Mathf.Exp(-acceleration / normalWalkSpeed * Time.deltaTime));

        _rigidbody.linearVelocityX = _currentVelocity;
        if (!_isAiming) _spriteRenderer.flipX = _currentVelocity < 0;
        _animator.SetBool(IsRunning, Mathf.Abs(_rigidbody.linearVelocityX) >= 0.3f);

        if (_t0 && _t1) return;
        switch (targetVelocity)
        {
            case > 0:
                //pressed D
                _completeTask.CompleteTask(1);
                _t1 = true;
                break;
            case < 0:
                //pressed A
                _completeTask.CompleteTask(0);
                _t0 = true;
                break;
        }
    }

    private void Jump()
    {
        if (_isGrounded && InputAction.Player.Jump.WasPressedThisFrame())
        {
            _rigidbody.linearVelocityY = jumpForce;


            if (_t2) return;
            _t2 = true;
            _completeTask.CompleteTask(2);
        }
    }

    private void Interact()
    {
        if (InputAction.Player.Interact.WasPressedThisFrame())
        {
            var results = new List<Collider2D>();
            _collider.Overlap(results);
            foreach (var col in results)
            {
                if (col.TryGetComponent(out IInteractable interactable))
                {
                    interactable.Interact();
                    _interactableCache = interactable;

                    if (!_t3)
                    {
                        _t3 = true;
                        _completeTask.CompleteTask(3);
                    }

                    return;
                }
            }
        }

        if (InputAction.UI.Interact.WasPressedThisFrame())
        {
            if (_interactableCache != null)
            {
                _interactableCache.Exit();
                _interactableCache = null;
            }

            GameManager.Instance.Inventory.OpenInventory(false);
            GameManager.Instance.OpenCrafting(false);
            EnablePlayerInput();
        }
    }

    private void OpenInventory()
    {
        if (InputAction.Player.Inventory.WasPressedThisFrame())
        {
            if (!_t4)
            {
                _t4 = true;
                _completeTask.CompleteTask(4);
            }

            GameManager.Instance.Inventory.OpenInventory();
            EnableUIInput();
        }

        if (InputAction.UI.Inventory.WasPressedThisFrame())
        {
            if (_interactableCache != null)
            {
                _interactableCache.Exit();
                _interactableCache = null;
            }

            GameManager.Instance.Inventory.OpenInventory(false);
            GameManager.Instance.OpenCrafting(false);
            EnablePlayerInput();
        }
    }

    private void Aim(Vector3 mousePos)
    {
        _isAiming = InputAction.Player.Aim.IsPressed();


        if (_isAiming)
        {
            if (isBuilding)
            {
                print($"is aiming: {_isAiming}, is building: {isBuilding}");
                isBuilding = false;
                GameManager.Instance.Build.ExitBuildMode();
                return;
            }

            if (!_t6)
            {
                _t6 = true;
                _completeTask.CompleteTask(6);
            }

            crosshairReference.transform.position = mousePos - new Vector3(0, 0, -10);
            _spriteRenderer.flipX = mousePos.x < transform.position.x;

            //ugly workaround to move bullet origin to correct side.
            float flip = _spriteRenderer.flipX ? -1f : 1f;
            bulletOrigin.localPosition = new Vector3(_bulletOriginPosition.x * flip, _bulletOriginPosition.y,
                _bulletOriginPosition.z);
        }


        crosshairReference.SetActive(_isAiming);
        _animator.SetBool(IsAiming, _isAiming);
    }

    private void Attack(Vector2 mousePos)
    {
        if (!InputAction.Player.Attack.WasPressedThisFrame()) return;
        if (isBuilding)
        {
            GameManager.Instance.Build.PlaceBuilding(mousePos);
        }

        if (!_isAiming) return;

        if (!_t5)
        {
            _t5 = true;
            _completeTask.CompleteTask(5);
        }

        _animator.SetTrigger(OnShoot);
        _crosshairAnimator.SetTrigger(OnShoot);
        var hit = HorizontalHitDetection();
        PlaySound(gunShotSound, true);
        GameManager.Instance.Sound.NoiseTrigger.TriggerNoise(transform.position, 10);
        DisplayBullet(hit.position, hit.critical, .1f).Forget();
    }

    private void Melee()
    {
        if (InputAction.Player.Melee.WasPressedThisFrame())
        {
            _animator.SetTrigger(OnHit);


            var zombies = new List<Collider2D>();
            zombies.AddRange(Physics2D.OverlapPointAll(transform.position + new Vector3(1, 0, 0), 1 << 10));
            zombies.AddRange(Physics2D.OverlapPointAll(transform.position + new Vector3(-1, 0, 0), 1 << 10));
            zombies.ForEach(z => z.gameObject.GetComponent<Zombie>().Hit(meleeDamage));
            if (zombies.Count > 0) PlaySound(gunSlapSound, true);

            if (_t8) return;
            
            _t8 = true;
            _completeTask.CompleteTask(8);
        }
    }

    private async UniTask DisplayBullet(Vector2 target, bool crit, float delay)
    {
        target = target == Vector2.zero
            ? bulletOrigin.position +
              (crosshairReference.transform.position - bulletOrigin.position).normalized * gunRange
            : target;
        _lineRenderer.SetPositions(new Vector3[] { bulletOrigin.position, target });
        _lineRenderer.colorGradient = crit ? criticalShotColor : normalShotColor;
        _lineRenderer.enabled = true;
        await UniTask.WaitForSeconds(delay);
        _lineRenderer.enabled = false;
        await UniTask.Yield();
    }

    private (Vector2 position, bool critical) HorizontalHitDetection()
    {
        //first check if crosshair is over a zombie
        bool critical = false;
        int zombieID = 0;
        float damageMultiplier = 1f;
        var preciseHit = Physics2D.Raycast(crosshairReference.transform.position, Vector2.zero, 0, 1 << 10);
        if (preciseHit && preciseHit.transform.TryGetComponent(out Zombie zombie))
        {
            zombieID = zombie.ID;
            critical = true;
            //headshot?
            damageMultiplier = preciseHit.point.y > zombie.transform.position.y + .3f
                ? preciseDamageMultiplierHead
                : preciseDamageMultiplierBody;
        }


        //then check if line crosses a zombie, and apply critical damage is crosshair is also on the same zombie
        var horizontalHit = Physics2D.Raycast(bulletOrigin.position,
            crosshairReference.transform.position - bulletOrigin.position, gunRange, 1 << 10);
        if (horizontalHit)
        {
            var z = horizontalHit.transform.GetComponent<Zombie>();
            critical = critical && z.ID == zombieID;
            var dmg = critical ? baseDamage * damageMultiplier : baseDamage;
            z.Hit(dmg);
            return (horizontalHit.point, critical);
        }

        return (Vector3.zero, false);
    }

    private void OpenMap()
    {
        if (_isOnTrain && InputAction.Player.Map.WasPressedThisFrame())
        {
            if (!_t7)
            {
                _t7 = true;
                _completeTask.CompleteTask(7);
            }

            GameManager.Instance.OpenMap();
            EnableUIInput();
            PlaySound(openMap, true);
        }

        if (InputAction.UI.Map.WasPressedThisFrame())
        {
            GameManager.Instance.OpenMap(false);
            EnablePlayerInput();
            PlaySound(openMap, true);
        }
    }

    private void Pause()
    {
        if (InputAction.Player.Pause.WasPressedThisFrame())
        {
            EnableUIInput();
            GameManager.Instance.PauseGame();
        }

        if (InputAction.UI.Pause.WasPressedThisFrame())
        {
            EnablePlayerInput();
            GameManager.Instance.PauseGame(false);
        }
    }

    public void EnablePlayerInput()
    {
        InputAction.UI.Disable();
        InputAction.Player.Enable();
    }

    public void EnableUIInput()
    {
        InputAction.Player.Disable();
        InputAction.UI.Enable();
    }

    public void PlaySound(AudioClip clip, bool priority = false)
    {
        if (_audioSource.isPlaying && !priority) return;
        _audioSource.clip = clip;
        _audioSource.Play();
    }
}