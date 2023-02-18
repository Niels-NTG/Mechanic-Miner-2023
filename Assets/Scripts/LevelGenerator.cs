using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = System.Random;

public class LevelGenerator : MonoBehaviour
{
    private const int MIN_ELEMENT_COUNT = 2;
    private const int MAX_ELEMENT_COUNT = 20;
    private const int MIN_BOX_SIZE = 3;
    private const int MIN_LINE_SIZE = 2;

    // Inner rect of the level measured in units of tiles.
    private static readonly RectInt levelSize = new RectInt(1, 1, 18, 13);

    public Tilemap impassableTilemap;
    public Tile impassableTile;
    public Tilemap spikesTilemap;
    public Tile spikeTile;
    public Tilemap entryTilemap;
    public Tile entryTile;
    public Tilemap exitTilemap;
    public Tile exitTile;

    [SerializeField] private GameObject player;
    
    [SerializeField] private int seed = 1289897231;
    private Random rng;

    public List<LineElement> lineElements = new List<LineElement>();
    public List<BoxElement> boxElements = new List<BoxElement>();

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
                BoxElement boxElement = CreateBoxElement();
                boxElement.Place();
                boxElements.Add(boxElement);
            }
            else
            {
                LineElement lineElement = CreateLineElement();
                lineElement.Place();
                lineElements.Add(lineElement);
            }
        }
    }

    private BoxElement CreateBoxElement()
    {
        return new BoxElement(rng, impassableTilemap, impassableTile);
    }

    private LineElement CreateLineElement()
    {
        return new LineElement(rng, impassableTilemap, impassableTile, spikesTilemap, spikeTile);
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

    private bool RandomState()
    {
        return RandomState(rng);
    }

    private static bool RandomState(Random _rng)
    {
        return RandomInt(_rng, 0, 2) == 1;
    }
    private int RandomInt(int min, int max)
    {
        return RandomInt(rng, min, max);
    }

    private static int RandomInt(Random _rng, int min, int max)
    {
        return _rng.Next(min, max);
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

    // TODO implement abstract class to share between BoxElement and LineElement
    
    public class BoxElement
    {
        
        private Vector2Int boxSize = new Vector2Int(MIN_BOX_SIZE, MIN_BOX_SIZE);
        public Vector2Int BoxSize
        {
            get
            {
                return boxSize;
            }
            set
            {
                boxSize = new Vector2Int(
                    Math.Clamp(value.x, MIN_BOX_SIZE, levelSize.width),
                    Math.Clamp(value.y, MIN_BOX_SIZE, levelSize.height)
                );
            }
        }

        private Vector2Int boxOrigin = new Vector2Int(levelSize.xMin, levelSize.yMin);
        public Vector2Int BoxOrigin
        {
            get
            {
                return boxOrigin;
            }
            set
            {
                boxOrigin = new Vector2Int(
                    Math.Clamp(value.x, levelSize.xMin, levelSize.xMax - MIN_BOX_SIZE),
                    Math.Clamp(value.y, levelSize.yMin, levelSize.yMax - MIN_BOX_SIZE)
                );
            }
        }

        public bool IsOutline
        {
            get;
            set;
        }

        private Tilemap tilemap;
        private Tile tile;

        public BoxElement(Random _rng, Tilemap tilemap, Tile tile)
        {
            BoxOrigin = new Vector2Int(
                RandomInt(_rng, levelSize.xMin, levelSize.xMax),
                RandomInt(_rng, levelSize.xMin, levelSize.xMax)
            );
            BoxSize = new Vector2Int(
                RandomInt(_rng, MIN_BOX_SIZE, levelSize.width),
                RandomInt(_rng, MIN_BOX_SIZE, levelSize.height)
            );
            IsOutline = RandomState(_rng);
            this.tilemap = tilemap;
            this.tile = tile;
        }

        private RectInt CreateRect()
        {
            return new RectInt(BoxOrigin, BoxSize);
        }

        public bool Contains(Vector2Int point)
        {
            return CreateRect().Contains(point);
        }

        public void Place()
        {
            RectInt box = CreateRect();
            box.SetMinMax(box.min, Vector2Int.Min(levelSize.max, box.max));
            foreach (Vector3Int point in box.allPositionsWithin)
            {
                if (IsOutline)
                {
                    if (point.x == box.xMin || point.y == box.yMin || point.x == box.xMax - 1 || point.y == box.yMax - 1)
                    {
                        tilemap.SetTile(point, tile);        
                    }
                    continue;
                }
                tilemap.SetTile(point, tile);
            }
        }
    }

    public class LineElement
    {
        private int lineSize = MIN_LINE_SIZE;
        public int LineSize
        {
            get
            {
                return lineSize;
            }
            set
            {
                lineSize = Math.Clamp(value, MIN_LINE_SIZE, Math.Max(levelSize.width, levelSize.height));
            }
        }

        private Vector2Int lineOrigin = new Vector2Int(levelSize.xMin, levelSize.yMin);
        public Vector2Int LineOrigin
        {
            get
            {
                return lineOrigin;
            }
            set
            {
                lineOrigin = new Vector2Int(
                    Math.Clamp(value.x, levelSize.xMin, levelSize.xMax),
                    Math.Clamp(value.y, levelSize.yMin, levelSize.yMax)
                );
            }
        }

        public bool IsVertical
        {
            get;
            set;
        }

        public bool IsSpike
        {
            get;
            set;
        }

        private Tilemap tilemap;
        private Tile tile;
        private Tilemap spikeTilemap;
        private Tile spikeTile;

        public LineElement(Random _rng, Tilemap tilemap, Tile tile, Tilemap spikeTilemap, Tile spikeTile)
        {
            LineSize = RandomInt(_rng, MIN_LINE_SIZE, Math.Max(levelSize.width, levelSize.height));
            LineOrigin = new Vector2Int(
                RandomInt(_rng, levelSize.xMin, levelSize.xMax),
                RandomInt(_rng, levelSize.xMin, levelSize.xMax)
            );
            IsVertical = RandomState(_rng);
            IsSpike = RandomState(_rng);
            this.tilemap = tilemap;
            this.tile = tile;
            this.spikeTilemap = spikeTilemap;
            this.spikeTile = spikeTile;
        }

        private RectInt CreateRect()
        {
            return new RectInt(
                LineOrigin,
                IsVertical
                    ? new Vector2Int(1, LineSize)
                    : new Vector2Int(LineSize, 1)
            );
        }

        public bool Contains(Vector2Int point)
        {
            return CreateRect().Contains(point);
        }

        public void Place()
        {
            RectInt lineRect = CreateRect();
            foreach (Vector2Int point in lineRect.allPositionsWithin)
            {
                if (levelSize.Contains(point) == false)
                {
                    continue;
                }
                
                Vector3Int tilePoint = (Vector3Int) point;
                if (IsSpike)
                {
                    spikeTilemap.SetTile(tilePoint, spikeTile);
                }
                else
                {
                    tilemap.SetTile(tilePoint, tile);
                }
            }
        }
    }
}
