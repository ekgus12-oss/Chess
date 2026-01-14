using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DG.Tweening;

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
    private bool isMentalReducedInThisTurn = false; // 이번 턴 멘탈 감소 여부

    [Header("Engine Connection")]
    public StockfishManager stockfish;

    [Header("Bubble Animation")]
    public float animationDuration = 0.3f;
    public Ease animationEase = Ease.OutBack;

    void Start()
    {
        if (choicePanel != null) choicePanel.SetActive(false);
        UpdateMentalUI();
    }

    // [기능 1] 모든 메시지 생성의 핵심
    public void AddMessage(string message, bool isPlayer)
    {
        GameObject prefab = isPlayer ? playerBubblePrefab : botBubblePrefab;
        if (prefab == null || chatContent == null) return;

        GameObject newBubble = Instantiate(prefab, chatContent);
        
        TextMeshProUGUI textComp = newBubble.GetComponentInChildren<TextMeshProUGUI>();
        if (textComp != null) textComp.text = message;

        newBubble.transform.localScale = Vector3.zero;
        newBubble.transform.DOScale(1f, animationDuration).SetEase(animationEase);

        // 플레이어가 말하면 선택창을 끄고, 봇이 말하면 선택창을 켬
        if (choicePanel != null) choicePanel.SetActive(!isPlayer);

        StartCoroutine(ScrollToBottom());
    }

    // [기능 2] 플레이어 선택지에 따른 반응 대사 로직
    public void OnClickChoice(int type)
    {
        string playerMsg = "";
        string botReply = "";
        float mentalDamage = 0;

        switch (type)
        {
            case 1:
                playerMsg = "방금 그건 일부러 준 거야!";
                botReply = "허세 부리기는... 당황한 거 다 안다.";
                mentalDamage = -20f;
                break;
            case 2:
                playerMsg = "아... 큰일 났다...";
                botReply = "이제야 형세를 파악했나 보군?";
                mentalDamage = 10f; // 안심해서 멘탈 회복
                break;
            case 3:
                playerMsg = "(말없이 판을 노려본다.)";
                botReply = "말이 없군. 집중해봐야 소용없을 거다.";
                mentalDamage = -5f;
                break;
        }

        // 1. 내 메시지 출력
        AddMessage(playerMsg, true);

        // 2. 멘탈 감소 (한 번만)
        if (!isMentalReducedInThisTurn)
        {
            UpdateMental(mentalDamage);
            isMentalReducedInThisTurn = true;
        }

        // 3. 봇의 반응 대사 (약간의 지연 시간 후 출력)
        StartCoroutine(DelayedBotReply(botReply));
    }

    private IEnumerator DelayedBotReply(string reply)
    {
        yield return new WaitForSeconds(0.8f);
        AddMessage(reply, false);
    }

    // [기능 3] 상대 턴 수 평가만 출력 (상대 StockfishManager 등에서 호출)
    public void ShowEvalOnly(string evalScore)
    {
        // 봇의 수 평가는 채팅 대사가 아니므로 멘탈 리셋만 해줌
        isMentalReducedInThisTurn = false; 
        
        // 상대 턴에 채팅 대신 "Eval: +1.2" 형태만 출력
        AddMessage($"[분석] {evalScore}", false);
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