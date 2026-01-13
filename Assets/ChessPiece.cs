using UnityEngine;

public enum PieceType { King, Queen, Rook, Bishop, Knight, Pawn }
public enum PieceColor { White, Black }

public class ChessPiece : MonoBehaviour
{
    public PieceType type;
    public PieceColor color;
    public Vector2Int pos; // 현재 배열상의 위치 (x, y)
}