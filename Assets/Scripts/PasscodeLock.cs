using UnityEngine;
using UnityEngine.UI;
using TMPro; // InputField (TMP) を使うために必要

public class PasscodeLock : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField[] inputFields = new TMP_InputField[4]; // 4つの数字表示フィールド

    public GameObject doorToOpen;        // 【非表示にする】元の扉のGameObject
    public GameObject doorOpenObject;    // 【表示する】開いた後の扉や通路のGameObject ★ここが追加されました★

    public GameObject passcodePanel;     // パスコード入力パネル

    [Header("Settings")]
    public int[] correctCode = { 1, 2, 3, 4 };          // 正解の暗証番号 (Inspectorで設定)
    private int[] currentCode = { 0, 0, 0, 0 };         // 現在入力されている数字

    void Start()
    {
        // パネル非表示を一時的に解除するため、この行はコメントアウトを継続します
        
        if (passcodePanel != null)
        {
            passcodePanel.SetActive(false);
        }
        

        // 強制的にInputFieldのテキストをクリア
        for (int i = 0; i < inputFields.Length; i++)
        {
            if (inputFields[i] != null)
            {
                inputFields[i].text = "";
            }
        }

        // 初期状態で、開いた扉のオブジェクトを非表示にしておく
        if (doorOpenObject != null)
        {
            doorOpenObject.SetActive(false);
        }

        UpdateDisplay();
    }

    // パスコードパネルを表示する（扉をクリックした時などに呼び出す）
    public void ShowPanel()
    {
        if (passcodePanel != null)
        {
            passcodePanel.SetActive(true);
        }
        UpdateDisplay();
    }

    // パネルを非表示にする
    public void HidePanel()
    {
        if (passcodePanel != null)
        {
            passcodePanel.SetActive(false);
        }
    }

    // 数字を更新するメソッド (UI Buttonから呼び出す)
    // index: 0～3 (どの桁か), direction: +1 (上) または -1 (下)
    public void ChangeNumber(int index, int direction)
    {
        if (index < 0 || index >= 4) return;

        currentCode[index] += direction;

        // 0～9の範囲でラップアラウンド
        if (currentCode[index] > 9)
        {
            currentCode[index] = 0;
        }
        else if (currentCode[index] < 0)
        {
            currentCode[index] = 9;
        }

        UpdateDisplay();
    }

    // UIの表示を更新する
    private void UpdateDisplay()
    {
        for (int i = 0; i < 4; i++)
        {
            // InputFieldのテキストを現在の数字に設定
            if (inputFields[i] != null)
            {
                inputFields[i].text = currentCode[i].ToString();
            }
        }
    }

    // 確認ボタンが押されたときの処理
    public void CheckCode()
    {
        bool isCorrect = true;
        for (int i = 0; i < 4; i++)
        {
            if (currentCode[i] != correctCode[i])
            {
                isCorrect = false;
                break;
            }
        }

        if (isCorrect)
        {
            Debug.Log("暗証番号が正解しました！扉を開きます。");

            // 1. 元の扉（閉じた状態）を非表示にする
            if (doorToOpen != null)
            {
                doorToOpen.SetActive(false);
            }

            // 2. 新しいオブジェクト（開いた扉や次の通路）を表示する
            if (doorOpenObject != null)
            {
                doorOpenObject.SetActive(true);
            }

            HidePanel();
        }
        else
        {
            Debug.Log("暗証番号が間違っています。");
        }
    }
}