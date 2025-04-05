using Mono.Cecil.Cil;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.UIElements;

public struct RailwayConstructorFindRequest
{
    public int _root;
    public Vector2Int _position;
    public Vector2Int _move;
    public float _estimation;

    public RailwayConstructorFindRequest(int root, Vector2Int position, Vector2Int move, float estimation)
    {
        _root = root;
        _position = position;
        _move = move;
        _estimation = estimation;
    }
}

public struct RailwayConstructorCellData
{
    public int _root;
    public float _estimation;
    public float _movePenalty;
    public Vector2Int _mainParent;
    public List<Vector2Int> _backMoves;


    public RailwayConstructorCellData(int root, float estimation, int movePenlty)
    {
        _root = root;
        _estimation = estimation;
        _movePenalty = movePenlty;
        _backMoves = new();
        _mainParent = new(-1, -1);
    }
}

public struct RailwayConstructoBackwardData
{
    public Vector2Int _position;
    public Vector2Int _move;
    public Vector2Int _parent;
    public Vector2Int _oldParent;
    public bool _meetOld;

    public RailwayConstructoBackwardData(Vector2Int position, Vector2Int move, Vector2Int parent, Vector2Int oldParent, bool meetOld)
    {
        _position = position;
        _move = move;
        _parent = parent;
        _oldParent = oldParent;
        _meetOld = meetOld;
    }
}

public class RailwayConstructor : MonoBehaviour
{
    [SerializeField] private CaveGenerator _caveGenerator;
    [SerializeField] private GameObject _railwayPrefab;
    [SerializeField] private GameObject _nodePrefab;
    [SerializeField] private GameObject _minecartPrefab;
    [SerializeField] private bool _construct;

    public void Update()
    {
        if (_construct)
        {
            _construct = false;
            ConstructRailway();
        }
    }

