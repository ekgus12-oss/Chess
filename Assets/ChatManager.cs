using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatManager : MonoBehaviour
{
    public Transform chatContent;
    public GameObject botBubblePrefab;    // 왼쪽 말풍선
    public GameObject playerBubblePrefab; // 오른쪽 말풍선 (새로 생성 필요)
    public GameObject choicePanel;        // 버튼들이 담긴 패널
    public ScrollRect scrollRect;

    [Header("Mental System")]
    public float botMental = 100f;        // 봇의 초기 멘탈
    public Image mentalBarFill;           // 상단 멘탈바 Fill 이미지

    void Start()
    {
        choicePanel.SetActive(false);     // 시작할 때는 선택지 숨김
    }

    // 메시지 출력 (isPlayer가 true면 내 말풍선, false면 봇 말풍선)
    public void AddMessage(string message, bool isPlayer)
    {
        GameObject prefab = isPlayer ? playerBubblePrefab : botBubblePrefab;
        GameObject newBubble = Instantiate(prefab, chatContent);
        newBubble.GetComponentInChildren<TextMeshProUGUI>().text = message;

        // 봇이 말하면 선택지 창을 띄우고, 내가 말하면 닫음
        choicePanel.SetActive(!isPlayer);

        StartCoroutine(ScrollToBottom());
    }

    // 버튼에 연결할 함수 (인스펙터의 Button -> OnClick에서 연결)
    public void OnClickChoice(int type)
    {
        if (type == 1) // 도발
        {
            AddMessage("방금 그건 일부러 준 거야!", true);
            UpdateMental(-20f); // 멘탈 깎음
        }
        else if (type == 2) // 당황
        {
            AddMessage("아... 큰일 났다...", true);
            UpdateMental(10f);  // 봇의 기를 살려줌
        }
    }

    void UpdateMental(float amount)
    {
        botMental = Mathf.Clamp(botMental + amount, 0, 100);
        if (mentalBarFill != null)
            mentalBarFill.fillAmount = botMental / 100f;

        //여기서 나중에 봇의 스톡피쉬 난이도를 조절하는 함수를 호출합니다!
    }

    private System.Collections.IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}