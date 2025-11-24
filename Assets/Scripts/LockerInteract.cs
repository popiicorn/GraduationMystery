using UnityEngine;

public class LockerInteract : MonoBehaviour
{
    [Header("参照するパスコードパネル")]
    public PasscodeLock passcodeLock; // PasscodePanelにアタッチされたスクリプト

    void Update()
    {
        // マウスの左クリックを検知
        if (Input.GetMouseButtonDown(0))
        {
            CheckClick();
        }
    }

    private void CheckClick()
    {
        // マウスの位置からRayを飛ばして、オブジェクトを検知
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // 3Dの場合の判定 (2Dの場合は下を参照)
        if (Physics.Raycast(ray, out hit))
        {
            // Rayが当たったオブジェクトが、このスクリプトがアタッチされたオブジェクト自身か確認
            if (hit.collider.gameObject == gameObject)
            {
                OnLockerClicked();
            }
        }

        /* // 2Dの場合の判定（RaycastHit2Dを使う場合）
        RaycastHit2D hit2D = Physics2D.Raycast(ray.origin, ray.direction);
        if (hit2D.collider != null && hit2D.collider.gameObject == gameObject)
        {
            OnLockerClicked();
        }
        */
    }

    private void OnLockerClicked()
    {
        Debug.Log("ロッカーの鍵がクリックされました！");

        // PasscodeLockスクリプトのShowPanelメソッドを呼び出す
        if (passcodeLock != null)
        {
            passcodeLock.ShowPanel();
        }
    }
}