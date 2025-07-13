using UnityEngine;
using System.Collections.Generic; // Listを使うために必要
using System.Linq; // 必要に応じてLinqを使う場合
using static ES3; // Easy Save 3 の静的メソッドを直接呼び出すために必要
// using UnityEngine.SceneManagement; // FindObjectsByType のために必要になる場合がありますが、通常は UnityEngine に含まれています

public class SaveDataManager : MonoBehaviour
{
    // インベントリのセーブキー (InventoryManagerと一致させる)
    private const string InventorySaveKey = "Inventory";
    // アイテムピックアップの状態を保存するキーのプレフィックス (ItemPickupと一致させる)
    private const string PickedUpKeyPrefix = "PickedUpItem_";

    // この関数をボタンのOnClick()イベントに割り当てる
    public void DeleteAllSaveData()
    {
        Debug.Log("セーブデータ削除処理を開始します。");

        // インベントリのセーブデータを削除
        if (KeyExists(InventorySaveKey)) // ES3. を省略
        {
            DeleteKey(InventorySaveKey); // ES3. を省略
            Debug.Log($"セーブデータ: '{InventorySaveKey}' を削除しました。");
        }
        else
        {
            Debug.Log($"セーブデータ: '{InventorySaveKey}' は存在しませんでした。");
        }

        // シーン上の各ItemPickupの状態を削除する場合
        // ここは注意が必要：シーンに存在する全てのItemPickupのIDを把握している必要があります。
        // 一般的には、個別のキーを削除するよりも、ファイル全体を削除する方が簡単です。
        // もし特定のIDを持つ拾得アイテムの状態を削除するなら、
        // List<string> allPickupIDs = GetAllKnownPickupIDs(); // あなたのプロジェクトで定義
        // foreach (string id in allPickupIDs)
        // {
        //     if (KeyExists(PickedUpKeyPrefix + id))
        //     {
        //         DeleteKey(PickedUpKeyPrefix + id);
        //         Debug.Log($"セーブデータ: '{PickedUpKeyPrefix + id}' を削除しました。");
        //     }
        // }

        // ★最も包括的な方法: ES3で保存した全てのキーを削除する (デフォルトファイル全体を削除)
        // デフォルトのES3.jsonファイルに全てを保存している場合、これが最も簡単です。
        // しかし、他のセーブデータも全て削除されることに注意！
        DeleteFile(); // ES3. を省略
        Debug.Log("全てのセーブファイル (ES3.jsonなど) を削除しました。");


        // セーブデータ削除後、ゲームの状態をリセット（オプション）
        // メモリ上のインベントリの内容をクリア
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.ClearInventory();
        }

        // シーン上のItemPickupの状態をリセット (シーンを再ロードしない場合)
        // 全てのItemPickupを探して、Activeに戻す (非アクティブなものも含む)
        ItemPickup[] allPickupsInScene = Object.FindObjectsByType<ItemPickup>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (ItemPickup pickup in allPickupsInScene)
        {
            if (pickup != null) // 念のためnullチェック
            {
                pickup.gameObject.SetActive(true);
                Debug.Log($"'{pickup.gameObject.name}' をアクティブに戻しました。");
            }
        }
        Debug.Log("シーン上の全ての拾得アイテムをリセットしました。");

        // シーンを再ロードしてゲームを最初から始めるのが最も確実なリセット方法
        // この行を有効にする場合、DeleteAllSaveData() が呼ばれた後にシーンが切り替わります。
        // UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        // Debug.Log("現在のシーンを再ロードしました。");
    }

    // 確認ダイアログを表示する関数 (よりユーザーフレンドリーにするため)
    public void ShowDeleteConfirmationDialog()
    {
        // ここにUIを使って「本当に削除しますか？」のようなダイアログを表示する処理を追加
        // 例: Pop-up UIを表示し、Yes/Noボタンにそれぞれ DeleteAllSaveData() とキャンセル処理を割り当てる
        Debug.Log("セーブデータ削除の確認ダイアログを表示するつもりです。");
        // デモとして直接削除を呼び出す
        DeleteAllSaveData();
    }
}