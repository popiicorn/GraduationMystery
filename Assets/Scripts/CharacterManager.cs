using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    [Header("設定")]
    [Tooltip("ボタンの親になるUIコンテナ（6列×5行などGridLayoutを使用）")]
    public Transform buttonContainer;

    [Tooltip("キャラクターボタンのプレハブ")]
    public GameObject characterButtonPrefab;

    [Tooltip("キャラクターデータ（ScriptableObject）の格納フォルダ名")]
    public string resourcesFolderName = "CharacterData";

    [Tooltip("現在のステージインデックス（画像・テキストの選択に使用）")]
    public int currentStageIndex = 0;

    [Header("詳細表示UI")]
    [Tooltip("詳細UIのプレハブ")]
    public CharacterDetailUI characterDetailUIPrefab;

    private void Start()
    {
        LoadAndCreateButtons();
    }

    void LoadAndCreateButtons()
    {
        CharacterData[] allCharacterData = Resources.LoadAll<CharacterData>(resourcesFolderName);

        foreach (var data in allCharacterData)
        {
            GameObject buttonObj = Instantiate(characterButtonPrefab, buttonContainer);
            CharacterButtonUI buttonUI = buttonObj.GetComponent<CharacterButtonUI>();
            buttonUI.Initialize(data, characterDetailUIPrefab, currentStageIndex);
        }
    }
}
