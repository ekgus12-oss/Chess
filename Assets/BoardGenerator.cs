using UnityEngine;

public class BoardGenerator : MonoBehaviour
{
    public GameObject tilePrefab;
    public GameObject piecePrefab;
    public Sprite[] whiteSprites; // 킹0, 퀸1, 룩2, 비숍3, 나이트4, 폰5
    public Sprite[] blackSprites;

    public ChessPiece[,] board = new ChessPiece[8, 8];

    void Start()
    {
        GenerateBoard();
        SpawnPieces();
    }

    void GenerateBoard()
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                GameObject tile = Instantiate(tilePrefab, new Vector3(x, y, 0), Quaternion.identity, transform);
                tile.GetComponent<SpriteRenderer>().color = (x + y) % 2 == 0 ? Color.white : Color.gray;
            }
        }
    }

    void SpawnPieces()
    {
        SetupRow(0, PieceColor.White);
        SetupPawns(1, PieceColor.White);
        SetupPawns(6, PieceColor.Black);
        SetupRow(7, PieceColor.Black);
    }

    void SetupRow(int y, PieceColor color)
    {
        Sprite[] s = (color == PieceColor.White) ? whiteSprites : blackSprites;
        PieceType[] types = { PieceType.Rook, PieceType.Knight, PieceType.Bishop, PieceType.Queen, PieceType.King, PieceType.Bishop, PieceType.Knight, PieceType.Rook };
        int[] sIdx = { 2, 4, 3, 1, 0, 3, 4, 2 };

        for (int x = 0; x < 8; x++)
            CreatePiece(x, y, s[sIdx[x]], types[x], color);
    }

    void SetupPawns(int y, PieceColor color)
    {
        Sprite s = (color == PieceColor.White) ? whiteSprites[5] : blackSprites[5];
        for (int x = 0; x < 8; x++)
            CreatePiece(x, y, s, PieceType.Pawn, color);
    }

    void CreatePiece(int x, int y, Sprite sprite, PieceType type, PieceColor color)
    {
        GameObject obj = Instantiate(piecePrefab, new Vector3(x, y, -1), Quaternion.identity);
        ChessPiece cp = obj.GetComponent<ChessPiece>();
        cp.type = type; cp.color = color; cp.pos = new Vector2Int(x, y);
        obj.GetComponent<SpriteRenderer>().sprite = sprite;
        board[x, y] = cp;
    }

    public string GetCurrentFEN(bool isWhiteTurn)
    {
        string fen = "";
        for (int y = 7; y >= 0; y--)
        {
            int empty = 0;
            for (int x = 0; x < 8; x++)
            {
                if (board[x, y] == null) empty++;
                else
                {
                    if (empty > 0) { fen += empty; empty = 0; }
                    fen += GetChar(board[x, y]);
                }
            }
            if (empty > 0) fen += empty;
            if (y > 0) fen += "/";
        }
        fen += isWhiteTurn ? " w " : " b ";
        fen += "KQkq - 0 1";
        return fen;
    }

    // BoardGenerator.cs 내부의 GetChar 함수 (기존 코드 확인 및 수정)
    char GetChar(ChessPiece p)
    {
        char c = ' ';
        switch (p.type)
        {
            case PieceType.Pawn: c = 'p'; break;
            case PieceType.Rook: c = 'r'; break;
            case PieceType.Knight: c = 'n'; break;
            case PieceType.Bishop: c = 'b'; break;
            case PieceType.Queen: c = 'q'; break;
            case PieceType.King: c = 'k'; break;
        }
        return (p.color == PieceColor.White) ? char.ToUpper(c) : c;
    }

    public void UpdatePieceSprite(ChessPiece cp)
    {
        Sprite[] s = (cp.color == PieceColor.White) ? whiteSprites : blackSprites;
        int[] sIdx = { 0, 1, 2, 3, 4, 5 };
        cp.GetComponent<SpriteRenderer>().sprite = s[sIdx[(int)cp.type]];
    }
    // 특정 색상의 왕 위치를 찾고, 상대 기물에 의해 공격받는지 확인
    public bool IsInCheck(PieceColor kingColor)
    {
        Vector2Int kingPos = new Vector2Int(-1, -1);
        // 1. 왕의 위치 찾기
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (board[x, y] != null && board[x, y].type == PieceType.King && board[x, y].color == kingColor)
                {
                    kingPos = new Vector2Int(x, y);
                    break;
                }
            }
        }

        // 2. 모든 상대 기물이 이 위치를 공격할 수 있는지 확인
        // (이 로직은 InputManager의 IsValidMove를 활용하여 호출합니다)
        return false; // 실제 구현은 InputManager에서 보조합니다.
    }
}