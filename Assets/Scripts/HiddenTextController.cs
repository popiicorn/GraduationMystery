using UnityEngine;
using UnityEngine.UI;

public class HiddenTextController : MonoBehaviour
{
    private Image hiddenImage;
    private Color originalColor;
    public bool isRevealed = false; // RedSheetでかざされているか

    void Awake()
    {
        hiddenImage = GetComponent<Image>();
        if (hiddenImage != null)
        {
            originalColor = hiddenImage.color;
            hiddenImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0);
        }
    }

    // 他のスクリプトから呼び出されるメソッド
    public void SetRevealed(bool revealed)
    {
        isRevealed = revealed;
    }

    void Update()
    {
        // RedSheetがかざされているときのみalphaを1にする
        if (isRevealed)
        {
            hiddenImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1);
        }
        else
        {
            hiddenImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0);
        }
    }
}