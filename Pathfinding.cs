using System.Collections.Generic;
using UnityEngine;
public class Pathfinding : MonoBehaviour
{
    private static Pathfinding _instance;
    public static Pathfinding Instance { get { return _instance; } }
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }
    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal)
    {
        var openSet = new List<Node>();
        var closedSet = new HashSet<Vector2Int>();
        var startNode = new Node(start, 0, CalculateHeuristic(start, goal));
        openSet.Add(startNode);
        while (openSet.Count > 0)
        {
            var current = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].FCost < current.FCost || (openSet[i].FCost == current.FCost && openSet[i].HCost < current.HCost))
                {
                    current = openSet[i];
                }
            }
            openSet.Remove(current);
            closedSet.Add(current.Position);
            if (current.Position == goal)
            {
                return RetracePath(startNode, current);
            }
            foreach (var neighbor in GetNeighbors(current.Position))
            {
                if (closedSet.Contains(neighbor) || !IsTileWalkable(neighbor))
                    continue;
                float newCostToNeighbor = current.GCost + CalculateHeuristic(current.Position, neighbor);
                var neighborNode = openSet.Find(n => n.Position == neighbor);
                if (neighborNode == null)
                {
                    neighborNode = new Node(neighbor, newCostToNeighbor, CalculateHeuristic(neighbor, goal));
                    neighborNode.Parent = current;
                    openSet.Add(neighborNode);
                }
                else if (newCostToNeighbor < neighborNode.GCost)
                {
                    neighborNode.GCost = newCostToNeighbor;
                    neighborNode.Parent = current;
                }
            }
        }
        return null; // No path found
    }
    private List<Vector2Int> RetracePath(Node startNode, Node endNode)
    {
        var path = new List<Vector2Int>();
        var currentNode = endNode;
        while (currentNode != startNode)
        {
            path.Add(currentNode.Position);
            currentNode = currentNode.Parent;
        }
        path.Reverse();
        return path;
    }
    private float CalculateHeuristic(Vector2Int a, Vector2Int b)
    {
        return Vector2Int.Distance(a, b);
    }
    private List<Vector2Int> GetNeighbors(Vector2Int position)
    {
        var neighbors = new List<Vector2Int>
        {
            new Vector2Int(position.x + 1, position.y),
            new Vector2Int(position.x - 1, position.y),
            new Vector2Int(position.x, position.y + 1),
            new Vector2Int(position.x, position.y - 1)
        };
        return neighbors.FindAll(n => IsPositionValid(n));
    }
    private bool IsPositionValid(Vector2Int position)
    {
        return position.x >= 0 && position.x < GameManager.Instance.worldSize &&
               position.y >= 0 && position.y < GameManager.Instance.worldSize;
    }
    public bool IsTileWalkable(Vector2Int position)
    {
        Tile tile = GameManager.Instance.GetTileAt(position.x, position.y);
        return tile != null && tile.type != Tile.TileType.Water;
    }
    private class Node
    {
        public Vector2Int Position;
        public float GCost;
        public float HCost;
        public float FCost { get { return GCost + HCost; } }
        public Node Parent;
        public Node(Vector2Int position, float gCost, float hCost)
        {
            Position = position;
            GCost = gCost;
            HCost = hCost;
        }
    }
}