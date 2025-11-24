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

    // ★追加★ Inspectorでリセット位置を上書きするための変数
    [Header("リセット位置の強制上書き")]
    public Vector2 resetPositionOverride = Vector2.zero;

    private Vector2 initialPosition;

    // Cornerの配列をクラス変数として宣言（メモリ最適化）
    private readonly Vector3[] corners1 = new Vector3[4];
    private readonly Vector3[] corners2 = new Vector3[4];


    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        image = GetComponent<Image>();

        TrySetAlphaThreshold();

        // ★修正★: Awake時に初期位置を保存 (overrideが設定されていればそれを優先)
        if (rectTransform != null)
        {
            if (resetPositionOverride == Vector2.zero)
            {
                // overrideがなければ、現在の位置を初期位置として保存
                initialPosition = rectTransform.anchoredPosition;
            }
            else
            {
                // overrideがあれば、その値をinitialPositionとして扱う
                initialPosition = resetPositionOverride;
            }
        }
    }

    private void TrySetAlphaThreshold()
    {
        if (image != null && image.sprite != null && image.sprite.texture != null)
        {
            Texture2D texture = image.sprite.texture;

            if (texture.isReadable && !IsCrunchCompressed(texture.format))
            {
                image.alphaHitTestMinimumThreshold = alphaThreshold;
            }
            else
            {
                Debug.LogWarning("alphaHitTestMinimumThreshold を設定できません（Textureが読み込み不可か、Crunch圧縮）");
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
        CheckForHiddenText();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        CheckForHiddenText(true);
    }

    // ★修正★ ResetPosition() メソッド
    public void ResetPosition()
    {
        if (rectTransform != null)
        {
            // initialPosition (Awake時に設定された強制位置または初期位置) にリセット
            rectTransform.anchoredPosition = initialPosition;
            CheckForHiddenText(true);
        }
    }

    private bool IsPointerOverImage(PointerEventData eventData)
    {
        return image != null && image.IsRaycastLocationValid(eventData.position, eventData.enterEventCamera);
    }

    private void CheckForHiddenText(bool onDragEnd = false)
    {
        RectTransform redSheetRect = rectTransform;
        HiddenTextController[] allHiddenTexts = FindObjectsOfType<HiddenTextController>();
        bool isAnyHiddenTextRevealed = false;

        foreach (var hiddenText in allHiddenTexts)
        {
            RectTransform hiddenTextRect = hiddenText.GetComponent<RectTransform>();

            if (hiddenTextRect != null && redSheetRect != null)
            {
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

    private bool IsFullyContained(RectTransform container, RectTransform content)
    {
        container.GetWorldCorners(corners1);
        content.GetWorldCorners(corners2);

        for (int i = 0; i < 4; i++)
        {
            if (corners2[i].x < corners1[0].x ||
                corners2[i].x > corners1[2].x ||
                corners2[i].y < corners1[0].y ||
                corners2[i].y > corners1[2].y)
            {
                return false;
            }
        }

        return true;
    }
}