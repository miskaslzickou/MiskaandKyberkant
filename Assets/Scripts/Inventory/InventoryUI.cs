using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Progress;



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
    private readonly GameObject _droppedItemPrefab;
    private bool isContextMenuOpen = false;
    private VisualElement menu;
    public ItemDragManipulator(VisualElement target, VisualElement root,
                               InventoryItem item, InventoryUI ui, GameObject droppedItemPrefab)
    {
        this.target = target;
        _root = root;
        _item = item;
        _ui = ui;
        _droppedItemPrefab = droppedItemPrefab;
    
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
        if (evt.button == 1) // pravé tlačítko
        {
            if (isContextMenuOpen)
            {
                _root.Remove(menu);
                isContextMenuOpen = false;
            }
            else
            {
                _ui.HideTooltip();
                ShowContextMenu(target, _item, evt.position);
                isContextMenuOpen = true;
            }
            evt.StopPropagation();
            return; // ← důležité, aby se nespustil drag
        }

        // Zavři menu při levém kliknutí
        if (isContextMenuOpen)
        {
            _root.Remove(menu);
            isContextMenuOpen = false;
        }

        _sourceSlot = target.parent;
        _sourceSlot?.RemoveFromClassList("slot-hover");
        _sourceSlot?.AddToClassList("slot-dragging");

        Translate current = target.resolvedStyle.translate;
        _startPosition = new Vector2(current.x.value, current.y.value);
        _pointerStartPosition = evt.position;
        target.CapturePointer(evt.pointerId);
        target.BringToFront();
        _dragging = true;
    }

    private void OnPointerMove(PointerMoveEvent evt)
    {
        if (!_dragging || !target.HasPointerCapture(evt.pointerId)) return;
        Vector3 delta = evt.position - _pointerStartPosition;
        SetTranslate(target, new Vector2(_startPosition.x + delta.x, _startPosition.y + delta.y));
    }
    private void ShowContextMenu(VisualElement itemElement, InventoryItem item, Vector2 position)
    {
        // Vytvoříme kontextové menu
         menu = new VisualElement();
        // Přidáme možnost "Drop Item"
        menu.style.position = Position.Absolute;
        menu.style.left = position.x;
        menu.style.top = position.y + 20;
        if (_item.category == ItemCategory.Item)

        {
            Button contextMenuButton1 = new Button(() => { UseItem(); _root.Remove(menu); }) { text = "Use Item" };
            contextMenuButton1.AddToClassList("context-menu-button");
            menu.Add(contextMenuButton1);
            
        }
        Button  contextMenuButton2 = new Button(() => { DropItemIntoWorld(); _root.Remove(menu); }) { text = "Drop Item" };
        contextMenuButton2.AddToClassList("context-menu-button");
        menu.Add(contextMenuButton2);

        // Zobrazíme menu na pozici kurzoru
        _root.Add(menu);
        
    }
    private void OnPointerUp(PointerUpEvent evt)
    {
        if (_dragging && target.HasPointerCapture(evt.pointerId))
            target.ReleasePointer(evt.pointerId);
        
    }
    private void UseItem()
    {
        // Zde implementujte logiku pro použití předmětu
        Debug.Log($"Používám předmět: {_item.itemName}");
        // Například můžete snížit množství, spustit efekt, atd.
        if (_item.quantity > 1)
        {
            _item.quantity--;
            _ui.UpdateItemQuantityUI(_item);
        }
        else
        {
            // Pokud je množství 1, odstraníme předmět z inventáře
            _ui.RemoveItemFromSlot(_sourceSlot.name);
        }
        isContextMenuOpen = false;
    }
    private void DropItemIntoWorld()
    {
        _ui.HideTooltip();

        if (_sourceSlot != null)
            _ui.RemoveItemFromSlot(_sourceSlot.name);
        else
            target.RemoveFromHierarchy(); // fallback

        // 3. Spawn ve světě
        GameObject player = GameObject.FindWithTag("Player");
        Vector3 playerPos = player != null ? player.transform.position : Vector3.zero;

        GameObject droppedObj = Object.Instantiate(_droppedItemPrefab, playerPos, Quaternion.identity);
        DroppedItem droppedItem = droppedObj.GetComponent<DroppedItem>();
        if (droppedItem != null)
        {
            droppedItem.InitializeItem(_item.icon);
            droppedItem.item = _item;
        }
        isContextMenuOpen = false;
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

        bool success = false;
        if (bestSlot != null)
        {
            success = _ui.OnItemDroppedInSlot(_item, bestSlot, _sourceSlot);
        }
        
        if (!success)
        {
            // Reset translace při nepovedeném puštění nebo neplatném swapu
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
        SlotType.Weapon => item == ItemCategory.Weapon,
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
    public GameObject droppedItemPrefab; // Reference to the prefab for the dropped item
    [Header("UI")]
    [SerializeField] private UIDocument uiDocument;

    

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
        _tooltipLabel.style.fontSize = 20;
        _tooltipLabel.style.display = DisplayStyle.None;
        _tooltipLabel.pickingMode = PickingMode.Ignore; // Aby neblokoval klikání myší
        _root.Add(_tooltipLabel);

        RegisterSlotTypes();
        
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

        // Pojistka pro položky přidané přímo v Unity Inspectoru přes startingItems, 
        // u kterých neproběhl konstruktor a nemají generované instanceId
        if (string.IsNullOrEmpty(item.instanceId))
        {
            item.instanceId = $"{item.itemName}_{System.Guid.NewGuid()}";
        }

        // Spawn the element inside the slot
        VisualElement element = CreateItemElement(item);
        slot.Add(element);

        // Skrytí původní siluety / ikony pozadí
        slot.AddToClassList("occupied");
        SetSlotSilhouetteTint(slot, Color.clear);

        _slotContents[slotName] = item;
        _itemElements[item.instanceId] = element;

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
            SetSlotSilhouetteTint(slot, new Color(1, 1, 1, 0.5f));
        }

        // Remove visual element pomocí unikátního ID
        if (_itemElements.TryGetValue(item.instanceId, out VisualElement element))
        {
            element.RemoveFromHierarchy();
            _itemElements.Remove(item.instanceId);
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
    public bool AddItemToContainer(InventoryItem item)
    {
        // Najdeme první prázdný obyčejný slot a vložíme předmět přímo do něj, místo volně do kontejneru
        foreach (VisualElement slot in _root.Query<VisualElement>(className: "inv-slot-item").ToList())
        {
            if (!IsSlotOccupied(slot.name))
            {
                InsertItem(item, slot.name);
                return true;
            }
        }

        Debug.LogWarning($"[Inventory] Nemohu přidat '{item.itemName}', kontejner je buď plný nebo nemá sloty.");
        return false;
    }

    // =========================================================================
    //  INTERNAL
    // =========================================================================

    // Called by drag manipulator after a successful drag-and-drop
    public bool OnItemDroppedInSlot(InventoryItem item, VisualElement slot, VisualElement sourceSlot)
    {
        string slotName = slot.name;
        string sourceSlotName = sourceSlot != null ? sourceSlot.name : null;

        // Pokud to hodíme do stejného slotu, kde to už bylo, jen resetujeme pozici
        if (slotName == sourceSlotName) return false;

        bool isOccupied = _slotContents.TryGetValue(slotName, out InventoryItem targetItem);

        // Zkusíme Stackování (pokud stejný předmět a oba jsou typu Item)
        if (isOccupied && targetItem.itemName == item.itemName && item.category == ItemCategory.Item)
        {
            // Přidáme množství ze zdrojového itemu k tomu cílovému
            targetItem.quantity += item.quantity;
            
            // Zničíme starý přesouvaný originál, protože už je v cílovém stacku
            RemoveItemFromSlot(sourceSlotName);
            
            // Aktualizujeme UI label s číslíčkem na cílovém elementu
            UpdateItemQuantityUI(targetItem);

            Debug.Log($"[Inventory] Stacked '{item.itemName}' together. New quantity: {targetItem.quantity}");
            return true;
        }

        // Zkusit SWAP (Prohození pozic, pokud je cílový slot obsazen jiným itemem)
        if (isOccupied)
        {
            // Musíme ověřit, jestli se item, který tam je, smí umístit do zdrojového (source) slotu
            SlotType sourceType = sourceSlot != null ? (SlotType)sourceSlot.userData : SlotType.ItemSlot;
            if (!ItemDragManipulator.IsCompatible(targetItem.category, sourceType))
            {
                Debug.LogWarning($"[Inventory] Nemohu prohodit, '{targetItem.itemName}' nemůže jít do zdrojového slotu.");
                return false; // Odmítneme drop a item se vrátí zpět na původní translaci
            }

            // Můžeme prohodit. Nejdřív je ale oba korektně vyjmeme z vizuálních slotů!
            RemoveItemFromSlot(slotName);      // vyjme targetItem
            RemoveItemFromSlot(sourceSlotName);// vyjme draggedItem (item)

            // Vložíme je na jejich nová místa
            InsertItem(targetItem, sourceSlotName);
            InsertItem(item, slotName);
            
            Debug.Log($"[Inventory] Swapped '{item.itemName}' & '{targetItem.itemName}'");
            return true;
        }

        // Pokud je slot volný (žádný swap se nekoná), klasicky ho tam přendáme
        if (sourceSlot != null)
        {
            RemoveItemFromSlot(sourceSlotName);
        }

        InsertItem(item, slotName);
        Debug.Log($"[Inventory] Drag-dropped '{item.itemName}' → '{slotName}'");
        return true;
    }

    private void SetSlotSilhouetteTint(VisualElement slot, Color color)
    {
        // Projdeme děti slotu a změníme tint barvu pozadí všem, které nejsou samotným itemem
        foreach (var child in slot.Children())
        {
            if (!child.ClassListContains("inv-item"))
            {
                child.style.unityBackgroundImageTintColor = new StyleColor(color);
                break;
            }
        }
    }

    private void RegisterSlotTypes()
    {
        SetSlotType("slot-hat", SlotType.Hat);
        SetSlotType("slot-chest", SlotType.Chest);
        SetSlotType("slot-boots", SlotType.Boots);
        SetSlotType("slot-ring", SlotType.Ring);
        SetSlotType("slot-weapon", SlotType.Weapon);

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

    private VisualElement CreateItemElement(InventoryItem item)
    {
        var element = new VisualElement();
        element.name = $"item-{item.instanceId}";
        element.AddToClassList("inv-item");

        if (item.icon != null)
            element.style.backgroundImage = new StyleBackground(item.icon);

        element.AddManipulator(new ItemDragManipulator(element, _root, item, this, droppedItemPrefab));

        // Label pro zobrazení množství (quantity), přidán do pravého dolního rohu itemu
        Label qtyLabel = new Label();
        qtyLabel.name = "qty-label";
        qtyLabel.style.position = Position.Absolute;
        qtyLabel.style.bottom = 2;
        qtyLabel.style.right = -3;
        qtyLabel.style.color = new StyleColor(Color.white);
        qtyLabel.style.fontSize = 14; // Můžeš si upravit ve stylopisu
        qtyLabel.style.backgroundColor = new StyleColor(new Color(0, 0, 0, 0.7f));
        qtyLabel.style.paddingLeft = 3;
        qtyLabel.style.paddingRight = 3;
        qtyLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        // Přidáme jemný stín (Outline) aby bylo číslo dobře vidět i na světlém pozadí ikonek
        qtyLabel.style.textShadow = new TextShadow { color = Color.black, offset = new Vector2(1, 1), blurRadius = 3 };
        qtyLabel.text =  item.quantity.ToString() ;
        qtyLabel.style.display = item.quantity > 1 ? DisplayStyle.Flex : DisplayStyle.None;

        if (item.category == ItemCategory.Item)
        element.Add(qtyLabel);

        string itemStats = "";
        foreach (StatBonus bonus in item.stats)
        {
            itemStats += $"{bonus.value} {bonus.statType}\n";
        }
        // Vlastní tooltip pomocí UI událostí
        string text = $"{item.itemName}\n\n {item.description}\n\n {itemStats} ";
        element.RegisterCallback<PointerEnterEvent>(evt => ShowTooltip(text, evt.position));
        element.RegisterCallback<PointerMoveEvent>(evt => ShowTooltip(text, evt.position));
        element.RegisterCallback<PointerLeaveEvent>(evt => HideTooltip());
        element.RegisterCallback<PointerDownEvent>(evt => HideTooltip()); // Skrýt během drag and drop

        return element;
    }

    public void UpdateItemQuantityUI(InventoryItem item)
    {
        if (_itemElements.TryGetValue(item.instanceId, out VisualElement element))
        {
            Label qtyLabel = element.Q<Label>("qty-label");
            if (qtyLabel != null)
            {
                if (item.quantity > 1)
                {
                    qtyLabel.text = item.quantity.ToString();
                    qtyLabel.style.display = DisplayStyle.Flex;
                }
                else
                {
                    qtyLabel.style.display = DisplayStyle.None;
                }
            }
        }
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