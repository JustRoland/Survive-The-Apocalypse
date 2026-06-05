using UnityEngine;
using System;
using System.Collections.Generic;
using Task_Manager.Scripts;
using UnityEngine.Serialization;


[Serializable]
[CreateAssetMenu(fileName = "Recipe", menuName = "Items/New Recipe")]
public class RecipeScriptableObject : ScriptableObject
{
    public ItemScriptableObject item;
    public int amount = 1;
    public ItemAndAmount[] requiredItems;
    public TaskScriptableObject task;
}

[Serializable]
public class ItemAndAmount
{
    public ItemAndAmount(ItemScriptableObject item, int amount)
    {
        this.item = item;
        this.amount = amount;
    }

    public ItemScriptableObject item;
    public int amount;

    public override string ToString() => $"{item.name}: {amount}";

    public override bool Equals(object obj)
    {
        if (obj is not ItemAndAmount other) return false;

        return other.item == item;
    }

    public override int GetHashCode()
    {
        return item.GetHashCode();
    }
}