using UnityEngine;

public class DroppedItem : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private SpriteRenderer spriteRenderer;
    private PolygonCollider2D polygonCollider;
    public InventoryItem item; // Reference to the item data
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        polygonCollider = GetComponent<PolygonCollider2D>();
    }
    public void InitializeItem(Sprite newSprite)
    {
        // 1. Nastavíme správný sprite
        spriteRenderer.sprite = newSprite;

        // 2. Automaticky vygenerujeme tvar collideru podle nového sprite
        // Unity 6 a novější má přímo metodu CreateFromSprite
        polygonCollider.pathCount = 0; // Reset starých cest
        polygonCollider.CreateFromSprite(newSprite);

   
    }

}
