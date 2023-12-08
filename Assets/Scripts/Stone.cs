using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum State
{
    None,
    Black,
    White,
    CanMove,
    Select,
    Capture,
    Choose
}
public class Stone : MonoBehaviour
{
    public SpriteRenderer objRenderer;
    public SpriteRenderer outlineRenderer;

    public State currentSate;

    public Color colorNone;
    public Color colorWhite;
    public Color colorBlack;
    public Color colorSelect;
    public Color colorCanMove;
    public Color colorCapture;
    public Color colorChoose;

    [Header("Coordinate")]
    public int x;
    public int y;

    public bool isFree;

    public List<Vector2> historys = new List<Vector2>();
    public List<Vector2> moves = new List<Vector2>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetCoordinate(int _x,int _y)
    {
        this.x = _x;
        this.y = _y;
    }

    public void SetState(State _state)
    {
        currentSate = _state;

        switch (_state)
        {
            case State.White:
                {
                    objRenderer.color = colorWhite;
                    outlineRenderer.color = colorNone;
                    break;
                }
            case State.Black:
                {
                    objRenderer.color = colorBlack;
                    outlineRenderer.color = colorNone;
                    break;
                }
            case State.Select:
                {
                    outlineRenderer.color = colorSelect;
                    break;
                }
            case State.Capture:
                {
                    outlineRenderer.color = colorCapture;
                    break;
                }
            case State.CanMove:
                {
                    objRenderer.color = colorCanMove;
                    break;
                }
            case State.None:
                {
                    objRenderer.color = colorNone;
                    outlineRenderer.color = colorNone;
                    break;
                }
            case State.Choose:
                {
                    outlineRenderer.color = colorChoose;
                    break;
                }
            default:
                break;
        }
    }

    public void SetFree(bool _free)
    {
        this.isFree = _free;

        if (_free)
        {       
            outlineRenderer.color = colorSelect;
        }
        else
        {
            outlineRenderer.color = colorNone;
        }
    }

    private void OnMouseDown()
    {
        if(GameManager.Instance.colorTurn == currentSate && isFree)
        {
            OnSelected();
        }

        if (currentSate == State.CanMove)
        {
            OnMove();
        }

        if (currentSate == State.Choose)
        {
            GameManager.Instance.ChooseApproachOrWithdrawn(this);
        }
    }

    public void OnSelected()
    {
        GameManager.Instance.SelectStone(this);
    }

    public void OnMove()
    {
        GameManager.Instance.MakeMove(this);
    }
}
