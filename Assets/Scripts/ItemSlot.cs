using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler を使用するために必要

public class ItemSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler // インターフェースを確認
{
    public ItemData item;
    public Image iconImage;
    public GameObject selectionFrame;

    private bool isSelected = false;
    private InventoryUIController inventoryUIController;

    void Awake()
    {
        inventoryUIController = FindObjectOfType<InventoryUIController>();
        if (inventoryUIController == null)
        {
            Debug.LogError("ItemSlot: InventoryUIController がシーンに見つかりません！");
        }

        if (selectionFrame != null)
        {
            selectionFrame.SetActive(false);
        }
    }

    public void Setup(ItemData newItem)
    {
        item = newItem;
        if (iconImage != null)
        {
            if (item != null && item.itemIcon != null)
            {
                iconImage.sprite = item.itemIcon;
                iconImage.enabled = true;
                // ここでレイキャストターゲットを常にtrueにしておくことが重要
                iconImage.raycastTarget = true;
            }
            else
            {
                iconImage.sprite = null;
                iconImage.enabled = false;
                // アイテムがない場合はレイキャストターゲットも無効にする
                iconImage.raycastTarget = false;
            }
        }
        ResetSelection();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (item == null) return;

        Debug.Log($"ItemSlot: '{item.itemName}' がクリックされました。現在選択状態: {isSelected}");

        if (isSelected)
        {
            // 既に選択されている場合、選択解除
            SetSelectionState(false);
            if (inventoryUIController != null)
            {
                inventoryUIController.SetActiveItem(null);
            }
            // クリックで選択解除された場合、詳細表示も非表示にする
            if (inventoryUIController != null)
            {
                inventoryUIController.HideItemDetail();
            }
            Debug.Log($"ItemSlot: '{item.itemName}' の選択状態をリセットしました。");
        }
        else
        {
            // 新しく選択
            if (inventoryUIController != null)
            {
                inventoryUIController.ResetActiveItem();
                inventoryUIController.SetActiveItem(item);
            }
            SetSelectionState(true);
            // 新しく選択されたアイテムの詳細をここで表示する（マウスオーバーと重複するが、確実性を高める）
            if (inventoryUIController != null)
            {
                inventoryUIController.ShowItemDetail(item);
            }
            Debug.Log($"ItemSlot: '{item.itemName}' を有効なアイテムとして設定しました。");
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // マウスがスロットに入った際に、常に詳細を表示する
        // ただし、ItemDataがnullの場合は表示しない
        if (item != null && inventoryUIController != null)
        {
            inventoryUIController.ShowItemDetail(item);
            Debug.Log($"OnPointerEnter: {item.itemName} の詳細を表示 (マウスオーバー)");
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // マウスがスロットから出た際に、詳細を非表示にする
        // ただし、現在クリックで選択中のアイテムと一致する場合は非表示にしないという選択肢もあるが、
        // 今回の要件「有効化されているアイテムアイコンをクリックしても有効化はリセット」を踏まえると、
        // マウスが離れたら非表示にするのがシンプルで良い
        if (inventoryUIController != null)
        {
            // 有効なアイテムが存在し、かつそれがマウスが離れたアイテムである場合のみ非表示にする
            // ここで HideItemDetail を呼ぶと、クリックで選択した後にマウスを離すと詳細が消えてしまうので注意
            // 基本的にはマウスが離れたら非表示で問題ないことが多い
            inventoryUIController.HideItemDetail();
            Debug.Log("OnPointerExit: アイテム詳細を非表示 (マウスアウト)");
        }
    }

    public void SetSelectionState(bool selected)
    {
        isSelected = selected;
        if (selectionFrame != null)
        {
            selectionFrame.SetActive(isSelected);
        }
    }

    public void ResetSelection()
    {
        SetSelectionState(false);
        // ここで詳細パネルを非表示にする必要はない (OnPointerExitで処理されるため)
    }
}