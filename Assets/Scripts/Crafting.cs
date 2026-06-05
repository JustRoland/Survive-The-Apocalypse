using System;
using System.Linq;
using Task_Manager.Scripts;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class Crafting : MonoBehaviour, IPlacementObject, IInteractable
{
    [SerializeField] protected RecipeScriptableObject recipe1, recipe2, recipe3;
    [SerializeField] protected AudioClip craftSound;
    
    protected UIDocument _craftingDocument;
    protected VisualElement _rootCrafting;
    protected Collider2D _collider;
    protected VisualElement _craftingWindow1, _craftingWindow2, _craftingWindow3;
    protected VisualElement _craftingIcon1, _craftingIcon2, _craftingIcon3;
    protected Label _itemName1, _itemName2, _itemName3;
    protected Label _itemDescription1, _itemDescription2, _itemDescription3;
    protected Button _craftingButton1, _craftingButton2, _craftingButton3;
    protected Action _craftAction1, _craftAction2, _craftAction3;

    protected void Start()
    {
        Debug.Assert(GameManager.Instance, "No Game Manager");
        _collider = GetComponent<Collider2D>();
        _craftingDocument = GameManager.Instance.CraftingDocument;
        _rootCrafting = _craftingDocument.rootVisualElement;

        Debug.Assert(_craftingDocument != null, "There is no active document");
        Debug.Assert(_rootCrafting != null, "There is no root in document");

        _craftingWindow1 = _rootCrafting.Q("CraftWindow1");
        _craftingIcon1 = _craftingWindow1.Q("ItemIcon");
        _itemName1 = _craftingWindow1.Q<Label>("ItemName");
        _itemDescription1 = _craftingWindow1.Q<Label>("ItemDescription");
        _craftingButton1 = _craftingWindow1.Q<Button>("CraftButton");


        _craftingWindow2 = _rootCrafting.Q("CraftWindow2");
        _craftingIcon2 = _craftingWindow2.Q("ItemIcon");
        _itemName2 = _craftingWindow2.Q<Label>("ItemName");
        _itemDescription2 = _craftingWindow2.Q<Label>("ItemDescription");
        _craftingButton2 = _craftingWindow2.Q<Button>("CraftButton");


        _craftingWindow3 = _rootCrafting.Q("CraftWindow3");
        _craftingIcon3 = _craftingWindow3.Q("ItemIcon");
        _itemName3 = _craftingWindow3.Q<Label>("ItemName");
        _itemDescription3 = _craftingWindow3.Q<Label>("ItemDescription");
        _craftingButton3 = _craftingWindow3.Q<Button>("CraftButton");

    }

    protected void OnDisable()
    {
        Exit();
    }

    public void Interact()
    {
        GameManager.Instance.OpenCrafting();
        PlayerController.InputAction.Player.Disable();
        PlayerController.InputAction.UI.Enable();
        
        _craftingIcon1.style.backgroundImage = new StyleBackground(recipe1.item.inventorySprite);
        _itemName1.text = recipe1.item.name;
        _itemDescription1.text = recipe1.item.description;
        _craftAction1 = () => { CraftItem(recipe1); };
        _craftingButton1.clicked += _craftAction1;
        _craftingButton1.SetEnabled(CheckIfCraftable(recipe1));
        
        _craftingIcon2.style.backgroundImage = new StyleBackground(recipe2.item.inventorySprite);
        _itemName2.text = recipe2.item.name;
        _itemDescription2.text = recipe2.item.description;
        _craftAction2 = () => { CraftItem(recipe2); };
        _craftingButton2.clicked += _craftAction2;
        _craftingButton2.SetEnabled(CheckIfCraftable(recipe2));
        
        _craftingIcon3.style.backgroundImage = new StyleBackground(recipe3.item.inventorySprite);
        _itemName3.text = recipe3.item.name;
        _itemDescription3.text = recipe3.item.description;
        _craftAction3 = () => { CraftItem(recipe3); };
        _craftingButton3.clicked += _craftAction3;
        _craftingButton3.SetEnabled(CheckIfCraftable(recipe3));

    }

    public void Exit()
    {
        _craftingButton1.clicked -= _craftAction1;
        _craftingButton2.clicked -= _craftAction2;
        _craftingButton3.clicked -= _craftAction3;
    }

    protected bool CheckIfCraftable(RecipeScriptableObject recipe)
    {
        return recipe.requiredItems.ToList().All(r => GameManager.Instance.Inventory.CheckItem(r));
    }

    protected void CraftItem(RecipeScriptableObject recipe)
    {
        foreach (var item in recipe.requiredItems)
        {
            if (!GameManager.Instance.Inventory.CheckItem(item)) return;
        }

        MessageManager.Instance.NewMessage($"Crafted {recipe.amount} {recipe.item.name}(s).");

        foreach (var item in recipe.requiredItems)
        {
            GameManager.Instance.Inventory.TakeItem(item);
        }
        
        GameManager.Instance.Inventory.GiveItem(new ItemAndAmount(recipe.item, recipe.amount));
        
        if (craftSound) GameManager.Instance.PlaySound(craftSound);
        
        GameManager.Instance.Task.CompleteTask(recipe.task);
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

    public void OnPlacement()
    {
        _collider.enabled = true;
    }
}