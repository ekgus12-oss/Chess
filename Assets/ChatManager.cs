using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DG.Tweening; // DOTween 필수

public class ChatManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform chatContent;
    public GameObject botBubblePrefab;
    public GameObject playerBubblePrefab;
    public GameObject choicePanel;
    public ScrollRect scrollRect;

    [Header("Mental System")]
    public float botMental = 100f;
    public Image mentalBarFill;

    [Header("Engine Connection")]
    public StockfishManager stockfish;

    [Header("Bubble Animation")]
    public float animationDuration = 0.3f; // 말풍선 커지는 시간
    public Ease animationEase = Ease.OutBack; // 톡 튀어나오는 효과

    void Start()
    {
        if (choicePanel != null) choicePanel.SetActive(false);
        UpdateMentalUI();
    }

    // 이 함수가 파일 안에 딱 하나만 있어야 합니다!
    public void AddMessage(string message, bool isPlayer)
    {
        GameObject prefab = isPlayer ? playerBubblePrefab : botBubblePrefab;
        if (prefab == null || chatContent == null) return;

        // 생성
        GameObject newBubble = Instantiate(prefab, chatContent);

        // 텍스트 설정
        TextMeshProUGUI textComp = newBubble.GetComponentInChildren<TextMeshProUGUI>();
        if (textComp != null) textComp.text = message;

        // --- 애니메이션 적용 ---
        newBubble.transform.localScale = Vector3.zero; // 크기 0에서 시작
        newBubble.transform.DOScale(1f, animationDuration).SetEase(animationEase);
        // ----------------------

        if (choicePanel != null) choicePanel.SetActive(!isPlayer);

        StartCoroutine(ScrollToBottom());
    }

    public void OnClickChoice(int type)
    {
        switch (type)
        {
            case 1:
                AddMessage("방금 그건 일부러 준 거야!", true);
                UpdateMental(-20f);
                break;
            case 2:
                AddMessage("아... 큰일 났다...", true);
                UpdateMental(10f);
                break;
            case 3:
                AddMessage("(말없이 판을 노려본다.)", true);
                UpdateMental(-5f);
                break;
        }
    }

    public void UpdateMental(float amount)
    {
        botMental = Mathf.Clamp(botMental + amount, 0, 100);
        UpdateMentalUI();
        ApplyMentalToEngine();
    }

    private void UpdateMentalUI()
    {
        if (mentalBarFill != null)
        {
            mentalBarFill.fillAmount = botMental / 100f;
            if (botMental < 30) mentalBarFill.color = Color.red;
            else if (botMental < 60) mentalBarFill.color = Color.yellow;
            else mentalBarFill.color = Color.green;
        }
    }

    private void ApplyMentalToEngine()
    {
        if (stockfish == null) return;
        int newLevel = Mathf.RoundToInt(Mathf.Lerp(3, 20, botMental / 100f));
        stockfish.SetSkillLevel(newLevel);
    }

    private IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 0f;
    }
}