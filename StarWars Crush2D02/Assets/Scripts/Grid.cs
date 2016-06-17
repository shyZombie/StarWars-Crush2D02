using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grid : MonoBehaviour
{
    public enum PieceType
    {
        EMPTY,
        NORMAL,
        COUNT
    }

    [System.Serializable]
    public struct PiecePrefab
    {
        public PieceType type;
        public GameObject prefab;
    }

    public int numberOfRows = 8;
    public int numberOfColumns = 8;
    public float fillTime = 0.1f;

    public PiecePrefab[] piecePrefabs;
    public GameObject backgroundPrefab;

    private Dictionary<PieceType, GameObject> piecePrefabDictionary;
    private GamePiece[,] pieces;

    void Start()
    {
        piecePrefabDictionary = new Dictionary<PieceType, GameObject>();
        for (int i = 0; i < piecePrefabs.Length; i++)
        {
            if (!piecePrefabDictionary.ContainsKey(piecePrefabs[i].type))
            {
                piecePrefabDictionary.Add(piecePrefabs[i].type, piecePrefabs[i].prefab);
            }
        }

        for (int x = 0; x < numberOfRows; x++)
        {
            for (int y = 0; y < numberOfColumns; y++)
            {
                GameObject background = (GameObject)Instantiate(
                    backgroundPrefab, 
                    GetWorldPositin(x, y, 0), 
                    Quaternion.identity);
                background.transform.parent = transform; //Making background a child of the grid object
            }
        }

        pieces = new GamePiece[numberOfRows, numberOfColumns];
        for (int x = 0; x < numberOfRows; x++)
        {
            for (int y = 0; y < numberOfColumns; y++)
            {
                SpawnNewPiece(x, y, PieceType.EMPTY);

                /* GameObject newPiece = (GameObject)Instantiate(
                    piecePrefabDictionary[PieceType.NORMAL], 
                    new Vector3(0, 0, -1),
                    Quaternion.identity);
                newPiece.name = "Piece(" + x + "," + y + ")";
                newPiece.transform.parent = transform;

                pieces[x, y] = newPiece.GetComponent<GamePiece>();
                pieces[x, y].Init(x, y, this, PieceType.NORMAL);

                if (pieces[x, y].IsMovable())
                {
                    pieces[x, y].MovableComponent.Move(x, y);
                }

                if (pieces[x, y].IsShaped())
                {
                    pieces[x, y].ShapeComponent.SetShape(
                        (ShapePiece.ShapeType)Random.Range(0, pieces[x, y].ShapeComponent.NumShapes));
                } */
            }
        }

        StartCoroutine(Fill());
    }

    //Calls FillStep() until the board is filled
    public IEnumerator Fill()
    {
        while (FillStep())
        {
            yield return new WaitForSeconds(fillTime);
        }
    }

    // Move each piece step by step
    public bool FillStep()
    {
        // TODO: returns true if any pieces are moved
        bool movedPiece = false;
        for (int y = numberOfColumns - 2; y >= 0; y--)
        {
            for (int x = 0; x < numberOfRows; x++)
            {
                GamePiece piece = pieces[x, y];

                if (piece.IsMovable())
                {
                    GamePiece pieceBelow = pieces[x, y + 1];

                    if (pieceBelow.Type == PieceType.EMPTY)
                    {
                        piece.MovableComponent.Move(x, y + 1);
                        pieces[x, y + 1] = piece;
                        SpawnNewPiece(x, y, PieceType.EMPTY);
                        movedPiece = true;
                    }
                }
            }
        }

        // bottom row check
        for (int x = 0; x < numberOfRows; x++)
        {
            GamePiece pieceBelow = pieces[x, 0];

            if (pieceBelow.Type == PieceType.EMPTY)
            {
                GameObject newPiece =
                    (GameObject)
                        Instantiate(piecePrefabDictionary[PieceType.NORMAL], 
                        GetWorldPositin(x, -1, -1),
                            Quaternion.identity);
                newPiece.transform.parent = transform;

                pieces[x, 0] = newPiece.GetComponent<GamePiece>();
                pieces[x, 0].Init(x, -1, this, PieceType.NORMAL);
                pieces[x, 0].MovableComponent.Move(x, 0);
                pieces[x, 0].ShapeComponent.SetShape(
                    (ShapePiece.ShapeType)Random.Range(0, pieces[x, 0].ShapeComponent.NumShapes));
                movedPiece = true;
            }
        }

        return movedPiece;
    }

    public Vector3 GetWorldPositin(int x, int y, float z)
    {
        return new Vector3(transform.position.x - numberOfRows / 2.0f + x, 
            transform.position.y + numberOfColumns / 2.0f - y - 0.5f, z);
    }

    public GamePiece SpawnNewPiece(int x, int y, PieceType type)
    {
        GameObject newPiece = (GameObject)Instantiate(
            piecePrefabDictionary[type], 
            GetWorldPositin(x, y, -1), 
            Quaternion.identity);
        newPiece.transform.parent = transform;

        pieces[x, y] = newPiece.GetComponent<GamePiece>();
        pieces[x, y].Init(x, y, this, type);

        return pieces[x, y];
    }
}
