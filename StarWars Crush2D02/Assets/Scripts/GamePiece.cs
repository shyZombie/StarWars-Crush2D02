using UnityEngine;
using System.Collections;

public class GamePiece : MonoBehaviour
{
    private int x;
    private int y;

    public int X
    {
        get { return this.x; }
        set
        {
            if (IsMovable())
            {
                this.x = value;
            }
        }
    }

    public int Y
    {
        get { return this.y; }
        set
        {
            if (IsMovable())
            {
                this.y = value;
            }
        }
    }

    private Grid.PieceType type;

    public Grid.PieceType Type
    {
        get { return this.type; }
    }

    private Grid grid;

    public Grid GridRef
    {
        get { return this.grid; }
    }

    private MovablePiece movableComponent;

    public MovablePiece MovableComponent
    {
        get { return movableComponent; }
    }

    private ShapePiece shapeComponent;

    public ShapePiece ShapeComponent
    {
        get { return shapeComponent;}
    }

    void Awake()
    {
        movableComponent = GetComponent<MovablePiece>();
        shapeComponent = GetComponent<ShapePiece>();
    }

    public void Init(int _x, int _y, Grid _grid, Grid.PieceType _type)
    {
        this.x = _x;
        this.y = _y;
        this.grid = _grid;
        this.type = _type;  
    }

    // when mouse enters an element
    void OnMouseEnter()
    {
        grid.EnterPiece(this);
    }

    // when mouse is pressed
    void OnMouseDown()
    {
        grid.PressPiece(this);
    }

    // when mouse is releasing
    void OnMouseUp()
    {
        grid.ReleasePiece();
    }

    public bool IsMovable()
    {
        return movableComponent != null;
    }

    public bool IsShaped()
    {
        return shapeComponent != null;
    }
}
