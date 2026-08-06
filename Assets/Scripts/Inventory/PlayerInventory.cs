using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerInventory : MonoBehaviour
{
    public UIDocument uiDocument;

    [SerializeField] private InventoryUI inventoryUI;

    private PlayerAction playerActions;
    private VisualElement root;
    private VisualElement inventoryPanel;

    void Awake()
    {
        playerActions = new PlayerAction();
        root = uiDocument.rootVisualElement;
        inventoryPanel = root.Q<VisualElement>("InventoryPanel");

        if (inventoryPanel != null)
        {
            inventoryPanel.style.display = DisplayStyle.None; // Hide the inventory panel initially
        }

        playerActions.Player.OpenInventory.performed += ctx =>
        {
            if (inventoryPanel != null)
            {
                inventoryPanel.style.display = inventoryPanel.style.display == DisplayStyle.None ? DisplayStyle.Flex : DisplayStyle.None;
            }
        };
        playerActions.Player.HotbarSlot1.performed += ctx =>
        {
            HotbarPerformed(0);
        };
        playerActions.Player.HotbarSlot2.performed += ctx =>
        {
            HotbarPerformed(1);
        };
        playerActions.Player.HotbarSlot3.performed += ctx =>
        {
            HotbarPerformed(2);
        };
        playerActions.Player.HotbarSlot4.performed += ctx =>
        {
            HotbarPerformed(3);
        };
        playerActions.Player.HotbarSlot5.performed += ctx =>
        {
            HotbarPerformed(4);
        };

    }

    void HotbarPerformed(int slotNum)
    {
      InventoryItem  hotbarItem =inventoryUI.GetItemInSlot("inv-slot-item-"+slotNum);
        if(hotbarItem != null &&hotbarItem.category == ItemCategory.Item) {
            inventoryUI.UseItem(hotbarItem, "inv-slot-item-"+slotNum);
        }
    }
    void Start()
    {
        
    }

    private void OnEnable()
    {
        playerActions.Enable();
    }

    private void OnDisable()
    {
        playerActions.Disable();
    }
}