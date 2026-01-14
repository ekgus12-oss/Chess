using TMPro;
using UnityEngine;
using UnityEngine.UI; // RawImage 사용을 위해 유지
using DG.Tweening;    // DOTween 사용을 위해 유지

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public StockfishManager stockfishManager;

    [Header("멘탈 시스템")]
    public Image mentalGaugeImage;       // Fill Amount를 조절할 이미지
    public TextMeshProUGUI mentalPercentageText; // 숫자를 표시할 텍스트
    public TextMeshProUGUI turnIndicatorText;

    [Header("화면 패널들")]
    public GameObject titlePanel;
    public GameObject botSelectPanel;
    public GameObject playPanel;

    [Header("배경 전환 설정")]
    public RawImage backgroundPanel;      // RawImage로 설정
    public Texture playerTurnBG;          // Texture 타입 유지
    public Texture botTurnBG;             // Texture 타입 유지
    public float fadeDuration = 0.8f;

    [Header("UI 요소")]
    public TextMeshProUGUI lastMoveText;
    public ChatManager chatManager;
    public GameObject evalPrefab;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }
    // --- [추가] 멘탈 게이지 및 숫자 업데이트 함수 ---
    public void UpdateMentalGauge(float currentMental, float maxMental)
    {
        if (mentalGaugeImage == null) return;

        // 1. 비율 계산 (0.0 ~ 1.0)
        float fillAmount = Mathf.Clamp01(currentMental / maxMental);

        // 2. 이미지 fillAmount 적용
        mentalGaugeImage.fillAmount = fillAmount;

        // 3. 텍스트 업데이트 (예: "85%")
        if (mentalPercentageText != null)
        {
            int percent = Mathf.RoundToInt(fillAmount * 100f);
            mentalPercentageText.text = $"{percent}%";

            // 위험 수치(30% 이하)일 때 숫자 색상을 빨간색으로 변경
            mentalPercentageText.color = (percent <= 30) ? Color.red : Color.white;
        }
    }
    void Start()
    {
        ShowTitle();
        // [수정] RawImage는 .sprite가 아니라 .texture를 사용합니다.
        if (backgroundPanel != null && playerTurnBG != null)
        {
            backgroundPanel.texture = playerTurnBG;
            backgroundPanel.color = Color.white;
        }
    }

    // --- [핵심] 배경 페이드 전환 함수 ---
    public void SwitchTurnBackground(bool isPlayerTurn)
    {
        if (backgroundPanel == null) return;

        // [수정] 변수명을 targetTexture로 통일하여 선언되지 않은 변수 에러 방지
        Texture targetTexture = isPlayerTurn ? playerTurnBG : botTurnBG;

        if (targetTexture == null) return;
        if (backgroundPanel.texture == targetTexture) return;

        // 자연스러운 페이드 전환
        backgroundPanel.DOFade(0.2f, fadeDuration / 2).OnComplete(() => {
            // [수정] .sprite 대신 .texture 사용
            backgroundPanel.texture = targetTexture;
            backgroundPanel.DOFade(1f, fadeDuration / 2);
        });
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

        // 게임 시작 시 플레이어 배경으로 세팅
        SwitchTurnBackground(true);
        UpdateTurnText(true); // 시작 시 플레이어 턴 표시
    }

    public void OnTurnChanged(bool isPlayerTurn)
    {
        SwitchTurnBackground(isPlayerTurn);
    }

    public void OnBackButtonClicked()
    {
        if (playPanel.activeSelf)
            ShowBotSelect();
        else if (botSelectPanel.activeSelf)
            ShowTitle();
    }

    public void UpdateLastMove(string color, string from, string to)
    {
        if (lastMoveText != null)
        {
            lastMoveText.text = $"{color}: {from} → {to}";
        }

        // [수정] 방금 둔 색상이 "White"라면 이제 "Black(봇)" 턴 (false)
        // 방금 둔 색상이 "Black"이라면 이제 "White(플레이어)" 턴 (true)
        bool isNextPlayerTurn = (color == "Black");

        Debug.Log($"Last move by {color}. Next is Player Turn: {isNextPlayerTurn}"); // 로그로 확인
        SwitchTurnBackground(isNextPlayerTurn);
    }

    // --- 난이도 및 평가 로직 (기존 코드 유지) ---
    public void SetDifficultyTeacher() { stockfishManager.SetDifficultyCustom("Teacher"); }
    public void SetDifficultyDefender() { stockfishManager.SetDifficultyCustom("Defender"); }
    public void SetDifficultyAttacker() { stockfishManager.SetDifficultyCustom("Attacker"); }
    public void SetDifficultyBalance() { stockfishManager.SetDifficultyCustom("Balance"); }

    public void ProcessMoveEvaluation(Vector3 position, float scoreChange)
    {
        string rank = DetermineEvaluation(scoreChange);
        ShowMoveEvaluation(position, rank);

        if (chatManager != null)
        {
            string botMessage = GetBotMessage(rank);
            chatManager.AddMessage(botMessage, false);
        }
    }

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

    private string DetermineEvaluation(float scoreChange)
    {
        if (scoreChange > 150) return "Brilliant";
        if (scoreChange >= 0) return "Best Move";
        if (scoreChange > -30) return "Excellent";
        if (scoreChange > -70) return "Good";
        if (scoreChange > -150) return "Inaccuracy";
        return "Blunder";
    }

    public void ShowMoveEvaluation(Vector3 position, string evaluation)
    {
        GameObject go = Instantiate(evalPrefab, position + new Vector3(0, 0.5f, -1), Quaternion.identity);
        EvaluationText ev = go.GetComponent<EvaluationText>();

        switch (evaluation)
        {
            case "Brilliant": ev.Setup("!! Brilliant", Color.cyan); break;
            case "Best Move": ev.Setup("Best Move", Color.green); break;
            case "Excellent": ev.Setup("Excellent", new Color(0.5f, 1f, 0f)); break;
            case "Good": ev.Setup("Good", Color.white); break;
            case "Inaccuracy": ev.Setup("? Inaccuracy", Color.yellow); break;
            case "Blunder": ev.Setup("?? Blunder", Color.red); break;
            case "Book": ev.Setup("Book", new Color(0.6f, 0.4f, 0.2f)); break;
        }
    }
    // --- [추가] 턴 텍스트 업데이트 함수 ---
    public void UpdateTurnText(bool isPlayerTurn)
    {
        if (turnIndicatorText != null)
        {
            if (isPlayerTurn)
            {
                turnIndicatorText.text = "PLAYER 1";
                turnIndicatorText.color = Color.white; // 플레이어 턴일 때 색상
            }
            else
            {
                turnIndicatorText.text = "PLAYER 2";
                turnIndicatorText.color = Color.white;   // 봇 턴일 때 색상 (압박감)
            }
            // 선택사항: DOTween으로 텍스트가 바뀔 때 살짝 커지게 연출
            turnIndicatorText.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.4f);
        }
    }
}