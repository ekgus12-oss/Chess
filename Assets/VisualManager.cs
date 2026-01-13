using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoardVisualizer : MonoBehaviour
{
    [Header("프리팹 설정")]
    public GameObject indicatorPrefab; // 이동 경로 (반투명 원)
    public GameObject highlightPrefab; // 잔상 효과 (노란색 사각형)

    private List<GameObject> indicators = new List<GameObject>();
    private List<GameObject> highlights = new List<GameObject>();

    // 1. 이동 가능한 칸 표시 (원 생성)
    public void ShowLegalMoves(List<Vector2Int> moves)
    {
        ClearIndicators();

        foreach (Vector2Int pos in moves)
        {
            // 기물(-1)과 바닥(0) 사이인 -0.5f 좌표에 생성
            GameObject obj = Instantiate(indicatorPrefab, new Vector3(pos.x, pos.y, -0.5f), Quaternion.identity);
            indicators.Add(obj);
        }
    }

    // 2. 잔상 표시 (사각형 생성)
    public void ShowLastMove(Vector2Int from, Vector2Int to)
    {
        ClearHighlights();

        CreateHighlight(from);
        CreateHighlight(to);
    }

    private void CreateHighlight(Vector2Int pos)
    {
        // 바닥 타일 바로 위인 -0.1f 좌표에 생성
        GameObject h = Instantiate(highlightPrefab, new Vector3(pos.x, pos.y, -0.1f), Quaternion.identity);
        highlights.Add(h);
    }

    // 3. 시각 효과 제거 함수들
    public void ClearIndicators()
    {
        foreach (var obj in indicators) Destroy(obj);
        indicators.Clear();
    }

    public void ClearHighlights()
    {
        foreach (var h in highlights) Destroy(h);
        highlights.Clear();
    }
} // <--- 클래스를 닫는 이 중괄호가 하나만 있어야 합니다!