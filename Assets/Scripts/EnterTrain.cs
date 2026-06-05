using UnityEngine;

public class EnterTrain : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform trainRelocatePosition;
    
    public void Interact()
    {
        GameManager.Instance.Player.transform.position = trainRelocatePosition.position;
        Camera.main.transform.position = trainRelocatePosition.position;
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
