using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector3 startPosition;
    private RectTransform rectTransform;
    private Canvas canvas;

    [Header("Alpha判定しきい値（0〜1）")]
    [Range(0f, 1f)]
    public float alphaThreshold = 0.5f;

    private Image image;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        image = GetComponent<Image>();

        TrySetAlphaThreshold();
    }

    private void TrySetAlphaThreshold()
    {
        if (image != null && image.sprite != null && image.sprite.texture != null)
        {
            Texture2D texture = image.sprite.texture;

            // テクスチャが読み込み可能でCrunch圧縮でない場合のみ設定
            if (texture.isReadable && !IsCrunchCompressed(texture.format))
            {
                image.alphaHitTestMinimumThreshold = alphaThreshold;
                //Debug.Log("alphaHitTestMinimumThreshold 設定成功");
            }
            else
            {
                Debug.LogWarning("alphaHitTestMinimumThreshold を設定できません（Textureが読み込み不可か、Crunch圧縮）この警告は問題なし");
            }
        }
    }

    private bool IsCrunchCompressed(TextureFormat format)
    {
        return format == TextureFormat.DXT1Crunched || format == TextureFormat.DXT5Crunched;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!IsPointerOverImage(eventData)) return;

        startPosition = rectTransform.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!IsPointerOverImage(eventData)) return;

        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        CheckForHiddenText(); // ドラッグ中に重なり判定を行う
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // ドラッグ終了時の処理
        CheckForHiddenText(true); // ドラッグ終了時にも重なり判定を行う
    }

    private bool IsPointerOverImage(PointerEventData eventData)
    {
        return image != null && image.IsRaycastLocationValid(eventData.position, eventData.enterEventCamera);
    }

    private void CheckForHiddenText(bool onDragEnd = false)
    {
        // RedSheetのRectTransformを取得
        RectTransform redSheetRect = rectTransform;

        // すべてのHiddenTextControllerを見つけて、RedSheetが完全に含まれているかを判定
        HiddenTextController[] allHiddenTexts = FindObjectsOfType<HiddenTextController>();
        bool isAnyHiddenTextRevealed = false;

        foreach (var hiddenText in allHiddenTexts)
        {
            // HiddenTextのRectTransformを取得
            RectTransform hiddenTextRect = hiddenText.GetComponent<RectTransform>();

            if (hiddenTextRect != null && redSheetRect != null)
            {
                // RectTransformが完全に含まれているかを判定
                if (IsFullyContained(redSheetRect, hiddenTextRect))
                {
                    hiddenText.SetRevealed(true);
                    isAnyHiddenTextRevealed = true;
                }
                else if (!onDragEnd)
                {
                    hiddenText.SetRevealed(false);
                }
            }
        }

        if (onDragEnd && !isAnyHiddenTextRevealed)
        {
            foreach (var hiddenText in allHiddenTexts)
            {
                hiddenText.SetRevealed(false);
            }
        }
    }

    // RectTransformが別のRectTransformに完全に含まれているかを判定するメソッド
    // contentがcontainerに完全に含まれている場合にtrueを返す
    private bool IsFullyContained(RectTransform container, RectTransform content)
    {
        Vector3[] containerCorners = new Vector3[4];
        Vector3[] contentCorners = new Vector3[4];
        container.GetWorldCorners(containerCorners);
        content.GetWorldCorners(contentCorners);

        // contentのすべての角がcontainerの内部にあるかをチェック
        for (int i = 0; i < 4; i++)
        {
            if (contentCorners[i].x < containerCorners[0].x ||
                contentCorners[i].x > containerCorners[2].x ||
                contentCorners[i].y < containerCorners[0].y ||
                contentCorners[i].y > containerCorners[2].y)
            {
                return false;
            }
        }

        return true;
    }
}