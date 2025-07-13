using UnityEngine;
using UnityEngine.UI; // UI.Image を操作する場合に必要
using UnityEngine.EventSystems; // IPointerClickHandler を使用するために必要
using System.Linq; // LINQ (Contains など) を使用するために必要
using System.Collections.Generic; // List<T> を使用するために必要

public class PuzzleObjectClicker : MonoBehaviour, IPointerClickHandler
{
    // ★ ImageChangeEntry クラスは、PuzzleObjectClicker クラスの "内側" に定義します ★
    // こうすることで、Unity EditorのInspectorでこのクラスのリストを表示・設定できるようになります。
    [System.Serializable]
    public class ImageChangeEntry
    {
        [Tooltip("変更したいImage、SpriteRenderer、またはRendererコンポーネントを持つGameObject。")]
        public GameObject targetObject;

        [Tooltip("新しいスプライト（2DスプライトやUI.Imageの場合）。")]
        public Sprite newSprite;

        [Tooltip("新しいマテリアル（3Dモデルの場合）。")]
        public Material newMaterial;
    }

    [Header("謎解きの設定")]
    [Tooltip("このオブジェクトで使用すると正解となるアイテムのID。複数指定する場合は配列で定義。")]
    public string[] requiredItemIds; // このオブジェクトが受け付けるアイテムのID (複数対応)

    [Tooltip("アイテムが不要なクリックイベント（純粋な調査など）の場合にチェック。")]
    public bool isInspectionOnly = false; // アイテムなしでクリックした際にイベントを発生させるか

    [Header("謎解き成功時の変化")]
    [Tooltip("謎解き成功時に変更する画像の設定リスト。複数の画像を変更できます。")]
    public List<ImageChangeEntry> imageChanges = new List<ImageChangeEntry>(); // 複数の画像変更エントリを保持

    [Tooltip("クリック後に追加する新しいオブジェクト（Prefabを割り当てる）。")]
    public GameObject objectToInstantiate; // クリック後に追加するオブジェクト（Prefab）
    [Tooltip("新しいオブジェクトを生成する場所。未設定の場合、このオブジェクトの位置に生成。")]
    public Transform instantiatePoint; // 新しいオブジェクトの生成位置

    [Tooltip("正解時に、使用したアイテムをインベントリから削除するか。")]
    public bool consumeActiveItem = true; // 正解時に使用したアイテムを消費するか

    [Header("状態管理")]
    [Tooltip("この謎解きが既に解決済みか。")]
    public bool isSolved = false; // この謎解きが既に解決済みか

    private InventoryUIController inventoryUIController; // インベントリコントローラーへの参照

    void Awake()
    {
        // シングルトンパターンで InventoryUIController のインスタンスを取得
        inventoryUIController = InventoryUIController.Instance;
        if (inventoryUIController == null)
        {
            Debug.LogError("PuzzleObjectClicker: InventoryUIController.Instance が見つかりません。InventoryUIController がシーンに存在し、Awake()でInstanceが設定されているか確認してください。");
        }
    }

    /// <summary>
    /// オブジェクトがクリックされたときに呼び出されます。
    /// </summary>
    /// <param name="eventData">クリックイベントデータ</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        // 既に謎解きが解決済みなら何もしない
        if (isSolved)
        {
            Debug.Log($"PuzzleObjectClicker: {gameObject.name} は既に解決済みです。");
            return;
        }

        Debug.Log($"PuzzleObjectClicker: {gameObject.name} がクリックされました。");

        // InventoryUIController が有効であることを確認
        if (inventoryUIController == null)
        {
            Debug.LogError($"PuzzleObjectClicker: {gameObject.name}: InventoryUIController が null です。クリック処理を続行できません。");
            return;
        }

        // 有効なアイテムを取得
        ItemData activeItem = inventoryUIController.GetActiveItem();

