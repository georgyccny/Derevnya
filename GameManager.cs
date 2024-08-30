using UnityEngine;
using System.Collections.Generic;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public int worldSize = 50;
    public GameObject tilePrefab;
    public GameObject npcPrefab;
    public int npcCount = 10;
    private Tile[,] tiles;
    private List<NPC> npcs = new List<NPC>();
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    void Start()
    {
        GenerateWorld();
        SpawnNPCs();
        SetupCamera();
    }
    void GenerateWorld()
    {
        tiles = new Tile[worldSize, worldSize];
        for (int x = 0; x < worldSize; x++)
        {
            for (int y = 0; y < worldSize; y++)
            {
                Vector3 position = new Vector3(x, y, 0);
                GameObject tileObject = Instantiate(tilePrefab, position, Quaternion.identity);
                tileObject.transform.SetParent(transform);
                Tile tile = tileObject.GetComponent<Tile>();
                tiles[x, y] = tile;
                // Simple terrain generation
                float perlinValue = Mathf.PerlinNoise(x * 0.1f, y * 0.1f);
                if (perlinValue < 0.3f)
                    tile.SetType(Tile.TileType.Water);
                else if (perlinValue < 0.6f)
                    tile.SetType(Tile.TileType.Ground);
                else
                    tile.SetType(Tile.TileType.Forest);
            }
        }
        // Add some houses
        for (int i = 0; i < worldSize / 10; i++)
        {
            int x = Random.Range(0, worldSize);
            int y = Random.Range(0, worldSize);
            if (tiles[x, y].type == Tile.TileType.Ground)
            {
                tiles[x, y].SetType(Tile.TileType.House);
            }
        }
    }
    void SpawnNPCs()
    {
        for (int i = 0; i < npcCount; i++)
        {
            Vector2Int position = GetRandomWalkableTile();
            GameObject npcObject = Instantiate(npcPrefab, new Vector3(position.x, position.y, -1), Quaternion.identity);
            NPC npc = npcObject.GetComponent<NPC>();
            npcs.Add(npc);
        }
    }
    void SetupCamera()
    {
        Camera.main.orthographicSize = worldSize / 2f;
        Camera.main.transform.position = new Vector3(worldSize / 2f, worldSize / 2f, -10f);
    }
    public Tile GetTileAt(int x, int y)
    {
        if (x >= 0 && x < worldSize && y >= 0 && y < worldSize)
        {
            return tiles[x, y];
        }
        return null;
    }
    Vector2Int GetRandomWalkableTile()
    {
        Vector2Int position;
        do
        {
            position = new Vector2Int(Random.Range(0, worldSize), Random.Range(0, worldSize));
        } while (!Pathfinding.Instance.IsTileWalkable(position));
        return position;
    }
}