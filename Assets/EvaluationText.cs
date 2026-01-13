using UnityEngine;
using TMPro;

public class EvaluationText : MonoBehaviour
{
    public TextMeshProUGUI textElement;

    public void Setup(string message, Color color)
    {
        textElement.text = message;
        textElement.color = color;

        // 1초 뒤에 오브젝트 삭제
        Destroy(gameObject, 1f);
    }

    void Update()
    {
        // 살짝 위로 떠오르는 효과 (선택 사항)
        transform.Translate(Vector3.up * Time.deltaTime * 0.5f);
    }
}