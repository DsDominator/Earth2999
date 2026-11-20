
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class GridPathfinder : MonoBehaviour
{
    [Header("Tilemaps")]
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap obstacleTilemap;

    [Header("Grid Debug")]
    [SerializeField] private bool showGrid = true;

    [Header("Pathfinding Settings")]
    [SerializeField] private float safetyMargin = 0.1f;
    [SerializeField] private int nearestNodeSearchRadius = 10;

    private Node[,] grid;
    private BoundsInt bounds;

    // =====================================
    // NODE
    // =====================================

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

    // =====================================
    // INITIALIZE
    // =====================================

    private void Awake()
    {
        CreateGrid();
    }

    public void CreateGrid()
    {
        if (groundTilemap == null || obstacleTilemap == null)
        {
            Debug.LogError(
                "GridPathfinder: Chưa gán Ground Tilemap hoặc Rock Tilemap!"
            );
            grid = null;
            return;
        }

        bounds = groundTilemap.cellBounds;

        if (bounds.size.x <= 0 || bounds.size.y <= 0)
        {
            Debug.LogError("GridPathfinder: Ground Tilemap không có tile!");
            grid = null;
            return;
        }

        grid = new Node[bounds.size.x, bounds.size.y];

        int walkableCount = 0;
        int obstacleCount = 0;

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

                Vector3Int rockCell =
                    obstacleTilemap.WorldToCell(worldPosition);

                bool hasObstacle =
                    obstacleTilemap.HasTile(rockCell);

                bool walkable = hasGround && !hasObstacle;

                grid[x, y] = new Node(
                    worldPosition,
                    cell,
                    walkable
                );

                if (walkable)
                    walkableCount++;

                if (hasObstacle)
                    obstacleCount++;
            }
        }

        Debug.Log(
            $"Grid created: {bounds.size.x} x {bounds.size.y} | " +
            $"Walkable: {walkableCount} | " +
            $"Obstacles: {obstacleCount}"
        );
    }

    // =====================================
    // GET NODE
    // =====================================

    private Node GetNode(Vector3 worldPosition)
    {
        if (grid == null || groundTilemap == null)
            return null;

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
    // CHECK WALKABLE FOR AGENT SIZE
    // =====================================

    private bool IsWalkableForAgent(
        Node node,
        Vector2 agentHalfSize
    )
    {
        if (node == null || !node.walkable)
            return false;

        Vector2 halfSize = agentHalfSize +
            Vector2.one * safetyMargin;

        Vector3 center = node.worldPosition;

        Vector3 minWorld = center - (Vector3)halfSize;
        Vector3 maxWorld = center + (Vector3)halfSize;

        Vector3Int minCell =
            obstacleTilemap.WorldToCell(minWorld);

        Vector3Int maxCell =
            obstacleTilemap.WorldToCell(maxWorld);

        // Lấy kích thước một ô Tilemap trong World Space.
        Vector3 cellSize = obstacleTilemap.cellSize;

        Vector3 scale = obstacleTilemap.transform.lossyScale;

        Vector2 tileHalfSize = new Vector2(
            Mathf.Abs(cellSize.x * scale.x) / 2f,
            Mathf.Abs(cellSize.y * scale.y) / 2f
        );

        for (int x = minCell.x; x <= maxCell.x; x++)
        {
            for (int y = minCell.y; y <= maxCell.y; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);

                if (!obstacleTilemap.HasTile(cell))
                    continue;

                Vector3 rockCenter =
                    obstacleTilemap.GetCellCenterWorld(cell);

                bool overlapX =
                    Mathf.Abs(center.x - rockCenter.x) <
                    halfSize.x + tileHalfSize.x;

                bool overlapY =
                    Mathf.Abs(center.y - rockCenter.y) <
                    halfSize.y + tileHalfSize.y;

                if (overlapX && overlapY)
                    return false;
            }
        }

        return true;
    }

    // =====================================
    // FIND PATH - A STAR
    // =====================================

    public List<Vector3> FindPath(
        Vector3 startPosition,
        Vector3 targetPosition,
        Vector2 agentHalfSize
    )
    {
        if (grid == null)
            return null;

        Node startNode = GetNode(startPosition);
        Node targetNode = GetNode(targetPosition);

        if (startNode == null || targetNode == null)
        {
            Debug.LogWarning(
                "GridPathfinder: Enemy hoặc Player nằm ngoài Grid!"
            );
            return null;
        }

        // Kiểm tra vị trí Player.
        // Không cần kiểm tra kích thước Enemy tại đích vì
        // Enemy sẽ dừng ở waypoint cuối cùng gần Player.
        if (!targetNode.walkable)
        {
            Debug.LogWarning(
                "GridPathfinder: Ô chứa Player không đi được!"
            );
            return null;
        }

        // Tìm node hợp lệ gần Enemy nếu Enemy đang ở vị trí không hợp lệ.
        if (!IsWalkableForAgent(startNode, agentHalfSize))
        {
            startNode = FindNearestWalkableNode(
                startNode,
                agentHalfSize
            );

            if (startNode == null)
            {
                Debug.LogWarning(
                    "GridPathfinder: Không tìm được node bắt đầu!"
                );
                return null;
            }
        }

        // Reset dữ liệu A*.
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
            {
                return RetracePath(startNode, targetNode);
            }

            foreach (Node neighbor in GetNeighbors(current))
            {
                if (closedSet.Contains(neighbor))
                    continue;

                if (!IsWalkableForAgent(neighbor, agentHalfSize))
                    continue;

                int newCost =
                    current.gCost + GetDistance(current, neighbor);

                if (newCost < neighbor.gCost)
                {
                    neighbor.gCost = newCost;
                    neighbor.hCost =
                        GetDistance(neighbor, targetNode);

                    neighbor.parent = current;

                    if (!openList.Contains(neighbor))
                        openList.Add(neighbor);
                }
            }
        }

        return null;
    }

    // =====================================
    // FIND NEAREST WALKABLE NODE
    // =====================================

    private Node FindNearestWalkableNode(
        Node origin,
        Vector2 agentHalfSize
    )
    {
        int originX =
            origin.cellPosition.x - bounds.xMin;

        int originY =
            origin.cellPosition.y - bounds.yMin;

        for (int radius = 1;
             radius <= nearestNodeSearchRadius;
             radius++)
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

                    if (IsWalkableForAgent(node, agentHalfSize))
                        return node;
                }
            }
        }

        return null;
    }

    // =====================================
    // GET NEIGHBORS
    // =====================================

    private List<Node> GetNeighbors(Node node)
    {
        List<Node> result = new List<Node>();

        int centerX =
            node.cellPosition.x - bounds.xMin;

        int centerY =
            node.cellPosition.y - bounds.yMin;

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

                // Không cho phép đi chéo qua góc vật cản.
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

    private List<Vector3> RetracePath(
        Node start,
        Node end
    )
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
    // DEBUG GRID
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
