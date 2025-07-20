using UnityEngine;
using System.Collections.Generic;
using System.Linq; // OrderByを使うため残します

public class CharacterManager : MonoBehaviour
{
    [Header("ボタン生成設定")]
    [Tooltip("キャラクターボタンのプレハブ")]
    public GameObject characterButtonPrefab;

    [Tooltip("キャラクターデータ（ScriptableObject）の格納フォルダ名")]
    public string resourcesFolderName = "Characters";

    [Tooltip("現在のステージインデックス（画像・テキストの選択に使用）")]
    public int currentStageIndex = 0;

    [Header("詳細表示UI")]
    [Tooltip("詳細UIのプレハブ")]
    public CharacterDetailUI characterDetailUIPrefab;

    [Header("ページ設定")]
    [Tooltip("左ページにボタンを配置する親Transform")]
    public Transform leftPageParent;

    [Tooltip("右ページにボタンを配置する親Transform")]
    public Transform rightPageParent;

    [Tooltip("教師ページにボタンを配置する親Transform")]
    public Transform teacherPageParent;

    // ゲーム内の全てのキャラクターデータ (Resourcesからロードされる - 教師も含む)
    private List<CharacterData> allCharactersInGame = new List<CharacterData>();

    // 現在左右のページに表示されているキャラクターのリスト (動的に更新される)
    private List<CharacterData> currentLeftPageCharacters = new List<CharacterData>();
    private List<CharacterData> currentRightPageCharacters = new List<CharacterData>();
    // currentTeacherCharacterは、initialTeacherCharacterの参照を保持するだけで良くなります
    private CharacterData currentTeacherCharacter;

    // 初期表示用のキャラクターデータ (Unityエディタで設定)
    [Header("初期表示キャラクター")]
    [Tooltip("ゲーム開始時に左ページに表示するキャラクターデータのリスト")]
    public List<CharacterData> initialLeftPageCharacters;
    [Tooltip("ゲーム開始時に右ページに表示するキャラクターデータのリスト")]
    public List<CharacterData> initialRightPageCharacters;
    [Tooltip("ゲーム開始時に教師ページに表示するキャラクターデータ")]
    public CharacterData initialTeacherCharacter; // ★教師のデータは直接ここから取得します


    private bool isAllCharactersUnlocked = false;


    private void Awake()
    {
        // ゲーム開始前に全てのキャラクターデータをロードしておく
        CharacterData[] loadedData = Resources.LoadAll<CharacterData>(resourcesFolderName);
        allCharactersInGame.AddRange(loadedData);

        // ロードしたデータを名前順などでソートすると管理しやすい
        // Teacherも含まれるが、UnlockAllCharactersでは別途処理される
        allCharactersInGame = allCharactersInGame.OrderBy(data => data.characterName).ToList();
    }

    private void Start()
    {
        currentLeftPageCharacters.AddRange(initialLeftPageCharacters);
        currentRightPageCharacters.AddRange(initialRightPageCharacters);
        currentTeacherCharacter = initialTeacherCharacter; // 初期表示の教師を設定

        GenerateCharacterButtons(currentLeftPageCharacters, leftPageParent);
        GenerateCharacterButtons(currentRightPageCharacters, rightPageParent);
        GenerateSingleCharacterButton(currentTeacherCharacter, teacherPageParent);
    }

    /// <summary>
    /// 指定されたキャラクターリストと親Transformを使ってボタンを生成する汎用メソッド。
    /// </summary>
    public void GenerateCharacterButtons(List<CharacterData> charactersToDisplay, Transform parentTransform)
    {
        if (charactersToDisplay == null || parentTransform == null)
        {
            Debug.LogError("キャラクターリストまたは親Transformが設定されていません。");
            return;
        }

        foreach (Transform child in parentTransform)
        {
            Destroy(child.gameObject);
        }

        foreach (var data in charactersToDisplay)
        {
            if (data == null) continue;

            GameObject buttonObj = Instantiate(characterButtonPrefab, parentTransform);
            CharacterButtonUI buttonUI = buttonObj.GetComponent<CharacterButtonUI>();
            if (buttonUI != null)
            {
                buttonUI.Initialize(data, characterDetailUIPrefab, currentStageIndex);
            }
            else
            {
                Debug.LogWarning($"生成されたボタン '{buttonObj.name}' に CharacterButtonUI スクリプトが見つかりません！");
            }
        }
    }

