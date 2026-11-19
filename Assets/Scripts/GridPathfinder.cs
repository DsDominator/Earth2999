
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class GridPathfinder : MonoBehaviour
{
    [Header("Tilemaps")]
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap obstacleTilemap;

    [Header("Grid")]
    [SerializeField] private bool showGrid = true;

    private Node[,] grid;
    private BoundsInt bounds;

    private class Node
    {
        public Vector3 worldPosition;
        public Vector3Int cellPosition;
        public bool walkable;

        public int gCost;
        public int hCost;
        public Node parent;

        public int fCost => gCost + hCost;

        public Node(Vector3 world, Vector3Int cell, bool canWalk)
        {
            worldPosition = world;
            cellPosition = cell;
            walkable = canWalk;
            gCost = int.MaxValue;
        }
    }

    private void Awake()
    {
        CreateGrid();
    }

    // =====================================
    // CREATE GRID FROM TILEMAP
    // =====================================

    public void CreateGrid()
    {
        if (groundTilemap == null || obstacleTilemap == null)
        {
            Debug.LogError("Chưa gán GroundTilemap hoặc ObstacleTilemap!");
            return;
        }

        bounds = groundTilemap.cellBounds;

        grid = new Node[bounds.size.x, bounds.size.y];

        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                Vector3Int cell = new Vector3Int(
                    bounds.xMin + x,
                    bounds.yMin + y,
                    0
                );

                Vector3 worldPosition =
                    groundTilemap.GetCellCenterWorld(cell);

                bool hasGround = groundTilemap.HasTile(cell);
                bool hasObstacle = obstacleTilemap.HasTile(cell);

                bool walkable = hasGround && !hasObstacle;

                grid[x, y] = new Node(
                    worldPosition,
                    cell,
                    walkable
                );
            }
        }

        Debug.Log(
            $"Grid created: {bounds.size.x} x {bounds.size.y}"
        );
    }

    // =====================================
    // GET NODE
    // =====================================

    private Node GetNode(Vector3 worldPosition)
    {
        Vector3Int cell =
            groundTilemap.WorldToCell(worldPosition);

        int x = cell.x - bounds.xMin;
        int y = cell.y - bounds.yMin;

        if (x < 0 || x >= bounds.size.x ||
            y < 0 || y >= bounds.size.y)
        {
            return null;
        }

        return grid[x, y];
    }

    // =====================================
    // FIND PATH - A STAR
    // =====================================

    public List<Vector3> FindPath(
        Vector3 startPosition,
        Vector3 targetPosition)
    {
        if (grid == null)
            return null;

        Node startNode = GetNode(startPosition);
        Node targetNode = GetNode(targetPosition);

        if (startNode == null || targetNode == null)
        {
            Debug.LogWarning("Enemy hoặc Player nằm ngoài Grid!");
            return null;
        }

        if (!targetNode.walkable)
        {
            Debug.LogWarning("Ô chứa Player đang bị đánh dấu là vật cản!");
            return null;
        }

        // Nếu Enemy đang đứng trong ô không đi được,
        // tìm một ô đi được gần đó.
        if (!startNode.walkable)
        {
            startNode = FindNearestWalkableNode(startNode);

            if (startNode == null)
                return null;
        }

        foreach (Node node in grid)
        {
            node.gCost = int.MaxValue;
            node.hCost = 0;
            node.parent = null;
        }

        List<Node> openList = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();

        startNode.gCost = 0;
        startNode.hCost = GetDistance(startNode, targetNode);

        openList.Add(startNode);

        while (openList.Count > 0)
        {
            Node current = openList[0];

            for (int i = 1; i < openList.Count; i++)
            {
                Node candidate = openList[i];

                if (candidate.fCost < current.fCost ||
                    (candidate.fCost == current.fCost &&
                     candidate.hCost < current.hCost))
                {
                    current = candidate;
                }
            }

            openList.Remove(current);
            closedSet.Add(current);

            if (current == targetNode)
                return RetracePath(startNode, targetNode);

            foreach (Node neighbor in GetNeighbors(current))
            {
                if (!neighbor.walkable || closedSet.Contains(neighbor))
                    continue;

                int newCost =
                    current.gCost + GetDistance(current, neighbor);

                if (newCost < neighbor.gCost)
                {
                    neighbor.gCost = newCost;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parent = current;

                    if (!openList.Contains(neighbor))
                        openList.Add(neighbor);
                }
            }
        }

        Debug.LogWarning("A* không tìm được đường đi!");
        return null;
    }

    // =====================================
    // FIND NEAREST WALKABLE NODE
    // =====================================

    private Node FindNearestWalkableNode(Node origin)
    {
        int originX = origin.cellPosition.x - bounds.xMin;
        int originY = origin.cellPosition.y - bounds.yMin;

        for (int radius = 1; radius <= 5; radius++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    int checkX = originX + x;
                    int checkY = originY + y;

                    if (checkX < 0 || checkX >= bounds.size.x ||
                        checkY < 0 || checkY >= bounds.size.y)
                        continue;

                    Node node = grid[checkX, checkY];

                    if (node.walkable)
                        return node;
                }
            }
        }

        return null;
    }

    // =====================================
    // NEIGHBORS
    // =====================================

    private List<Node> GetNeighbors(Node node)
    {
        List<Node> result = new List<Node>();

        int centerX = node.cellPosition.x - bounds.xMin;
        int centerY = node.cellPosition.y - bounds.yMin;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                int checkX = centerX + x;
                int checkY = centerY + y;

                if (checkX < 0 || checkX >= bounds.size.x ||
                    checkY < 0 || checkY >= bounds.size.y)
                    continue;

                // Chặn đi chéo qua góc vật cản
                if (x != 0 && y != 0)
                {
                    if (!grid[centerX + x, centerY].walkable ||
                        !grid[centerX, centerY + y].walkable)
                        continue;
                }

                result.Add(grid[checkX, checkY]);
            }
        }

        return result;
    }

    // =====================================
    // DISTANCE
    // =====================================

    private int GetDistance(Node a, Node b)
    {
        int dx = Mathf.Abs(
            a.cellPosition.x - b.cellPosition.x
        );

        int dy = Mathf.Abs(
            a.cellPosition.y - b.cellPosition.y
        );

        int diagonal = Mathf.Min(dx, dy);
        int straight = Mathf.Abs(dx - dy);

        return diagonal * 14 + straight * 10;
    }

    // =====================================
    // RETRACE PATH
    // =====================================

    private List<Vector3> RetracePath(Node start, Node end)
    {
        List<Vector3> path = new List<Vector3>();

        Node current = end;

        while (current != start)
        {
            path.Add(current.worldPosition);
            current = current.parent;

            if (current == null)
                return null;
        }

        path.Reverse();
        return path;
    }

    // =====================================
    // DEBUG
    // =====================================

    private void OnDrawGizmos()
    {
        if (!showGrid || grid == null)
            return;

        foreach (Node node in grid)
        {
            Gizmos.color = node.walkable
                ? new Color(1f, 1f, 1f, 0.15f)
                : Color.red;

            Gizmos.DrawCube(
                node.worldPosition,
                Vector3.one * 0.8f
            );
        }
    }
}
