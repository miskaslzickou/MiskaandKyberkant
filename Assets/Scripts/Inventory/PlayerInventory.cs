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