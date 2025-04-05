using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    [Header("Railway Settings")]
    [SerializeField] RailwayConstructor _railwayConstructor;

    [Header("Grid Settings")]
    public int width = 20;
    public int height = 15;
    public float cellSize = 1f;


    [Header("Wall References")]
    public GameObject wallPrefab;
    public Sprite edgeWallSprite;
    public Sprite solidWallSprite;

    protected bool[,] walls;
    private Dictionary<Vector2Int, GameObject> wallsDictionary = new Dictionary<Vector2Int, GameObject>();

    public bool[,] Walls => walls;
    public int Width => width;
    public int Height => height;



    void Start()
    {
        GenerateLevel();
        Debug.Log("generated");
    }

    public void GenerateLevel()
    {
        RemoveWalls();
        InitializeGrid();
        InitializeBorder();
        CreateWalls();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) {
            Debug.Log("pressed");
            GenerateLevel();
        }
    }

    void InitializeBorder()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || x == width-1 || y == 0 || y == height-1) {
                    walls[x, y] = true;
                }   
            }
        }
    }

    protected virtual void InitializeGrid()
    {
        walls = new bool[width, height];

        for (int x = 1; x < width-1; x++)
        {
            for (int y = 1; y < height-1; y++)
            {
                walls[x, y] = Random.Range(0, 10) > 0;
            }
        }
    }

    void CreateWalls()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (walls[x, y])
                {
                    CreateWall(x, y);
                }
            }
        }
    }


    void RemoveWalls()
    {
        foreach (var wall in wallsDictionary) {
            Destroy(wall.Value);
        }
    }


    void CreateWall(int x, int y)
    {
        Vector3 position = new Vector3(x * cellSize + transform.position.x, y * cellSize + transform.position.y, y);
        GameObject wall = Instantiate(wallPrefab, position, Quaternion.identity, transform);
        wall.name = $"Wall_{x}_{y}";

        Vector2Int gridPos = new Vector2Int(x, y);
        wallsDictionary[gridPos] = wall;
        
        SpriteRenderer sr = wall.GetComponent<SpriteRenderer>();
        bool useEdgeSprite = ShouldUseEdgeSprite(x, y);
        sr.sprite = useEdgeSprite ? edgeWallSprite : solidWallSprite;
    }

    bool ShouldUseEdgeSprite(int x, int y)
    {
        bool isBorder = y == 0;

        if (isBorder)
        {
            return false;
        }
        else
        {
            bool hasWallInFront = CheckForward(x, y, Vector2.down);
            return hasWallInFront;
        }
    }

    bool CheckForward(int x, int y, Vector2 direction)
    {
        int checkX = x + (int)direction.x;
        int checkY = y + (int)direction.y;

        // If checking outside walls, consider it empty
        if (checkX < 0 || checkX >= width || checkY < 0 || checkY >= height)
            return false;

        return walls[checkX, checkY];
    }

}