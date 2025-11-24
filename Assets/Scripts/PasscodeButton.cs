using UnityEngine;
using UnityEngine.UI;

public class PasscodeButton : MonoBehaviour
{
    // Inspectorで設定する情報
    public PasscodeLock passcodeLock; // PasscodeLockスクリプトへの参照
    public int index;                // どの桁（0～3）を操作するか
    public int direction;            // どの方向（+1 または -1）に動かすか

    void Start()
    {
        // Buttonコンポーネントを取得し、OnClickイベントに関数を登録
        Button button = GetComponent<Button>();
        if (button != null)
        {
            // OnClick時に「OnClickAction」を呼び出すように設定
            button.onClick.AddListener(OnClickAction);
        }

        // PasscodeLockが未設定の場合、親から探す
        if (passcodeLock == null)
        {
            passcodeLock = GetComponentInParent<PasscodeLock>();
        }
    }

    // OnClickイベントから呼び出されるアクション（引数なし）
    public void OnClickAction()
    {
        if (passcodeLock != null)
        {
            // 保持している index と direction を渡して本体の関数を呼び出す
            passcodeLock.ChangeNumber(index, direction);
        }
    }
}