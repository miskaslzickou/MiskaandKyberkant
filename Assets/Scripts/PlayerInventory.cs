using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


public class PlayerInventory : MonoBehaviour
{
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public UIDocument uiDocument;
   private PlayerAction playerActions;
   private VisualElement root;
   private VisualElement inventoryPanel;
   [SerializeField] private InventoryUI inventoryUI;
    void Awake()
    {
        playerActions = new PlayerAction();
        root = uiDocument.rootVisualElement;
        inventoryPanel = root.Q<VisualElement>("InventoryPanel");
        inventoryPanel.style.display = DisplayStyle.None; // Hide the inventory panel initially

        playerActions.Player.OpenInventory.performed += ctx =>
        {
            inventoryPanel.style.display = inventoryPanel.style.display == DisplayStyle.None ? DisplayStyle.Flex : DisplayStyle.None;
        };
     

    }
    private void OnEnable()
    {
        playerActions.Enable();

    }
    private void OnDisable()
    {
        playerActions.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
