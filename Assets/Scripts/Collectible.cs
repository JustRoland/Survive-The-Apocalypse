using Task_Manager.Scripts;
using UnityEngine;


public class Collectible : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemScriptableObject item;
    private int _amount;
    private CompleteTaskScript _completeTask;

    private Collider2D _collider2D;
    private Rigidbody2D _rigidbody2D;
    

    public void Initialize(int amount)
    {
        _collider2D = GetComponent<Collider2D>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _completeTask = GetComponent<CompleteTaskScript>();
        
        _amount = amount;
    }
    
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.layer == 7)
        {
            _collider2D.isTrigger = true;
            _rigidbody2D.bodyType = RigidbodyType2D.Static;
            _rigidbody2D.excludeLayers = 0;
        }
    }

    public void Interact()
    {
        GameManager.Instance.Inventory.GiveItem(new ItemAndAmount(item, _amount));
        if (_completeTask) _completeTask.CompleteTask(0);
        Destroy(gameObject);
    }

    public void Exit()
    {
        
    }

    public void OnPlayerEnter()
    {
        Interact();
    }

    public void OnPlayerStay()
    {
        
    }

    public void OnPlayerExit()
    {
        
    }
}
