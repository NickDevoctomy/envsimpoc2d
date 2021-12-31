using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Map : MonoBehaviour
{
    public static Map Instance => _instance;

    [Range(2, 400)] public int Width = 200;
    [Range(2, 400)] public int Height = 200;
    public int Seed;
    public int GrassSeedOffset = 100;

    public TileBase WaterTile;
    public TileBase LandTile;
    public TileBase SandTile;
    public TileBase GrassTile;
    public TileBase FlowersTile;
    public TileBase RockTile;

    private static Map _instance;
    private GameObject _tilesRoot;
    private GameObject _gridRoot;
    private Tilemap _tilemap;

    private PerlinNoiseMapGenerator _perlinNoiseMapGenerator;
    private TileType[,] _terrain;
    private bool[,] _walkable;

    public Map()
    {
        _instance = this;
        _perlinNoiseMapGenerator = new PerlinNoiseMapGenerator();
    }

    void Start()
    {
        Prepare();
        CreateMap();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3Int tilemapPos = _tilemap.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            //tilemapPos is the tile that was clicked
        }
    }

    private void Prepare()
    {
        _tilesRoot = transform.AssureEmptyGameObjectChild("Tiles");
        _tilesRoot.AddComponent<Grid>();

        _gridRoot = _tilesRoot.transform.AssureEmptyGameObjectChild("Grid");
        _tilemap = _gridRoot.AddComponent<Tilemap>();
        _gridRoot.AddComponent<TilemapRenderer>();
    }

    private void CreateMap()
    {
        _walkable = new bool[Width, Height];
        _terrain = new TileType[Width, Height];
        var terrain = _perlinNoiseMapGenerator.Generate(Seed, Width, Height);
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                var tileType = GetTileTypeFromHeight(terrain[x, y]);
                var tileBase = GetTileBaseFromTileType(tileType);

                _tilemap.SetTile(
                    new Vector3Int(x, y, 0),
                    WaterTile);

                switch (tileType)
                {
                    case TileType.Land:
                        {
                            _tilemap.SetTile(
                                new Vector3Int(x, y, 1),
                                tileBase);
                            _walkable[x, y] = true;
                            break;
                        }

                    case TileType.Rock:
                        {
                            _tilemap.SetTile(
                                new Vector3Int(x, y, 1),
                                LandTile);

                            _tilemap.SetTile(
                                new Vector3Int(x, y, 5),
                                tileBase);
                            break;
                        }
                }

                _terrain[x,y] = tileType;
            }
        }

        AddLandLayer(
            2,
            terrain,
            SandTile,
            0.475f,
            0.6f,
            new List<TileType> { TileType.Water, TileType.Land, TileType.Rock });

        var grassNoiseLayer = AddLandLayer(
            3,
            GrassSeedOffset,
            GrassTile,
            0.2f,
            0.6f,
            new List<TileType> { TileType.Land, TileType.Rock });

        AddLandLayer(
            4,
            grassNoiseLayer,
            FlowersTile,
            0.25f,
            0.55f,
            new List<TileType> { TileType.Land, TileType.Rock });
    }

    private float[,] AddLandLayer(
        int zIndex,
        int seedOffset,
        TileBase tile,
        float minHeight,
        float maxHeight,
        List<TileType> ontop)
    {
        var noiseLayer = _perlinNoiseMapGenerator.Generate(Seed + seedOffset, Width, Height);
        AddLandLayer(zIndex, noiseLayer, tile, minHeight, maxHeight, ontop);
        return noiseLayer;
    }

    private void AddLandLayer(
        int zIndex,
        float[,] noiseLayer,
        TileBase tile,
        float minHeight,
        float maxHeight,
        List<TileType> ontop)
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                var height = noiseLayer[x, y];
                if (height > minHeight && height < maxHeight)
                {
                    if (ontop.Contains(_terrain[x, y]))
                    {
                        _tilemap.SetTile(
                            new Vector3Int(x, y, zIndex),
                            tile);

                        _walkable[x,y] = _terrain[x,y] != TileType.Rock;
                    }
                }
            }
        }
    }

    private TileType GetTileTypeFromHeight(float height)
    {
        if (height < 0.5f)
        {
            return TileType.Water;
        }
        else if (height < 0.75f)
        {
            return TileType.Land;
        }
        else
        {
            return TileType.Rock;
        }
    }

    private TileBase GetTileBaseFromTileType(TileType tileType)
    {
        switch(tileType)
        {
            case TileType.Water:
                {
                    return WaterTile;
                }

            case TileType.Land:
                {
                    return LandTile;
                }

            case TileType.Rock:
                {
                    return RockTile;
                }

            default:
                {
                    throw new System.NotSupportedException($"Tile type {tileType} is not supported by GetTileBaseFromTileType.");
                }
        }
    }
}
