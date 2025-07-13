using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic; // Listを使うために必要

/*
 * 役割：ズームされた写真内にある「拾えるアイテム」を表すスクリプト
 * 詳細：Unityの IPointerClickHandler を使い、クリックされた時に反応。
 * 指定された ItemData を取得。
 * InventoryManager に追加し、ゲーム内の「所持アイテム」に加える。
 * 拾った後は gameObject.SetActive(false) で非表示にする（何度も拾えないように）
 * ゲーム終了・再開後も拾われた状態を維持するため、その状態を保存・復元する。
 */

public class ItemPickup : MonoBehaviour, IPointerClickHandler
{
    [Header("ScriptableObject で作成したアイテムデータをアタッチ")]
    public ItemData itemData;

    [Header("アイテムの一意なID (手動で設定)")]
    // シーン内の各拾えるアイテムに異なるIDを設定してください！
    public string uniquePickupID;

    private const string PickedUpKeyPrefix = "PickedUpItem_";

    void Start()
    {
        // ゲーム開始時に、このアイテムが以前に拾われたか確認
        if (string.IsNullOrEmpty(uniquePickupID))
        {
            Debug.LogError($"ItemPickup: '{gameObject.name}' に uniquePickupID が設定されていません！");
            return;
        }

        // ES3 で保存された状態をチェック (Easy Save 3を使用している場合)
        if (ES3.KeyExists(PickedUpKeyPrefix + uniquePickupID) && ES3.Load<bool>(PickedUpKeyPrefix + uniquePickupID))
        {
            gameObject.SetActive(false); // 以前に拾われていれば非表示にする
            Debug.Log($"ItemPickup: '{itemData.itemName}' (ID: {uniquePickupID}) は以前拾われました。非表示にします。");
        }
        else
        {
            gameObject.SetActive(true); // 拾われていなければ表示する
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // インベントリにアイテムを追加
        InventoryManager.Instance.AddItem(itemData);

        // このアイテムGameObjectを非表示にする
        gameObject.SetActive(false);
        Debug.Log($"ItemPickup: '{itemData.itemName}' (ID: {uniquePickupID}) を拾いました。非表示にします。");

        // このアイテムが拾われた状態をセーブ
        if (!string.IsNullOrEmpty(uniquePickupID))
        {
            ES3.Save(PickedUpKeyPrefix + uniquePickupID, true);
            Debug.Log($"ItemPickup: 状態をセーブしました。Key: {PickedUpKeyPrefix + uniquePickupID}");
        }
        else
        {
            Debug.LogError($"ItemPickup: '{gameObject.name}' に uniquePickupID が設定されていないため、状態をセーブできませんでした。");
        }
    }
}