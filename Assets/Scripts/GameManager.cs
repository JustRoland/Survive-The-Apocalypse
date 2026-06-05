using System;
using System.Linq;
using Task_Manager.Scripts;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;


public interface IManager
{
    public void Initialize();
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private PlayerController _player;
    private InventoryManager _inventory;
    private SoundManager _sound;
    private BuildManager _build;
    private TaskManager _task;
    private MessageManager _message;

    public PlayerController Player => _player;
    public InventoryManager Inventory => _inventory;
    public SoundManager Sound => _sound;
    public BuildManager Build => _build;
    public TaskManager Task => _task;
    public MessageManager Message => _message;


    [SerializeField] private UIDocument craftingDocument;
    [SerializeField] private UIDocument inventoryDocument;

    public UIDocument CraftingDocument => craftingDocument;
    public UIDocument InventoryDocument => inventoryDocument;


    [SerializeField] private GameObject mapReference;
    [SerializeField] private GameObject interactTooltipReference;
    [SerializeField] private GameObject pauseReference;
    [SerializeField] private Vector3 tooltipPositionOffset;

    private VisualElement _rootCrafting;
    private AudioSource _audioSource;


    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        _audioSource = GetComponent<AudioSource>();
        _player = FindFirstObjectByType<PlayerController>();

        _inventory = GetComponentInChildren<InventoryManager>();
        _sound = GetComponentInChildren<SoundManager>();
        _build = GetComponentInChildren<BuildManager>();
        _task = GetComponentInChildren<TaskManager>();
        _message = GetComponentInChildren<MessageManager>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        //Crafting
        _rootCrafting = CraftingDocument.rootVisualElement;

        Debug.Assert(CraftingDocument != null, "There is no active document");
        Debug.Assert(_rootCrafting != null, "There is no root in document");

        _rootCrafting.visible = false;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None).OfType<IManager>()
            .ToList().ForEach(m => m.Initialize());

        OpenCrafting(false);
        OpenMap(false);
        Inventory.OpenInventory(false);

        PlayerController.InputAction.Player.Enable();

        print($"Loaded {scene.name} scene, mode: {mode}");
    }

    private void OnSceneUnloaded(Scene scene)
    {
        PlayerController.InputAction.Player.Disable();
        PlayerController.InputAction.UI.Disable();
        print($"Unloaded {scene.name} scene");
    }


    public void EnterPlacementMode(ItemScriptableObject item)
    {
        //UI and input toggles
        Inventory.OpenInventory(false);
        OpenCrafting(false);
        PlayerController.InputAction.Player.Enable();
        PlayerController.InputAction.UI.Disable();

        _player.isBuilding = Build.EnterBuildMode(item);
    }

    public void ShowInteractTooltip(bool show = true)
    {
        interactTooltipReference?.SetActive(show);
    }

    public void UpdateTooltipPosition(Vector3 callerWorldPosition)
    {
        interactTooltipReference.transform.position =
            Camera.main.WorldToScreenPoint(callerWorldPosition) + tooltipPositionOffset;
    }


    public void OpenCrafting(bool open = true)
    {
        _rootCrafting.visible = open;
    }

    public void OpenMap(bool open = true)
    {
        mapReference.SetActive(open);
    }

    public void PauseGame(bool pause = true)
    {
        Time.timeScale = pause ? 0f : 1f;
        pauseReference.SetActive(pause);
    }

    public void PlaySound(AudioClip clip, bool priority = false)
    {
        if (!clip) return;
        if (_audioSource.isPlaying && !priority) return;
        _audioSource.clip = clip;
        _audioSource.Play();
    }
}