        if (activeItem != null)
        {
            // アイテムが有効化されている場合
            Debug.Log($"PuzzleObjectClicker: 有効なアイテムは '{activeItem.itemName}' (ID: {activeItem.id}) です。");

            // 有効アイテムがこのオブジェクトで必要なアイテムのいずれかに合致するかチェック
            // ItemData.id が必須
            bool isCorrectItem = requiredItemIds.Contains(activeItem.id);

            if (isCorrectItem)
            {
                // --- 謎解き成功時の処理 ---
                Debug.Log($"PuzzleObjectClicker: 正解！'{activeItem.itemName}' が {gameObject.name} に使用されました。");
                SolvePuzzle(activeItem); // 謎解き成功処理を呼び出す
            }
            else
            {
                // 間違ったアイテムが使用された
                Debug.Log($"PuzzleObjectClicker: '{activeItem.itemName}' は {gameObject.name} では使用できません。");
                // 失敗時のフィードバック（例: 効果音、メッセージ表示など）
            }
        }
        else // activeItem == null
        {
            // アイテムが有効化されていない場合
            if (isInspectionOnly)
            {
                // アイテム不要のクリック（調査など）
                Debug.Log($"PuzzleObjectClicker: {gameObject.name} を調査しました。");
                // ここに調査時の処理を実装（例: メッセージ表示、音を鳴らすなど）
                SolvePuzzle(null); // アイテムなしで謎が解ける場合
            }
            else
            {
                // アイテムが必要だが有効化されていない
                Debug.Log($"PuzzleObjectClicker: {gameObject.name} をクリックしましたが、使用するアイテムが有効化されていません。");
            }
        }
    }

    /// <summary>
    /// 謎解き成功時の具体的な処理をまとめます。
    /// </summary>
    /// <param name="usedItem">使用されたアイテムデータ（アイテム不要な場合はnull）</param>
    private void SolvePuzzle(ItemData usedItem)
    {
        isSolved = true; // 謎解き解決済みとしてマーク

        // 1. オブジェクトの見た目や状態の変化
        // imageChanges リスト内のすべてのエントリに対してループ処理を行う
        foreach (ImageChangeEntry entry in imageChanges)
        {
            if (entry.targetObject != null)
            {
                // SpriteRenderer を持つオブジェクトの見た目変更
                SpriteRenderer spriteRenderer = entry.targetObject.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null && entry.newSprite != null)
                {
                    spriteRenderer.sprite = entry.newSprite;
                    Debug.Log($"PuzzleObjectClicker: '{entry.targetObject.name}' のスプライトを変更しました。");
                }

                // Renderer (MeshRendererなど) を持つ3Dオブジェクトの見た目変更
                Renderer meshRenderer = entry.targetObject.GetComponent<Renderer>();
                if (meshRenderer != null && entry.newMaterial != null)
                {
                    meshRenderer.material = entry.newMaterial;
                    Debug.Log($"PuzzleObjectClicker: '{entry.targetObject.name}' のマテリアルを変更しました。");
                }

                // UI.Image を持つオブジェクトの見た目変更 (Canvas上の画像など)
                Image uiImage = entry.targetObject.GetComponent<Image>();
                if (uiImage != null && entry.newSprite != null) // UI.Image の場合は newSprite を使用
                {
                    uiImage.sprite = entry.newSprite;
                    uiImage.enabled = true; // Imageが非表示の場合のために有効化
                    Debug.Log($"PuzzleObjectClicker: '{entry.targetObject.name}' のUI Imageスプライトを変更しました。");
                }
            }
            else
            {
                Debug.LogWarning($"PuzzleObjectClicker: ImageChangeEntry の Target Object が割り当てられていません。");
            }
        }

        // 2. 新しいオブジェクトの追加
        if (objectToInstantiate != null)
        {
            Vector3 spawnPoint = (instantiatePoint != null) ? instantiatePoint.position : transform.position;
            Instantiate(objectToInstantiate, spawnPoint, Quaternion.identity);
            Debug.Log($"PuzzleObjectClicker: '{objectToInstantiate.name}' を生成しました。");
        }

        // 3. 使用したアイテムの削除（消費）
        if (consumeActiveItem && usedItem != null)
        {
            inventoryUIController.RemoveItem(usedItem); // InventoryUIController にアイテム削除を依頼
            Debug.Log($"PuzzleObjectClicker: '{usedItem.itemName}' をインベントリから削除しました。");
        }

        // 4. 有効化状態のリセット
        // アイテムを使用したか否かに関わらず、クリックアクション後に現在の有効アイテムをリセットする
        inventoryUIController.ResetActiveItem();
        Debug.Log($"PuzzleObjectClicker: アクティブアイテムの状態をリセットしました。");

        // 謎解き成功後の追加処理（例: 次の謎へ進む、イベント発火など）
        // GameEvents.onPuzzleSolved?.Invoke(); // 必要に応じてUnityEventなどを追加
    }
}