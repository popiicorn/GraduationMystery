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
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 必要なら終了処理を記述
    }

    private bool IsPointerOverImage(PointerEventData eventData)
    {
        return image != null && image.IsRaycastLocationValid(eventData.position, eventData.enterEventCamera);
    }
}
