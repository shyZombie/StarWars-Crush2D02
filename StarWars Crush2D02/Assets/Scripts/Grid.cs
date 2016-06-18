using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Grid : MonoBehaviour
{
    public enum PieceType
    {
        EMPTY,
        NORMAL,
        OBSTACLE,
        ROW_CLEAR,
        COLUMN_CLEAR,
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
            }
        }

        Destroy(pieces[4, 4].gameObject);
        SpawnNewPiece(4, 4, PieceType.OBSTACLE);

        StartCoroutine(Fill());
    }

    //Calls FillStep() until the board is filled
    public IEnumerator Fill()
    {
        bool needsRefill = true;

        while (needsRefill)
        {
            yield return new WaitForSeconds(fillTime);

            while (FillStep())
            {
                inverse = !inverse;
                yield return new WaitForSeconds(fillTime);
            }

            needsRefill = ClearAllValidMatches();
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
        return (piece1.X == piece2.X && (int)Mathf.Abs(piece1.Y - piece2.Y) == 1)
            || (piece1.Y == piece2.Y && (int)Mathf.Abs(piece1.X - piece2.X) == 1);
    }

    public void SwapPieces(GamePiece piece1, GamePiece piece2)
    {
        if (piece1.IsMovable() && piece2.IsMovable())
        {
            pieces[piece1.X, piece1.Y] = piece2;
            pieces[piece2.X, piece2.Y] = piece1;

            if (GetMatch(piece1, piece2.X, piece2.Y) != null || GetMatch(piece2, piece1.X, piece1.Y) != null)
            {
                int piece1X = piece1.X;
                int piece1Y = piece1.Y;

                piece1.MovableComponent.Move(piece2.X, piece2.Y, fillTime);
                piece2.MovableComponent.Move(piece1X, piece1Y, fillTime);

                ClearAllValidMatches();

                if (piece1.Type == PieceType.ROW_CLEAR || piece1.Type == PieceType.COLUMN_CLEAR)
                {
                    ClearPiece(piece1.X, piece1.Y);
                }

                if (piece2.Type == PieceType.ROW_CLEAR || piece2.Type == PieceType.COLUMN_CLEAR)
                {
                    ClearPiece(piece2.X, piece2.Y);
                }

                pressedPiece = null;
                enteredPiece = null;

                StartCoroutine(Fill());
            }
            else
            {
                pieces[piece1.X, piece1.Y] = piece1;
                pieces[piece2.X, piece2.Y] = piece2;
            }
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

    public List<GamePiece> GetMatch(GamePiece piece, int newX, int newY)
    {
        if (piece.IsShaped())
        {
            ShapePiece.ShapeType shape = piece.ShapeComponent.Shape;
            var horizontalPieces = new List<GamePiece>();
            var verticalPieces = new List<GamePiece>();
            var matchingPieces = new List<GamePiece>();

            // First check horizontal
            horizontalPieces.Add(piece);

            for (int dir = 0; dir <= 1; dir++)
            {
                for (int xOffset = 1; xOffset < numberOfRows; xOffset++)
                {
                    int x;

                    if (dir == 0) // Left direction
                    {
                        x = newX - xOffset;
                    }
                    else // Right
                    {
                        x = newX + xOffset;
                    }

                    if (x < 0 || x >= numberOfRows)
                    {
                        break;
                    }

                    if (pieces[x, newY].IsShaped() && pieces[x, newY].ShapeComponent.Shape == shape)
                    {
                        horizontalPieces.Add(pieces[x, newY]);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (horizontalPieces.Count >= 3)
            {
                for (int i = 0; i < horizontalPieces.Count; i++)
                {
                    matchingPieces.Add(horizontalPieces[i]);
                }
            }

            //Traverse vertically in case of match with L or T shape
            if (horizontalPieces.Count >= 3)
            {
                for (int i = 0; i < horizontalPieces.Count; i++)
                {
                    for (int dir = 0; dir <= 1; dir++)
                    {
                        for (int yOffset = 0; yOffset < numberOfColumns; yOffset++)
                        {
                            int y;

                            if (dir == 0) // Up
                            {
                                y = newY - yOffset;
                            }
                            else // Down
                            {
                                y = newY + yOffset;
                            }

                            if (y < 0 || y >= numberOfColumns)
                            {
                                break;
                            }

                            if (pieces[horizontalPieces[i].X, y].IsShaped() 
                                && pieces[horizontalPieces[i].X, y].ShapeComponent.Shape == shape)
                            {
                                verticalPieces.Add(pieces[horizontalPieces[i].X, y]);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    if (verticalPieces.Count < 2)
                    {
                        verticalPieces.Clear();
                    }
                    else
                    {
                        for (int j = 0; j < verticalPieces.Count; j++)
                        {
                            matchingPieces.Add(verticalPieces[j]);
                        }

                        break;
                    }     
                }
            }

            if (matchingPieces.Count >= 3)
            {
                return matchingPieces;
            }

            // If there are no horizontal matches we check vertically
            horizontalPieces.Clear();
            verticalPieces.Clear();
            verticalPieces.Add(piece);

            for (int dir = 0; dir <= 1; dir++)
            {
                for (int yOffset = 1; yOffset < numberOfColumns; yOffset++)
                {
                    int y;

                    if (dir == 0) // Up
                    {
                        y = newY - yOffset;
                    }
                    else // Down
                    {
                        y = newY + yOffset;
                    }

                    if (y < 0 || y >= numberOfColumns)
                    {
                        break;
                    }

                    if (pieces[newX, y].IsShaped() && pieces[newX, y].ShapeComponent.Shape == shape)
                    {
                        verticalPieces.Add(pieces[newX, y]);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (verticalPieces.Count >= 3)
            {
                for (int i = 0; i < verticalPieces.Count; i++)
                {
                    matchingPieces.Add(verticalPieces[i]);
                }
            }

            //Traverse horizontally in case of match with L or T shape
            if (verticalPieces.Count >= 3)
            {
                for (int i = 0; i < verticalPieces.Count; i++)
                {
                    for (int dir = 0; dir <= 1; dir++)
                    {
                        for (int xOffset = 0; xOffset < numberOfColumns; xOffset++)
                        {
                            int x;

                            if (dir == 0) // Left
                            {
                                x = newX - xOffset;
                            }
                            else // Right
                            {
                                x = newX + xOffset;
                            }

                            if (x < 0 || x >= numberOfRows)
                            {
                                break;
                            }

                            if (pieces[x, verticalPieces[i].Y].IsShaped()
                                && pieces[x, verticalPieces[i].Y].ShapeComponent.Shape == shape)
                            {
                                horizontalPieces.Add(pieces[x, verticalPieces[i].Y]);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    if (horizontalPieces.Count < 2)
                    {
                        horizontalPieces.Clear();
                    }
                    else
                    {
                        for (int j = 0; j < horizontalPieces.Count; j++)
                        {
                            matchingPieces.Add(horizontalPieces[j]);
                        }

                        break;
                    }
                }
            }

            if (matchingPieces.Count >= 3)
            {
                return matchingPieces;
            }
        }

        return null;
    }

    public bool ClearAllValidMatches()
    {
        bool needsRefill = false;

        for (int y = 0; y < numberOfColumns; y++)
        {
            for (int x = 0; x < numberOfRows; x++)
            {
                if (pieces[x, y].IsClearable())
                {
                    List<GamePiece> match = GetMatch(pieces[x, y], x, y);

                    if (match != null)
                    {
                        PieceType specialPieceType = PieceType.COUNT;
                        GamePiece randomPiece = match[Random.Range(0, match.Count)];
                        int specialPieceX = randomPiece.X;
                        int specialPieceY = randomPiece.Y;

                        if (match.Count == 4)
                        {
                            if (pressedPiece == null || enteredPiece == null)
                            {
                                specialPieceType = (PieceType)Random.Range((int)PieceType.ROW_CLEAR, (int)PieceType.COLUMN_CLEAR);
                            }
                            else if (pressedPiece.Y == enteredPiece.Y)
                            {
                                specialPieceType = PieceType.ROW_CLEAR;
                            }
                            else
                            {
                                specialPieceType = PieceType.COLUMN_CLEAR;
                            }
                        }

                        for (int i = 0; i < match.Count; i++)
                        {
                            if (ClearPiece(match[i].X, match[i].Y))
                            {
                                needsRefill = true;

                                if (match[i] == pressedPiece || match[i] == enteredPiece)
                                {
                                    specialPieceX = match[i].X;
                                    specialPieceY = match[i].Y;
                                }
                            }
                        }

                        if (specialPieceType != PieceType.COUNT)
                        {
                            Destroy(pieces[specialPieceX, specialPieceY]);
                            GamePiece newPiece =  SpawnNewPiece(specialPieceX, specialPieceY, specialPieceType);

                            if ((specialPieceType == PieceType.ROW_CLEAR || specialPieceType == PieceType.COLUMN_CLEAR)
                                && newPiece.IsShaped() && match[0].IsShaped())
                            {
                                newPiece.ShapeComponent.SetShape(match[0].ShapeComponent.Shape);
                            }
                        }
                    }
                }
            }
        }

        return needsRefill;
    }

    public bool ClearPiece(int x, int y)
    {
        if (pieces[x, y].IsClearable() 
            && !pieces[x, y].ClearableComponent.IsBeingCleared)
        {
            pieces[x, y].ClearableComponent.Clear();
            SpawnNewPiece(x, y, PieceType.EMPTY);

            ClearObstacle(x, y);

            return true;
        }

        return false;
    }

    public void ClearObstacle(int x, int y)
    {
        for (int adjacentX = x - 1; adjacentX <= x + 1; adjacentX++)
        {
            if (adjacentX != x && adjacentX >= 0 && adjacentX < numberOfRows)
            {
                if (pieces[adjacentX, y].Type == PieceType.OBSTACLE && pieces[adjacentX, y].IsClearable())
                {
                    pieces[adjacentX, y].ClearableComponent.Clear();
                    SpawnNewPiece(adjacentX, y, PieceType.EMPTY);
                }
            }
        }

        for (int adjacentY = y - 1; adjacentY <= y + 1; adjacentY++)
        {
            if (adjacentY != y && adjacentY >= 0 && adjacentY < numberOfColumns)
            {
                if (pieces[x, adjacentY].Type == PieceType.OBSTACLE && pieces[x, adjacentY].IsClearable())
                {
                    pieces[x, adjacentY].ClearableComponent.Clear();
                    SpawnNewPiece(x, adjacentY, PieceType.EMPTY);
                }
            }
        }
    }
}
