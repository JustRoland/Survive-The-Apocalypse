using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
[CreateAssetMenu(fileName = "Item", menuName = "Items/New Item")]
public class ItemScriptableObject : ScriptableObject
{
    public string description;
    public Sprite inventorySprite;
    public GameObject objectPrefab;
    public bool placeable;
}
