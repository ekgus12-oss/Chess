using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public StockfishManager stockfishManager;

    [Header("화면 패널들")]
    public GameObject titlePanel;
    public GameObject botSelectPanel;
    public GameObject playPanel;

    [Header("UI 요소")]
    public TextMeshProUGUI lastMoveText;

    void Start()
    {
        // 게임 시작 시 타이틀 화면만 켜기
        ShowTitle();
    }

    // --- 화면 전환 함수들 ---

    public void ShowTitle()
    {
        titlePanel.SetActive(true);
        botSelectPanel.SetActive(false);
        playPanel.SetActive(false);
    }

    public void ShowBotSelect()
    {
        titlePanel.SetActive(false);
        botSelectPanel.SetActive(true);
        playPanel.SetActive(false);
    }

    public void ShowPlay()
    {
        titlePanel.SetActive(false);
        botSelectPanel.SetActive(false);
        playPanel.SetActive(true);

        // 체스판 생성 및 게임 시작 로직을 여기서 호출할 수도 있습니다.
    }

    // --- 이전 화면으로 가기 ---
    public void OnBackButtonClicked()
    {
        if (playPanel.activeSelf)
            ShowBotSelect();
        else if (botSelectPanel.activeSelf)
            ShowTitle();
    }

    // 기물이 움직였을 때 InputManager가 이 함수를 호출할 겁니다.
    public void UpdateLastMove(string color, string from, string to)
    {
        if (lastMoveText != null)
        {
            // 예: "White: e2 → e4"
            lastMoveText.text = $"{color}: {from} → {to}";
        }
    }
    // 1. 친절한 선생님
    public void SetDifficultyTeacher()
    {
        stockfishManager.SetDifficultyCustom("Teacher");
        Debug.Log("모드 변경: 친절한 선생님");
    }

    // 2. 견고한 수비
    public void SetDifficultyDefender()
    {
        stockfishManager.SetDifficultyCustom("Defender");
        Debug.Log("모드 변경: 견고한 수비");
    }

    // 3. 공격적인 (가끔 실수)
    public void SetDifficultyAttacker()
    {
        stockfishManager.SetDifficultyCustom("Attacker");
        Debug.Log("모드 변경: 공격적인");
    }

    // 4. 밸런스형
    public void SetDifficultyBalance()
    {
        stockfishManager.SetDifficultyCustom("Balance");
        Debug.Log("모드 변경: 밸런스형");
    }
}