using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    public UIManager uiManager;

    // [도우미 함수] 0,1 좌표를 "a2"로 변환
    string PosToString(Vector2Int pos)
    {
        char file = (char)('a' + pos.x);
        char rank = (char)('1' + pos.y);
        return $"{file}{rank}";
    }

    [Header("연결 설정")]
    public BoardGenerator bg;
    public StockfishManager stockfish;
    public BoardVisualizer visualizer;

    [Header("게임 상태")]
    private ChessPiece selectedPiece;
    private bool isWhiteTurn = true;
    private bool isGameOver = false;
    private float lastEvaluation = 0f;

    void Update()
    {
        if (isGameOver) return;

        if (isWhiteTurn && Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            int x = Mathf.FloorToInt(mousePos.x + 0.5f);
            int y = Mathf.FloorToInt(mousePos.y + 0.5f);

            if (x >= 0 && x < 8 && y >= 0 && y < 8)
            {
                HandleInput(x, y);
            }
        }
    }

    void HandleInput(int x, int y)
    {
        if (selectedPiece == null)
        {
            ChessPiece target = bg.board[x, y];
            if (target != null && target.color == PieceColor.White)
            {
                selectedPiece = target;
                selectedPiece.GetComponent<SpriteRenderer>().color = Color.yellow;

                // 이동 가능한 칸 표시
                List<Vector2Int> moves = GetLegalMovesList(selectedPiece);
                visualizer.ShowLegalMoves(moves);
            }
        }
        else
        {
            Vector2Int targetPos = new Vector2Int(x, y);
            if (IsValidMove(selectedPiece, targetPos))
            {
                if (!WouldBeCheckAfterMove(selectedPiece, targetPos))
                {
                    MovePiece(selectedPiece, x, y);
                    EndTurn();
                }
                else
                {
                    Debug.Log("<color=orange>자살 수입니다.</color>");
                    Deselect();
                }
            }
            else
            {
                Deselect();
            }
        }
        // [수정] 기물을 두기 직전에 현재 판의 점수를 먼저 기록합니다.
        if (selectedPiece != null && IsValidMove(selectedPiece, new Vector2Int(x, y)))
        {
            // 스톡피쉬에게 현재 FEN을 주고 점수를 물어봅니다.
            // (주의: StockfishManager에 GetEvaluation 함수가 있다고 가정)
            lastEvaluation = stockfish.GetEvaluation(bg.GetCurrentFEN(isWhiteTurn));

            MovePiece(selectedPiece, x, y);
            EndTurn();
        }
    }

    void EndTurn()
    {
        isWhiteTurn = !isWhiteTurn;
        PieceColor nextColor = isWhiteTurn ? PieceColor.White : PieceColor.Black;

        if (!HasAnyLegalMoves(nextColor))
        {
            DetermineWinner();
        }
        else if (!isWhiteTurn)
        {
            StartCoroutine(AIDelay());
        }
    }

    IEnumerator AIDelay()
    {
        yield return new WaitForSeconds(0.8f);
        if (stockfish == null) yield break;

        string fen = bg.GetCurrentFEN(isWhiteTurn);
        string best = stockfish.GetBestMove(fen);

        if (!string.IsNullOrEmpty(best) && best != "none")
        {
            Vector2Int s = StringToPos(best.Substring(0, 2));
            Vector2Int e = StringToPos(best.Substring(2, 2));

            MovePiece(bg.board[s.x, s.y], e.x, e.y, best.Length > 4 ? best[4] : ' ');
            EndTurn();
        }
        else
        {
            DetermineWinner();
        }
    }

    void MovePiece(ChessPiece piece, int x, int y, char promo = ' ')
    {
        if (isGameOver || piece == null) return;

        // 1. [핵심] 잔상을 표시하기 위해 좌표 정보를 변수에 담아둡니다.
        Vector2Int oldPos = piece.pos;
        Vector2Int newPos = new Vector2Int(x, y); // <-- 이 줄이 없어서 에러가 난 것입니다!

        // 2. 실제 데이터 업데이트 (기존 로직)
        if (bg.board[x, y] != null) Destroy(bg.board[x, y].gameObject);

        bg.board[oldPos.x, oldPos.y] = null;
        bg.board[x, y] = piece;
        piece.pos = newPos;

        // 3. 화면상 위치 이동
        piece.transform.position = new Vector3(x, y, -1);

        if (piece.type == PieceType.Pawn && (y == 7 || y == 0))
        {
            piece.type = PieceType.Queen;
            bg.UpdatePieceSprite(piece);
        }

        // 4. [잔상 표시] 비주얼라이저에게 방금 움직인 정보를 전달
        if (visualizer != null)
        {
            visualizer.ShowLastMove(oldPos, newPos);
        }
        if (uiManager != null)
        {
            // 1. 기물을 두기 전 점수 (LastEvaluation은 이미 저장되어 있다고 가정)
            float beforeMove = lastEvaluation;

            // 2. 기물을 둔 후 점수 측정
            float afterMove = stockfish.GetEvaluation(bg.GetCurrentFEN(isWhiteTurn));

            // 3. [핵심] 현재 누구의 턴이었느냐에 따라 이득/손해 계산을 뒤집어야 합니다.
            float scoreChange;

            if (isWhiteTurn)
            {
                // 백의 턴: 점수가 높아질수록(양수) 좋은 수
                scoreChange = afterMove - beforeMove;
            }
            else
            {
                // 흑의 턴: 점수가 낮아질수록(음수 쪽으로 갈수록) 좋은 수
                // 흑에게 유리해지는 것(음수 증가)을 '양수'로 변환해줘야 UI가 "Best"라고 인식함
                scoreChange = beforeMove - afterMove;
            }

            // 이제 이 scoreChange를 UIManager에 보냅니다.
            uiManager.ProcessMoveEvaluation(new Vector3(x, y, -1), scoreChange);
        }
    }

    void Deselect()
    {
        if (selectedPiece != null)
            selectedPiece.GetComponent<SpriteRenderer>().color = Color.white;

        selectedPiece = null;
        if (visualizer != null) visualizer.ClearIndicators();
    }

    List<Vector2Int> GetLegalMovesList(ChessPiece piece)
    {
        List<Vector2Int> moves = new List<Vector2Int>();
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                Vector2Int targetPos = new Vector2Int(i, j);
                if (IsValidMove(piece, targetPos) && !WouldBeCheckAfterMove(piece, targetPos))
                    moves.Add(targetPos);
            }
        }
        return moves;
    }

    public bool IsValidMove(ChessPiece p, Vector2Int end)
    {
        if (end.x < 0 || end.x > 7 || end.y < 0 || end.y > 7) return false;

        Vector2Int start = p.pos;
        if (start == end) return false;
        if (bg.board[end.x, end.y] != null && bg.board[end.x, end.y].color == p.color) return false;

        int dx = Mathf.Abs(end.x - start.x);
        int dy = Mathf.Abs(end.y - start.y);

        switch (p.type)
        {
            case PieceType.Pawn:
                int dir = (p.color == PieceColor.White) ? 1 : -1;
                if (dx == 0 && (end.y - start.y) == dir) return bg.board[end.x, end.y] == null;
                if (dx == 0 && (end.y - start.y) == dir * 2 && start.y == (p.color == PieceColor.White ? 1 : 6))
                    return bg.board[end.x, end.y] == null && bg.board[start.x, start.y + dir] == null;
                if (dx == 1 && (end.y - start.y) == dir) return bg.board[end.x, end.y] != null;
                return false;
            case PieceType.Rook: return (start.x == end.x || start.y == end.y) && IsPathClear(start, end);
            case PieceType.Bishop: return dx == dy && IsPathClear(start, end);
            case PieceType.Queen: return (dx == dy || start.x == end.x || start.y == end.y) && IsPathClear(start, end);
            case PieceType.Knight: return (dx == 1 && dy == 2) || (dx == 2 && dy == 1);
            case PieceType.King: return dx <= 1 && dy <= 1;
        }
        return false;
    }

    bool IsPathClear(Vector2Int s, Vector2Int e)
    {
        Vector2Int dir = new Vector2Int(System.Math.Sign(e.x - s.x), System.Math.Sign(e.y - s.y));
        Vector2Int curr = s + dir;
        while (curr != e)
        {
            if (bg.board[curr.x, curr.y] != null) return false;
            curr += dir;
        }
        return true;
    }

    bool IsKingInCheck(PieceColor color)
    {
        Vector2Int kingPos = new Vector2Int(-1, -1);
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                ChessPiece p = bg.board[x, y];
                if (p != null && p.type == PieceType.King && p.color == color)
                {
                    kingPos = new Vector2Int(x, y);
                    break;
                }
            }
        }
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                ChessPiece p = bg.board[x, y];
                if (p != null && p.color != color)
                {
                    if (IsValidMove(p, kingPos)) return true;
                }
            }
        }
        return false;
    }

    bool HasAnyLegalMoves(PieceColor color)
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                ChessPiece p = bg.board[x, y];
                if (p != null && p.color == color)
                {
                    for (int tx = 0; tx < 8; tx++)
                    {
                        for (int ty = 0; ty < 8; ty++)
                        {
                            Vector2Int targetPos = new Vector2Int(tx, ty);
                            if (IsValidMove(p, targetPos) && !WouldBeCheckAfterMove(p, targetPos)) return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    bool WouldBeCheckAfterMove(ChessPiece piece, Vector2Int targetPos)
    {
        Vector2Int originalPos = piece.pos;
        ChessPiece targetPiece = bg.board[targetPos.x, targetPos.y];
        bg.board[originalPos.x, originalPos.y] = null;
        bg.board[targetPos.x, targetPos.y] = piece;
        piece.pos = targetPos;
        bool isCheck = IsKingInCheck(piece.color);
        piece.pos = originalPos;
        bg.board[originalPos.x, originalPos.y] = piece;
        bg.board[targetPos.x, targetPos.y] = targetPiece;
        return isCheck;
    }

    void DetermineWinner()
    {
        bool isCheck = IsKingInCheck(isWhiteTurn ? PieceColor.White : PieceColor.Black);
        if (isCheck)
            Debug.Log("<color=red>CHECKMATE!</color>");
        else
            Debug.Log("<color=yellow>STALEMATE!</color>");

        isGameOver = true;
        Invoke("RestartGame", 5f);
    }

    void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    Vector2Int StringToPos(string s) => new Vector2Int(s[0] - 'a', s[1] - '1');
}