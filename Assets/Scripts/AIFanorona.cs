using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.Windows;

public enum Action{
    AppRoach,
    WithDrawn,
    Paika
}

public struct Step{
    public Vector2 direction;
    public Action stepAction;

    public Step(Vector2 _direction,Action _stepAction)
    {
        this.direction = _direction;
        this.stepAction = _stepAction;
    }
}



public class AIFanorona : MonoBehaviour
{
    public static AIFanorona Instance;
    public List<Vector2> direction_advance = new List<Vector2>() { new Vector2(0, 1), new Vector2(1, 0), new Vector2(0, -1), new Vector2(-1, 0), new Vector2(1, 1), new Vector2(1, -1), new Vector2(-1, 1), new Vector2(-1, -1) };
    public List<Vector2> direction_basic = new List<Vector2>() { new Vector2(0, 1), new Vector2(1, 0), new Vector2(0, -1), new Vector2(-1, 0) };

    int[,] afterboard;

    List<Vector2> approachCaptured = new List<Vector2>();
    List<Vector2> withdrawnCaptured = new List<Vector2>();

    List<Step> stepsList;

    public Stone selectStone;
    public List<Step> selectStoneStep;
    public int[,] selectStoneMap;

    public int limit = 3;



    private void Awake()
    {
        AIFanorona.Instance = this;
    }

    private void Start()
    {
    }

    public void AISetup(State colorTurn,int[,] board)
    {
        int nb = 0;
        SelectMinMaxStone(colorTurn,board,ref nb,0,-1000,1000,new Vector2(0,0));
        Debug.Log(selectStone.x + " " + selectStone.y);
        foreach (Step step in selectStoneStep)
        {
            Debug.Log(step.direction + " " + step.stepAction);
        }
    }

    public void SelectMinMaxStone(State colorturn, int[,] board, ref int nb, int depth, int alpha, int beta,Vector2 kk)
    {
        int max = -1000;
        int current_capture_num = 0;
        GameManager.Instance.FreeStone(colorturn, board);
        List<Stone> newFreeStones = new List<Stone>(GameManager.Instance.freeStones);

        int optimal_capture_num;

        if (depth % 2 == 0)
        {//MAX
            optimal_capture_num = -1000;
        }
        else
        {//MIN
            optimal_capture_num = 1000;
        }

        foreach (Stone stone in newFreeStones)
        {
            int color = colorturn == State.Black ? 1 : 0;

            current_capture_num = MaxStoneCaptured(stone, board, color);
            List<Step> currentSteps = new List<Step>(stepsList);
            int[,] newboard = (int[,])afterboard.Clone();

            int later_capture_num = 0;
            int total_capture_num = 0;

            if (depth < limit - 1)
            {
                if (colorturn == State.White)
                {
                    SelectMinMaxStone(State.Black, newboard, ref later_capture_num, depth + 1, alpha, beta,new Vector2(stone.x,stone.y));
                }
                else
                {
                    SelectMinMaxStone(State.White, newboard, ref later_capture_num, depth + 1, alpha, beta, new Vector2(stone.x, stone.y));
                }
            }
            else
                later_capture_num = 0;

            if (depth % 2 == 0)
                total_capture_num = current_capture_num + later_capture_num;
            else
                total_capture_num = -current_capture_num + later_capture_num;

            if (depth == 0 && total_capture_num > max)
            {
                selectStone = stone;
                selectStoneStep = currentSteps;
                max = total_capture_num;
            }

            if (depth % 2 == 0)
            {
                if (current_capture_num != 0 && total_capture_num >= optimal_capture_num)
                { 
                    optimal_capture_num = total_capture_num;
                    alpha = Mathf.Max(alpha, optimal_capture_num);
                    if (beta <= alpha)
                    {
                        break;
                    }
                }
            }
            else
            {
                if (current_capture_num != 0 && total_capture_num <= optimal_capture_num)//non-paika move
                {
                    optimal_capture_num = total_capture_num;
                    beta = Mathf.Min(beta, optimal_capture_num);

                    if (beta <= alpha)
                    {
                        break;
                    }
                }
            }
        }

        nb = optimal_capture_num;
    }

