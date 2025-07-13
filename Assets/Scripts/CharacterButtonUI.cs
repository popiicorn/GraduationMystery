using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterButtonUI : MonoBehaviour
{
    [Header("UI Components")]
    [Tooltip("キャラクターの画像を表示するImageコンポーネント")]
    public Image characterImage;

    [Tooltip("キャラクターの名前を表示するTextMeshProUGUI")]
    public TextMeshProUGUI characterNameText;

    [Header("内部参照（自動設定されるので触らない）")]
    [Tooltip("キャラクターのデータ（自動設定）")]
    [SerializeField] private CharacterData characterData;

    [Tooltip("詳細表示UI（自動設定）")]
    [SerializeField] private CharacterDetailUI detailUI;

    [Tooltip("現在のステージインデックス（自動設定）")]
    [SerializeField] private int stageIndex = 0;

    public void Initialize(CharacterData data, CharacterDetailUI detailUIPrefab, int currentStageIndex)
    {
        characterData = data;
        detailUI = detailUIPrefab;
        stageIndex = currentStageIndex;

        if (data.characterImages != null && data.characterImages.Length > stageIndex)
            characterImage.sprite = data.characterImages[stageIndex];

        if (characterNameText != null)
            characterNameText.text = data.characterName;

        GetComponent<Button>().onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        if (detailUI != null && characterData != null)
            detailUI.ShowCharacterDetail(characterData, stageIndex);
    }
}
