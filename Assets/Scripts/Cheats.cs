using UnityEngine;
using UnityEngine.InputSystem;

public class Cheats : MonoBehaviour
{
    [Header("Odkaz na Inventář")]
    public InventoryUI inventoryUI;

    [Header("Itemy k naklikání (Scriptable Objects)")]
    public ItemData cheatWeapon;
    public ItemData cheatArmor;
    public ItemData cheatConsumable;
    

    void Update()
    {
        // Pro cheaty je nejlepší použít klasický Input, ať se s tím nemusíš 
        // složitě bindovat v New Input Systemu. Tyhle klávesy před vydáním hry smažeš.

        // Zmáčkni F1 pro zbraň
        if (Keyboard.current == null) return;

        // 3. Nový způsob čtení kláves: wasPressedThisFrame
        if (Keyboard.current.f1Key.wasPressedThisFrame)
        {
            GiveCheatItem(cheatWeapon);
        }

        if (Keyboard.current.f2Key.wasPressedThisFrame)
        {
            GiveCheatItem(cheatArmor);
        }
        if(Keyboard.current.f3Key.wasPressedThisFrame)
        {
            GiveCheatItem(cheatConsumable);
        }
    }

    private void GiveCheatItem(ItemData data)
    {
        if (data == null || inventoryUI == null)
        {
            Debug.LogWarning("[CHEAT] Chybí item nebo inventář v Inspectoru!");
            return;
        }

        // 1. Tady využijeme ten náš parádní konstruktor, co jsme napsali minule!
        InventoryItem newItem = new InventoryItem(data);

        // 2. Hodíme to do inventáře
        inventoryUI.AddItemToContainer(newItem);

        Debug.Log($"[CHEAT] 🎁 Vývojářský drop: Hráč dostal {data.itemName}!");
    }
}
