using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = System.Random;

public class LevelGenerator : Level
{
    private const int MIN_ELEMENT_COUNT = 2;
    private const int MAX_ELEMENT_COUNT = 20;
    private const int MIN_BOX_SIZE = 3;
    private const int MIN_LINE_SIZE = 2;

    public bool useEditorSeed;
    public int editorSeed = 877;

    private Random rng;

    private readonly List<LevelElement> LevelElements = new List<LevelElement>();

    [ContextMenu("Generate")]
    public void Generate()
    {
        Clear();

        if (useEditorSeed)
        {
            rng = new Random(editorSeed);
        } else if (rng == null)
        {
            rng = new Random();
        }

        int numberOfElements = RandomInt(MIN_ELEMENT_COUNT, MAX_ELEMENT_COUNT + 1);
        for (int elementIndex = 0; elementIndex < numberOfElements; elementIndex++)
        {
            if (RandomState())
            {
                LevelElements.Add(CreateBoxElement());
            }
            else
            {
                LevelElements.Add(CreateLineElement());
            }
        }

        foreach (LevelElement element in LevelElements)
        {
            element.Place();
        }
        PlaceEntryAndExit();
    }

    public void Generate(Random inputRng)
    {
        rng = inputRng;
        Generate();
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
        Vector2Int candidateEntryLocation;
        Vector2Int candidateExitLocation;
        do
        {
            candidateEntryLocation = randomLocation();
            candidateExitLocation = randomLocation();
        } while (
            candidateEntryLocation == candidateExitLocation ||
            LevelElements.Exists(el => el.Contains(candidateEntryLocation)) ||
            LevelElements.Exists(el => el.Contains(candidateExitLocation))
        );

        entryLocation = candidateEntryLocation;
        exitLocation = candidateExitLocation;

        entryTilemap.SetTile((Vector3Int) candidateEntryLocation, entryTile);
        exitTilemap.SetTile((Vector3Int)candidateExitLocation, exitTile);
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

    private Vector2Int randomLocation()
    {
        return new Vector2Int(
            RandomInt(levelSize.xMin, levelSize.xMax),
            RandomInt(levelSize.yMin, levelSize.yMax)
        );
    }

    [ContextMenu("Clear")]
    public void Clear()
    {
        impassableTilemap.ClearAllTiles();
        spikesTilemap.ClearAllTiles();
        entryTilemap.ClearAllTiles();
        exitTilemap.ClearAllTiles();
        LevelElements.Clear();
        entryLocation = new Vector2Int();
        exitLocation = new Vector2Int();
    }

    private interface LevelElement
    {
        public bool Contains(Vector2Int point)
        {
            return false;
        }

        public void Place()
        {

        }
    }

    private class BoxElement : LevelElement
    {

        private Vector2Int boxSize = new Vector2Int(MIN_BOX_SIZE, MIN_BOX_SIZE);

        private Vector2Int BoxSize
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

        private Vector2Int BoxOrigin
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

        private bool IsOutline
        {
            get;
            set;
        }

        private readonly Tilemap tilemap;
        private readonly Tile tile;

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
            RectInt rect = CreateRect();
            if (
                IsOutline &&
                Vector2Int.Max(point, rect.min + 2 * Vector2Int.one) == point &&
                Vector2Int.Min(point, rect.max - 2 * Vector2Int.one) == point
            )
            {
                return false;
            }
            return rect.Contains(point);
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

    private class LineElement : LevelElement
    {
        private int lineSize = MIN_LINE_SIZE;

        private int LineSize
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

        private Vector2Int LineOrigin
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

        private bool IsVertical
        {
            get;
            set;
        }

        private bool IsSpike
        {
            get;
            set;
        }

        private readonly Tilemap tilemap;
        private readonly Tile tile;
        private readonly Tilemap spikeTilemap;
        private readonly Tile spikeTile;

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
