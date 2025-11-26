using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DijkstraMaze : MonoBehaviour
{
    public int width = 21;
    public int height = 21;

    [Header("Prefabs")]
    public GameObject wallPrefab;
    public GameObject groundPrefab;
    public GameObject forestPrefab;
    public GameObject mudPrefab;
    public GameObject pathPrefab;

    int[,] map;
    Vector2Int start;
    Vector2Int goal;
    List<Vector2Int> currentPath;

    private List<GameObject> spawnedPathObjects = new List<GameObject>();

    void Start()
    {
        do
        {
            GenerateRandomMap();
            start = new Vector2Int(1, 1);
            goal = new Vector2Int(width - 2, height - 2);

            map[start.x, start.y] = 1;
            map[goal.x, goal.y] = 1;

        } while (!IsPathPossible_DFS(start, goal));

        VisualizeMap();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            currentPath = Dijkstras(map, start, goal);
            VisualizePath(currentPath);
        }
    }

    void GenerateRandomMap()
    {
        map = new int[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || y == 0 || x == width - 1 || y == height - 1)
                {
                    map[x, y] = 0;
                    continue;
                }

                int rand = Random.Range(0, 100);
                if (rand < 30) map[x, y] = 0;
                else if (rand < 70) map[x, y] = 1;
                else if (rand < 90) map[x, y] = 2;
                else map[x, y] = 3;
            }
        }
    }

    void VisualizeMap()
    {

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject prefabToSpawn = null;
                switch (map[x, y])
                {
                    case 0: prefabToSpawn = wallPrefab; break;
                    case 1: prefabToSpawn = groundPrefab; break;
                    case 2: prefabToSpawn = forestPrefab; break;
                    case 3: prefabToSpawn = mudPrefab; break;
                }

                if (prefabToSpawn != null)
                {
                    GameObject go = Instantiate(prefabToSpawn, new Vector3(x, 0, y), Quaternion.identity);
                }
            }
        }
    }

    void VisualizePath(List<Vector2Int> path)
    {
        if (path == null) return;

        foreach (var p in path)
        {
            GameObject go = Instantiate(pathPrefab, new Vector3(p.x, 0.5f, p.y), Quaternion.identity);
            spawnedPathObjects.Add(go);
        }
    }

    bool IsPathPossible_DFS(Vector2Int s, Vector2Int g)
    {
        bool[,] visited = new bool[width, height];
        Stack<Vector2Int> stack = new Stack<Vector2Int>();

        stack.Push(s);
        visited[s.x, s.y] = true;

        int[] dx = { 1, -1, 0, 0 };
        int[] dy = { 0, 0, 1, -1 };

        while (stack.Count > 0)
        {
            Vector2Int cur = stack.Pop();

            if (cur == g) return true;

            for (int i = 0; i < 4; i++)
            {
                int nx = cur.x + dx[i];
                int ny = cur.y + dy[i];

                if (InBounds(map, nx, ny) && !visited[nx, ny] && map[nx, ny] != 0)
                {
                    visited[nx, ny] = true;
                    stack.Push(new Vector2Int(nx, ny));
                }
            }
        }
        return false;
    }

    List<Vector2Int> Dijkstras(int[,] map, Vector2Int start, Vector2Int goal)
    {
        int w = map.GetLength(0);
        int h = map.GetLength(1);

        int[,] dist = new int[w, h];
        Vector2Int?[,] parent = new Vector2Int?[w, h];

        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                dist[x, y] = int.MaxValue;

        dist[start.x, start.y] = 0;

        SimplePriorityQueue<Vector2Int> pq = new SimplePriorityQueue<Vector2Int>();
        pq.Enqueue(start, 0);

        Vector2Int[] dirs = {
            new Vector2Int( 1, 0), new Vector2Int(-1, 0),
            new Vector2Int( 0, 1), new Vector2Int( 0,-1),
        };

        while (pq.Count > 0)
        {
            Vector2Int cur = pq.Dequeue();

            if (cur == goal)
                return ReconstructPath(parent, start, goal);

            foreach (var d in dirs)
            {
                int nx = cur.x + d.x;
                int ny = cur.y + d.y;

                if (!InBounds(map, nx, ny)) continue;
                if (map[nx, ny] == 0) continue;

                int moveCost = TileCost(map[nx, ny]);
                int newDist = dist[cur.x, cur.y] + moveCost;

                if (newDist < dist[nx, ny])
                {
                    dist[nx, ny] = newDist;
                    parent[nx, ny] = cur;
                    pq.Enqueue(new Vector2Int(nx, ny), newDist);
                }
            }
        }

        return null;
    }

    int TileCost(int tile)
    {
        switch (tile)
        {
            case 1: return 1;
            case 2: return 3;
            case 3: return 5;
            default: return 999;
        }
    }

    bool InBounds(int[,] map, int x, int y)
    {
        return x >= 0 && y >= 0 && x < map.GetLength(0) && y < map.GetLength(1);
    }

    List<Vector2Int> ReconstructPath(Vector2Int?[,] parent, Vector2Int start, Vector2Int goal)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int? cur = goal;
        while (cur.HasValue)
        {
            path.Add(cur.Value);
            cur = parent[cur.Value.x, cur.Value.y];
        }
        path.Reverse();
        return path;
    }
}