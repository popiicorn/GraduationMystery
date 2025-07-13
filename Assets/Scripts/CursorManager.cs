using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [SerializeField] Texture2D icon; // Inspectorから設定するデフォルトのカーソル画像
    private int size; // カーソル画像のサイズを保持する変数
    [SerializeField] float hotSpotX = 0; // Inspectorから設定するホットスポットのX座標

    // Awake() は Start() より早く呼ばれるため、初期化処理に適しています。
    private void Awake()
    {
        // 画面幅に基づいてカーソルサイズを計算します。
        // この計算式は特定の解像度 (1920) に基づいていますが、必要に応じて調整してください。
        // カーソルサイズは通常、小さい固定値 (例: 32, 64) を直接指定する方が一般的です。
        size = ((57 * 100 * Screen.width) / 1920) / 100;

        // シーンをロードしてもカーソルマネージャーが破棄されないようにする場合 (任意)
        // DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // ゲーム開始時にカスタムカーソルを設定します。
        SetCursor(icon);
    }

    // Update() は不要なので削除しました。
    // カーソルは一度設定すれば通常はUpdateで毎フレーム設定し直す必要はありません。
    // private void Update()
    // {
    //     // Cursor.SetCursor(ResizeTexture(icon, size, size), new Vector2(1, 1)* hotSpotX, CursorMode.ForceSoftware);
    // }

    /// <summary>
    /// マウスカーソルの画像とホットスポットを設定します。
    /// </summary>
    /// <param name="cursorTexture">設定するカーソル画像。nullの場合はデフォルトのアイコンを使用。</param>
    public void SetCursor(Texture2D cursorTexture)
    {
        // 引数で渡されたiconがnullの場合、Inspectorで設定されたデフォルトのiconを使用します。
        if (cursorTexture == null)
        {
            cursorTexture = this.icon;
        }

        // カーソル画像が依然としてnullの場合は、システムデフォルトに戻します。
        if (cursorTexture == null)
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto); // null と Vector2.zero でシステムデフォルトカーソルに戻る
            Debug.Log("CursorManager: カーソル画像が設定されていないため、システムデフォルトに戻しました。");
            return;
        }

        // ResizeTexture で画像をリサイズし、ホットスポットを設定してカーソルを設定します。
        // Vector2.one * hotSpotX は、(hotSpotX, hotSpotX) というホットスポットになります。
        // もしホットスポットをX軸のみで制御したい場合は、new Vector2(hotSpotX, hotSpotY) のようにY座標も明示的に設定することを検討してください。
        Cursor.SetCursor(ResizeTexture(cursorTexture, size, size), new Vector2(hotSpotX, hotSpotX), CursorMode.ForceSoftware);
        Debug.Log($"CursorManager: カーソルを '{cursorTexture.name}' に変更しました。サイズ: {size}x{size}, ホットスポット: ({hotSpotX},{hotSpotX})");
    }

    /// <summary>
    /// 指定されたテクスチャを新しいサイズにリサイズします。
    /// </summary>
    /// <param name="srcTexture">元のテクスチャ。</param>
    /// <param name="newWidth">新しい幅。</param>
    /// <param name="newHeight">新しい高さ。</param>
    /// <returns>リサイズされた新しいTexture2Dオブジェクト。</returns>
    Texture2D ResizeTexture(Texture2D srcTexture, int newWidth, int newHeight)
    {
        // ResizeTextureが呼ばれる際に、srcTextureがnullでないことを確認します。
        if (srcTexture == null)
        {
            Debug.LogError("CursorManager: ResizeTexture の入力テクスチャが null です。");
            return null; // nullを返すか、エラー処理を行う
        }

        // 新しいTexture2Dを作成
        var resizedTexture = new Texture2D(newWidth, newHeight, TextureFormat.RGBA32, false);

        // Graphics.ConvertTexture を使用してソーステクスチャからデータを変換
        // これはGPU上でのテクスチャ変換を行うため、通常は高速です。
        Graphics.ConvertTexture(srcTexture, resizedTexture);

        // ResizeTextureは新しいTexture2Dを返すため、元のテクスチャに影響を与えません。
        return resizedTexture;
    }
}