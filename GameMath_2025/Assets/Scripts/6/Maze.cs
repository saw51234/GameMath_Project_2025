using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Maze : MonoBehaviour
{
    public int w = 21, h = 21;
    [Range(0, 1)] public float wallProb = 0.35f;
    public GameObject wallPrf, floorPrf, pathPrf, charPrf, farPrf;

    int[,] map;
    List<Vector2Int> pathList;
    Vector2Int start = new Vector2Int(1, 1);
    Vector2Int goal;
    Vector2Int[] dirs = { new(1, 0), new(-1, 0), new(0, 1), new(0, -1) };
    GameObject charInst;

    void Start()
    {
        GenMaze();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GenMaze();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            DrawPath();
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            StartCoroutine(Move());
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            DrawFurthest();
        }
    }

    void GenMaze()
    {
        StopAllCoroutines();
        ClearObj();
        goal = new Vector2Int(w - 2, h - 2);

        while (true)
        {
            map = new int[w, h];
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    map[x, y] = (x == 0 || x == w - 1 || y == 0 || y == h - 1) ? 1 :
                    ((x == 1 && y == 1) || (x == goal.x && y == goal.y)) ? 0 :
                    (Random.value < wallProb ? 1 : 0);
                } 
            }

            pathList = GetBFS();
            if (pathList != null) break;
        }

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                Instantiate(map[x, y] == 1 ? wallPrf : floorPrf, new Vector3(x, 0, y), Quaternion.identity, transform).name = "Map";
            }
        }

        charInst = Instantiate(charPrf, new Vector3(1, 0.5f, 1), Quaternion.identity, transform);
    }

    List<Vector2Int> GetBFS()
    {

        bool[,] visited = new bool[w, h];
        Vector2Int?[,] parent = new Vector2Int?[w, h];
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        q.Enqueue(start);
        visited[start.x, start.y] = true;

        while (q.Count > 0)
        {
            Vector2Int cur = q.Dequeue();
            if (cur == goal)
            {
                List<Vector2Int> path = new List<Vector2Int>();
                Vector2Int? n = goal;

                while (n != null)
                {
                    path.Add(n.Value); n = parent[n.Value.x, n.Value.y];
                }

                path.Reverse();
                return path;
            }

            foreach (var d in dirs)
            {
                int nx = cur.x + d.x, ny = cur.y + d.y;
                if (nx < 0 || ny < 0 || nx >= w || ny >= h || map[nx, ny] == 1 || visited[nx, ny]) continue;
                visited[nx, ny] = true;
                parent[nx, ny] = cur;
                q.Enqueue(new Vector2Int(nx, ny));
            }
        }
        return null;
    }

    void DrawPath()
    {
        foreach (Transform t in transform)
        {
            if (t.name == "Path") Destroy(t.gameObject);
        }

        if (pathList != null)
        {
            foreach (var p in pathList) 
            {
                Instantiate(pathPrf, new Vector3(p.x, 0.1f, p.y), Quaternion.identity, transform).name = "Path";
            }
        }
    }

    IEnumerator Move()
    {
        if (charInst == null || pathList == null) yield break;
        foreach (var p in pathList)
        {
            charInst.transform.position = new Vector3(p.x, 0.5f, p.y);
            yield return new WaitForSeconds(0.1f);
        }
    }

    void ClearObj()
    {
        foreach (Transform t in transform) Destroy(t.gameObject);
    }

    void DrawFurthest()
    {
        foreach (Transform t in transform)
        {
            if (t.name == "Far") Destroy(t.gameObject);
        }

        int[,] dist = new int[w, h];
        Queue<Vector2Int> q = new Queue<Vector2Int>();

        q.Enqueue(start); dist[start.x, start.y] = 1;
        int max = 1;

        while (q.Count > 0)
        {
            var c = q.Dequeue();
            foreach (var d in dirs)
            {
                int nx = c.x + d.x, ny = c.y + d.y;
                if (nx >= 0 && ny >= 0 && nx < w && ny < h && map[nx, ny] == 0 && dist[nx, ny] == 0)
                {
                    dist[nx, ny] = dist[c.x, c.y] + 1;
                    max = dist[nx, ny];
                    q.Enqueue(new Vector2Int(nx, ny));
                }
            }
        }

        for (int x = 0; x < w; x++) for (int y = 0; y < h; y++)
            {
                if (dist[x, y] == max) Instantiate(farPrf, new Vector3(x, 0.1f, y), Quaternion.identity, transform).name = "Far";
            }
    }
}