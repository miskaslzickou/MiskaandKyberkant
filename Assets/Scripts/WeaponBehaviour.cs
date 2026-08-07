using UnityEngine;

public abstract class WeaponBehaviour : MonoBehaviour
{
    protected GameObject player;
    protected GameObject weapon;
    protected InventoryItem weaponItem;

    // Odstraň Awake úplně

    public virtual void Initialize(InventoryItem inventoryItem)
    {
        weaponItem = inventoryItem;
        weapon = transform.parent.gameObject;
        player = transform.parent.parent.gameObject;
    }

    public abstract void Attack();
    public abstract void OnEquip();
    public abstract void OnUnequip();
    public virtual void WhileEquipped() { }

    private void Update()
    {
        WhileEquipped();
    }
}