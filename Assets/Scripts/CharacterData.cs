using UnityEngine;

// キャラクターごとの情報を格納するデータアセット
[CreateAssetMenu(fileName = "CharacterData", menuName = "MysteryGame/CharacterData")]
public class CharacterData : ScriptableObject
{
    [Header("基本情報")]
    [Tooltip("キャラクターの名前")]
    public string characterName;

    [Header("画像データ")]
    [Tooltip("ストーリー進行に合わせて変化するキャラ画像")]
    public Sprite[] characterImages;

    [Header("詳細テキスト")]
    [Tooltip("ステージごとの詳細説明文")]
    [TextArea(3, 10)]
    public string[] detailTexts;
}
