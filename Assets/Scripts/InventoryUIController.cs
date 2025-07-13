using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq; // LINQ を使用するために必要 (FirstOrDefault など)
using TMPro;

public class InventoryUIController : MonoBehaviour
{
    public static InventoryUIController Instance { get; private set; }

    [Header("UI参照")]
    public GameObject inventoryPanel;
    [SerializeField] private Transform _itemSlotParent;
    public GameObject itemSlotPrefab;

    [Header("アイテム詳細UI")]
    [SerializeField] private GameObject itemDetailPanel;
    [SerializeField] private TextMeshProUGUI detailNameText;
    [SerializeField] private TextMeshProUGUI detailDescriptionText;
    [SerializeField] private Image detailIconImage;

    // ページングボタンの参照 (今回の機能では直接使用しないが、以前のコードを維持)
    [Header("ページングボタン")]
    public Button nextButton;
    public Button backButton;

    // 現在有効なアイテムと、生成された全アイテムスロットのリスト
    private ItemData currentActiveItem; // 現在有効化されているアイテム
    private List<ItemSlot> instantiatedItemSlots = new List<ItemSlot>(); // 現在生成されている ItemSlot の全リスト

    private bool isVisible = false;
    private bool _uiUpdateRequested = false;
    private bool _needsUIUpdateOnOpen = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // シーン遷移時に破棄しない場合
            Debug.Log("InventoryUIController: Awake: インスタンスが設定されました。");
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Debug.Log("InventoryUIController: Awake が呼び出されました。");

        // _itemSlotParent の動的取得（Inspectorで設定されていれば不要）
        if (_itemSlotParent == null)
        {
            Debug.LogWarning("InventoryUIController: Awake: _itemSlotParent が Inspector で割り当てられていません。Hierarchyから検索を試みます。");
            Transform canvasTransform = FindFirstObjectByType<Canvas>()?.transform;
            if (canvasTransform != null)
            {
                _itemSlotParent = canvasTransform.Find("InventoryPanel/ItemScrollView/Viewport/Content");
                if (_itemSlotParent != null)
                {
                    Debug.Log($"InventoryUIController: Awake: 'Content' をHierarchyから動的に取得しました: {_itemSlotParent.name}.");
                }
                else
                {
                    Debug.LogError("InventoryUIController: Awake: 'Content' を動的に取得できませんでした。パスが間違っているか、オブジェクトが存在しません。");
                }
            }
            else
            {
                Debug.LogError("InventoryUIController: Awake: シーン内にCanvasオブジェクトが見つかりません。");
            }
        }
        else
        {
            Debug.Log($"InventoryUIController: Awake: _itemSlotParent が Inspector で設定されています: {_itemSlotParent.name}.");
        }

        if (itemDetailPanel != null)
        {
            itemDetailPanel.SetActive(false);
        }

