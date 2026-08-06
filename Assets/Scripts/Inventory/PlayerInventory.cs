using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerInventory : MonoBehaviour
{
    public UIDocument uiDocument;

    [SerializeField] private InventoryUI inventoryUI;
    public WeaponManager weaponManager;

    private PlayerAction playerActions;
    private VisualElement root;
    private VisualElement inventoryPanel;

    void Awake()
    {
        playerActions = new PlayerAction();
        root = uiDocument.rootVisualElement;
        inventoryPanel = root.Q<VisualElement>("InventoryPanel");
        inventoryUI.OnItemRemoved += OnItemRemoved;

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
    private void OnItemRemoved(InventoryItem item, string slotName)
    {
        if (weaponManager.CurrentWeapon == item)
            weaponManager.CurrentWeapon = null;
    }

    private void OnDestroy()
    {
        inventoryUI.OnItemRemoved -= OnItemRemoved;

    }

    void HotbarPerformed(int slotNum)
    {
        InventoryItem hotbarItem = inventoryUI.GetItemInSlot("inv-slot-item-" + slotNum);

        // Kliknutí na prázdný slot - neřešíme (nebo můžeš chtít unequip, ale obvykle ne)
        if (hotbarItem == null)
        {
            weaponManager.CurrentWeapon = null;
        }

        //  Pokud je to obyčejný item (lektvar atd.)
        if (hotbarItem.category == ItemCategory.Item)
        {
            inventoryUI.UseItem(hotbarItem, "inv-slot-item-" + slotNum);
            return; // Ukončíme, dál už nic neřešíme
        }

        // Pokud je to zbraň
        if (hotbarItem.category == ItemCategory.Weapon)
        {
            // Pokud už tuhle konkrétní zbraň držíme v ruce, tak ji unequipneme (schováme)
            if (weaponManager.CurrentWeapon == hotbarItem)
            {
                weaponManager.CurrentWeapon = null;
            }
            else
            {
                // Jinak ji normálně equipneme
                weaponManager.CurrentWeapon = hotbarItem;
            }
        }
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