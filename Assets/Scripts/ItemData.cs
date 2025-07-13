using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    [Tooltip("アイテムを識別するためのユニークなID。例: 'key_silver', 'hammer_old'.")]
    public string id; // ユニークな識別子。セーブ・ロードや謎解き判定に必要

    [Header("表示情報")]
    public string itemName;
    public Sprite itemIcon;
    [TextArea(3, 5)] // Inspectorでの表示を複数行にする
    public string itemDescription;

    // 必要に応じて追加のプロパティ
    // public ItemType itemType; // 例えば、鍵、道具、食べ物など
    // public bool canBeUsed;
}