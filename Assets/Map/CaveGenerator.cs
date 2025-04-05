using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CaveGenerator : GridGenerator
{
    [Header("Cave Settings")]
    public int smoothingPasses = 5;
    public int minRooms = 3;
    public int maxRooms = 5;
    public int minRoomSize = 3;
    public int maxRoomSize = 6;
    public int border = 3;

    public int desired_volume = 200;
    [Range(0f, 1f)] public float fillProbability = 0.85f;

    protected override void InitializeGrid()
    {
        CreateBaseGrid();
        CarveRandomRooms();
        ApplyCellularAutomaton();
        ApplyDetermenisticCellularAutomaton();
        GeneratePoints();

        int fail_state = 0;

        while (!EnsureConnectivity()) {
            if (fail_state > 150) {
                Debug.Log("Failed to generate map with constrains");
                break;
            }
            CreateBaseGrid();
            CarveRandomRooms();
            ApplyCellularAutomaton();
            ApplyDetermenisticCellularAutomaton();
            GeneratePoints();
            fail_state += 1;
        }
        
    }

    void CreateBaseGrid()
    {
        walls = new bool[width, height];
        
        // Create solid borders
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                bool isBorder = x == 0 || x == width-1 || y == 0 || y == height-1;
                walls[x, y] = isBorder || Random.value < fillProbability;
            }
        }
    }

    void CarveRandomRooms()
    {
        int rooms = Random.Range(minRooms, maxRooms + 1);
        for (int i = 0; i < rooms; i++)
        {
            int roomWidth = Random.Range(minRoomSize, maxRoomSize);
            int roomHeight = Random.Range(minRoomSize, maxRoomSize);
            
            int x = Random.Range(border, width - roomWidth - border);
            int y = Random.Range(border, height - roomHeight - border);

            CarveRectangle(x, y, roomWidth, roomHeight);
        }
    }

    void CarveRectangle(int startX, int startY, int w, int h)
    {
        for (int x = startX; x < startX + w; x++)
        {
            for (int y = startY; y < startY + h; y++)
            {
                if (x < width-1 && y < height-1)
                    walls[x, y] = Random.Range(0, 5) == 0;
            }
        }
    }

    void ApplyCellularAutomaton()
    {
        for (int i = 0; i < smoothingPasses; i++)
        {
            bool[,] newGrid = (bool[,])walls.Clone();
            
            for (int x = 1; x < width-1; x++)
            {
                for (int y = 1; y < height-1; y++)
                {
                    int wallCount = CountMooreNeighbors(x, y);
                    newGrid[x, y] = wallCount > 4 || (wallCount == 4 && Random.value > 0.5f);
                }
            }
            walls = newGrid;
        }
    }

    void ApplyDetermenisticCellularAutomaton()
    {
        for (int i = 0; i < 1; i++)
        {
            bool[,] newGrid = (bool[,])walls.Clone();
            
            for (int x = 1; x < width-1; x++)
            {
                for (int y = 1; y < height-1; y++)
                {
                    int wallCount = CountMooreNeighbors(x, y);
                    if (wallCount == 3) {
                        newGrid[x, y] = false;
                    }
                }
            }
            walls = newGrid;
        }
    }

    int CountMooreNeighbors(int x, int y)
    {
        int count = 0;
        for (int nx = -1; nx <= 1; nx++)
        {
            for (int ny = -1; ny <= 1; ny++)
            {
                if (nx == 0 && ny == 0) continue;
                
                int checkX = x + nx;
                int checkY = y + ny;

                if (checkX >= 0 && checkX < width && checkY >= 0 && checkY < height)
                {
                    if (walls[checkX, checkY]) count++;
                }
                else
                {
                    count++;
                }
            }
        }
        return count;
    }


    bool EnsureConnectivity()
    {

        bool[,] visited = new bool[width, height];
        Queue<Vector2Int> toCheck = new Queue<Vector2Int>();
        
        if (walls[width/2, height/2]) {
            return false;
        }
        Vector2Int start = new Vector2Int(width/2, height/2);
        toCheck.Enqueue(start);
        visited[start.x, start.y] = true;

        int volume = 0;
        

        // Flood fill algorithm
        while (toCheck.Count > 0)
        {
            Vector2Int current = toCheck.Dequeue();
            
            foreach (Vector2Int dir in new Vector2Int[] {
                Vector2Int.up, Vector2Int.down, 
                Vector2Int.left, Vector2Int.right
            })
            {
                Vector2Int neighbor = current + dir;
                
                if (neighbor.x > border && neighbor.x < width-border-1 && 
                    neighbor.y > border && neighbor.y < height-border-1)
                {
                    if (!walls[neighbor.x, neighbor.y] && !visited[neighbor.x, neighbor.y])
                    {
                        visited[neighbor.x, neighbor.y] = true;
                        toCheck.Enqueue(neighbor);
                        volume += 1;
                    }
                }
            }
        }

        if (volume < desired_volume) {
            return false;
        }

        // Remove unvisited areas
        for (int x = 1; x < width-1; x++)
        {
            for (int y = 1; y < height-1; y++)
            {
                if (!walls[x, y] && !visited[x, y])
                {
                    walls[x, y] = true; // Fill unconnected areas
                }
            }
        }
        return true;
    }

    public float wallClearanceRadius = 3f;
    [Tooltip("Minimum distance between points")]
    public float pointSpacing = 10f;
    [Tooltip("Number of points to generate")]
    public int numberOfPoints = 5;

    [Header("Gizmo Settings")]
    public float gizmoRadius = 0.5f;
    public Color gizmoColor = Color.cyan;
    private List<Vector2> points = new List<Vector2>();

    public void GeneratePoints()
    {
        points.Clear();
        List<Vector2> validPositions = new List<Vector2>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!walls[x, y] && IsPositionValid(x, y))
                {
                    validPositions.Add(new Vector2(x, y));
                }
            }
        }

        for (int i = validPositions.Count - 1; i > 0; i--) {
            int j = Random.Range(0, i + 1);
            Vector2 temp = validPositions[i];
            validPositions[i] = validPositions[j];
            validPositions[j] = temp;
        }

        foreach (Vector2 candidate in validPositions)
        {
            if (points.Count >= numberOfPoints) break;

            bool validCandidate = true;
            foreach (Vector2 point in points)
            {
                if (Vector2.Distance(candidate, point) < pointSpacing)
                {
                    validCandidate = false;
                    break;
                }
            }

            if (validCandidate) points.Add(candidate);
        }
    }

    private bool IsPositionValid(int x, int y)
    {
        int clearance = Mathf.CeilToInt(wallClearanceRadius);
        
        for (int dx = -clearance; dx <= clearance; dx++)
        {
            for (int dy = -clearance; dy <= clearance; dy++)
            {
                int checkX = x + dx;
                int checkY = y + dy;
                if (checkX < 0 || checkX >= width || checkY < 0 || checkY >= height)
                    return false;
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(checkX, checkY));
                if (distance <= wallClearanceRadius && walls[checkX, checkY])
                    return false;
            }
        }
        return true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        foreach (Vector2 point in points)
        {
            Vector3 worldPosition = new Vector3(
                point.x + transform.position.x, 
                point.y + transform.position.y,
                0
            );
            Gizmos.DrawSphere(worldPosition, gizmoRadius);
        }
    }

}