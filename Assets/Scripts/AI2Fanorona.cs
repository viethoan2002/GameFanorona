using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

public struct Node
{
    public Vector2 position;
    public int value;
    public List<Step> stepsHistory;
    public int[,] board;

    public Node(Vector2 _position,int _value,List<Step> stepsList, int[,] board)
    {
        this.position = _position;
        this.value = _value;
        this.stepsHistory = stepsList;
        this.board = board;
    }

    public void SetUp(Vector2 _position, int _value, List<Step> stepsList, int[,] board)
    {
        this.position = _position;
        this.value = _value;
        this.stepsHistory = new List<Step>(stepsList);
        this.board = (int[,])board.Clone();
    }

    public void Update(int _value, List<Step> stepsList, int[,] board)
    {
        this.value = _value;
        this.stepsHistory = new List<Step>(stepsList);
        this.board =(int[,])board.Clone();
    }
}

public struct StoneFX
{
    public Vector2 position;
    public List<Vector2> moveDir;

    public StoneFX(Vector2 _position,List<Vector2> _moveDir)
    {
        this.position = _position;
        this.moveDir = new List<Vector2>(_moveDir);
    }
}
public class AI2Fanorona : MonoBehaviour
{
    public static AI2Fanorona Instance;

    public List<Vector2> direction_advance = new List<Vector2>() { new Vector2(0, 1), new Vector2(1, 0), new Vector2(0, -1), new Vector2(-1, 0), new Vector2(1, 1), new Vector2(1, -1), new Vector2(-1, 1), new Vector2(-1, -1) };
    public List<Vector2> direction_basic = new List<Vector2>() { new Vector2(0, 1), new Vector2(1, 0), new Vector2(0, -1), new Vector2(-1, 0) };

    List<Vector2> approachCaptured = new List<Vector2>();
    List<Vector2> withdrawnCaptured = new List<Vector2>();

    public int limit = 3;

    public Node selectStone;
    private void Awake()
    {
        AI2Fanorona.Instance = this;
    }

    public Node AIMove(State color, int[,] board)
    {
        int nb = 0;
        if (color == State.Black)
        {
            SelectMinMaxStone(1, board, ref nb, 0, -1000, 1000);
        }
        else
            SelectMinMaxStone(0, board, ref nb, 0, -1000, 1000);
        Debug.Log(selectStone.position);
        foreach (Step step in selectStone.stepsHistory)
        {
            Debug.Log(step.direction + " " + step.stepAction);
        }
        return selectStone;
    }

    public void SelectMinMaxStone(int colorturn, int[,] board, ref int nb, int depth, int alpha, int beta)
    {
        if (colorturn == 1)
        {
            GameManager.Instance.FreeStone(State.Black, board);
        }
        else
        {
            GameManager.Instance.FreeStone(State.White, board);
        }

        List<StoneFX> freeStones = new List<StoneFX>();
        foreach (Stone stone in GameManager.Instance.freeStones)
        {
            freeStones.Add(new StoneFX(new Vector2(stone.x, stone.y), stone.moves));
        }

        int max = -1000;
        Node current_capture_node;
        int optimal_capture_num;

        if (depth % 2 == 0)
        {//MAX
            optimal_capture_num = -1000;
        }
        else
        {//MIN
            optimal_capture_num = 1000;
        }

        foreach (StoneFX stoneFX in freeStones)
        {
            foreach (Vector2 dir in stoneFX.moveDir)
            {
                current_capture_node = MaxStoneCaptured(stoneFX.position, dir, board, colorturn);
                List<Step> currentSteps = new List<Step>(current_capture_node.stepsHistory);
                int[,] newboard = (int[,])current_capture_node.board.Clone();

                int later_capture_num = 0;
                int total_capture_num = 0;

                if (depth < limit - 1)
                {
                    if (colorturn == 1)
                    {
                        SelectMinMaxStone(0, newboard, ref later_capture_num, depth + 1, alpha, beta);
                    }
                    else
                    {
                        SelectMinMaxStone(1, newboard, ref later_capture_num, depth + 1, alpha, beta);
                    }
                }
                else
                    later_capture_num = 0;

                if (depth % 2 == 0)
                    total_capture_num = current_capture_node.value + later_capture_num;
                else
                    total_capture_num = -current_capture_node.value + later_capture_num;

                if (depth == 0 && total_capture_num >= max)
                {
                    selectStone = current_capture_node;
                    max = total_capture_num;
                }

                if (depth % 2 == 0)
                {
                    optimal_capture_num = total_capture_num;

                    if (optimal_capture_num > alpha)
                    {
                        nb = optimal_capture_num;
                    }

                    alpha = Mathf.Max(alpha, optimal_capture_num);

                    if (beta <= alpha)
                    {
                        break;
                    }
                }
                else
                {
                    optimal_capture_num = total_capture_num;

                    if (optimal_capture_num < beta)
                    {
                        nb = optimal_capture_num;
                    }

                    beta = Mathf.Min(beta, optimal_capture_num);

                    if (beta <= alpha)
                    {
                        break;
                    }
                }
            }
        }
    }

