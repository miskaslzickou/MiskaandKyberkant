using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.UIElements;
public class ItemPickup : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public UIDocument uiDocument;
    public InventoryUI inventoryUI;
    private VisualElement root;
    private Label itemPickupLabel;
    private PlayerAction playerActions;
    private DroppedItem nearestItem;
    private void Awake()
    {
        root= uiDocument.rootVisualElement;
        itemPickupLabel = new Label();
        itemPickupLabel.style.position = Position.Absolute;
        itemPickupLabel.style.display = DisplayStyle.None;
        itemPickupLabel.style.backgroundColor=new Color(0,0, 0, 0.5f);
        itemPickupLabel.style.color = Color.white;
        root.Add(itemPickupLabel);
        playerActions = new PlayerAction();
        playerActions.Player.ItemPickup.performed += OnPickup;

    }
    private void OnPickup(InputAction.CallbackContext ctx)
    {
        if (nearestItem == null) return;

        bool added = inventoryUI.AddItemToContainer(nearestItem.item);
        if (added)
        {
            Destroy(nearestItem.gameObject);
            HideItemPickupUI();
            nearestItem = null;
        }
    }
    private void OnEnable()
    {
        playerActions.Enable();
    }

    private void OnDisable()
    {
        playerActions.Disable(); // důležité, jinak zůstanou aktivní i po zničení objektu
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("DroppedItem"))
            ShowItemPickupUI();

    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("DroppedItem")) return;

        DroppedItem candidate = other.GetComponent<DroppedItem>();
        if (nearestItem == null ||
            Vector2.Distance(transform.position, other.transform.position) <
            Vector2.Distance(transform.position, nearestItem.transform.position))
        {
            nearestItem = candidate;
        }

        UpdateItemPickupUI(other);
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("DroppedItem"))
        {
            if (other.GetComponent<DroppedItem>() == nearestItem)
                nearestItem = null;
            HideItemPickupUI();
        }
    }
    public void ShowItemPickupUI()
    {
        itemPickupLabel.style.display = DisplayStyle.Flex;
        itemPickupLabel.BringToFront();

    }
    public void UpdateItemPickupUI(Collider2D other)
    {
        if (nearestItem == null) return;

        Vector2 screenPos = Camera.main.WorldToScreenPoint(nearestItem.transform.position);
        screenPos.y = Screen.height - screenPos.y - 15;

        Vector2 panelPos = RuntimePanelUtils.ScreenToPanel(root.panel, screenPos);

        itemPickupLabel.style.left = panelPos.x;
        itemPickupLabel.style.top = panelPos.y;
        itemPickupLabel.text = $"Press [{playerActions.Player.ItemPickup.GetBindingDisplayString()}] to pick up {nearestItem.item.itemName}";
    }
    public void HideItemPickupUI()
    {
        itemPickupLabel.style.display = DisplayStyle.None;
    }
}
