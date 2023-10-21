using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Level : MonoBehaviour
{
    // Inner rect of the level measured in units of tiles.
    public static readonly RectInt levelSize = new RectInt(1, 1, 18, 13);

    public Tilemap impassableTilemap;
    public Tile impassableTile;
    public Tilemap spikesTilemap;
    public Tile spikeTile;
    public Tilemap entryTilemap;
    public Tile entryTile;
    public Tilemap exitTilemap;
    public Tile exitTile;

    [NonSerialized]
    public Vector2Int entryLocation;
    [NonSerialized]
    public Vector2Int exitLocation;

    [Header("Components of the level that can be used to construct a Toggleable Game Mechanic (TGM)")]
    public List<Component> componentsWithToggleableProperties;

    private void Awake()
    {
        // Find entry and exit tiles
        foreach (Vector3Int pos in entryTilemap.cellBounds.allPositionsWithin)
        {
            if (entryTilemap.GetTile(pos) != null)
            {
                entryLocation = new Vector2Int(pos.x, pos.y);
                break;
            }
        }
        foreach (Vector3Int pos in exitTilemap.cellBounds.allPositionsWithin)
        {
            if (exitTilemap.GetTile(pos) != null)
            {
                exitLocation = new Vector2Int(pos.x, pos.y);
                break;
            }
        }
    }
}
