using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class UIManager : MonoBehaviour
{
    public StockfishManager stockfishManager;

    [Header("화면 패널들")]
    public GameObject titlePanel;
    public GameObject botSelectPanel;
    public GameObject playPanel;

    [Header("UI 요소")]
    public TextMeshProUGUI lastMoveText;
    public ChatManager chatManager;

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
    public GameObject evalPrefab; // 1단계에서 만든 프리팹 연결


    // [추가] 이 함수가 점수를 받아서 판정하고 화면에 띄우는 것까지 다 합니다.
    public void ProcessMoveEvaluation(Vector3 position, float scoreChange)
    {
        // 1. 점수에 따라 등급 결정 (Brilliant, Best Move, Blunder 등)
        string rank = DetermineEvaluation(scoreChange);

        // 2. 체스판 위에 평가 아이콘/텍스트 띄우기
        ShowMoveEvaluation(position, rank);

        // 3. 봇 대사 출력 (조건문 대신 rank 활용)
        if (chatManager != null)
        {
            string botMessage = GetBotMessage(rank); // 등급에 맞는 대사 가져오기
            chatManager.AddMessage(botMessage, false);      // 채팅창에 추가
        }
    }

    // 등급별 대사 목록 함수
    private string GetBotMessage(string rank)
    {
        switch (rank)
        {
            case "Brilliant": return "이건 예상 못 했는데... 제법이군.";
            case "Best Move": return "정석대로군. 지루해.";
            case "Excellent": return "나쁘지 않은 수야.";
            case "Good": return "평범하네.";
            case "Inaccuracy": return "흐름을 못 읽는군. 과연 그럴까?";
            case "Blunder": return "방금 그게 최선인가? 실망이야.";
            default: return "다음은 어디지?";
        }
    }

    // [추가] 점수 차이에 따라 등급을 나누는 로직
    private string DetermineEvaluation(float scoreChange)
    {
        if (scoreChange > 150) return "Brilliant";   // 아주 큰 반전이나 묘수
        if (scoreChange >= 0) return "Best Move";    // 최선의 수
        if (scoreChange > -30) return "Excellent";   // 아주 좋은 수
        if (scoreChange > -70) return "Good";        // 무난한 수
        if (scoreChange > -150) return "Inaccuracy"; // 부정확한 수
        return "Blunder";                            // 치명적인 실수 (점수 1.5점 이상 손해)
    }
    public void ShowMoveEvaluation(Vector3 position, string evaluation)
    {
        // 평가 문구 생성
        GameObject go = Instantiate(evalPrefab, position + new Vector3(0, 0.5f, -1), Quaternion.identity);
        EvaluationText ev = go.GetComponent<EvaluationText>();

        // 평가 종류에 따른 색상 설정
        switch (evaluation)
        {
            case "Brilliant": ev.Setup("!! Brilliant", Color.cyan); break;
            case "Best Move": ev.Setup("Best Move", Color.green); break;
            case "Excellent": ev.Setup("Excellent", new Color(0.5f, 1f, 0f)); break; // 연두색
            case "Good": ev.Setup("Good", Color.white); break;
            case "Inaccuracy": ev.Setup("? Inaccuracy", Color.yellow); break;
            case "Blunder": ev.Setup("?? Blunder", Color.red); break;
            case "Book": ev.Setup("Book", new Color(0.6f, 0.4f, 0.2f)); break; // 갈색
        }
    }

}