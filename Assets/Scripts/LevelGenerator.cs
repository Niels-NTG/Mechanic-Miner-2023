using UnityEngine;
using UnityEngine.Tilemaps;
using Random = System.Random;

public class LevelGenerator : MonoBehaviour
{
    private const int MIN_ELEMENT_COUNT = 2;
    private const int MAX_ELEMENT_COUNT = 20;
    
    // Inner rect of the level measured in units of tiles.
    private RectInt levelSize = new RectInt(1, 1, 18, 13);
    
    [SerializeField] private Tilemap impassableTilemap;
    [SerializeField] private Tile impassableTile;
    [SerializeField] private Tilemap spikesTilemap;
    [SerializeField] private Tile spikeTile;
    [SerializeField] private Tilemap entryTilemap;
    [SerializeField] private Tile entryTile;
    [SerializeField] private Tilemap exitTilemap;
    [SerializeField] private Tile exitTile;

    [SerializeField] private GameObject player;
    
    [SerializeField] private int seed = 1289897231;
    private Random rng;

    [ContextMenu("Generate")]
    private void Generate()
    {
        Clear();

        rng = new Random(seed);
        
        PlaceEntryAndExit();

        int numberOfElements = RandomInt(MIN_ELEMENT_COUNT, MAX_ELEMENT_COUNT + 1);
        for (int elementIndex = 0; elementIndex < numberOfElements; elementIndex++)
        {
            if (RandomState())
            {
                PlaceBox();
            }
            else
            {
                PlaceLine();
            }
        }
    }

    private void PlaceEntryAndExit()
    {
        Vector3Int entryLocation;
        Vector3Int exitLocation;
        do
        {
            entryLocation = randomLocation();
            exitLocation = randomLocation();
        } while (entryLocation == exitLocation);
        entryTilemap.SetTile(entryLocation, entryTile);
        exitTilemap.SetTile(exitLocation, exitTile);

        player.transform.position = entryLocation + new Vector3(0.5f, 0.5f);
    }

    private void PlaceBox()
    {
        Vector3Int aa;
        Vector3Int bb;
        do
        {
            aa = randomLocation();
            bb = randomLocation();
            aa = Vector3Int.Min(aa, bb);
            bb = Vector3Int.Max(aa, bb);
        } while (bb.x - aa.x < 3 || bb.y - aa.y < 3);

        bool isOutline = RandomState();

        RectInt box = new RectInt((Vector2Int)aa, (Vector2Int)(bb - aa));
        foreach (Vector3Int point in box.allPositionsWithin)
        {
            if (isOutline)
            {
                if (point.x == box.xMin || point.y == box.yMin || point.x == box.xMax - 1 || point.y == box.yMax - 1)
                {
                    impassableTilemap.SetTile(point, impassableTile);        
                }
                continue;
            }
            impassableTilemap.SetTile(point, impassableTile);            
        }
    }

    private void PlaceLine()
    {
        bool isVertical = RandomState();
        Vector3Int aa = randomLocation();
        int lineLength = isVertical
            ? RandomInt(levelSize.yMin + 1, levelSize.yMax)
            : RandomInt(levelSize.xMin + 1, levelSize.xMax);
        bool isSpike = RandomState();

        for (int i = isVertical ? aa.y : aa.x; i < lineLength; i++)
        {
            Vector3Int point = isVertical ? new Vector3Int(aa.x, i) : new Vector3Int(i, aa.y);
            if (isSpike)
            {
                spikesTilemap.SetTile(point, spikeTile);
            }
            else
            {
                impassableTilemap.SetTile(point, impassableTile);
            }
        }
    }

    private bool RandomState()
    {
        return RandomInt(0, 2) == 1;
    }
    private int RandomInt(int min, int max)
    {
        return rng.Next(min, max);
    }
    
    private Vector3Int randomLocation()
    {
        return new Vector3Int(
            RandomInt(levelSize.xMin, levelSize.xMax),
            RandomInt(levelSize.yMin, levelSize.yMax)
        );
    }

    [ContextMenu("Clear")]
    private void Clear()
    {
        impassableTilemap.ClearAllTiles();
        spikesTilemap.ClearAllTiles();
        entryTilemap.ClearAllTiles();
        exitTilemap.ClearAllTiles();
    }

    private void Awake()
    {
        Generate();
    }
}
