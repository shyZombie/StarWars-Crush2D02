using UnityEngine;
using System.Collections;

public class MovablePiece : MonoBehaviour
{
    private GamePiece piece;

    void Awake()
    {
        piece = GetComponent<GamePiece>();
    }

    public void Move(int newX,int newY)
    {
        piece.X = newX;
        piece.Y = newY;

        piece.transform.localPosition = piece.GridRef.GetWorldPositin(newX, newY, -0.1f);
    }
}
