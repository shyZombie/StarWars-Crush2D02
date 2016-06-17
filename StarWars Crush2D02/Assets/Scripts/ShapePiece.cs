using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShapePiece : MonoBehaviour
{
    public enum ShapeType
    {
        DEATH_STAR,
        HAVOK_SQUAD,
        REBEL_ALLIANCE,
        STORMTROOPER,
        XWING,
        BLACK_SUN,
        JEDI_ORDER, // Bonus
        ANY, // ???
        COUNT
    }

    [System.Serializable]
    public struct ShapeSprite
    {
        public ShapeType shape;
        public Sprite sprite;
    }

    public ShapeSprite[] shapeSprites;

    private ShapeType shape;

    public ShapeType Shape
    {
        get { return shape; }
        set { SetShape(value); }
    }

    public int NumShapes
    {
        get { return shapeSprites.Length; }
    }

    private SpriteRenderer sprite;

    private Dictionary<ShapeType, Sprite> shapeSpriteDict;

    void Awake()
    {
        sprite = transform.Find("piece").GetComponent<SpriteRenderer>();

        shapeSpriteDict = new Dictionary<ShapeType, Sprite>();

        for (int i = 0; i < shapeSprites.Length; i++)
        {
            if (!shapeSpriteDict.ContainsKey(shapeSprites[i].shape))
            {
                shapeSpriteDict.Add(shapeSprites[i].shape, shapeSprites[i].sprite);
            }
        }
    }

    public void SetShape(ShapeType newShape)
    {
        shape = newShape;

        if (shapeSpriteDict.ContainsKey(newShape))
        {
            sprite.sprite = shapeSpriteDict[newShape];
        }
    }
}
