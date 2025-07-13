using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CharacterDetailUI : MonoBehaviour
{
    [Header("UI参照")]
    [Tooltip("キャラ画像を表示するImage")]
    public Image characterImage;

    [Tooltip("キャラ名を表示するTextMeshPro")]
    public TextMeshProUGUI characterNameText;

    [Tooltip("キャラ説明を表示するTextMeshPro（複数行）")]
    public TextMeshProUGUI detailText;

    [Tooltip("背景タップで閉じるためのボタン")]
    public Button backgroundCloseButton;

    private void Start()
    {
        if (backgroundCloseButton != null)
            backgroundCloseButton.onClick.AddListener(() => gameObject.SetActive(false));
    }

    public void ShowCharacterDetail(CharacterData data, int stageIndex)
    {
        if (data.characterImages.Length > stageIndex)
            characterImage.sprite = data.characterImages[stageIndex];

        characterNameText.text = data.characterName;

        if (data.detailTexts.Length > stageIndex)
            detailText.text = data.detailTexts[stageIndex];

        gameObject.SetActive(true);
    }

    public void HideSelf()
    {
        gameObject.SetActive(false);
    }
}
