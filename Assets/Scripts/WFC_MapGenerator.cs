using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class WFC_MapGenerator : MonoBehaviour
{
    [Header("Map Settings")]
    public int mapWidth = 20;
    public int mapHeight = 20;
    public float cellSize = 1f;

    [Header("Tile Database")]
    public TileDatabase tileDatabase;

    // Wraps a tile entry and one of its rotated corner configs
    public class Option
    {
        public TileDatabase.TileEntry entry;
        public TileDatabase.CornerConfig config;
    }

    // Represents each cell in the WFC grid
    public class Cell
    {
        public List<Option> options;
        public bool collapsed;
        public Option chosen;

        public Cell(List<Option> opts)
        {
            options = new List<Option>(opts);
            collapsed = false;
            chosen = null;
        }
    }

    private Cell[,] grid;

    void Start()
    {
        GenerateMap();
    }

    void GenerateMap()
    {
        ClearExistingTiles();

        int maxRetries = 50;
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                InitializeGrid();
                CollapseBordersToWater();
                RunWFC();
                return;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"WFC failed on attempt {attempt + 1}: {e.Message}, retrying...");
                ClearExistingTiles();
            }
        }
        Debug.LogError("WFC failed after multiple attempts.");
    }

    void ClearExistingTiles()
    {
        // Make a copy of the children before destroying
        List<Transform> children = new List<Transform>();
        foreach (Transform child in transform)
        {
            children.Add(child);
        }

        foreach (Transform child in children)
        {
            DestroyImmediate(child.gameObject);
        }
    }

    void InitializeGrid()
    {
        var allOptions = new List<Option>();
        foreach (var tile in tileDatabase.tiles)
            foreach (var cfg in tile.rotations)
                allOptions.Add(new Option { entry = tile, config = cfg });

        grid = new Cell[mapWidth, mapHeight];
        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
                grid[x, y] = new Cell(allOptions);
    }

    void CollapseBordersToWater()
    {
        var waterOptions = new List<Option>();
        foreach (var tile in tileDatabase.tiles)
        {
            if (tile.tileName == "Water")
                foreach (var cfg in tile.rotations)
                    waterOptions.Add(new Option { entry = tile, config = cfg });
        }

        for (int x = 0; x < mapWidth; x++)
        {
            CollapseCell(x, 0, waterOptions);
            CollapseCell(x, mapHeight - 1, waterOptions);
        }
        for (int y = 0; y < mapHeight; y++)
        {
            CollapseCell(0, y, waterOptions);
            CollapseCell(mapWidth - 1, y, waterOptions);
        }


    }

    void CollapseCell(int x, int y, List<Option> allowed)
    {
        var cell = grid[x, y];

        if (cell.collapsed)
            return; // Prevent multiple collapses and multiple instantiations

        if (allowed.Count == 0) throw new System.Exception("No options to collapse to");

        cell.options = new List<Option>(allowed);
        cell.chosen = allowed[Random.Range(0, allowed.Count)];
        cell.collapsed = true;

        Vector3 pos = transform.position + new Vector3(x * cellSize, 0, y * cellSize);
        var prefab = cell.chosen.entry.prefab;
        if (prefab == null)
        {
            throw new System.Exception($"Missing prefab for tile '{cell.chosen.entry.tileName}' at ({x}, {y})");
        }

        Quaternion baseRot = prefab.transform.rotation;
        Quaternion tileRot = Quaternion.Euler(0, cell.chosen.config.rotationDegrees, 0);
        Quaternion finalRot = tileRot * baseRot;
        Instantiate(prefab, pos, finalRot, transform);
    }
    void RunWFC()
    {
        var queue = new Queue<Vector2Int>();
        for (int x = 0; x < mapWidth; x++)
        {
            queue.Enqueue(new Vector2Int(x, 0));
            queue.Enqueue(new Vector2Int(x, mapHeight - 1));
        }
        for (int y = 0; y < mapHeight; y++)
        {
            queue.Enqueue(new Vector2Int(0, y));
            queue.Enqueue(new Vector2Int(mapWidth - 1, y));
        }
        Propagate(queue);

        while (true)
        {
            var next = GetLowestEntropyCell();
            if (!next.HasValue) break;

            var pos = next.Value;
            var opts = grid[pos.x, pos.y].options;
            if (opts.Count == 0) throw new System.Exception("No valid options to collapse");
            CollapseCell(pos.x, pos.y, opts);
            queue.Enqueue(pos);
            Propagate(queue);
        }
    }

    Vector2Int? GetLowestEntropyCell()
    {
        int minOptions = int.MaxValue;
        Vector2Int? best = null;
        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
            {
                var cell = grid[x, y];
                if (cell.collapsed) continue;
                int count = cell.options.Count;
                if (count < minOptions)
                {
                    minOptions = count;
                    best = new Vector2Int(x, y);
                }
            }
        return best;
    }

    void Propagate(Queue<Vector2Int> queue)
    {
        Vector2Int[] directions = new Vector2Int[] {
            new Vector2Int(0, 1),
            new Vector2Int(1, 0),
            new Vector2Int(0, -1),
            new Vector2Int(-1, 0)
        };

        while (queue.Count > 0)
        {
            Vector2Int cur = queue.Dequeue();
            int x = cur.x, y = cur.y;
            var cell = grid[x, y];
            if (!cell.collapsed) continue;
            var chosen = cell.chosen;

            foreach (var dir in directions)
            {
                int nx = x + dir.x;
                int ny = y + dir.y;
                if (nx < 0 || nx >= mapWidth || ny < 0 || ny >= mapHeight) continue;

                var neighbor = grid[nx, ny];
                if (neighbor.collapsed) continue;

                int before = neighbor.options.Count;
                neighbor.options = FilterOptions(neighbor.options, chosen, dir);

                if (neighbor.options.Count == 1)
                {
                    CollapseCell(nx, ny, neighbor.options);
                    queue.Enqueue(new Vector2Int(nx, ny));
                }
                else if (neighbor.options.Count < before)
                {
                    queue.Enqueue(new Vector2Int(nx, ny));
                }
            }
        }
    }

    List<Option> FilterOptions(List<Option> opts, Option chosen, Vector2Int dir)
    {
        var filtered = new List<Option>();
        foreach (var opt in opts)
            if (IsCompatible(chosen.config, opt.config, dir))
                filtered.Add(opt);
        return filtered;
    }

    bool IsCompatible(TileDatabase.CornerConfig a, TileDatabase.CornerConfig b, Vector2Int dir)
    {
        if (dir == Vector2Int.up)
            return b.corners[3] == a.corners[0] && b.corners[2] == a.corners[1];
        if (dir == Vector2Int.right)
            return b.corners[0] == a.corners[1] && b.corners[3] == a.corners[2];
        if (dir == Vector2Int.down)
            return b.corners[0] == a.corners[3] && b.corners[1] == a.corners[2];
        return b.corners[1] == a.corners[0] && b.corners[2] == a.corners[3];
    }

    void OnDrawGizmos()
    {
        if (grid == null) return;
        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
            {
                var cell = grid[x, y];
                Vector3 pos = transform.position + new Vector3(x * cellSize, 0, y * cellSize);
                Gizmos.color = cell.collapsed ? Color.white : Color.gray;
                Gizmos.DrawWireCube(pos + Vector3.up * 0.01f, Vector3.one * (cellSize * 0.9f));
            }
    }
}