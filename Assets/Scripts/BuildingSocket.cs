using UnityEngine;

public class BuildingSocket : MonoBehaviour
{
    [SerializeField] private bool startOccupied;
    public bool Occupied { get; private set; }
    public GameObject ObjectReference { get; private set; }

    public void Initialize()
    {
        Occupied = startOccupied;
    }
    
    public void Occupy(GameObject obj)
    {
        Occupied = true;
        ObjectReference = obj;
    }
}
