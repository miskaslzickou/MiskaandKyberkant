using System;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public event Action<InventoryItem> OnActiveWeaponChanged;
    private InventoryItem _currWeapon;   
    private SpriteRenderer spriteRenderer;
    public InventoryItem CurrentWeapon
    {
        get => _currWeapon;
        set
        {
            if (_currWeapon != value)
            {
                _currWeapon = value;
                OnActiveWeaponChanged?.Invoke(_currWeapon);
                EquipWeapon();
            }
        }
    }

    private void EquipWeapon()
    {
       spriteRenderer.sprite = CurrentWeapon != null ? CurrentWeapon.icon : null;
    }
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
