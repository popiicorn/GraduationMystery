using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class PhotoZoomController : MonoBehaviour, IScrollHandler
{
    [Header("ズームパネル")]
    public GameObject zoomPanel;

    [Header("ズーム対象の画像_MovableObject_Rootをアタッチ")]
    public RectTransform zoomTarget;

    [Header("ズーム設定")]
    public float zoomSpeed = 0.1f;
    public float minScale = 0.5f;
    public float maxScale = 3f;


    // 子オブジェクトの初期位置保存用
    private Dictionary<RectTransform, Vector3> originalPositions = new ();

    private void Start()
    {
        // 初期位置を保存（RectTransformの子要素のみ対象）
        foreach (RectTransform child in zoomTarget)
        {
            originalPositions[child] = child.anchoredPosition;
        }
    }

    public void ShowZoomPanel()
    {
        zoomPanel.SetActive(true);

        // ズーム倍率リセット
        zoomTarget.localScale = Vector3.one;

        // 子の位置をリセット
        foreach (var pair in originalPositions)
        {
            pair.Key.anchoredPosition = pair.Value;
        }

        //Debug.Log("ズームパネルを表示＆位置リセット");
    }

    public void HideZoomPanel()
    {
        zoomPanel.SetActive(false);
        //Debug.Log("ズームパネルを非表示");
    }

    public void OnScroll(PointerEventData eventData)
    {
        float scrollDelta = eventData.scrollDelta.y;
        if (scrollDelta == 0 || zoomTarget == null) return;

        Vector3 scale = zoomTarget.localScale;
        scale += Vector3.one * scrollDelta * zoomSpeed;
        scale = ClampScale(scale);
        zoomTarget.localScale = scale;

        //Debug.Log($"ズーム倍率: {zoomTarget.localScale}");
    }

    private Vector3 ClampScale(Vector3 scale)
    {
        float clamped = Mathf.Clamp(scale.x, minScale, maxScale);
        return new Vector3(clamped, clamped, clamped);
    }
}
