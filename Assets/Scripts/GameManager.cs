using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject pauseUI;
    public AudioSource audioSource;

    int[,] map = new int[,] {
    { 1,1,1,1,1,1,1,1,1},
    { 1,1,1,1,1,1,1,1,1},
    { 1,0,1,0,2,1,0,1,0},
    { 0,0,0,0,0,0,0,0,0},
    { 0,0,0,0,0,0,0,0,0}
    //{2,2,2,2,2,2,2,2,2 },
    //{2,2,2,2,2,2,2,2,2 },
    //{2,2,2,2,2,2,2,2,1 },
    //{2,2,2,2,0,2,0,2,2 },
    //{2,2,2,2,2,2,2,2,2 },
    };

    int[,] currentMap = new int[5, 9];

    public Stone[,] BOARD = new Stone[5, 9];

    [SerializeField]
    private List<Stone> stones = new List<Stone>();
    public List<Stone> freeStones = new List<Stone>();
    public List<Stone> approachStone = new List<Stone>();
    public List<Stone> withdrawStone = new List<Stone>();

    public Stone currentStone;
    public GameObject stone;
    public TimeCountDown timeCountDown;


    public bool paika;
    public bool EndGame = false;
    public bool isAI = false;

    public List<Vector2> direction_advance = new List<Vector2>() { new Vector2(0, 1), new Vector2(1, 0), new Vector2(0, -1), new Vector2(-1, 0), new Vector2(1, 1), new Vector2(1, -1), new Vector2(-1, 1), new Vector2(-1, -1) };
    public List<Vector2> direction_basic = new List<Vector2>() { new Vector2(0, 1), new Vector2(1, 0), new Vector2(0, -1), new Vector2(-1, 0) };

    public AudioClip audioClip;

    public State colorTurn = State.Black;
    public State colorAI;

    private void Awake()
    {
        if (GameManager.Instance == null)
        {
            GameManager.Instance = this;
        }

        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        currentMap = (int[,])map.Clone();
        timeCountDown = GetComponent<TimeCountDown>();

        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                if (currentMap[i, j] == 1)
                {
                    var blackStone = Instantiate(stone, new Vector3(j * 1.5f, -i * 1.5f), Quaternion.identity, transform) as GameObject;
                    blackStone.GetComponent<Stone>().SetState(State.Black);
                    blackStone.GetComponent<Stone>().SetCoordinate(i, j);
                    BOARD[i, j] = blackStone.GetComponent<Stone>();
                    stones.Add(blackStone.GetComponent<Stone>());
                }
                else if (currentMap[i, j] == 0)
                {
                    var whiteStone = Instantiate(stone, new Vector3(j * 1.5f, -i * 1.5f), Quaternion.identity, transform) as GameObject;
                    whiteStone.GetComponent<Stone>().SetState(State.White);
                    whiteStone.GetComponent<Stone>().SetCoordinate(i, j);
                    BOARD[i, j] = whiteStone.GetComponent<Stone>();
                    stones.Add(whiteStone.GetComponent<Stone>());
                }
                else
                {
                    var noneStone = Instantiate(stone, new Vector3(j * 1.5f, -i * 1.5f), Quaternion.identity, transform) as GameObject;
                    noneStone.GetComponent<Stone>().SetState(State.None);
                    noneStone.GetComponent<Stone>().SetCoordinate(i, j);
                    BOARD[i, j] = noneStone.GetComponent<Stone>();
                    stones.Add(noneStone.GetComponent<Stone>());
                }
            }
        }

        if (PlayerPrefs.GetInt("GameMode") == 0)
            isAI = true;
        else
            isAI = false;

        if (PlayerPrefs.GetInt("ColorMode") == 0)
            colorAI = State.Black;
        else
            colorAI = State.White;

        CreateMap();
    }

    private void Update()
    {
        if (timeCountDown.endTime)
        {
            EndGame = true;
            if (colorTurn == State.Black)
            {
                GameEnd(State.White);
            }
            else
            {
                GameEnd(State.Black);
            }  
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            Log();
        }
    }

    public void SwitchTurn()
    {
        if (EndGame)
            return;
        else
        {
            timeCountDown.ResetTime();
        }

        currentStone = null;
        freeStones.Clear();

        if (colorTurn == State.White)
        {
            colorTurn = State.Black;
        }
        else
        {
            colorTurn = State.White;
        }

        ClearAllStone();
        if(isAI && colorTurn== colorAI)
        {
            Node kk=AI2Fanorona.Instance.AIMove(colorTurn, currentMap);
            AImove(BOARD[(int)kk.position.x, (int)kk.position.y], kk.stepsHistory);
        }
        else
        {
            FreeStone(colorTurn, currentMap);
            foreach (Stone stone in freeStones)
            {
                stone.SetFree(true);
            }
        }

        CheckWin();
    }

    public void SelectStone(Stone _stone)
    {
        if (EndGame)
            return;
        currentStone = _stone;
        currentStone.SetState(State.Select);
        currentStone.historys.Clear();
        currentStone.historys.Add(new Vector2(currentStone.x, currentStone.y));


        foreach (Stone stone in freeStones)
        {
            if (stone != currentStone)
            {
                stone.SetFree(false);
            }
        }

        foreach(Vector2 i in currentStone.moves)
        {
            BOARD[(int)i.x+_stone.x, (int)i.y+_stone.y].SetState(State.CanMove);
        }
    }

    public void MakeMove(Stone _stoneFree)
    {
        if (EndGame)
            return;
        else
        {
            timeCountDown.ResetTime();
        }

        audioSource.PlayOneShot(audioClip);

        foreach (Vector2 i in currentStone.moves)
        {
            if (_stoneFree != BOARD[(int)i.x+currentStone.x, (int)i.y+currentStone.y])
                BOARD[(int)i.x+currentStone.x, (int)i.y+currentStone.y].SetState(State.None);
        }

        Vector2 direction = new Vector2(_stoneFree.x - currentStone.x, _stoneFree.y - currentStone.y);

        //paika
        if(Approach(colorTurn, new Vector2(currentStone.x, currentStone.y), currentMap,direction) == 0 && Withdraw(colorTurn, new Vector2(currentStone.x, currentStone.y), currentMap, direction) == 0)
        {
            _stoneFree.SetState(colorTurn);
            currentStone.SetState(State.None);
            currentMap[_stoneFree.x, _stoneFree.y] = colorTurn == State.Black ? 1 : 0;
            currentMap[currentStone.x, currentStone.y] = 2;
            SwitchTurn();
            return;
        }

        //appdrawn and withdrawn
        if (Approach(colorTurn, new Vector2(currentStone.x, currentStone.y), currentMap, direction) > 0 && Withdraw(colorTurn, new Vector2(currentStone.x, currentStone.y), currentMap, direction) > 0)
        {
            approachStone[0].SetState(State.Choose);
            withdrawStone[0].SetState(State.Choose);

            currentStone.historys.Add(new Vector2(_stoneFree.x, _stoneFree.y));
            _stoneFree.historys = new List<Vector2>(currentStone.historys);

            _stoneFree.SetState(colorTurn);
            currentStone.SetState(State.None);

            currentMap[_stoneFree.x, _stoneFree.y] = colorTurn == State.Black ? 1 : 0;
            currentMap[currentStone.x, currentStone.y] = 2;

            currentStone = _stoneFree;
            currentStone.SetState(State.Select);
           
            return;
        }

        //appdrawn
        if (Approach(colorTurn, new Vector2(currentStone.x, currentStone.y), currentMap, direction) > 0)
        {
            _stoneFree.SetState(colorTurn);
            currentStone.SetState(State.None);
            currentMap[_stoneFree.x, _stoneFree.y] = colorTurn == State.Black ? 1 : 0;
            currentMap[currentStone.x, currentStone.y] = 2;

            foreach (Stone stone in approachStone)
            {
                stone.SetState(State.None);
                currentMap[stone.x, stone.y] = 2;
            }
        }
        
        //withdraw
        if(Withdraw(colorTurn, new Vector2(currentStone.x,currentStone.y), currentMap, direction) > 0)
        {
            _stoneFree.SetState(colorTurn);
            currentStone.SetState(State.None);
            currentMap[_stoneFree.x, _stoneFree.y] = colorTurn == State.Black ? 1 : 0;
            currentMap[currentStone.x, currentStone.y] = 2;

            foreach (Stone stone in withdrawStone)
            {
                stone.SetState(State.None);
                currentMap[stone.x, stone.y] = 2;
            }
        }

        currentStone.historys.Add(new Vector2(_stoneFree.x, _stoneFree.y));
        _stoneFree.historys = new List<Vector2>(currentStone.historys);

        if (CanMoveCaptured(colorTurn, new Vector2(_stoneFree.x,_stoneFree.y), currentMap))
        {
            currentStone = _stoneFree;
            currentStone.SetState(State.Select);

            foreach (Vector2 i in currentStone.moves)
            {
                BOARD[(int)i.x + currentStone.x, (int)i.y + currentStone.y].SetState(State.CanMove);
            }
        }
        else
        {
            _stoneFree.SetState(colorTurn);
            currentStone.SetState(State.None);
            currentMap[_stoneFree.x, _stoneFree.y] = colorTurn == State.Black ? 1 : 0;
            currentMap[currentStone.x, currentStone.y] = 2;
            SwitchTurn();
            return;
        }

    }

    public void ChooseApproachOrWithdrawn(Stone _stone)
    {
        if (approachStone.Contains(_stone))
        {
            withdrawStone[0].SetState(State.None);

            if (colorTurn == State.Black)
            {
                withdrawStone[0].SetState(State.White);
            }
            else
            {
                withdrawStone[0].SetState(State.Black);
            }

            ApproachCaptured();
        }
        else
        {
            approachStone[0].SetState(State.None);

            if (colorTurn == State.Black)
            {
                approachStone[0].SetState(State.White);
            }
            else
            {
                approachStone[0].SetState(State.Black);
            }

            WithDrawCaptured();
        }

        if (CanMoveCaptured(colorTurn, new Vector2(currentStone.x,currentStone.y), currentMap))
        {
            foreach (Vector2 i in currentStone.moves)
            {
                BOARD[(int)i.x + currentStone.x, (int)i.y + currentStone.y].SetState(State.CanMove);
            }
        }
        else
        {
            currentStone.SetState(State.None);
            currentStone.SetState(colorTurn);
            SwitchTurn();
            return;
        }
    }

    public void FreeStone(State _colorTurn, int[,] board)
    {
        freeStones.Clear();
        int color = _colorTurn == State.Black ? 1 : 0;

        #region capturedMove

        for(int i = 0; i < 5; i++)
        {
            for(int j = 0; j < 9; j++)
            {
                BOARD[i, j].moves.Clear();
                
                if (board[i,j]==color && CanMoveCaptured(_colorTurn, new Vector2(i,j),board))
                {
                    freeStones.Add(BOARD[i,j]);
                }
            }
        }
        #endregion

        if (freeStones.Count > 0)
            return;

        #region paikaMove

        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                BOARD[i, j].moves.Clear();
                if (board[i, j] == color && CanMovePaika(new Vector2(i, j), board))
                {
                    freeStones.Add(BOARD[i, j]);
                }
            }
        }
        #endregion
    }

    public bool CanMoveCaptured(State _colorTurn,Vector2 position, int[,] board)
    {
        bool canMove = false;

        if (position.x % 2 != position.y % 2)
        {
            foreach (Vector2 i in direction_basic)
            {
                if (incurrentMap(position, i) && !BOARD[(int)position.x, (int)position.y].historys.Contains(new Vector2((int)position.x + (int)i.x, (int)position.y + (int)i.y)))
                {
                    if (board[(int)position.x + (int)i.x, (int)position.y + (int)i.y] == 2)
                    {
                        if ((Approach(_colorTurn, position, board, i) > 0 || Withdraw(_colorTurn,position, board, i) > 0))
                        {
                            BOARD[(int)position.x, (int)position.y].moves.Add(i);
                            canMove = true;
                        }
                    }
                }
            }

            return canMove;
        }
        else
        {
            foreach (Vector2 i in direction_advance)
            {
                if (incurrentMap(position, i) && !BOARD[(int)position.x, (int)position.y].historys.Contains(new Vector2((int)position.x + (int)i.x, (int)position.y + (int)i.y)))
                {
                    if (board[(int)position.x + (int)i.x, (int)position.y + (int)i.y] == 2)
                    {
                        if ((Approach(_colorTurn, position, board, i) > 0 || Withdraw(_colorTurn, position, board, i) > 0))
                        {
                            BOARD[(int)position.x, (int)position.y].moves.Add(i);
                            canMove = true;
                        }
                    }
                }
            }

            return canMove;
        }
    }

    public void ApproachCaptured()
    {
        foreach (Stone stone in approachStone)
        {
            stone.SetState(State.None);
            currentMap[stone.x, stone.y] = 2;
        }
    }

    public void WithDrawCaptured()
    {
        foreach (Stone stone in withdrawStone)
        {
            stone.SetState(State.None);
            currentMap[stone.x, stone.y] = 2;
        }
    }

    public int Approach(State _colorTurn,Vector2 position, int[,] board, Vector2 direction)
    {
        bool isEnd = false;
        int stonesCaptured = 0;
        int t = 2;
        approachStone.Clear();
        while (!isEnd)
        {
            if (incurrentMap(position, direction * t))
            {
                int color = _colorTurn == State.Black ? 0 : 1;
                if (board[(int)position.x + (int)direction.x * t, (int)position.y + (int)direction.y * t] == color)
                {
                    stonesCaptured += 1;
                    approachStone.Add(BOARD[(int)position.x + (int)direction.x * t, (int)position.y + (int)direction.y * t]);
                }
                else
                {
                    isEnd = true;
                }
            }
            else
            {
                isEnd = true;
            }

            t += 1;
        }

        return stonesCaptured;
    }

    public int Withdraw(State _colorTurn ,Vector2 position, int[,] board, Vector2 direction)
    {
        bool isEnd = false;
        int stonesCaptured = 0;
        int t = -1;
        withdrawStone.Clear();
        while (!isEnd)
        {
            if (incurrentMap(position, direction*t))
            {
                int color = _colorTurn == State.Black ? 0 : 1;
                if (board[(int)position.x + (int)direction.x * t, (int)position.y + (int)direction.y * t] == color)
                {
                    stonesCaptured += 1;
                    withdrawStone.Add(BOARD[(int)position.x + (int)direction.x * t, (int)position.y + (int)direction.y * t]);
                }
                else
                {
                    isEnd = true;
                }
            }
            else
            {
                isEnd = true;
            }

            t -= 1;
        }

        return stonesCaptured;
    }

    public bool CanMovePaika(Vector2 position, int[,] board)
    {
        bool canMove = false;
        if (position.x % 2 != position.y % 2)
        {
            foreach (Vector2 i in direction_basic)
            {
                if (incurrentMap(position, i) && !BOARD[(int)position.x, (int)position.y].historys.Contains(new Vector2((int)position.x + (int)i.x, (int)position.y + (int)i.y)))
                {
                    if (board[(int)position.x + (int)i.x, (int)position.y + (int)i.y] == 2 )
                    {
                        BOARD[(int)position.x, (int)position.y].moves.Add(i);
                        canMove = true;
                    }
                }

            }
            return canMove;
        }
        else
        {
            foreach (Vector2 i in direction_advance)
            {
                if (incurrentMap(position,i) && !BOARD[(int)position.x, (int)position.y].historys.Contains(new Vector2((int)position.x + (int)i.x, (int)position.y + (int)i.y)))
                {
                    if (board[(int)position.x + (int)i.x, (int)position.y + (int)i.y] == 2)
                    {
                        BOARD[(int)position.x, (int)position.y].moves.Add(i);
                        canMove = true;
                    }
                }
            }
            return canMove;
        }
    }

    public bool incurrentMap(Vector2 position,Vector2 i)
    {
        if ((position.x + (int)i.x) < 5 && (position.x + (int)i.x) >= 0 && (position.y + (int)i.y) < 9 && (position.y + (int)i.y) >= 0)
        {
            return true;
        }
        else
            return false;
    }

    public void AImove(Stone selectStone,List<Step> stepList)
    {

        selectStone.SetState(State.Select);
        currentStone = selectStone;

        StartCoroutine(Move(stepList));
    }

    IEnumerator Move(List<Step> steps)
    {
        while (steps.Count > 0)
        {
            yield return new WaitForSeconds(1f);

            if (steps[0].stepAction == Action.AppRoach)
            {
                Approach(colorTurn, new Vector2(currentStone.x, currentStone.y), currentMap, steps[0].direction);
                ApproachCaptured();
            }
            else
            {
                Withdraw(colorTurn, new Vector2(currentStone.x, currentStone.y), currentMap, steps[0].direction);
                WithDrawCaptured();
            }

            currentMap[currentStone.x, currentStone.y] = 2;
            currentMap[currentStone.x + (int)(steps[0].direction.x), currentStone.y + (int)(steps[0].direction.y)] = colorTurn == State.Black ? 1 : 0;
            currentStone.SetState(State.None);
            currentStone = BOARD[currentStone.x + (int)steps[0].direction.x, currentStone.y + (int)steps[0].direction.y];
            currentStone.SetState(colorTurn);
            currentStone.SetState(State.Select);
            steps.RemoveAt(0);
            audioSource.PlayOneShot(audioClip);
        }
        currentStone.SetState(State.None);
        currentStone.SetState(colorTurn);
        SwitchTurn();
    }

    public void CreateMap()
    {
        currentMap = (int[,])map.Clone();
        ClearAllStone();
        EndGame = false;

        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                if (currentMap[i, j] == 1)
                {
                    BOARD[i, j].SetState(State.Black);
                }
                else if (currentMap[i, j] == 0)
                {
                    BOARD[i, j].SetState(State.White);
                }
                else
                {
                    BOARD[i, j].SetState(State.None);
                }
            }
        }

        colorTurn = State.White;

        if (isAI && colorTurn == colorAI)
        {
            Node kk = AI2Fanorona.Instance.AIMove(colorTurn, currentMap);
            AImove(BOARD[(int)kk.position.x, (int)kk.position.y], kk.stepsHistory);
        }
        else
        {
            FreeStone(colorTurn, currentMap);

            foreach (Stone stone in freeStones)
            {
                stone.SetFree(true);
            }
        }

        timeCountDown.ResetTime();
    }

    public void CheckWin()
    {
        bool blackwin = true;
        bool whitewin = true;

        for(int i = 0; i < 5; i++)
        {
            for(int j = 0; j < 9; j++)
            {
                if (currentMap[i, j] == 1)
                    whitewin = false;
                if (currentMap[i, j] == 0)
                    blackwin = false;
            }
        }

        if (blackwin)
        {
            EndGame = true;
            GameEnd(State.Black);
        }
        else if (whitewin)
        {
            EndGame = true;
            GameEnd(State.White);
        }
    }

    public void GameEnd(State colorWin)
    {
        pauseUI.SetActive(true);

        if (colorWin == State.White)
        {
            Debug.Log("white win");
        }
        else
        {
            Debug.Log("black win");
        }
    }

    public void ClearAllStone()
    {
        for(int i = 0; i < 5; i++)
        {
            for(int j = 0; j < 9; j++)
            {
                BOARD[i, j].moves.Clear();
                BOARD[i, j].historys.Clear();
            }
        }
    }

    public void Log()
    {
        for(int i = 0; i < 5; i++)
        {

            Debug.Log(currentMap[i, 0] + " " + currentMap[i, 1] + " " + currentMap[i, 2] + " " + currentMap[i, 3] + " " + currentMap[i, 4] + " " + currentMap[i, 5] + " " + currentMap[i, 6] + " " + currentMap[i, 7] + " " + currentMap[i, 8]);

        }
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
