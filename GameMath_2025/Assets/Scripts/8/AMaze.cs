using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AMaze : MonoBehaviour
{
    public int width = 21;
    public int height = 21;

    public GameObject wallPrefab;
    public GameObject groundPrefab;
    public GameObject forestPrefab;
    public GameObject mudPrefab;
    public GameObject pathPrefab;
    public GameObject enemyPrefab;

    public int enemyCount = 10;

    int[,] map;
    Vector2Int start;
    Vector2Int goal;
    List<Vector2Int> enemyPositions = new List<Vector2Int>();
    List<Vector2Int> currentPath;

    private List<GameObject> spawnedPathObjects = new List<GameObject>();
    private List<GameObject> spawnedEnemyObjects = new List<GameObject>();

    void Start()
    {
        do
        {
            GenerateRandomMap();
            start = new Vector2Int(1, 1);
            goal = new Vector2Int(width - 2, height - 2);

            map[start.x, start.y] = 1;
            map[goal.x, goal.y] = 1;

            GenerateEnemy();

        } while (!IsPathPossible_DFS(start, goal));

        VisualizeMap();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            currentPath = CustomAStar(map, start, goal);
            VisualizePath(currentPath);
        }
    }

    List<Vector2Int> CustomAStar(int[,] map, Vector2Int start, Vector2Int goal)
    {
        int w = map.GetLength(0);
        int h = map.GetLength(1);

        int[,] gCost = new int[w, h];
        bool[,] visited = new bool[w, h];
        Vector2Int?[,] parent = new Vector2Int?[w, h];

        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                gCost[x, y] = int.MaxValue;

        gCost[start.x, start.y] = 0;

        List<Vector2Int> open = new List<Vector2Int>();
        open.Add(start);

        Vector2Int[] dirs = {
            new Vector2Int(1, 0), new Vector2Int(-1, 0),
            new Vector2Int(0, 1), new Vector2Int(0, -1)
        };

        while (open.Count > 0)
        {
            int bestIndex = 0;
            int bestF = GetF(open[0], gCost, goal);

            for (int i = 1; i < open.Count; i++)
            {
                int f = GetF(open[i], gCost, goal);
                if (f < bestF)
                {
                    bestF = f;
                    bestIndex = i;
                }
            }

            Vector2Int cur = open[bestIndex];
            open.RemoveAt(bestIndex);

            if (visited[cur.x, cur.y]) continue;
            visited[cur.x, cur.y] = true;

            if (cur == goal)
                return ReconstructPath(parent, start, goal);

            foreach (var d in dirs)
            {
                int nx = cur.x + d.x;
                int ny = cur.y + d.y;

                if (!InBounds(map, nx, ny)) continue;
                if (map[nx, ny] == 0) continue;
                if (visited[nx, ny]) continue;

                int tileCost = TileCost(map[nx, ny]);
                int newG = gCost[cur.x, cur.y] + tileCost;

                if (newG < gCost[nx, ny])
                {
                    gCost[nx, ny] = newG;
                    parent[nx, ny] = cur;

                    if (!open.Contains(new Vector2Int(nx, ny)))
                    {
                        open.Add(new Vector2Int(nx, ny));
                    }
                }
            }
        }

        return null;
    }

    int GetF(Vector2Int pos, int[,] gCost, Vector2Int goal)
    {
        int h = Mathf.Abs(pos.x - goal.x) + Mathf.Abs(pos.y - goal.y);

        int enemyPenalty = GetEnemyPenalty(pos);

        return gCost[pos.x, pos.y] + h + enemyPenalty;
    }

    int GetEnemyPenalty(Vector2Int pos)
    {
        int maxPenalty = 0;
        int safeDistance = 5;

        foreach (var enemyPos in enemyPositions)
        {
            int distToEnemy = Mathf.Abs(pos.x - enemyPos.x) + Mathf.Abs(pos.y - enemyPos.y);

            if (distToEnemy < safeDistance)
            {
                int penalty = (safeDistance - distToEnemy) * 100;
                if (penalty > maxPenalty)
                {
                    maxPenalty = penalty;
                }
            }
        }

        return maxPenalty;
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
                if (rand < 10) map[x, y] = 0;
                else if (rand < 70) map[x, y] = 1;
                else if (rand < 90) map[x, y] = 2;
                else map[x, y] = 3;
            }
        }
    }

    void GenerateEnemy()
    {
        enemyPositions.Clear();

        for (int i = 0; i < enemyCount; i++)
        {
            while (true)
            {
                int rx = Random.Range(1, width - 1);
                int ry = Random.Range(1, height - 1);
                Vector2Int pos = new Vector2Int(rx, ry);

                if (pos == start || pos == goal) continue;
                if (map[rx, ry] == 0) continue;
                if (enemyPositions.Contains(pos)) continue;

                enemyPositions.Add(pos);
                break;
            }
        }
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

    void VisualizeMap()
    {
        foreach (var obj in spawnedPathObjects) Destroy(obj);
        spawnedPathObjects.Clear();

        foreach (var enemy in spawnedEnemyObjects) Destroy(enemy);
        spawnedEnemyObjects.Clear();

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
                    Instantiate(prefabToSpawn, new Vector3(x, 0, y), Quaternion.identity);
                }
            }
        }

        if (enemyPrefab != null)
        {
            foreach (var pos in enemyPositions)
            {
                GameObject go = Instantiate(enemyPrefab, new Vector3(pos.x, 0.5f, pos.y), Quaternion.identity);
                spawnedEnemyObjects.Add(go);
            }
        }
    }

    void VisualizePath(List<Vector2Int> path)
    {
        if (path == null) return;

        foreach (var go in spawnedPathObjects) Destroy(go);
        spawnedPathObjects.Clear();

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
            if (cur.Value == start) break;
            cur = parent[cur.Value.x, cur.Value.y];
        }
        path.Reverse();
        return path;
    }
}