        // ボタンの初期状態を設定 (必要であれば)
        // UpdateButtonStates(); // ページングボタンがない場合、この行は削除またはコメントアウト
    }

    private void OnEnable()
    {
        Debug.Log($"InventoryUIController: OnEnable が呼び出されました。GameObject: {gameObject.name}");

        if (_itemSlotParent == null)
        {
            Debug.LogError("InventoryUIController: OnEnable: _itemSlotParent がまだ null のため、イベント購読やUI更新ができません。Inspector設定を確認してください。");
            return;
        }
        else
        {
            Debug.Log($"InventoryUIController: OnEnable: _itemSlotParent (Content) が有効です: {_itemSlotParent.name}.");
        }

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.onInventoryChangedCallback += OnInventoryChanged;
            Debug.Log("InventoryUIController: InventoryManager の onInventoryChangedCallback を購読しました。");

            Debug.Log("InventoryUIController: OnEnable: 初期UI更新を呼び出します。");
            UpdateInventoryUI();
            _needsUIUpdateOnOpen = false;
        }
        else
        {
            Debug.LogWarning("InventoryUIController: OnEnable時にInventoryManager.Instanceがnullです。InventoryManagerが先に初期化されているか確認してください。");
        }
    }

    private void OnDisable()
    {
        Debug.Log($"InventoryUIController: OnDisable が呼び出されました。GameObject: {gameObject.name}");
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.onInventoryChangedCallback -= OnInventoryChanged;
            Debug.Log("InventoryUIController: InventoryManager の onInventoryChangedCallback の購読を解除しました。");
        }
        // インベントリが閉じられた時に全ての選択状態をリセット
        ResetAllSelections();
    }

    public void RequestUIUpdateFromManager()
    {
        Debug.Log("InventoryUIController: ManagerからUI更新リクエストを受け取りました。");
        _needsUIUpdateOnOpen = true;

        if (isVisible)
        {
            Debug.Log("InventoryUIController: パネルが開いているため、Managerからのリクエストを即座に処理します。");
            if (!_uiUpdateRequested)
            {
                _uiUpdateRequested = true;
                StartCoroutine(RequestUIUpdateOnNextFrame());
            }
        }
        else
        {
            Debug.Log("InventoryUIController: パネル非表示のためManagerからのUI更新リクエストは保留しました。");
        }
    }

    void OnInventoryChanged()
    {
        Debug.Log("InventoryUIController: OnInventoryChanged イベントを受け取りました。（イベント購読成功！）");
        _needsUIUpdateOnOpen = true;
        Debug.Log("InventoryUIController: UI更新を保留し、_needsUIUpdateOnOpen を true に設定しました。(イベント経由)");

        if (isVisible)
        {
            Debug.Log("InventoryUIController: パネルが開いているため、即座にUI更新を呼び出します。(イベント経由)");
            if (!_uiUpdateRequested)
            {
                _uiUpdateRequested = true;
                StartCoroutine(RequestUIUpdateOnNextFrame());
            }
        }
        else
        {
            Debug.Log("InventoryUIController: パネル非表示のためUI更新は保留しました。(イベント経由)");
        }
    }

    private IEnumerator RequestUIUpdateOnNextFrame()
    {
        yield return null;

        Debug.Log("RequestUIUpdateOnNextFrame: UpdateInventoryUI() を実行します。");
        UpdateInventoryUI();
        _uiUpdateRequested = false;
        _needsUIUpdateOnOpen = false;
    }

    /// <summary>
    /// インベントリパネルの表示/非表示を切り替えます。
    /// </summary>
    public void ToggleInventory()
    {
        isVisible = !isVisible;

        if (inventoryPanel == null)
        {
            Debug.LogError("ToggleInventory: inventoryPanel が割り当てられていません！パネルを開けません。");
            return;
        }

        inventoryPanel.SetActive(isVisible);

        Debug.Log($"ToggleInventory: isVisible = {isVisible}");
        Debug.Log($"ToggleInventory: InventoryPanel の新しいアクティブ状態 = {inventoryPanel.activeSelf}");

        if (!isVisible) // インベントリを閉じるとき
        {
            if (itemDetailPanel != null)
            {
                itemDetailPanel.SetActive(false);
            }
            ResetAllSelections(); // インベントリを閉じるときに全ての選択状態をリセット
        }

        if (isVisible) // インベントリを開くとき
        {
            Debug.Log("ToggleInventory: インベントリ表示、UI更新が必要かチェックします。");
            if (_needsUIUpdateOnOpen || (InventoryManager.Instance != null && InventoryManager.Instance.GetOwnedItems().Count > 0))
            {
                Debug.Log("ToggleInventory: _needsUIUpdateOnOpen が true またはアイテムがあるため、UpdateInventoryUI() を呼び出します。");
                UpdateInventoryUI();
                _needsUIUpdateOnOpen = false;
            }
            else
            {
                Debug.Log("ToggleInventory: 更新は不要です（アイテムなし or すでに更新済み）。");
            }
        }
    }

    /// <summary>
    /// インベントリUIの表示を更新します。
    /// </summary>
    private void UpdateInventoryUI()
    {
        if (_itemSlotParent == null)
        {
            Debug.LogError("UpdateInventoryUI: _itemSlotParent が null のため、UI の更新をスキップします！Inspectorで設定またはAwake()での取得を確認してください。");
            return;
        }

        Debug.Log($"UpdateInventoryUI: _itemSlotParent は null ではありません。UI更新を続行します。参照元: {_itemSlotParent.gameObject.name}");

        Debug.Log("UpdateInventoryUI: 既存のアイテムスロットを削除し、リストをクリアします。");
        // 既存のスロットを全て破棄し、リストもクリア
        for (int i = _itemSlotParent.childCount - 1; i >= 0; i--)
        {
            GameObject child = _itemSlotParent.GetChild(i).gameObject;
            if (child != null)
            {
#if UNITY_EDITOR
                DestroyImmediate(child); // Editor上での即時破棄
#else
                Destroy(child); // ゲーム実行中の破棄
#endif
            }
        }
        instantiatedItemSlots.Clear(); // リストもクリア

        List<ItemData> allOwnedItems = (InventoryManager.Instance != null) ? InventoryManager.Instance.GetOwnedItems() : new List<ItemData>();
        Debug.Log($"UpdateInventoryUI: 全ての所持アイテム数 = {allOwnedItems.Count}");

        if (allOwnedItems.Count == 0)
        {
            Debug.LogWarning("UpdateInventoryUI: 所持アイテムが0個のため、スロットは生成されません。");
        }

        // 全ての所持アイテムに対してスロットを生成
        foreach (ItemData item in allOwnedItems)
        {
            Debug.Log($"UpdateInventoryUI: 生成するアイテムスロット: {item.itemName}");

            if (itemSlotPrefab == null)
            {
                Debug.LogError("UpdateInventoryUI: itemSlotPrefab が Inspector で設定されていません！このアイテムのスロットを生成できません。");
                continue;
            }
            if (item == null)
            {
                Debug.LogWarning("UpdateInventoryUI: ItemData が null です。このアイテムのスロットは生成されません。");
                continue;
            }
            if (item.itemIcon == null)
            {
                Debug.LogWarning($"UpdateInventoryUI: アイテム '{item.itemName}' のアイコンが設定されていません。");
            }

            GameObject slotGO = Instantiate(itemSlotPrefab, _itemSlotParent); // スロットのGameObjectを生成
            Debug.Log($"UpdateInventoryUI: 生成したスロット: {slotGO.name}");

            ItemSlot itemSlotScript = slotGO.GetComponent<ItemSlot>();
            if (itemSlotScript != null)
            {
                itemSlotScript.Setup(item); // ItemSlotスクリプトにアイテムデータをセットアップ
                instantiatedItemSlots.Add(itemSlotScript); // 生成した ItemSlot スクリプトをリストに追加
            }
            else
            {
                Debug.LogError($"UpdateInventoryUI: 生成されたスロット '{slotGO.name}' に ItemSlot スクリプトが見つかりません！");
            }
        }

        Debug.Log($"UpdateInventoryUI: スロット生成後のContentの子オブジェクト数: {_itemSlotParent.childCount}");

        // レイアウト再構築
        if (_itemSlotParent.GetComponent<LayoutGroup>() != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(_itemSlotParent as RectTransform);
            Debug.Log("UpdateInventoryUI: LayoutGroup のレイアウトを強制的に再構築しました。");
        }
        else
        {
            Canvas.ForceUpdateCanvases();
            Debug.Log("UpdateInventoryUI: Canvas を強制的に更新しました。");
        }
        Debug.Log("UpdateInventoryUI: UI更新処理を完了しました。");
    }

    /// <summary>
    /// アイテム詳細UIを表示します。
    /// </summary>
    /// <param name="item">表示するアイテムデータ</param>
    public void ShowItemDetail(ItemData item)
    {
        if (itemDetailPanel == null || detailNameText == null || detailDescriptionText == null)
        {
            Debug.LogError("ShowItemDetail: ItemDetailPanelまたは関連するUI要素がInspectorで設定されていません。");
            return;
        }

        itemDetailPanel.SetActive(true);
        detailNameText.text = item.itemName;
        detailDescriptionText.text = item.itemDescription;

        if (detailIconImage != null)
        {
            detailIconImage.sprite = item.itemIcon;
            detailIconImage.enabled = (item.itemIcon != null);
        }
        else
        {
            Debug.LogWarning("InventoryUIController: detailIconImage が Inspector で設定されていません。アイコンは表示されません。");
        }
    }

    /// <summary>
    /// アイテム詳細UIを非表示にします。
    /// </summary>
    public void HideItemDetail()
    {
        if (itemDetailPanel != null)
        {
            itemDetailPanel.SetActive(false);
        }
    }

    // --- 新しいメソッド ---

    /// <summary>
    /// 現在有効化されているアイテムを設定します。
    /// </summary>
    /// <param name="item">有効化するアイテムデータ。nullを設定するとリセットされます。</param>
    public void SetActiveItem(ItemData item)
    {
        currentActiveItem = item;
        Debug.Log($"InventoryUIController: 現在有効なアイテム: {(item != null ? item.itemName : "なし")}");
        // 必要に応じて、有効なアイテムに関するUI（例えば、特定のテキスト表示など）をここで更新できます
    }

    /// <summary>
    /// 現在有効化されているアイテムを取得します。
    /// </summary>
    /// <returns>現在有効なアイテムデータ。有効なアイテムがない場合は null。</returns>
    public ItemData GetActiveItem()
    {
        return currentActiveItem;
    }

    /// <summary>
    /// 他のアイテムスロットの選択状態をリセットします。
    /// </summary>
    public void ResetActiveItem()
    {
        if (currentActiveItem != null)
        {
            // currentActiveItem に対応する ItemSlot を見つけ出し、選択状態をリセット
            // FirstOrDefault は、条件に合う最初の一つを返し、見つからなければ null を返す
            ItemSlot slotToReset = instantiatedItemSlots.FirstOrDefault(slot => slot.item == currentActiveItem);
            if (slotToReset != null)
            {
                slotToReset.ResetSelection();
                Debug.Log($"InventoryUIController: 以前有効だった '{currentActiveItem.itemName}' の選択状態をリセットしました。");
            }
            currentActiveItem = null; // 有効アイテムをクリア
        }
    }

    /// <summary>
    /// 全てのアイテムスロットの選択状態をリセットします。
    /// インベントリを閉じたり、OnDisable時などに使用します。
    /// </summary>
    public void ResetAllSelections()
    {
        currentActiveItem = null; // 有効アイテムをクリア
        foreach (var slot in instantiatedItemSlots)
        {
            slot.ResetSelection(); // 各スロットの選択状態をリセット
        }
        Debug.Log("InventoryUIController: 全てのアイテムスロットの選択状態をリセットしました。");
    }

    /// <summary>
    /// 指定されたアイテムをインベントリから削除します。
    /// </summary>
    /// <param name="itemToRemove">削除するアイテムデータ</param>
    public void RemoveItem(ItemData itemToRemove)
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.RemoveItem(itemToRemove); // InventoryManagerにアイテム削除を依頼
            // InventoryManager が変更を通知し、それが UpdateInventoryUI をトリガーするはずです
        }
        else
        {
            Debug.LogError("InventoryUIController: InventoryManager.Instance が null のためアイテムを削除できません。");
        }
        // 削除後に有効アイテムがそのアイテムだった場合、リセットする
        if (currentActiveItem == itemToRemove)
        {
            ResetActiveItem();
        }
    }

    // --- ページングボタンに関する既存のメソッド（必要なければ削除可） ---
    // UpdateButtonStates() は ItemSlot の有効化機能とは直接関係ないので、削除しても問題ありません。
    // public void UpdateButtonStates() { ... } 
    // private int GetMaxPages() { ... }
    // public void NextPage() { ... }
    // public void PreviousPage() { ... }
}