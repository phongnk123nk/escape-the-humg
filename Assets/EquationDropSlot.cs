using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
[AddComponentMenu("Lab/Equation Drop Slot")]
public class EquationDropSlot : MonoBehaviour, IDropHandler
{
    public string requiredItemId;
    public List<string> nearCorrectItemIds = new List<string>();
    public Image slotIcon;
    public TextMeshProUGUI slotLabel;
    public Image slotBorder;
    public Color emptyColor = new Color(1f, 1f, 1f, 0.35f);
    public Color correctColor = new Color(0.2f, 0.95f, 0.45f, 0.95f);
    public Color nearColor = new Color(1f, 0.85f, 0.2f, 0.95f);
    public Color wrongColor = new Color(1f, 0.2f, 0.2f, 0.95f);

    public string CurrentItemId { get; private set; }
    public string CurrentItemName { get; private set; }
    public Sprite CurrentItemIcon { get; private set; }
    public DropState CurrentState { get; private set; }

    private LabEquationPuzzleManager puzzleManager;

    public enum DropState
    {
        Empty,
        Correct,
        NearCorrect,
        Wrong
    }

    private void Awake()
    {
        puzzleManager = GetComponentInParent<LabEquationPuzzleManager>();
        ResetSlot();
    }

    public void OnDrop(PointerEventData eventData)
    {
        InventoryDragItem dragItem = eventData.pointerDrag != null ? eventData.pointerDrag.GetComponent<InventoryDragItem>() : null;
        if (dragItem != null)
        {
            ReceiveItem(dragItem.itemId, dragItem.itemName, dragItem.itemIcon);
        }
    }

    public void ReceiveItem(string itemId, string itemName, Sprite itemIcon)
    {
        CurrentItemId = itemId;
        CurrentItemName = itemName;
        CurrentItemIcon = itemIcon;

        if (slotIcon != null)
        {
            slotIcon.sprite = itemIcon;
            slotIcon.enabled = itemIcon != null;
            slotIcon.color = Color.white;
        }

        if (slotLabel != null)
        {
            slotLabel.text = string.IsNullOrWhiteSpace(itemName) ? itemId : itemName;
            slotLabel.enabled = true;
        }

        if (string.Equals(itemId, requiredItemId, System.StringComparison.OrdinalIgnoreCase))
        {
            CurrentState = DropState.Correct;
            SetBorderColor(correctColor);
        }
        else if (nearCorrectItemIds != null && nearCorrectItemIds.Exists(x => string.Equals(x, itemId, System.StringComparison.OrdinalIgnoreCase)))
        {
            CurrentState = DropState.NearCorrect;
            SetBorderColor(nearColor);
            if (puzzleManager != null)
            {
                puzzleManager.SetMessage("Chất này gần đúng, nhưng chưa chính xác.");
            }
        }
        else
        {
            CurrentState = DropState.Wrong;
            SetBorderColor(wrongColor);
            if (puzzleManager != null)
            {
                puzzleManager.SetMessage("Sai chất");
            }
        }
    }

    public void ResetSlot()
    {
        CurrentItemId = "";
        CurrentItemName = "";
        CurrentItemIcon = null;
        CurrentState = DropState.Empty;

        if (slotIcon != null)
        {
            slotIcon.sprite = null;
            slotIcon.enabled = false;
        }

        if (slotLabel != null)
        {
            slotLabel.text = "";
            slotLabel.enabled = false;
        }

        SetBorderColor(emptyColor);
    }

    public bool HasCorrectItem()
    {
        return CurrentState == DropState.Correct;
    }

    private void SetBorderColor(Color color)
    {
        if (slotBorder != null)
        {
            slotBorder.color = color;
        }
    }
}