    public Node MaxStoneCaptured(Vector2 position, Vector2 direction, int[,] board,int color)
    {
        List<Vector2> history = new List<Vector2>();
        history.Add(position);
        int max = 0;
        List<Step> newStepList;
        Node maxNode = new Node(position, 0,new List<Step>(),board);

        if ( Approach(new Vector2(position.x, position.y),direction,board,color)== 0 && Withdraw(new Vector2(position.x, position.y), direction, board, color) == 0)
        {
            newStepList = new List<Step>();
            newStepList.Add(new Step(direction, Action.Paika));
            int[,] newnoard = (int[,])board.Clone();
            newnoard[(int)position.x, (int)position.y] = 2;
            newnoard[(int)(position.x + direction.x), (int)(position.y + direction.y)] = color;
            maxNode.Update(0, newStepList, newnoard);
        }
        else
        {
            CanMakeCapturingChainMove(position, board, ref max, 0, history, new List<Step>(), color,ref maxNode);
        }
        return maxNode;
    }

    public void CanMakeCapturingChainMove(Vector2 position, int[,] board,ref int max, int tong, List<Vector2> history, List<Step> steps, int color, ref Node maxNode)
    {
        if ((int)position.x % 2 == (int)position.y % 2)
        {
            foreach (Vector2 i in direction_advance)
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
                            List<Vector2> newhistory = new List<Vector2>(history);
                            newhistory.Add(position + i);
                            List<Step>  newSteps = new List<Step>(steps);
                            newSteps.Add(new Step(i, Action.AppRoach));

                            foreach (Vector2 _i in approachCaptured)
                            {
                                newboard[(int)_i.x, (int)_i.y] = 2;
                            }

                            if (tong + Approach(position, i, board, color) >= max)
                            {
                                max = tong + Approach(position, i, board, color); ;
                                maxNode.Update(max, newSteps, newboard);
                            }

                            CanMakeCapturingChainMove(position + i, newboard, ref max, tong + Approach(position, i, board, color), newhistory, newSteps, color,ref maxNode);
                        }

                        else if (Withdraw(position, i, board, color) > 0)
                        {
                            int[,] newboard = (int[,])board.Clone();
                            newboard[(int)position.x, (int)position.y] = 2;
                            newboard[(int)(position.x + i.x), (int)(position.y + i.y)] = color;
                            List<Vector2> newhistory = new List<Vector2>(history);
                            newhistory.Add(position + i);
                            List<Step> newSteps = new List<Step>(steps);
                            newSteps.Add(new Step(i, Action.WithDrawn));

                            foreach (Vector2 _i in withdrawnCaptured)
                            {
                                newboard[(int)_i.x, (int)_i.y] = 2;
                            }

                            if (tong + Approach(position, i, board, color) >= max)
                            {
                                max = tong + Approach(position, i, board, color); ;
                                maxNode.Update(max, newSteps, newboard);
                            }

                            CanMakeCapturingChainMove(position + i, newboard, ref max, tong + Withdraw(position, i, board, color), newhistory, newSteps, color, ref maxNode);
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
                            List<Vector2> newhistory = new List<Vector2>(history);
                            newhistory.Add(position + i);
                            List<Step> newSteps = new List<Step>(steps);
                            newSteps.Add(new Step(i, Action.AppRoach));

                            foreach (Vector2 _i in approachCaptured)
                            {
                                newboard[(int)_i.x, (int)_i.y] = 2;
                            }

                            if (tong + Approach(position, i, board, color) >= max)
                            {
                                max = tong + Approach(position, i, board, color); ;
                                maxNode.Update(max, newSteps, newboard);
                            }

                            CanMakeCapturingChainMove(position + i, newboard, ref max, tong + Approach(position, i, board, color), newhistory, newSteps, color, ref maxNode);
                        }
                        else if (Withdraw(position, i, board, color) > 0)
                        {
                            int[,] newboard = (int[,])board.Clone();
                            newboard[(int)position.x, (int)position.y] = 2;
                            newboard[(int)(position.x + i.x), (int)(position.y + i.y)] = color;
                            List<Vector2> newhistory = new List<Vector2>(history);
                            newhistory.Add(position + i);
                            List<Step> newSteps = new List<Step>(steps);
                            newSteps.Add(new Step(i, Action.WithDrawn));

                            foreach (Vector2 _i in withdrawnCaptured)
                            {
                                newboard[(int)_i.x, (int)_i.y] = 2;
                            }

                            if (tong + Approach(position, i, board, color) >= max)
                            {
                                max = tong + Approach(position, i, board, color); ;
                                maxNode.Update(max, newSteps, newboard);
                            }

                            CanMakeCapturingChainMove(position + i, newboard, ref max, tong + Withdraw(position, i, board, color), newhistory, newSteps, color, ref maxNode);
                        }
                    }

                }
            }
        }
    }

    public int Approach(Vector2 stone, Vector2 direction, int[,] board, int color)
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

    public bool incurrentMap(Vector2 stone, Vector2 i)
    {
        if ((stone.x + (int)i.x) < 5 && (stone.x + (int)i.x) >= 0 && (stone.y + (int)i.y) < 9 && (stone.y + (int)i.y) >= 0)
        {
            return true;
        }
        else
            return false;
    }
}
