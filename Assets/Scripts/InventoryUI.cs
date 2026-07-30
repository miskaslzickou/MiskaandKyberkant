using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

// ── Item categories ──────────────────────────────────────────────────────────
public enum ItemCategory { Hat, Chest, Boots, Ring, Weapon, Item }

// ── Slot types ────────────────────────────────────────────────────────────────
public enum SlotType { Hat, Chest, Boots, Ring, ItemSlot }

// ── Data class for an inventory item ─────────────────────────────────────────
[System.Serializable]
public class InventoryItem
{
    public string itemName;
    [TextArea(2, 4)]
    public string description;
    public ItemCategory category;
    public Sprite icon;
}

// ── Drag manipulator ──────────────────────────────────────────────────────────
public class ItemDragManipulator : PointerManipulator
{
    private VisualElement _sourceSlot;
    private Vector2 _startPosition;
    private Vector3 _pointerStartPosition;
    private bool _dragging;

    private readonly VisualElement _root;
    private readonly InventoryItem _item;
    private readonly InventoryUI _ui;

    public ItemDragManipulator(VisualElement target, VisualElement root,
                               InventoryItem item, InventoryUI ui)
    {
        this.target = target;
        _root = root;
        _item = item;
        _ui = ui;
    }

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<PointerDownEvent>(OnPointerDown);
        target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        target.RegisterCallback<PointerUpEvent>(OnPointerUp);
        target.RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
        target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
        target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
        target.UnregisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
    }

    private void OnPointerDown(PointerDownEvent evt)
    {
        _sourceSlot = target.parent;

        _sourceSlot?.RemoveFromClassList("slot-hover");
        _sourceSlot?.AddToClassList("slot-dragging");

        // Read current translate as start position
        Translate current = target.resolvedStyle.translate;
        _startPosition = new Vector2(current.x.value, current.y.value);
        _pointerStartPosition = evt.position;
        target.CapturePointer(evt.pointerId);
        target.BringToFront();

        // Odstranili jsme Position.Absolute, aby item neskákal mimo slot,
        // a použijeme pouze čistou vizuální vrstvu (translate) pro tažení.
        _dragging = true;
    }

    private void OnPointerMove(PointerMoveEvent evt)
    {
        if (!_dragging || !target.HasPointerCapture(evt.pointerId)) return;
        Vector3 delta = evt.position - _pointerStartPosition;
        SetTranslate(target, new Vector2(_startPosition.x + delta.x, _startPosition.y + delta.y));
    }

    private void OnPointerUp(PointerUpEvent evt)
    {
        if (_dragging && target.HasPointerCapture(evt.pointerId))
            target.ReleasePointer(evt.pointerId);
    }

    private void OnPointerCaptureOut(PointerCaptureOutEvent evt)
    {
        // Vždy odstraň slot-hover a slot-dragging bez ohledu na _dragging flag
        _sourceSlot?.RemoveFromClassList("slot-hover");
        _sourceSlot?.RemoveFromClassList("slot-dragging");

        if (!_dragging) return;
        _dragging = false;

        List<VisualElement> allSlots = _root.Query<VisualElement>(className: "inv-slot").ToList();

        VisualElement bestSlot = null;
        float bestDistSq = float.MaxValue;

        foreach (VisualElement slot in allSlots)
        {
            if (slot.userData is not SlotType slotType) continue;
            if (!IsCompatible(_item.category, slotType)) continue;
            if (!target.worldBound.Overlaps(slot.worldBound)) continue;

            Vector2 slotPos = RootSpaceOf(slot);
            Vector2 current = _startPosition;
            float distSq = ((Vector2)(slotPos - current)).sqrMagnitude;
            if (distSq < bestDistSq) { bestDistSq = distSq; bestSlot = slot; }
        }

        if (bestSlot != null)
        {
            _ui.OnItemDroppedInSlot(_item, bestSlot, _sourceSlot);
        }
        else
        {
            // Reset translace při nepovedeném puštění
            SetTranslate(target, new Vector2(0, 0));
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private static void SetTranslate(VisualElement el, Vector2 pos)
        => el.style.translate = new StyleTranslate(new Translate(pos.x, pos.y));

    public static bool IsCompatible(ItemCategory item, SlotType slot) => slot switch
    {
        SlotType.Hat => item == ItemCategory.Hat,
        SlotType.Chest => item == ItemCategory.Chest,
        SlotType.Boots => item == ItemCategory.Boots,
        SlotType.Ring => item == ItemCategory.Ring,
        SlotType.ItemSlot => true,
        _ => false
    };

    private Vector3 RootSpaceOf(VisualElement slot)
    {
        Vector2 worldPos = slot.parent.LocalToWorld(slot.layout.position);
        return _root.WorldToLocal(worldPos);
    }
}

// ── Main MonoBehaviour ────────────────────────────────────────────────────────
public class InventoryUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private UIDocument uiDocument;

    [Header("Starting items")]
    [SerializeField] private List<InventoryItem> startingItems = new();

    // slotName → item currently in that slot
    private readonly Dictionary<string, InventoryItem> _slotContents = new();
    // itemName → its VisualElement
    private readonly Dictionary<string, VisualElement> _itemElements = new();

    private VisualElement _root;
    private Label _tooltipLabel;

    // ── Events ────────────────────────────────────────────────────────────────
    public event System.Action<InventoryItem, string> OnItemInserted;   // item, slotName
    public event System.Action<InventoryItem, string> OnItemRemoved;    // item, slotName

    // ── Unity lifecycle ───────────────────────────────────────────────────────
    private void OnEnable()
    {
        _root = uiDocument.rootVisualElement;

        // Inicializace vlastního tooltipu
        _tooltipLabel = new Label();
        _tooltipLabel.style.position = Position.Absolute;
        _tooltipLabel.style.backgroundColor = new StyleColor(new Color(0.15f, 0.15f, 0.15f, 0.95f));
        _tooltipLabel.style.color = new StyleColor(Color.white);
        _tooltipLabel.style.paddingTop = 5;
        _tooltipLabel.style.paddingBottom = 5;
        _tooltipLabel.style.paddingLeft = 8;
        _tooltipLabel.style.paddingRight = 8;
        _tooltipLabel.style.borderTopLeftRadius = 4;
        _tooltipLabel.style.borderTopRightRadius = 4;
        _tooltipLabel.style.borderBottomLeftRadius = 4;
        _tooltipLabel.style.borderBottomRightRadius = 4;
        _tooltipLabel.style.display = DisplayStyle.None;
        _tooltipLabel.pickingMode = PickingMode.Ignore; // Aby neblokoval klikání myší
        _root.Add(_tooltipLabel);

        RegisterSlotTypes();
        PopulateStartingItems();
    }

    public void ShowTooltip(string text, Vector2 mousePos)
    {
        _tooltipLabel.text = text;
        _tooltipLabel.style.display = DisplayStyle.Flex;
        _tooltipLabel.style.left = mousePos.x + 15;
        _tooltipLabel.style.top = mousePos.y + 15;
        _tooltipLabel.BringToFront();
    }

    public void HideTooltip()
    {
        _tooltipLabel.style.display = DisplayStyle.None;
    }

    // =========================================================================
    //  PUBLIC API — call these from your game systems
    // =========================================================================

    /// <summary>
    /// Insert an item into a specific slot by slot name.
    /// Returns false if the slot doesn't exist, is incompatible, or is occupied.
    /// </summary>
    public bool InsertItem(InventoryItem item, string slotName)
    {
        VisualElement slot = _root.Q<VisualElement>(slotName);
        if (slot == null)
        {
            Debug.LogWarning($"[Inventory] Slot '{slotName}' not found.");
            return false;
        }

        if (slot.userData is not SlotType slotType)
        {
            Debug.LogWarning($"[Inventory] Slot '{slotName}' has no SlotType assigned.");
            return false;
        }

        if (!ItemDragManipulator.IsCompatible(item.category, slotType))
        {
            Debug.LogWarning($"[Inventory] '{item.itemName}' ({item.category}) is not compatible with slot '{slotName}' ({slotType}).");
            return false;
        }

        if (_slotContents.ContainsKey(slotName))
        {
            Debug.LogWarning($"[Inventory] Slot '{slotName}' is already occupied by '{_slotContents[slotName].itemName}'. Remove it first.");
            return false;
        }

        // Spawn the element inside the slot
        VisualElement element = CreateItemElement(item);
        slot.Add(element);

        // Skrytí původní siluety / ikony pozadí
        slot.AddToClassList("occupied");
        slot.style.unityBackgroundImageTintColor = new StyleColor(Color.clear);

        _slotContents[slotName] = item;
        _itemElements[item.itemName] = element;

        OnItemInserted?.Invoke(item, slotName);
        Debug.Log($"[Inventory] Inserted '{item.itemName}' → '{slotName}'");
        return true;
    }

    /// <summary>
    /// Remove the item currently in a specific slot.
    /// Returns the removed item, or null if the slot was empty.
    /// </summary>
    public InventoryItem RemoveItemFromSlot(string slotName)
    {
        if (!_slotContents.TryGetValue(slotName, out InventoryItem item))
        {
            Debug.LogWarning($"[Inventory] Slot '{slotName}' is already empty.");
            return null;
        }

        VisualElement slot = _root.Q<VisualElement>(slotName);
        if (slot != null)
        {
            slot.RemoveFromClassList("occupied");
            slot.style.unityBackgroundImageTintColor = new Color(1, 1, 1, 0.5f);
        }

        // Remove visual element
        if (_itemElements.TryGetValue(item.itemName, out VisualElement element))
        {
            element.RemoveFromHierarchy();
            _itemElements.Remove(item.itemName);
        }

        _slotContents.Remove(slotName);

        OnItemRemoved?.Invoke(item, slotName);

        Debug.Log($"[Inventory] Removed '{item.itemName}' from '{slotName}'");
        return item;
    }

    /// <summary>
    /// Remove a specific item by name from whichever slot it is in.
    /// Returns the removed item, or null if not found.
    /// </summary>
    public InventoryItem RemoveItem(string itemName)
    {
        foreach (var kvp in _slotContents)
        {
            if (kvp.Value.itemName == itemName)
                return RemoveItemFromSlot(kvp.Key);
        }
        Debug.LogWarning($"[Inventory] Item '{itemName}' not found in any slot.");
        return null;
    }

    /// <summary>
    /// Returns the item in a slot, or null if empty.
    /// </summary>
    public InventoryItem GetItemInSlot(string slotName)
    {
        _slotContents.TryGetValue(slotName, out InventoryItem item);
        return item;
    }

    /// <summary>
    /// Returns true if a slot contains an item.
    /// </summary>
    public bool IsSlotOccupied(string slotName) => _slotContents.ContainsKey(slotName);

    /// <summary>
    /// Swap items between two slots. Returns false if incompatible.
    /// </summary>
    public bool SwapSlots(string slotNameA, string slotNameB)
    {
        InventoryItem itemA = GetItemInSlot(slotNameA);
        InventoryItem itemB = GetItemInSlot(slotNameB);

        // Validate compatibility both ways
        VisualElement slotB = _root.Q<VisualElement>(slotNameB);
        VisualElement slotA = _root.Q<VisualElement>(slotNameA);

        if (slotA?.userData is not SlotType typeA || slotB?.userData is not SlotType typeB)
            return false;

        if (itemA != null && !ItemDragManipulator.IsCompatible(itemA.category, typeB)) return false;
        if (itemB != null && !ItemDragManipulator.IsCompatible(itemB.category, typeA)) return false;

        if (itemA != null) RemoveItemFromSlot(slotNameA);
        if (itemB != null) RemoveItemFromSlot(slotNameB);
        if (itemB != null) InsertItem(itemB, slotNameA);
        if (itemA != null) InsertItem(itemA, slotNameB);

        return true;
    }

    /// <summary>
    /// Add item to items-container (unequipped inventory) without placing it in a slot.
    /// </summary>
    public void AddItemToContainer(InventoryItem item)
    {
        // Najdeme první prázdný obyčejný slot a vložíme předmět přímo do něj, místo volně do kontejneru
        foreach (VisualElement slot in _root.Query<VisualElement>(className: "inv-slot-item").ToList())
        {
            if (!IsSlotOccupied(slot.name))
            {
                InsertItem(item, slot.name);
                return;
            }
        }

        Debug.LogWarning($"[Inventory] Nemohu přidat '{item.itemName}', kontejner je buď plný nebo nemá sloty.");
    }

    // =========================================================================
    //  INTERNAL
    // =========================================================================

    // Called by drag manipulator after a successful drag-and-drop
    public void OnItemDroppedInSlot(InventoryItem item, VisualElement slot, VisualElement sourceSlot)
    {
        string slotName = slot.name;

        // Reset source slotu
        if (sourceSlot != null)
        {
            sourceSlot.RemoveFromClassList("occupied");
            sourceSlot.style.unityBackgroundImageTintColor = new Color(1,1,1,0.5f);
            _slotContents.Remove(sourceSlot.name);
            OnItemRemoved?.Invoke(item, sourceSlot.name);
        }

        // If slot was already occupied, record the eviction
        if (_slotContents.TryGetValue(slotName, out InventoryItem previous))
        {
            _slotContents.Remove(slotName);
            OnItemRemoved?.Invoke(previous, slotName);
        }

        _slotContents[slotName] = item;
        VisualElement element = _itemElements[item.itemName];

        // Přesuneme element fyzicky jako child slotu a vyresetujeme jeho translaci
        slot.Add(element);
        element.style.translate = new StyleTranslate(new Translate(0, 0));

        // Nastavíme occupied stav na novém slotu
        slot.AddToClassList("occupied");
        slot.style.unityBackgroundImageTintColor = new StyleColor(Color.clear);

        OnItemInserted?.Invoke(item, slotName);
        Debug.Log($"[Inventory] Drag-dropped '{item.itemName}' → '{slotName}'");
    }

    private void RegisterSlotTypes()
    {
        SetSlotType("slot-hat", SlotType.Hat);
        SetSlotType("slot-chest", SlotType.Chest);
        SetSlotType("slot-boots", SlotType.Boots);
        SetSlotType("slot-ring", SlotType.Ring);

        int slotIndex = 0;
        foreach (VisualElement s in _root.Query<VisualElement>(className: "inv-slot-item").ToList())
        {
            // Ujistíme se, že má každý generický slot pro itemy unikátní jméno, aby ho funkce InsertItem dokázala vyhledat
            if (string.IsNullOrEmpty(s.name))
            {
                s.name = $"inv-slot-item-{slotIndex}";
            }

            s.AddToClassList("inv-slot");
            s.userData = SlotType.ItemSlot;
            slotIndex++;

            // slot-dragging check zabraňuje hover efektu během dragu
            s.RegisterCallback<PointerEnterEvent>(_ => {
                if (!s.ClassListContains("slot-dragging"))
                    s.AddToClassList("slot-hover");
            });
            s.RegisterCallback<PointerLeaveEvent>(_ => s.RemoveFromClassList("slot-hover"));
        }
    }

    private void SetSlotType(string slotName, SlotType type)
    {
        VisualElement slot = _root.Q<VisualElement>(slotName);
        if (slot == null) { Debug.LogWarning($"[InventoryUI] Slot '{slotName}' not found."); return; }
        slot.AddToClassList("inv-slot");
        slot.userData = type;

        // slot-dragging check zabraňuje hover efektu během dragu
        slot.RegisterCallback<PointerEnterEvent>(_ => {
            if (!slot.ClassListContains("slot-dragging"))
                slot.AddToClassList("slot-hover");
        });
        slot.RegisterCallback<PointerLeaveEvent>(_ => slot.RemoveFromClassList("slot-hover"));
    }

    private void PopulateStartingItems()
    {
        foreach (InventoryItem item in startingItems)
            AddItemToContainer(item);
    }

    private VisualElement CreateItemElement(InventoryItem item)
    {
        var element = new VisualElement();
        element.name = $"item-{item.itemName}";
        element.AddToClassList("inv-item");

        if (item.icon != null)
            element.style.backgroundImage = new StyleBackground(item.icon);

        element.AddManipulator(new ItemDragManipulator(element, _root, item, this));

        // Vlastní tooltip pomocí UI událostí
        string text = $"{item.itemName}\n\n{item.description}";
        element.RegisterCallback<PointerEnterEvent>(evt => ShowTooltip(text, evt.position));
        element.RegisterCallback<PointerMoveEvent>(evt => ShowTooltip(text, evt.position));
        element.RegisterCallback<PointerLeaveEvent>(evt => HideTooltip());
        element.RegisterCallback<PointerDownEvent>(evt => HideTooltip()); // Skrýt během drag and drop

        return element;
    }

    //pozdeji barva na zakladě rarity, ted jsem nechal old kod
    private static Color CategoryColor(ItemCategory cat) => cat switch
    {
        ItemCategory.Hat => Color.yellow,
        ItemCategory.Chest => Color.blue,
        ItemCategory.Boots => Color.green,
        ItemCategory.Ring => Color.magenta,
        ItemCategory.Weapon => Color.red,
        ItemCategory.Item => Color.white,
        _ => Color.gray
    };
}