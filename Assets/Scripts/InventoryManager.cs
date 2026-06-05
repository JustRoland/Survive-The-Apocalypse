using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class InventoryManager : MonoBehaviour, IManager
{
    private VisualElement _rootInventory;

    //Items
    private ObservableArray<ItemAndAmount> _items;
    private ItemScriptableObject _sidePanelSelectedItem;

    //Item slots in Inventory
    List<Button> _icons = new();
    List<Label> _labels = new();

    //Side Panel
    private VisualElement _sidePanel;
    private VisualElement _sidePanelIcon;
    private Label _sidePanelName;
    private Label _sidePanelDescription;
    private Button _sidePanelEquipButton;
    private Button _sidePanelDropButton;
    

    private void OnEnable()
    {
        Debug.Assert(GameManager.Instance.InventoryDocument != null, "There is no active document");
        
        //Inventory
        _rootInventory = GameManager.Instance.InventoryDocument.rootVisualElement;

        Debug.Assert(_rootInventory != null, "There is no root in document");

        _icons = _rootInventory.Query<Button>("Icon").ToList();
        _labels = _rootInventory.Query<Label>("Number").ToList();

        _icons.ForEach(SetupButton);

        _sidePanel = _rootInventory.Q<VisualElement>("SidePanel");
        _sidePanelIcon = _sidePanel.Q<VisualElement>("ItemIcon");
        _sidePanelName = _sidePanel.Q<Label>("ItemName");
        _sidePanelDescription = _sidePanel.Q<Label>("ItemDescription");
        _sidePanelEquipButton = _sidePanel.Q<Button>("EquipButton");
        _sidePanelDropButton = _sidePanel.Q<Button>("DropButton");

        _items = new ObservableArray<ItemAndAmount>(_icons.Count);
        _items.OnAnyValueChanged += HandleValueChange;
        _sidePanelEquipButton.clicked += EnterPlacementMode;
        
        _rootInventory.visible = false;
    }
    
    
    public void Initialize()
    {
        HandleValueChange(null);
    }

    private void OnDisable()
    {
        _items.OnAnyValueChanged -= HandleValueChange;
        _sidePanelEquipButton.clicked -= EnterPlacementMode;
        _icons.ForEach(UnsetButton);
    }

    private void SetupButton(Button button)
    {
        button.clickable.clickedWithEventInfo += LoadItemInfoOnSidePanel;
    }

    private void UnsetButton(Button button)
    {
        button.clickable.clickedWithEventInfo -= LoadItemInfoOnSidePanel;
    }


    private void LoadItemInfoOnSidePanel(EventBase obj = null)
    {
        var buttonIndex = obj?.target is Button button ? _icons.IndexOf(button) : 0;
        var item = _items[buttonIndex];
        
        if (item == null)
        {
            _sidePanelSelectedItem = null;
            _sidePanelIcon.style.backgroundImage = null;
            _sidePanelName.text = string.Empty;
            _sidePanelDescription.text = string.Empty;
            return;
        }
        
        _sidePanelSelectedItem = item.item;
        _sidePanelIcon.style.backgroundImage = new StyleBackground(item.item.inventorySprite);
        _sidePanelName.text = item.item.name;
        _sidePanelDescription.text = item.item.description;
    }

    private void EnterPlacementMode()
    {
        if (!_sidePanelSelectedItem.objectPrefab) return;
        GameManager.Instance.EnterPlacementMode(_sidePanelSelectedItem); //TODO Clean this up.
    }


    private void HandleValueChange(ItemAndAmount[] obj)
    {
        for (int i = 0; i < _icons.Count; i++)
        {
            if (_items[i] == null)
            {
                _icons[i].style.backgroundImage = null;
                _labels[i].text = string.Empty;
                continue;
            }

            _icons[i].style.backgroundImage = new StyleBackground(_items[i].item.inventorySprite);
            _labels[i].text = _items[i].amount > 1 ? _items[i].amount.ToString() : string.Empty;
        }
    }

    public bool GiveItem(ItemAndAmount item)
    {
        for (int i = 0; i < _items.Count; i++)
        {
            if (item.item.name != _items[i].item.name) continue;

            _items[i].amount += item.amount;
            _items.Invoke();
            return true;
        }

        return _items.TryAdd(item);
    }

    public bool TakeItem(ItemAndAmount item)
    {
        for (int i = 0; i < _items.Length; i++)
        {
            if (_items[i] == null || item.item.name != _items[i].item.name) continue;

            if (_items[i].amount > item.amount)
            {
                _items[i].amount -= item.amount;
                _items.Invoke();
                return true;
            }

            if (_items[i].amount == item.amount)
            {
                return _items.TryRemove(item);
            }
        }

        return false;
    }

    public bool CheckItem(ItemAndAmount item)
    {
        for (int i = 0; i < _items.Count; i++)
        {
            if (item.item.name != _items[i].item.name) continue;

            if (_items[i].amount >= item.amount)
            {
                return true;
            }
        }

        return false;
    }

    public void OpenInventory(bool open = true) 
    {
        LoadItemInfoOnSidePanel();
        _rootInventory.visible = open;
    }

}