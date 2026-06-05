using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class ResourceProvider : MonoBehaviour, IInteractable
{
    private AudioSource _audioSource;
    
    [FormerlySerializedAs("woodPrefab")] [SerializeField] private Collectible resourcePrefab;
    [SerializeField] private int amountPerResource;
    [SerializeField] private int totalAmount;
    
    [SerializeField] private float randomForceMin, randomForceMax;
    
    [SerializeField] private AudioClip[] interactSounds;


    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void Interact()
    {
        var collectible = Instantiate(resourcePrefab, transform.position, Quaternion.identity);
        var forceX = Random.Range(randomForceMin, randomForceMax);
        var forceY = Random.Range(0, randomForceMax);
        collectible.Initialize(amountPerResource >= totalAmount ? amountPerResource : totalAmount);
        collectible.GetComponent<Rigidbody2D>().AddForce(new Vector2(forceX, forceY), ForceMode2D.Impulse);
        
        totalAmount -= amountPerResource;
        
        _audioSource.clip = interactSounds[Random.Range(0, interactSounds.Length)];
        _audioSource.Play();
        
        if (totalAmount <= 0) Destroy(gameObject);

    }

    public void Exit()
    {
        
    }

    public void OnPlayerEnter()
    {
        GameManager.Instance.UpdateTooltipPosition(transform.position);
        GameManager.Instance.ShowInteractTooltip();

    }

    public void OnPlayerStay()
    {
        GameManager.Instance.UpdateTooltipPosition(transform.position);
    }

    public void OnPlayerExit()
    {
        GameManager.Instance.ShowInteractTooltip(false);
    }
}
