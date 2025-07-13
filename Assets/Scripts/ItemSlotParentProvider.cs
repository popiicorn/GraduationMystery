using UnityEngine;

// このスクリプトは、ItemSlotParent（Content）のTransformを他のスクリプトに提供するためのものです。
public class ItemSlotParentProvider : MonoBehaviour
{
    // 自身のTransformを公開します。Inspectorには表示されません。
    // これをInventoryUIControllerが取得します。
    public Transform GetContentTransform()
    {
        return transform;
    }

    // シングルトンパターン（オプション）: 他の場所から簡単にアクセスできるようにする
    // ただし、Contentは一つしかないので、GetComponentInChildrenなどで取得する方が適切かもしれません。
    // public static ItemSlotParentProvider Instance { get; private set; }

    // void Awake()
    // {
    //     if (Instance != null && Instance != this)
    //     {
    //         Destroy(gameObject);
    //     }
    //     else
    //     {
    //         Instance = this;
    //     }
    // }
}