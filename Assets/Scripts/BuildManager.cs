using System;
using System.Collections.Generic;
using System.Linq;
using Task_Manager.Scripts;
using UnityEngine;
using UnityEngine.Serialization;

public class BuildManager : MonoBehaviour, IManager
{
    private ItemScriptableObject _itemToPlace;
    private GameObject _object;

    [SerializeField] private float socketDetectionRange;
    [SerializeField] private SpriteRenderer ghostObject;
    [SerializeField] private Color validGhostColor;
    [SerializeField] private Color invalidGhostColor;
    [SerializeField] private AudioClip placeSound;
    [SerializeField] private AudioClip enterBuildModeSound;

    private readonly List<BuildingSocket> _sockets = new();


    public void Initialize()
    {
        _sockets.Clear();
        _sockets.AddRange(FindObjectsByType<BuildingSocket>(FindObjectsInactive.Exclude, FindObjectsSortMode.None));
        _sockets.ForEach(s => s.Initialize());
    }


    public bool EnterBuildMode(ItemScriptableObject item)
    {
        if (!item) return false;
        if (!item.placeable) return false;
        _itemToPlace = item;
        _object = Instantiate(item.objectPrefab, transform);
        GameManager.Instance.PlaySound(enterBuildModeSound, true);
        return true;
    }

    public void ExitBuildMode()
    {
        ghostObject.gameObject.SetActive(false);
        Destroy(_object);
    }

    public BuildingSocket ValidateBuildingSocket(Vector3 mousePosition)
    {
        Debug.Assert(_sockets.Count > 0, "Must have at least one socket");


        //move placementObject to mouse pos
        _object.transform.position = mousePosition;

        //look for sockets around position and pick closest that is within min distance
        var socket = _sockets
            .Select(s => new
            {
                s, distance = (
                    s.transform.position - mousePosition).sqrMagnitude
            })
            .FirstOrDefault(p => p.distance < socketDetectionRange)?.s;
        if (!socket)
        {
            ghostObject.gameObject.SetActive(false);
            return null;
        }

        //snap ghost to socket
        ghostObject.gameObject.SetActive(true);
        ghostObject.transform.position = socket.transform.position;

        //check validity of socket
        if (socket.Occupied)
        {
            ghostObject.color = invalidGhostColor;
            return null;
        }

        ghostObject.color = validGhostColor;
        return socket;
    }

    public void PlaceBuilding(Vector3 mousePosition)
    {
        var socket = ValidateBuildingSocket(mousePosition);
        if (!socket) return;
        socket.Occupy(_object.gameObject);
        MessageManager.Instance.NewMessage($"Placing {_itemToPlace.name} in slot {socket.name}");
        print(
            $"Try take item {_itemToPlace.name}: {GameManager.Instance.Inventory.TakeItem(new ItemAndAmount(_itemToPlace, 1))}");
        _object.transform.position = ghostObject.transform.position;
        if (_object.TryGetComponent(out IPlacementObject component)) component.OnPlacement();
        _object = null;
        ghostObject.gameObject.SetActive(false);
        if (TryGetComponent(out CompleteTaskScript task))
        {
            task.CompleteTask(0);
        }
        GameManager.Instance.PlaySound(placeSound, true);
        GameManager.Instance.EnterPlacementMode(null); // Exits build mode
    }
}