    /// <summary>
    /// 単一のキャラクターデータと親Transformを使ってボタンを生成するメソッド。
    /// </summary>
    public void GenerateSingleCharacterButton(CharacterData characterToDisplay, Transform parentTransform)
    {
        if (characterToDisplay == null || parentTransform == null)
        {
            return; // 教師が設定されていない場合でもエラーにしない
        }

        foreach (Transform child in parentTransform)
        {
            Destroy(child.gameObject);
        }

        GameObject buttonObj = Instantiate(characterButtonPrefab, parentTransform);
        CharacterButtonUI buttonUI = buttonObj.GetComponent<CharacterButtonUI>();
        if (buttonUI != null)
        {
            buttonUI.Initialize(characterToDisplay, characterDetailUIPrefab, currentStageIndex);
        }
        else
        {
            Debug.LogWarning($"生成されたボタン '{buttonObj.name}' に CharacterButtonUI スクリプトが見つかりません！");
        }
    }

    /// <summary>
    /// 謎を解いて全てのキャラクターを表示する際に呼び出すメソッド。
    /// 教師はinitialTeacherCharacterとして別途表示され、生徒が左右に表示されます。
    /// </summary>
    public void UnlockAllCharacters()
    {
        if (isAllCharactersUnlocked) return;

        Debug.Log("謎が解かれました！全てのキャラクターが表示されます。");
        isAllCharactersUnlocked = true;

        currentLeftPageCharacters.Clear();
        currentRightPageCharacters.Clear();
        // currentTeacherCharacterはinitialTeacherCharacterのまま保持されるか、
        // 必要に応じてここで再設定することも可能ですが、今回は初期設定を維持します。

        // allCharactersInGameからinitialTeacherCharacterを除外したリストを生徒として扱う
        List<CharacterData> studentCharacters = allCharactersInGame
                                                    .Where(data => data != initialTeacherCharacter)
                                                    .ToList();

        // 生徒のキャラクターを左右のページに均等に割り振る
        int totalStudents = studentCharacters.Count;
        int halfCount = (totalStudents + 1) / 2;

        for (int i = 0; i < totalStudents; i++)
        {
            if (i < halfCount)
            {
                currentLeftPageCharacters.Add(studentCharacters[i]);
            }
            else
            {
                currentRightPageCharacters.Add(studentCharacters[i]);
            }
        }

        // 左右のページと教師ページを新しいリストで再生成
        GenerateCharacterButtons(currentLeftPageCharacters, leftPageParent);
        GenerateCharacterButtons(currentRightPageCharacters, rightPageParent);
        // 教師は UnlockAllCharacters が呼び出された後も initialTeacherCharacter の内容がそのまま教師ページに表示されます。
        // もし教師の表示自体も謎解きで切り替えるなら、GenerateSingleCharacterButton(initialTeacherCharacter, teacherPageParent)をここに入れる。
        // 今回は教師は常に表示される想定で、Start()で一度生成したらそのままです。
        // ただし、stageIndexの更新で教師も更新されるよう、UpdateStageAndRefreshButtons()からは呼び出します。
        GenerateSingleCharacterButton(initialTeacherCharacter, teacherPageParent); // 明示的に再生成を呼び出す
    }

    // ステージインデックスを更新し、現在表示されているボタンを再初期化する
    public void UpdateStageAndRefreshButtons(int newStageIndex)
    {
        currentStageIndex = newStageIndex;
        // 現在表示されているキャラクターリストを基に再生成
        GenerateCharacterButtons(currentLeftPageCharacters, leftPageParent);
        GenerateCharacterButtons(currentRightPageCharacters, rightPageParent);
        GenerateSingleCharacterButton(currentTeacherCharacter, teacherPageParent); // 教師ページも更新
    }
}