    public bool ConstructRailway()
    {
        Vector2Int size = new Vector2Int(_caveGenerator.Width, _caveGenerator.Height);
        RailwayConstructorCellData[,] findingMatrixEstimation = new RailwayConstructorCellData[size.x, size.y];
        bool[,] walls = _caveGenerator.Walls;
        bool[,] railways = new bool[size.x, size.y];
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                railways[i, j] = false;
            }
        }
        List<Vector2> points = _caveGenerator.Points;
        int compsCount = points.Count;
        int[] components = new int[points.Count];
        for (int i = 0; i < components.Length; i++)
        {
            components[i] = i;
        }

        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                findingMatrixEstimation[i, j] = new(-1, -1, 0);
                if (!walls[i, j])
                {
                    int closestWall = 6;
                    for (int di = -5; di <= 5; di++)
                    {
                        for (int dj = -5; dj <= 5; dj++)
                        {
                            if (InRange(i + di, j + dj, size))
                            {
                                if (walls[i + di, j + dj])
                                {
                                    closestWall = Mathf.Min(closestWall, Mathf.Max(Mathf.Abs(di), Mathf.Abs(dj)));
                                }
                            }
                        }
                    }
                    findingMatrixEstimation[i, j]._movePenalty = Mathf.Pow(2, 6 - closestWall);
                }
            }
        }

        Dictionary<Vector2Int, List<Vector2Int>> controlPoints = new();
        Utils.PriorityQueue<RailwayConstructorFindRequest, float> requests = new();
       
        for (int i = 0; i < points.Count; i++)
        {
            requests.Enqueue(new(i, new((int) points[i].x, (int)points[i].y), new(0, 0), 0), 0);
            railways[(int)points[i].x, (int)points[i].y] = true;
            controlPoints.Add(new((int)points[i].x, (int)points[i].y), new());
        }

        bool constructed = false;
        Queue<RailwayConstructoBackwardData> backwards = new Queue<RailwayConstructoBackwardData>();

        while (requests.Count > 0)
        {
            if (compsCount == 1)
            {
                constructed = true;
                break;
            }

            RailwayConstructorFindRequest request = requests.Dequeue();

            int root = findingMatrixEstimation[request._position.x, request._position.y]._root;

            if (root == -1)
            {
                findingMatrixEstimation[request._position.x, request._position.y]._root = request._root;
                findingMatrixEstimation[request._position.x, request._position.y]._backMoves.Add(new(-request._move.x, -request._move.y));
            }
            else {
                if (components[root] != components[request._root])
                {
                    int minRoot = Mathf.Min(components[root], components[request._root]);
                    int maxRoot = Mathf.Max(components[root], components[request._root]);

                    for (int i = 0; i < components.Length; i++)
                    {
                        if (components[i] == maxRoot) components[i] = minRoot;
                    }
                    compsCount--;
                    findingMatrixEstimation[request._position.x, request._position.y]._backMoves.Add(new(-request._move.x, -request._move.y));
                    railways[request._position.x, request._position.y] = true;

                    Vector2Int oldParent = request._position;

                    if (!controlPoints.ContainsKey(request._position))
                    {
                        controlPoints.Add(request._position, new());
                        controlPoints[request._position].Add(findingMatrixEstimation[request._position.x, request._position.y]._mainParent);
                        oldParent = findingMatrixEstimation[request._position.x, request._position.y]._mainParent;
                    }

                    for (int i = 0; i < findingMatrixEstimation[request._position.x, request._position.y]._backMoves.Count; i++)
                    {
                        Vector2Int move = findingMatrixEstimation[request._position.x, request._position.y]._backMoves[i];
                        backwards.Enqueue(new(request._position + move, move, request._position, oldParent, false));
                    }

                    while (backwards.Count > 0)
                    {
                        RailwayConstructoBackwardData backward = backwards.Dequeue();
                        if (railways[backward._position.x, backward._position.y] == true && controlPoints.ContainsKey(backward._position))
                        {
                            if (controlPoints[backward._position].Contains(backward._oldParent)) {
                                controlPoints[backward._position].Remove(backward._oldParent);
                            }
                            controlPoints[backward._position].Add(backward._parent);
                            findingMatrixEstimation[backward._position.x, backward._position.y]._mainParent = backward._parent;
                            continue;
                        }
                        bool meetOld = backward._meetOld;
                        Vector2Int oldMainParent = findingMatrixEstimation[backward._position.x, backward._position.y]._mainParent;
                        findingMatrixEstimation[backward._position.x, backward._position.y]._mainParent = backward._parent;
                        Vector2Int oldBackwardParent = backward._oldParent;
                        bool haveRailways = railways[backward._position.x, backward._position.y];
                        if (haveRailways && !meetOld) {
                            oldBackwardParent = oldMainParent;
                        }

                        railways[backward._position.x, backward._position.y] = true;
                        for (int i = 0; i < findingMatrixEstimation[backward._position.x, backward._position.y]._backMoves.Count; i++)
                        {
                            Vector2Int backwardMove = findingMatrixEstimation[backward._position.x, backward._position.y]._backMoves[i];
                            Vector2Int backwardPosition = backward._position + backwardMove;
                            Vector2Int backwardParent = backward._parent;
                            if (backwardMove.x != backward._move.x || backwardMove.y != backward._move.y || (haveRailways && !meetOld))
                            {
                                controlPoints.Add(backward._position, new());
                                backwardParent = backward._position;

                                if (backwardMove.x != backward._move.x || backwardMove.y != backward._move.y)
                                {
                                    controlPoints[backward._position].Add(backward._parent);
                                }

                                if (haveRailways)
                                {
                                    controlPoints[backward._position].Add(oldBackwardParent);
                                    meetOld = true;
                                }
                            }
                            backwards.Enqueue(new(backwardPosition, backwardMove, backwardParent, oldBackwardParent, meetOld));
                        }
                    }

                }
                continue;
            }

            Vector2 moveIndexNorm = new Vector2(request._move.x, request._move.y).normalized;

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0) continue;

                    if (InRange(request._position.x + i, request._position.y + j, size) && !walls[request._position.x + i, request._position.y + j])
                    {
                        float addEsti = findingMatrixEstimation[request._position.x, request._position.y]._movePenalty;
                        float mult = 1;
                        Vector2 newMoveIndexNorm = new Vector2(i, j).normalized;
                        float change = Mathf.Abs(moveIndexNorm.x * newMoveIndexNorm.y - moveIndexNorm.y * newMoveIndexNorm.x);
                        if ((Mathf.Abs(i) + Mathf.Abs(j)) != 1)
                        {
                            mult = 1.5f;
                        }
                        requests.Enqueue(new(request._root, new(request._position.x + i, request._position.y + j), new(i, j), request._estimation + addEsti), request._estimation + addEsti * mult + Mathf.Exp(change * 2) - 1);
                    }

                }
            }

        }

        Dictionary<Vector2Int, Node> nodes = new();

        if (constructed)
        {
            for (int i = 0; i < size.x; i++)
            {
                for (int j = 0; j < size.y; j++)
                {
                    if (railways[i, j])
                    {
                        GameObject railwayObject = Instantiate(_railwayPrefab);
                        railwayObject.transform.position = new(i, j);
                        if (controlPoints.ContainsKey(new(i, j))) {
                            SpriteRenderer spriteRenderer = railwayObject.GetComponent<SpriteRenderer>();
                            spriteRenderer.color = Color.red;

                            GameObject nodeObject = Instantiate(_nodePrefab);
                            nodeObject.transform.position = new(i, j);
                            Node node = nodeObject.GetComponent<Node>();
                            nodes.Add(new(i, j), node);
                        }
                    }
                }
            }
        }

        foreach (var nodeNote in nodes)
        {
            Vector2Int key = nodeNote.Key;
            Node node = nodeNote.Value;
            for (int i = 0; i < controlPoints[key].Count; i++)
            {
                Vector2Int k = controlPoints[key][i];
                if (k.x == -1 || k.y == -1)
                {
                    continue;
                }
                Node conn = nodes[controlPoints[key][i]];
                node.connectedNodes.Add(conn);
                conn.connectedNodes.Add(node);
            }
        }

        GameObject minecartObject = Instantiate(_minecartPrefab);
        Minecart minecart = minecartObject.GetComponent<Minecart>();
        minecart.startingNode = nodes.First().Value;
        return constructed;
    }

    public bool InRange(int x, int y, Vector2Int size)
    {
        return x >= 0 && x < size.x && y >= 0 && y < size.y;
    }

}