    public int MaxStoneCaptured(Stone stone, int[,] board,int color)
    {
        int max=0;
        List<Vector2> history = new List<Vector2>();
        history.Add(new Vector2(stone.x, stone.y));

        CanMakeCapturingChainMove(new Vector2(stone.x, stone.y), board, ref max, 0, history, new List<Step>(), color);
        return max;
    }

    public void CanMakeCapturingChainMove(Vector2 position,int[,] board, ref int max,int tong,List<Vector2> history,List<Step> steps,int color)
    {
        if ((int)position.x % 2 == (int)position.y % 2)
        {
            foreach (Vector2 i in direction_advance)
            {
                if (incurrentMap(position, i) && !history.Contains(position+i))
                {
                    if (board[(int)position.x + (int)i.x, (int)position.y + (int)i.y] == 2)
                    {

                        if (Approach(position, i, board, color) > 0)
                        {
                            int[,] newboard = (int[,])board.Clone();
                            newboard[(int)position.x, (int)position.y] = 2;
                            newboard[(int)(position.x + i.x), (int)(position.y + i.y)] = color;

                            foreach (Vector2 _i in approachCaptured)
                            {
                                newboard[(int)_i.x, (int)_i.y] = 2;
                            }

                            if (tong + Approach(position, i, board, color) >= max)
                            {
                                max = tong + Approach(position, i, board, color); ;

                                stepsList = new List<Step>(steps);
                                stepsList.Add(new Step(i, Action.AppRoach));

                                afterboard = (int[,])newboard.Clone();
                            }

                            List<Vector2> newhistory = new List<Vector2>(history);
                            newhistory.Add(position + i);
                            List<Step> newSteps = new List<Step>(steps);
                            newSteps.Add(new Step(i, Action.AppRoach));
                            CanMakeCapturingChainMove(position + i, newboard, ref max, tong + Approach(position, i, board, color), newhistory, newSteps, color);
                        }

                        else if (Withdraw(position, i, board, color) > 0)
                        {
                            int[,] newboard = (int[,])board.Clone();
                            newboard[(int)position.x, (int)position.y] = 2;
                            newboard[(int)(position.x + i.x), (int)(position.y + i.y)] = color;

                            foreach (Vector2 _i in withdrawnCaptured)
                            {
                                newboard[(int)_i.x, (int)_i.y] = 2;
                            }

                            if (tong + Withdraw(position, i, board, color) >= max)
                            {
                                max = tong + Withdraw(position, i, board, color);

                                stepsList = new List<Step>(steps);
                                stepsList.Add(new Step(i, Action.WithDrawn));

                                afterboard = (int[,])newboard.Clone();
                            }

                            List<Vector2> newhistory = new List<Vector2>(history);
                            newhistory.Add(position + i);
                            List<Step> newSteps = new List<Step>(steps);
                            newSteps.Add(new Step(i, Action.WithDrawn));
                            CanMakeCapturingChainMove(position + i, newboard, ref max, tong + Withdraw(position, i, board, color), newhistory, newSteps, color);
                        }
                    }
             
                }
            }
        }
        else
        {
            foreach (Vector2 i in direction_basic)
            {
                if (incurrentMap(position, i) && !history.Contains(position + i))
                {
                    if (board[(int)position.x + (int)i.x, (int)position.y + (int)i.y] == 2)
                    {

                        if (Approach(position, i, board, color) > 0)
                        {
                            int[,] newboard = (int[,])board.Clone();
                            newboard[(int)position.x, (int)position.y] = 2;
                            newboard[(int)(position.x + i.x), (int)(position.y + i.y)] = color;

                            foreach (Vector2 _i in approachCaptured)
                            {
                                newboard[(int)_i.x, (int)_i.y] = 2;
                            }

                            if (tong + Approach(position, i, board, color) >= max)
                            {
                                max = tong + Approach(position, i, board, color);

                                stepsList = new List<Step>(steps);
                                stepsList.Add(new Step(i, Action.AppRoach));

                                afterboard = (int[,])newboard.Clone();
                            }

                            List<Vector2> newhistory = new List<Vector2>(history);
                            newhistory.Add(position + i);
                            List<Step> newSteps = new List<Step>(steps);
                            newSteps.Add(new Step(i, Action.AppRoach));
                            CanMakeCapturingChainMove(position + i, newboard, ref max, tong + Approach(position, i, board, color), newhistory, newSteps, color);
                        } 
                        else if (Withdraw(position, i, board, color) > 0)
                        {
                            int[,] newboard = (int[,])board.Clone();
                            newboard[(int)position.x, (int)position.y] = 2;
                            newboard[(int)(position.x + i.x), (int)(position.y + i.y)] = color;

                            foreach (Vector2 _i in withdrawnCaptured)
                            {
                                newboard[(int)_i.x, (int)_i.y] = 2;
                            }

                            if (tong + Withdraw(position, i, board, color) >= max)
                            {
                                max = tong + Withdraw(position, i, board, color);

                                stepsList = new List<Step>(steps);
                                stepsList.Add(new Step(i, Action.WithDrawn));

                                afterboard = (int[,])newboard.Clone();
                            }                           

                            List<Vector2> newhistory = new List<Vector2>(history);
                            newhistory.Add(position + i);
                            List<Step> newSteps = new List<Step>(steps);
                            newSteps.Add(new Step(i, Action.WithDrawn));
                            CanMakeCapturingChainMove(position + i, newboard, ref max, tong + Withdraw(position, i, board, color), newhistory, newSteps, color);
                        }
                    }
                        
                }
            }
        }
    }

