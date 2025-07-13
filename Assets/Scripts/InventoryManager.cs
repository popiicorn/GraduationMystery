using System.Collections.Generic;
using UnityEngine;
using System;
using static ES3; // Easy Save 3 の静的メソッドを直接呼び出すために必要
using System.Linq; // FirstOrDefault を使うために必要

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    public event Action onInventoryChangedCallback;

    private List<ItemData> ownedItems = new List<ItemData>();
    public List<ItemData> OwnedItems => ownedItems;

    // InventoryUIControllerへの直接参照
    [SerializeField] private InventoryUIController inventoryUIController;

    // インベントリのセーブキー
    private const string INVENTORY_SAVE_KEY = "PlayerInventory";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
                Debug.Log("InventoryManager: DontDestroyOnLoad を適用しました。");
            }
            Debug.Log("InventoryManager: Awake: インスタンスが設定されました。ハッシュコード: " + GetHashCode());
        }
        else if (Instance != this)
        {
            Debug.LogWarning("InventoryManager: 複数のInventoryManagerインスタンスを検出しました！このオブジェクトを破棄します。");
            Destroy(gameObject);
            return;
        }

        if (inventoryUIController == null)
        {
            // FindObjectOfType を FindFirstObjectByType に変更 (警告回避)
            inventoryUIController = FindFirstObjectByType<InventoryUIController>();
            if (inventoryUIController != null)
            {
                Debug.Log("InventoryManager: Awake: InventoryUIControllerを動的に取得しました。");
            }
            else
            {
                Debug.LogError("InventoryManager: Awake: InventoryUIControllerが見つかりません。Inspectorで設定してください。");
            }
        }
        else
        {
            Debug.Log("InventoryManager: Awake: InventoryUIControllerがInspectorで設定されています。");
        }

        Debug.Log("InventoryManager: Awake が呼び出されました。");

        // ゲーム開始時にインベントリをロードする
        LoadInventory();
    }

    // ゲーム終了時にインベントリをセーブする
    void OnApplicationQuit()
    {
        Debug.Log("InventoryManager: OnApplicationQuit が呼び出されました。インベントリをセーブします。");
        SaveInventory();
    }

    public void AddItem(ItemData item)
    {
        if (item == null)
        {
            Debug.LogWarning("InventoryManager: AddItem: 追加しようとしたアイテムがnullです。");
            return;
        }

        ownedItems.Add(item);
        Debug.Log($"InventoryManager: アイテム取得: {item.itemName} (現在 {ownedItems.Count} 個)");

        if (inventoryUIController != null)
        {
            Debug.Log("InventoryManager: AddItem: InventoryUIControllerにUI更新を要求します。");
            inventoryUIController.RequestUIUpdateFromManager();
        }
        else
        {
            Debug.LogWarning("InventoryManager: AddItem: InventoryUIControllerがnullのため、UIを更新できませんでした。");
        }

        if (onInventoryChangedCallback != null)
        {
            Debug.Log("InventoryManager: onInventoryChangedCallback イベントを呼び出します。");
            onInventoryChangedCallback.Invoke();
        }
        else
        {
            Debug.LogWarning("InventoryManager: onInventoryChangedCallback に購読者がいません。");
        }

        // アイテム追加後にインベントリをセーブする
        SaveInventory();
    }

    public List<ItemData> GetOwnedItems()
    {
        return ownedItems;
    }

    public void RemoveItem(ItemData item)
    {
        if (ownedItems.Contains(item))
        {
            ownedItems.Remove(item);
            Debug.Log($"InventoryManager: アイテム削除: {item.itemName} (現在 {ownedItems.Count} 個)");

            if (inventoryUIController != null)
            {
                Debug.Log("InventoryManager: RemoveItem: InventoryUIControllerにUI更新を要求します。");
                inventoryUIController.RequestUIUpdateFromManager();
            }

            onInventoryChangedCallback?.Invoke();

            // アイテム削除後にインベントリをセーブする
            SaveInventory();
        }
    }

    public void ClearInventory()
    {
        ownedItems.Clear();
        Debug.Log("InventoryManager: インベントリの内容をクリアしました。");

        if (inventoryUIController != null)
        {
            Debug.Log("InventoryManager: ClearInventory: InventoryUIControllerにUI更新を要求します。");
            inventoryUIController.RequestUIUpdateFromManager();
        }

        onInventoryChangedCallback?.Invoke();

        // インベントリクリア後にセーブする（空の状態を保存）
        SaveInventory();
    }

    public void SaveInventory()
    {
        List<string> itemIDsToSave = new List<string>();
        foreach (ItemData item in ownedItems)
        {
            // item.id が存在しない場合のチェックも追加
            if (item != null && !string.IsNullOrEmpty(item.id))
            {
                itemIDsToSave.Add(item.id);
            }
            else if (item != null)
            {
                Debug.LogWarning($"InventoryManager: SaveInventory: アイテム '{item.itemName}' にIDが設定されていません。スキップします。");
            }
            else
            {
                Debug.LogWarning("InventoryManager: SaveInventory: ownedItemsリストにnullのItemDataが含まれています。");
            }
        }

        ES3.Save(INVENTORY_SAVE_KEY, itemIDsToSave);
        Debug.Log($"InventoryManager: インベントリをセーブしました。アイテム数: {itemIDsToSave.Count}");
    }

    private void LoadInventory()
    {
        if (ES3.KeyExists(INVENTORY_SAVE_KEY))
        {
            List<string> loadedItemIDs = ES3.Load<List<string>>(INVENTORY_SAVE_KEY);
            ownedItems.Clear(); // 既存のインベントリをクリア

            // ★修正点: パスを "Items" に変更
            // `Assets/Resources/Items` フォルダを指します。
            ItemData[] allAvailableItems = Resources.LoadAll<ItemData>("Items");

            foreach (string itemID in loadedItemIDs)
            {
                ItemData item = allAvailableItems.FirstOrDefault(i => i.id == itemID);

                if (item != null)
                {
                    ownedItems.Add(item);
                    Debug.Log($"InventoryManager: ロード: アイテム '{item.itemName}' (ID: {itemID}) をインベントリに追加しました。");
                }
                else
                {
                    Debug.LogWarning($"InventoryManager: ロード: ID '{itemID}' のアイテムが見つかりませんでした。ResourcesフォルダのパスとItemDataのID設定を確認してください。");
                }
            }
            Debug.Log($"InventoryManager: インベントリをロードしました。アイテム数: {ownedItems.Count}");
        }
        else
        {
            Debug.Log("InventoryManager: ロード: 保存されたインベントリデータはありませんでした。");
        }

        // ロード後、UIを更新する
        if (inventoryUIController != null)
        {
            inventoryUIController.RequestUIUpdateFromManager();
        }
    }
}