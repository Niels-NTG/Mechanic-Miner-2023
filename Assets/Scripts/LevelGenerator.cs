using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class LevelGenerator : MonoBehaviour
{
    private const int MIN_ELEMENT_COUNT = 2;
    private const int MAX_ELEMENT_COUNT = 20;

    public Tile impassableTile;
    public Tile borderTile;
    public Tile spikeTile;
    public Tile levelEntryTile;
    public Tile levelExitTile;

    public Tilemap tilemap;

    private RectInt levelSize = new RectInt(1, 1, 18, 13);
    
    

    [ContextMenu("Generate")]
    void Generate()
    {
        Clear();
        
        for (int y = 0; y < levelSize.yMax + 1; y++)
        {
            tilemap.SetTile(new Vector3Int(0, y), borderTile);
            tilemap.SetTile(new Vector3Int(levelSize.xMax, y), borderTile);
        }

        for (int x = 0; x < levelSize.xMax + 1; x++)
        {
            tilemap.SetTile(new Vector3Int(x, 0), borderTile);
            tilemap.SetTile(new Vector3Int(x, levelSize.yMax), borderTile);
        }

        Vector3Int entryLocation;
        Vector3Int exitLocation;
        do
        {
            entryLocation = randomLocation();
            exitLocation = randomLocation();
        } while (entryLocation == exitLocation);
        tilemap.SetTile(entryLocation, levelEntryTile);
        tilemap.SetTile(exitLocation, levelExitTile);

        int numberOfElements = Random.Range(MIN_ELEMENT_COUNT, MAX_ELEMENT_COUNT + 1);
        Debug.Log(numberOfElements);
        for (int elementIndex = 0; elementIndex < numberOfElements; elementIndex++)
        {
            Vector3Int aa;
            Vector3Int bb;
            Tile elementTileType;
            
            if (Random.value > 0.5f)
            {
                // Create box
                aa = randomLocation();
                bb = randomLocation();
                elementTileType = impassableTile;
            }
            else
            {
                // Create line
                aa = randomLocation();
                if (Random.value > 0.5f)
                {
                    bb = new Vector3Int(aa.x, Random.Range(levelSize.yMin, levelSize.yMax));
                }
                else
                {
                    bb = new Vector3Int(Random.Range(levelSize.xMin, levelSize.xMax), aa.y);
                }

                elementTileType = spikeTile;
            }
            
            aa = Vector3Int.Min(aa, bb);
            bb = Vector3Int.Max(aa, bb);
                
            tilemap.BoxFill(aa, elementTileType, aa.x, aa.y, bb.x, bb.y);
        }
    }

    private Vector3Int randomLocation()
    {
        return new Vector3Int(
            Random.Range(levelSize.xMin, levelSize.xMax),
            Random.Range(levelSize.yMin, levelSize.yMax)
        );
    }

    [ContextMenu("Clear")]
    void Clear()
    {
        tilemap.ClearAllTiles();
    }

    private void Awake()
    {
        tilemap.size = new Vector3Int(20, 15);
    }

    void Start()
    {
        Generate();
    }

}