    public int Approach(Vector2 stone,Vector2 direction, int[,] board,int color)
    {
        bool isEnd = false;
        int stonesCaptured = 0;
        int t = 2;
        int e_color = color == 0 ? 1 : 0;
        approachCaptured.Clear();
        while (!isEnd)
        {
            if (incurrentMap(stone, direction * t))
            {
                if (board[(int)stone.x + (int)direction.x * t, (int)stone.y + (int)direction.y * t] == e_color)
                {
                    stonesCaptured += 1;
                    approachCaptured.Add(new Vector2((int)stone.x + (int)direction.x * t, (int)stone.y + (int)direction.y * t));
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

    public int Withdraw(Vector2 stone, Vector2 direction, int[,] board, int color)
    {
        bool isEnd = false;
        int stonesCaptured = 0;
        int t = -1;
        int e_color = color == 0 ? 1 : 0;
        withdrawnCaptured.Clear();
        while (!isEnd)
        {
            if (incurrentMap(stone, direction * t))
            {
                if (board[(int)stone.x + (int)direction.x * t, (int)stone.y + (int)direction.y * t] == e_color)
                {
                    stonesCaptured += 1;
                    withdrawnCaptured.Add(new Vector2((int)stone.x + (int)direction.x * t, (int)stone.y + (int)direction.y * t));
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

    public bool inHistory(List<Step> steps,Vector2 i)
    {
        return steps.Any(item => item.direction == i);
    }
    public bool incurrentMap(Vector2 stone,Vector2 i)
    {
        if ((stone.x + (int)i.x) < 5 && (stone.x + (int)i.x) >= 0 && (stone.y + (int)i.y) < 9 && (stone.y + (int)i.y) >= 0)
        {
            return true;
        }
        else
            return false;
    }

    public void logcurrentMap(int[,] currentMap)
    {
        for (int i = 0; i < 5; i++)
        {
            Debug.Log(currentMap[i, 0] + " " + currentMap[i, 1] + " " + currentMap[i, 2] + " " + currentMap[i, 3] + " " + currentMap[i, 4] + " " + currentMap[i, 5] + " " + currentMap[i, 6] + " " + currentMap[i, 7] + " " + currentMap[i, 8]);
        }
    }
}
