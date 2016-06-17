using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grid : MonoBehaviour
{
    public enum PieceType
    {
        EMPTY,
        NORMAL,
        OBSTACLE,
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

    private bool inverse = false;

    private GamePiece pressedPiece;
    private GamePiece enteredPiece;


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

        Destroy(pieces[4, 4].gameObject);
        SpawnNewPiece(4, 4, PieceType.OBSTACLE);

        StartCoroutine(Fill());
    }

    //Calls FillStep() until the board is filled
    public IEnumerator Fill()
    {
        while (FillStep())
        {
            inverse = !inverse;
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
            for (int loopX = 0; loopX < numberOfRows; loopX++)
            {
                int x = loopX;

                if (inverse)
                {
                    x = numberOfRows - 1 - loopX;
                }

                GamePiece piece = pieces[x, y];

                if (piece.IsMovable())
                {
                    GamePiece pieceBelow = pieces[x, y + 1];

                    if (pieceBelow.Type == PieceType.EMPTY)
                    {
                        Destroy(pieceBelow.gameObject);
                        piece.MovableComponent.Move(x, y + 1, fillTime);
                        pieces[x, y + 1] = piece;
                        SpawnNewPiece(x, y, PieceType.EMPTY);
                        movedPiece = true;
                    }
                    else
                    {
                        for (int diag = -1; diag <= 1; diag++)
                        {
                            if (diag != 0)
                            {
                                int diagX = x + diag;

                                if (inverse)
                                {
                                    diagX = x - diag;
                                }

                                if (diagX >= 0 && diagX < numberOfRows)
                                {
                                    GamePiece diagonalPiece = pieces[diagX, y + 1];

                                    if (diagonalPiece.Type == PieceType.EMPTY)
                                    {
                                        bool hasPieceAbove = true;

                                        for (int aboveY = y; aboveY >= 0; aboveY--)
                                        {
                                            GamePiece pieceAbove = pieces[diagX, aboveY];

                                            if (pieceAbove.IsMovable())
                                            {
                                                break;
                                            }
                                            else if (!pieceAbove.IsMovable() && pieceAbove.Type != PieceType.EMPTY)
                                            {
                                                hasPieceAbove = false;
                                                break;
                                            }
                                        }

                                        if (!hasPieceAbove)
                                        {
                                            Destroy(diagonalPiece.gameObject);
                                            piece.MovableComponent.Move(diagX, y + 1, fillTime);
                                            pieces[diagX, y + 1] = piece;
                                            SpawnNewPiece(x, y, PieceType.EMPTY);
                                            movedPiece = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
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
                Destroy(pieceBelow.gameObject);
                GameObject newPiece =
                    (GameObject)
                        Instantiate(piecePrefabDictionary[PieceType.NORMAL], 
                        GetWorldPositin(x, -1, -1),
                            Quaternion.identity);
                newPiece.transform.parent = transform;

                pieces[x, 0] = newPiece.GetComponent<GamePiece>();
                pieces[x, 0].Init(x, -1, this, PieceType.NORMAL);
                pieces[x, 0].MovableComponent.Move(x, 0, fillTime);
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

    public bool IsAjacent(GamePiece piece1, GamePiece piece2)
    {
        // check if two pieces are next to each other(in the same row)
        // and their Y coords are one space of each other  
        // or are next to each other by column and their X coords are one space of each other
        return (piece1.X == piece2.X && (int) Mathf.Abs(piece1.Y - piece2.Y) == 1)
            || (piece1.Y == piece2.Y && (int) Mathf.Abs(piece1.X - piece2.X) == 1);
    }

    public void SwapPieces(GamePiece piece1, GamePiece piece2)
    {
        if (piece1.IsMovable() && piece2.IsMovable())
        {
            pieces[piece1.X, piece1.Y] = piece2;
            pieces[piece2.X, piece2.Y] = piece1;

            int piece1X = piece1.X;
            int piece1Y = piece1.Y;

            piece1.MovableComponent.Move(piece2.X, piece2.Y, fillTime);
            piece2.MovableComponent.Move(piece1X, piece1Y, fillTime);
        }
    }

    public void PressPiece(GamePiece piece)
    {
        pressedPiece = piece;
    }

    public void EnterPiece(GamePiece piece)
    {
        enteredPiece = piece;
    }

    public void ReleasePiece()
    {
        if (IsAjacent(pressedPiece, enteredPiece))
        {
            SwapPieces(pressedPiece, enteredPiece);
        }
    }
}
